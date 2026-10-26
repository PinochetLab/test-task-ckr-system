using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Core.Network;
using TestTask.Features.Weather.Config;
using TestTask.Features.Weather.Data;

namespace TestTask.Features.Weather.Service
{
    public sealed class WeatherService : IWeatherService
    {
        private readonly IRequestQueue _queue;
        private readonly IHttpClient _http;
        private readonly IJsonParser _json;
        private readonly WeatherConfig _config;

        public WeatherService(IRequestQueue queue, IHttpClient http, IJsonParser json, WeatherConfig config)
        {
            _queue = queue;
            _http = http;
            _json = json;
            _config = config;
        }

        public async UniTask<WeatherForecast> GetTodayAsync(CancellationToken cancellationToken)
        {
            string url = _config.ForecastUrl;
            string body = await _queue.Enqueue(token => _http.GetTextAsync(url, token), cancellationToken);

            var response = _json.Parse<WeatherResponseDto>(body);
            ForecastPeriodDto[] periods = response?.Properties?.Periods;

            if (periods == null || periods.Length == 0)
                throw new FormatException("Weather response has no forecast periods");

            // Первый период — ближайший («Today» / «This Afternoon» / «Tonight»).
            ForecastPeriodDto today = periods[0];
            return new WeatherForecast(today.Temperature, today.TemperatureUnit, today.Icon);
        }
    }
}
