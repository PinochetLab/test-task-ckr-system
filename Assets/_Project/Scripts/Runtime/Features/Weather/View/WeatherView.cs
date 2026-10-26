using TestTask.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.Features.Weather.View
{
    public sealed class WeatherView : TabScreenView, IWeatherView
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _forecastText;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private LoadingIndicatorView _loader;

        public void ResetState()
        {
            _icon.enabled = false;
            _icon.sprite = null;
            _forecastText.text = string.Empty;
            _errorText.text = string.Empty;
            _loader.SetVisible(false);
        }

        public void SetLoading(bool loading)
        {
            _loader.SetVisible(loading);
        }

        public void SetForecast(Sprite icon, string text)
        {
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _forecastText.text = text;
            _errorText.text = string.Empty;
        }

        public void SetError(string message)
        {
            _errorText.text = message;
        }
    }
}
