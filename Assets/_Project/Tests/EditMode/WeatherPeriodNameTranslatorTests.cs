using NUnit.Framework;
using TestTask.Features.Weather.Presenter;

namespace TestTask.Tests
{
    public sealed class WeatherPeriodNameTranslatorTests
    {
        [TestCase("Today", "Сегодня")]
        [TestCase("This Afternoon", "Сегодня днём")]
        [TestCase("Tonight", "Сегодня ночью")]
        [TestCase("tonight", "Сегодня ночью")]
        [TestCase("  Overnight ", "Ночью")]
        [TestCase("Monday", "Понедельник")]
        [TestCase("Monday Night", "Понедельник, ночь")]
        public void Translate_KnownName_ReturnsRussian(string source, string expected)
        {
            Assert.AreEqual(expected, WeatherPeriodNameTranslator.Translate(source));
        }

        [Test]
        public void Translate_UnknownName_ReturnsOriginal()
        {
            Assert.AreEqual("Thanksgiving Day", WeatherPeriodNameTranslator.Translate("Thanksgiving Day"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Translate_Empty_ReturnsAsIs(string source)
        {
            Assert.AreEqual(source, WeatherPeriodNameTranslator.Translate(source));
        }
    }
}
