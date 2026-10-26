using System;
using System.Collections.Generic;

namespace TestTask.Features.Weather.Presenter
{
    /// <summary>
    /// Переводит названия периодов прогноза api.weather.gov («Today», «Tonight», «Monday Night» …) на русский.
    /// Незнакомые названия (например, праздники — «Thanksgiving Day») возвращаются как есть.
    /// </summary>
    public static class WeatherPeriodNameTranslator
    {
        private const string NightSuffix = " Night";

        private static readonly Dictionary<string, string> Phrases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Today"] = "Сегодня",
                ["This Morning"] = "Сегодня утром",
                ["This Afternoon"] = "Сегодня днём",
                ["Late Afternoon"] = "Ближе к вечеру",
                ["This Evening"] = "Сегодня вечером",
                ["Tonight"] = "Сегодня ночью",
                ["Overnight"] = "Ночью",
                ["Tomorrow"] = "Завтра",
                ["Tomorrow Night"] = "Завтра ночью"
            };

        private static readonly Dictionary<string, string> Days =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Monday"] = "Понедельник",
                ["Tuesday"] = "Вторник",
                ["Wednesday"] = "Среда",
                ["Thursday"] = "Четверг",
                ["Friday"] = "Пятница",
                ["Saturday"] = "Суббота",
                ["Sunday"] = "Воскресенье"
            };

        public static string Translate(string periodName)
        {
            if (string.IsNullOrWhiteSpace(periodName))
                return periodName;

            string name = periodName.Trim();

            if (Phrases.TryGetValue(name, out string phrase))
                return phrase;

            if (Days.TryGetValue(name, out string day))
                return day;

            // «Monday Night» → «Понедельник, ночь»
            if (name.EndsWith(NightSuffix, StringComparison.OrdinalIgnoreCase))
            {
                string dayPart = name.Substring(0, name.Length - NightSuffix.Length);
                if (Days.TryGetValue(dayPart, out string nightDay))
                    return $"{nightDay}, ночь";
            }

            return name;
        }
    }
}
