using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace TestTask.Features.Weather.Data
{
    // DTO повторяют формат ответа https://api.weather.gov/gridpoints/{office}/{x},{y}/forecast

    [Preserve]
    public sealed class WeatherResponseDto
    {
        [JsonProperty("properties")] public ForecastPropertiesDto Properties { get; set; }
    }

    [Preserve]
    public sealed class ForecastPropertiesDto
    {
        [JsonProperty("periods")] public ForecastPeriodDto[] Periods { get; set; }
    }

    [Preserve]
    public sealed class ForecastPeriodDto
    {
        [JsonProperty("number")] public int Number { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("temperature")] public int Temperature { get; set; }
        [JsonProperty("temperatureUnit")] public string TemperatureUnit { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("shortForecast")] public string ShortForecast { get; set; }
    }
}
