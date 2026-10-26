namespace TestTask.Features.Weather.Data
{
    /// <summary>Прогноз на текущий период (доменная модель).</summary>
    public sealed class WeatherForecast
    {
        public WeatherForecast(string periodName, int temperature, string unit, string iconUrl)
        {
            PeriodName = periodName;
            Temperature = temperature;
            Unit = unit;
            IconUrl = iconUrl;
        }

        /// <summary>Название периода из ответа API: «Today», «This Afternoon», «Tonight» и т.п.</summary>
        public string PeriodName { get; }

        public int Temperature { get; }
        public string Unit { get; }
        public string IconUrl { get; }
    }
}
