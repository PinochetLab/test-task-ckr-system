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

        // Один запрос выполняется и один ждёт в очереди. Если сеть отвечает дольше интервала
        // (например, без интернета запрос висит до таймаута), новые тики пропускаются,
        // чтобы очередь не копила одинаковые запросы.
        private const int MaxRequestsInFlight = 2;

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
        private bool _hasResult;

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
            _hasResult = false;

            CancellationToken token = _cts.Token;
            var interval = TimeSpan.FromSeconds(_config.PollIntervalSeconds);

            // Первый запрос сразу, дальше строго раз в interval. Время игры (timeScale) не влияет.
            _timer = Observable.Timer(TimeSpan.Zero, interval, Scheduler.MainThreadIgnoreTimeScale)
                .Subscribe(_ => OnTick(token));
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

        private void OnTick(CancellationToken cancellationToken)
        {
            if (_requestsInFlight >= MaxRequestsInFlight)
                return;

            RequestForecastAsync(cancellationToken).Forget();
        }

        private async UniTaskVoid RequestForecastAsync(CancellationToken cancellationToken)
        {
            int requestId = ++_lastRequestId;
            _requestsInFlight++;
            UpdateLoading();

            try
            {
                WeatherForecast forecast = await _service.GetCurrentAsync(cancellationToken);
                Sprite icon = await LoadIconAsync(forecast.IconUrl, cancellationToken);

                // Более свежий запрос уже успел показать результат.
                if (requestId < _lastAppliedId)
                    return;

                _lastAppliedId = requestId;
                _view.SetForecast(icon, FormatForecast(forecast));
                _hasResult = true;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Weather] {exception.Message}");

                if (requestId > _lastAppliedId)
                {
                    _view.SetError(ErrorMessage);
                    _hasResult = true;
                }
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

        private string FormatForecast(WeatherForecast forecast)
        {
            // Если API не прислал название периода, показываем хотя бы температуру.
            if (string.IsNullOrWhiteSpace(forecast.PeriodName))
                return $"{forecast.Temperature}{forecast.Unit}";

            string periodName = WeatherPeriodNameTranslator.Translate(forecast.PeriodName);
            return string.Format(_config.DisplayFormat, periodName, forecast.Temperature, forecast.Unit);
        }

        /// <summary>
        /// Индикатор нужен только до первого результата — прогноза или ошибки.
        /// Иначе при запросах «внахлёст» он не гас бы никогда.
        /// </summary>
        private void UpdateLoading()
        {
            _view.SetLoading(!_hasResult && _requestsInFlight > 0);
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
