namespace TestTask.Features.Weather.Data
{
    /// <summary>Прогноз на текущий период (доменная модель).</summary>
    public sealed class WeatherForecast
    {
        public WeatherForecast(int temperature, string unit, string iconUrl)
        {
            Temperature = temperature;
            Unit = unit;
            IconUrl = iconUrl;
        }

        public int Temperature { get; }
        public string Unit { get; }
        public string IconUrl { get; }
    }
}
