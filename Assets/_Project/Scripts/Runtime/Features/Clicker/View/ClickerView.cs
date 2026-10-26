using System;
using TestTask.Core.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.Features.Clicker.View
{
    /// <summary>
    /// Экран кликера. Сам кликабельный объект живёт на сцене под UI (<see cref="ClickTargetView"/>),
    /// здесь только счётчики; объект показывается и прячется вместе с вкладкой.
    /// </summary>
    public sealed class ClickerView : TabScreenView, IClickerView
    {
        [SerializeField] private ClickTargetView _clickTarget;
        [SerializeField] private Text _currencyText;
        [SerializeField] private Text _energyText;
        [SerializeField] private RectTransform _energyFill;
        [SerializeField] private Image _energyFillImage;
        [SerializeField] private Color _energyColor = new Color(0.25f, 0.55f, 0.95f);
        [SerializeField] private Color _energyEmptyColor = new Color(0.85f, 0.3f, 0.3f);

        public IObservable<Unit> Clicked => _clickTarget.Clicked;

        public override void Show()
        {
            base.Show();
            _clickTarget.SetVisible(true);
        }

        public override void Hide()
        {
            base.Hide();
            _clickTarget.SetVisible(false);
        }

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

        public void PlayClickFeedback(int particleCount)
        {
            _clickTarget.Punch();

            ParticleSystem particles = _clickTarget.Burst;
            if (particles != null && particleCount > 0)
                particles.Emit(particleCount);
        }

        public void PlayDenied()
        {
            _clickTarget.Shake();
        }
    }
}
