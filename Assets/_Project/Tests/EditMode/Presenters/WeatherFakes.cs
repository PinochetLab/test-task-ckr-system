using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Core.Content;
using TestTask.Features.Weather.Data;
using TestTask.Features.Weather.Service;
using TestTask.Features.Weather.View;
using UnityEngine;

namespace TestTask.Tests
{
    internal sealed class FakeWeatherView : IWeatherView
    {
        public bool IsVisible { get; private set; } = true;
        public bool IsLoading { get; private set; }
        public string ForecastText { get; private set; }
        public string ErrorText { get; private set; }
        public int ResetCount { get; private set; }

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;

        public void ResetState()
        {
            ResetCount++;
            IsLoading = false;
            ForecastText = null;
            ErrorText = null;
        }

        public void SetLoading(bool loading) => IsLoading = loading;

        public void SetForecast(Sprite icon, string text)
        {
            ForecastText = text;
            ErrorText = null;
        }

        public void SetError(string message) => ErrorText = message;
    }

    /// <summary>Каждый запрос висит, пока тест сам не завершит его через Source.</summary>
    internal sealed class FakeWeatherService : IWeatherService
    {
        public sealed class Request
        {
            public UniTaskCompletionSource<WeatherForecast> Source;
            public CancellationToken Token;
        }

        public List<Request> Requests { get; } = new List<Request>();

        public UniTask<WeatherForecast> GetCurrentAsync(CancellationToken cancellationToken)
        {
            var source = new UniTaskCompletionSource<WeatherForecast>();
            cancellationToken.Register(() => source.TrySetCanceled(cancellationToken));
            Requests.Add(new Request { Source = source, Token = cancellationToken });
            return source.Task;
        }
    }

    internal sealed class FakeSpriteLoader : ISpriteLoader
    {
        public UniTask<Sprite> LoadAsync(string url, CancellationToken cancellationToken)
        {
            return UniTask.FromResult<Sprite>(null);
        }
    }
}
