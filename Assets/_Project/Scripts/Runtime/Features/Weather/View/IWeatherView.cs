using TestTask.Core.UI;
using UnityEngine;

namespace TestTask.Features.Weather.View
{
    public interface IWeatherView : IScreenView
    {
        void ResetState();

        /// <summary>Крупный индикатор загрузки — пока на экране ещё нет ни прогноза, ни ошибки.</summary>
        void SetLoading(bool loading);

        void SetForecast(Sprite icon, string text);

        /// <summary>Прогноз на экране устарел: последний опрос не удался.</summary>
        void SetForecastStale(bool stale);

        /// <summary>Строка статуса под прогнозом: идёт обновление, время обновления или ошибка.</summary>
        void SetStatus(string text, bool isError);
    }
}
