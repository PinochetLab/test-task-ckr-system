using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Core.Content;
using TestTask.Core.Navigation;
using TestTask.Features.Weather.Config;
using TestTask.Features.Weather.Data;
using TestTask.Features.Weather.Service;
using TestTask.Features.Weather.View;
using UniRx;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Weather.Presenter
{
    /// <summary>
    /// Пока вкладка открыта, каждые N секунд ставит в очередь запрос прогноза.
    /// Таймер идёт с фиксированным шагом и не ждёт завершения предыдущего запроса и загрузки иконки,
    /// поэтому период не «плывёт» от времени ответа сервера.
    /// При уходе с вкладки активный запрос отменяется, ожидающие — удаляются из очереди.
    /// </summary>
    public sealed class WeatherPresenter : ITabPresenter, IInitializable, IDisposable
    {
        private const string ErrorMessage = "Не удалось загрузить погоду";

        private readonly WeatherView _view;
        private readonly IWeatherService _service;
        private readonly ISpriteLoader _spriteLoader;
        private readonly WeatherConfig _config;

        private CancellationTokenSource _cts;
        private IDisposable _timer;

        // Ответы могут прийти «внахлёст», показываем только самый свежий.
        private int _lastRequestId;
        private int _lastAppliedId;
        private int _requestsInFlight;
        private bool _hasData;

        public WeatherPresenter(WeatherView view, IWeatherService service, ISpriteLoader spriteLoader, WeatherConfig config)
        {
            _view = view;
            _service = service;
            _spriteLoader = spriteLoader;
            _config = config;
        }

        public TabId Id => TabId.Weather;

        public void Initialize()
        {
            _view.ResetState();
            _view.Hide();
        }

        public void Enter()
        {
            CancelPolling();

            _view.ResetState();
            _view.Show();

            _cts = new CancellationTokenSource();
            _lastRequestId = 0;
            _lastAppliedId = 0;
            _requestsInFlight = 0;
            _hasData = false;

            CancellationToken token = _cts.Token;
            var interval = TimeSpan.FromSeconds(_config.PollIntervalSeconds);

            // Первый запрос сразу, дальше строго раз в interval. Время игры (timeScale) не влияет.
            _timer = Observable.Timer(TimeSpan.Zero, interval, Scheduler.MainThreadIgnoreTimeScale)
                .Subscribe(_ => RequestForecastAsync(token).Forget());
        }

        public void Exit()
        {
            CancelPolling();
            _view.Hide();
        }

        public void Dispose()
        {
            CancelPolling();
        }

        private async UniTaskVoid RequestForecastAsync(CancellationToken cancellationToken)
        {
            int requestId = ++_lastRequestId;
            _requestsInFlight++;
            UpdateLoading();

            try
            {
                WeatherForecast forecast = await _service.GetTodayAsync(cancellationToken);
                Sprite icon = await LoadIconAsync(forecast.IconUrl, cancellationToken);

                // Более свежий запрос уже успел показать результат.
                if (requestId < _lastAppliedId)
                    return;

                _lastAppliedId = requestId;
                _view.SetForecast(icon, string.Format(_config.DisplayFormat, forecast.Temperature, forecast.Unit));
                _hasData = true;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Weather] {exception.Message}");

                if (requestId > _lastAppliedId)
                    _view.SetError(ErrorMessage);
            }
            finally
            {
                // После отмены счётчики уже сброшены новым заходом на вкладку — не трогаем их.
                if (!cancellationToken.IsCancellationRequested)
                {
                    _requestsInFlight--;
                    UpdateLoading();
                }
            }
        }

        /// <summary>Индикатор нужен, только пока на экране ещё нет ни одного прогноза.</summary>
        private void UpdateLoading()
        {
            _view.SetLoading(!_hasData && _requestsInFlight > 0);
        }

        private async UniTask<Sprite> LoadIconAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                return await _spriteLoader.LoadAsync(url, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                // Без иконки показываем хотя бы температуру.
                Debug.LogWarning($"[Weather] Icon load failed: {exception.Message}");
                return null;
            }
        }

        private void CancelPolling()
        {
            _timer?.Dispose();
            _timer = null;

            if (_cts == null)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}
