using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// Одна «частица» UI: маленький квадрат, разлетающийся от точки клика.
    /// Сделана на UI, чтобы рисоваться поверх Canvas в режиме Screen Space Overlay.
    /// </summary>
    public sealed class UiParticleView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField, Min(0.05f)] private float _duration = 0.45f;

        private Sequence _sequence;

        public void Burst(Vector3 origin, Vector2 direction, float distance, Color color, Action onComplete)
        {
            Kill();

            Transform self = transform;
            self.position = origin;
            self.localScale = Vector3.one;
            self.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 90f));
            _image.color = color;

            float scale = self.parent != null ? self.parent.lossyScale.x : 1f;
            Vector3 target = origin + (Vector3)(direction.normalized * distance * scale);
            Color transparent = new Color(color.r, color.g, color.b, 0f);

            _sequence = DOTween.Sequence()
                .Append(self.DOMove(target, _duration).SetEase(Ease.OutCubic))
                .Join(self.DOScale(0.2f, _duration).SetEase(Ease.InQuad))
                .Join(self.DOLocalRotate(new Vector3(0f, 0f, 180f), _duration, RotateMode.LocalAxisAdd))
                .Join(DOTween.To(() => _image.color, value => _image.color = value, transparent, _duration))
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

        public sealed class Pool : MonoMemoryPool<UiParticleView>
        {
        }
    }
}
