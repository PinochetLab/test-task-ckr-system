using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Features.Weather.Data;

namespace TestTask.Features.Weather.Service
{
    public interface IWeatherService
    {
        UniTask<WeatherForecast> GetTodayAsync(CancellationToken cancellationToken);
    }
}
