using TestTask.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TestTask.Features.Weather.View
{
    public sealed class WeatherView : TabScreenView, IWeatherView
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _forecastText;
        [FormerlySerializedAs("_errorText")]
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private LoadingIndicatorView _loader;

        [SerializeField] private Color _statusColor = new Color(0.65f, 0.65f, 0.7f);
        [SerializeField] private Color _errorColor = new Color(0.95f, 0.4f, 0.4f);

        [Tooltip("Прозрачность устаревшего прогноза")]
        [SerializeField, Range(0f, 1f)] private float _staleAlpha = 0.45f;

        public void ResetState()
        {
            _icon.enabled = false;
            _icon.sprite = null;
            _forecastText.text = string.Empty;
            SetForecastStale(false);
            SetStatus(string.Empty, false);
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
            SetForecastStale(false);
        }

        public void SetForecastStale(bool stale)
        {
            float alpha = stale ? _staleAlpha : 1f;

            Color iconColor = _icon.color;
            iconColor.a = alpha;
            _icon.color = iconColor;

            _forecastText.alpha = alpha;
        }

        public void SetStatus(string text, bool isError)
        {
            _statusText.text = text;
            _statusText.color = isError ? _errorColor : _statusColor;
        }
    }
}
