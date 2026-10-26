using UnityEngine;

namespace TestTask.Features.Weather.Config
{
    [CreateAssetMenu(menuName = "TestTask/Configs/Weather", fileName = "WeatherConfig")]
    public sealed class WeatherConfig : ScriptableObject
    {
        [SerializeField] private string _forecastUrl = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";
        [SerializeField, Min(0.5f)] private float _pollIntervalSeconds = 5f;

        [Tooltip("{0} — название периода из ответа API, переведённое на русский (Сегодня, Сегодня ночью, ...), {1} — температура, {2} — единица измерения")]
        [SerializeField] private string _displayFormat = "{0} - {1}{2}";

        public string ForecastUrl => _forecastUrl;
        public float PollIntervalSeconds => _pollIntervalSeconds;
        [SerializeField] private string _errorMessage = "Не удалось загрузить погоду";

        public string DisplayFormat => _displayFormat;
        public string ErrorMessage => _errorMessage;
    }
}
