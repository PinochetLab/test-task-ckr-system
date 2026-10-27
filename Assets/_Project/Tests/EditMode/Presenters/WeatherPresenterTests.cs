using System;
using NUnit.Framework;
using TestTask.Features.Weather.Config;
using TestTask.Features.Weather.Data;
using TestTask.Features.Weather.Presenter;
using UnityEngine;

namespace TestTask.Tests
{
    public sealed class WeatherPresenterTests
    {
        private WeatherConfig _config;
        private FakeWeatherView _view;
        private FakeWeatherService _service;
        private ManualScheduler _scheduler;
        private WeatherPresenter _presenter;

        private double Interval => _config.PollIntervalSeconds;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<WeatherConfig>();
            _view = new FakeWeatherView();
            _service = new FakeWeatherService();
            _scheduler = new ManualScheduler();
            _presenter = new WeatherPresenter(_view, _service, new FakeSpriteLoader(), _config, _scheduler);
            _presenter.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
            UnityEngine.Object.DestroyImmediate(_config);
        }

        private void EnterAndRunFirstTick()
        {
            _presenter.Enter();
            _scheduler.AdvanceBy(TimeSpan.Zero);
        }

        private static WeatherForecast Forecast(string period, int temperature) =>
            new WeatherForecast(period, temperature, "F", "https://example.com/icon.png");

        [Test]
        public void Enter_RequestsImmediately_AndShowsLoader()
        {
            EnterAndRunFirstTick();

            Assert.IsTrue(_view.IsVisible);
            Assert.AreEqual(1, _service.Requests.Count);
            Assert.IsTrue(_view.IsLoading);
        }

        [Test]
        public void Polling_KeepsFixedRate_RegardlessOfResponseTime()
        {
            EnterAndRunFirstTick();

            // Первый ответ пришёл через 3 секунды — следующий запрос всё равно в момент interval, а не 3 + interval.
            _scheduler.AdvanceBySeconds(3);
            _service.Requests[0].Source.TrySetResult(Forecast("Today", 70));
            Assert.AreEqual(1, _service.Requests.Count);

            _scheduler.AdvanceBySeconds(Interval - 3);
            Assert.AreEqual(2, _service.Requests.Count);

            _service.Requests[1].Source.TrySetResult(Forecast("Today", 71));
            _scheduler.AdvanceBySeconds(Interval);
            Assert.AreEqual(3, _service.Requests.Count);
        }

        [Test]
        public void Forecast_IsShownWithTranslatedPeriodName()
        {
            EnterAndRunFirstTick();

            _service.Requests[0].Source.TrySetResult(Forecast("Tonight", 58));

            Assert.AreEqual("Сегодня ночью - 58F", _view.ForecastText);
            Assert.IsFalse(_view.IsLoading);
        }

        [Test]
        public void Error_ShowsMessage_AndHidesLoader()
        {
            EnterAndRunFirstTick();

            _service.Requests[0].Source.TrySetException(new Exception("network down"));

            Assert.AreEqual(_config.ErrorMessage, _view.StatusText);
            Assert.IsTrue(_view.IsStatusError);
            Assert.IsFalse(_view.IsLoading);
        }

        [Test]
        public void StaleResponse_DoesNotOverwriteNewerForecast()
        {
            EnterAndRunFirstTick();
            _scheduler.AdvanceBySeconds(Interval);

            _service.Requests[1].Source.TrySetResult(Forecast("Today", 70));
            _service.Requests[0].Source.TrySetResult(Forecast("Tonight", 50));

            Assert.AreEqual("Сегодня - 70F", _view.ForecastText);
        }

        [Test]
        public void SlowNetwork_SkipsTicks_WhenTwoRequestsAreInFlight()
        {
            EnterAndRunFirstTick();

            _scheduler.AdvanceBySeconds(Interval);
            _scheduler.AdvanceBySeconds(Interval);
            _scheduler.AdvanceBySeconds(Interval);

            Assert.AreEqual(2, _service.Requests.Count);
        }

        [Test]
        public void Exit_CancelsRequests_AndStopsPolling()
        {
            EnterAndRunFirstTick();

            _presenter.Exit();
            _scheduler.AdvanceBySeconds(Interval * 3);

            Assert.IsFalse(_view.IsVisible);
            Assert.IsTrue(_service.Requests[0].Token.IsCancellationRequested);
            Assert.AreEqual(1, _service.Requests.Count);
        }

        private string ReceivedAt(DateTimeOffset time, string format) => string.Format(format, time.ToLocalTime());

        [Test]
        public void Forecast_ShowsLastUpdateTime()
        {
            EnterAndRunFirstTick();
            DateTimeOffset receivedAt = _scheduler.Now;

            _service.Requests[0].Source.TrySetResult(Forecast("Today", 70));

            Assert.AreEqual(ReceivedAt(receivedAt, _config.UpdatedStatusFormat), _view.StatusText);
            Assert.IsFalse(_view.IsStatusError);
        }

        [Test]
        public void RepeatPoll_ShowsUpdatingStatus_InsteadOfBigLoader()
        {
            EnterAndRunFirstTick();
            _service.Requests[0].Source.TrySetResult(Forecast("Today", 70));

            _scheduler.AdvanceBySeconds(Interval);

            Assert.AreEqual(2, _service.Requests.Count);
            Assert.AreEqual(_config.UpdatingStatus, _view.StatusText);
            Assert.IsFalse(_view.IsLoading, "Крупный индикатор не должен закрывать уже показанный прогноз");
            Assert.AreEqual("Сегодня - 70F", _view.ForecastText);
        }

        [Test]
        public void RepeatPollError_KeepsForecast_MarksItStale_AndExplains()
        {
            EnterAndRunFirstTick();
            DateTimeOffset receivedAt = _scheduler.Now;
            _service.Requests[0].Source.TrySetResult(Forecast("Today", 70));

            _scheduler.AdvanceBySeconds(Interval);
            _service.Requests[1].Source.TrySetException(new Exception("network down"));

            Assert.AreEqual("Сегодня - 70F", _view.ForecastText);
            Assert.IsTrue(_view.IsStale);
            Assert.IsTrue(_view.IsStatusError);
            Assert.AreEqual(ReceivedAt(receivedAt, _config.StaleStatusFormat), _view.StatusText);
        }

        [Test]
        public void SuccessAfterError_ClearsStaleMark()
        {
            EnterAndRunFirstTick();
            _service.Requests[0].Source.TrySetResult(Forecast("Today", 70));
            _scheduler.AdvanceBySeconds(Interval);
            _service.Requests[1].Source.TrySetException(new Exception("network down"));

            _scheduler.AdvanceBySeconds(Interval);
            DateTimeOffset receivedAt = _scheduler.Now;
            _service.Requests[2].Source.TrySetResult(Forecast("Today", 72));

            Assert.IsFalse(_view.IsStale);
            Assert.IsFalse(_view.IsStatusError);
            Assert.AreEqual("Сегодня - 72F", _view.ForecastText);
            Assert.AreEqual(ReceivedAt(receivedAt, _config.UpdatedStatusFormat), _view.StatusText);
        }
    }
}
