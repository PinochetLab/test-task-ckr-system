using System;
using TestTask.Core.UI;
using TestTask.Features.Clicker.Effects;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.Features.Clicker.View
{
    public sealed class ClickerView : TabScreenView
    {
        [SerializeField] private Button _clickButton;
        [SerializeField] private ButtonPunchAnimator _buttonAnimator;
        [SerializeField] private Text _currencyText;
        [SerializeField] private RectTransform _currencyIcon;
        [SerializeField] private Text _energyText;
        [SerializeField] private RectTransform _energyFill;
        [SerializeField] private Image _energyFillImage;
        [SerializeField] private Color _energyColor = new Color(0.25f, 0.55f, 0.95f);
        [SerializeField] private Color _energyEmptyColor = new Color(0.85f, 0.3f, 0.3f);

        public IObservable<Unit> Clicked => _clickButton.OnClickAsObservable();

        /// <summary>Откуда вылетают эффекты клика (мировая позиция).</summary>
        public Vector3 ClickOrigin => _clickButton.transform.position;

        /// <summary>Куда летит валюта (мировая позиция).</summary>
        public Vector3 CurrencyTarget => _currencyIcon.position;

        public void SetCurrency(long value)
        {
            _currencyText.text = $"Валюта: {value}";
        }

        public void SetEnergy(int current, int max)
        {
            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

            _energyText.text = $"Энергия: {current} / {max}";
            _energyFill.anchorMax = new Vector2(ratio, 1f);
            _energyFillImage.color = ratio > 0.1f ? _energyColor : _energyEmptyColor;
        }

        public void PlayClickFeedback()
        {
            _buttonAnimator.Punch();
        }

        public void PlayDenied()
        {
            _buttonAnimator.Shake();
        }
    }
}
