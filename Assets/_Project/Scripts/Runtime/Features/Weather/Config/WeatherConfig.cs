using UnityEngine;

namespace TestTask.Features.Weather.Config
{
    [CreateAssetMenu(menuName = "TestTask/Configs/Weather", fileName = "WeatherConfig")]
    public sealed class WeatherConfig : ScriptableObject
    {
        [SerializeField] private string _forecastUrl = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";
        [SerializeField, Min(0.5f)] private float _pollIntervalSeconds = 5f;

        [Header("Тексты")]
        [Tooltip("{0} — название периода из ответа API, переведённое на русский (Сегодня, Сегодня ночью, ...), {1} — температура, {2} — единица измерения")]
        [SerializeField] private string _displayFormat = "{0} - {1}{2}";

        [Tooltip("Ошибка, когда прогноза на экране ещё нет")]
        [SerializeField] private string _errorMessage = "Не удалось загрузить погоду";

        [Tooltip("Статус во время повторного опроса, пока на экране старый прогноз")]
        [SerializeField] private string _updatingStatus = "Обновление…";

        [Tooltip("{0} — время последнего успешного обновления")]
        [SerializeField] private string _updatedStatusFormat = "Обновлено в {0:HH:mm:ss}";

        [Tooltip("Повторный опрос не удался, на экране старый прогноз. {0} — время, когда он был получен")]
        [SerializeField] private string _staleStatusFormat = "Не удалось обновить. Показан прогноз от {0:HH:mm:ss}";

        public string ForecastUrl => _forecastUrl;
        public float PollIntervalSeconds => _pollIntervalSeconds;
        public string DisplayFormat => _displayFormat;
        public string ErrorMessage => _errorMessage;
        public string UpdatingStatus => _updatingStatus;
        public string UpdatedStatusFormat => _updatedStatusFormat;
        public string StaleStatusFormat => _staleStatusFormat;
    }
}
