using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// Квадратик валюты, который летит от кнопки к счётчику.
    /// </summary>
    public sealed class FlyingCurrencyView : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float _duration = 0.6f;
        [SerializeField] private float _jumpPower = 150f;

        private Sequence _sequence;

        /// <param name="from">Мировая позиция старта.</param>
        /// <param name="to">Мировая позиция цели.</param>
        /// <param name="delay">Задержка старта.</param>
        /// <param name="onComplete">Вызывается по завершении (в т.ч. для возврата в пул).</param>
        public void Fly(Vector3 from, Vector3 to, float delay, Action onComplete)
        {
            Kill();

            Transform self = transform;
            self.position = from;
            self.localScale = Vector3.zero;

            float scale = self.parent != null ? self.parent.lossyScale.x : 1f;

            _sequence = DOTween.Sequence()
                .AppendInterval(delay)
                .Append(self.DOScale(1f, 0.15f).SetEase(Ease.OutBack))
                .Append(self.DOJump(to, _jumpPower * scale, 1, _duration).SetEase(Ease.InQuad))
                .Join(self.DOScale(0.5f, _duration).SetEase(Ease.InQuad))
                .OnComplete(() => onComplete?.Invoke())
                .SetLink(gameObject);
        }

        // Анимацию на OnDisable не убиваем: слой эффектов лежит внутри экрана кликера и выключается
        // вместе с вкладкой. Твин дорабатывает в фоне и сам возвращает объект в пул; при возврате
        // на вкладку не остаётся «зависших» эффектов. Уничтожение объекта гасит твин через SetLink.

        private void Kill()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        public sealed class Pool : MonoMemoryPool<FlyingCurrencyView>
        {
        }
    }
}
