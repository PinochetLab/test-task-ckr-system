using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Features.Weather.Data;

namespace TestTask.Features.Weather.Service
{
    public interface IWeatherService
    {
        /// <summary>Прогноз на ближайший период (первый в ответе API).</summary>
        UniTask<WeatherForecast> GetCurrentAsync(CancellationToken cancellationToken);
    }
}
