using TestTask.Core.UI;
using UnityEngine;

namespace TestTask.Features.Weather.View
{
    public interface IWeatherView : IScreenView
    {
        void ResetState();

        void SetLoading(bool loading);

        void SetForecast(Sprite icon, string text);

        void SetError(string message);
    }
}
