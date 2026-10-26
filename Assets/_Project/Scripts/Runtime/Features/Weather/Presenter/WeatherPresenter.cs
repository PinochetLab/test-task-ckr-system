using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Core.Content;
using TestTask.Core.Navigation;
using TestTask.Features.Weather.Config;
using TestTask.Features.Weather.Data;
using TestTask.Features.Weather.Service;
using TestTask.Features.Weather.View;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Weather.Presenter
{
    /// <summary>
    /// Пока вкладка открыта, раз в N секунд запрашивает прогноз.
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
            PollAsync(_cts.Token).Forget();
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

        private async UniTaskVoid PollAsync(CancellationToken cancellationToken)
        {
            bool hasData = false;
            var interval = TimeSpan.FromSeconds(_config.PollIntervalSeconds);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!hasData)
                    _view.SetLoading(true);

                try
                {
                    WeatherForecast forecast = await _service.GetTodayAsync(cancellationToken);
                    Sprite icon = await LoadIconAsync(forecast.IconUrl, cancellationToken);

                    _view.SetForecast(icon, string.Format(_config.DisplayFormat, forecast.Temperature, forecast.Unit));
                    hasData = true;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Weather] {exception.Message}");
                    _view.SetError(ErrorMessage);
                }
                finally
                {
                    if (!cancellationToken.IsCancellationRequested)
                        _view.SetLoading(false);
                }

                bool cancelled = await UniTask.Delay(interval, cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();

                if (cancelled)
                    return;
            }
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
            if (_cts == null)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}
