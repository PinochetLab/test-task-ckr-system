using UnityEngine;

namespace TestTask.Features.Weather.Config
{
    [CreateAssetMenu(menuName = "TestTask/Configs/Weather", fileName = "WeatherConfig")]
    public sealed class WeatherConfig : ScriptableObject
    {
        [SerializeField] private string _forecastUrl = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";
        [SerializeField, Min(0.5f)] private float _pollIntervalSeconds = 5f;

        [Tooltip("{0} — температура, {1} — единица измерения")]
        [SerializeField] private string _displayFormat = "Сегодня - {0}{1}";

        public string ForecastUrl => _forecastUrl;
        public float PollIntervalSeconds => _pollIntervalSeconds;
        public string DisplayFormat => _displayFormat;
    }
}
