using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// «+1 и монетка»: появляется в точке нажатия, подпрыгивает и улетает в счётчик валюты.
    /// </summary>
    public sealed class RewardFlyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _amount;

        [Header("Анимация")]
        [SerializeField, Min(0.05f)] private float _popDuration = 0.15f;
        [Tooltip("На сколько пикселей (в единицах Canvas) строка подпрыгивает перед полётом")]
        [SerializeField] private float _riseDistance = 110f;
        [SerializeField, Min(0.05f)] private float _riseDuration = 0.35f;
        [SerializeField, Min(0.05f)] private float _flyDuration = 0.45f;
        [SerializeField, Range(0.1f, 1f)] private float _endScale = 0.45f;

        private Sequence _sequence;

        /// <param name="from">Мировая позиция старта (для Overlay Canvas — пиксели экрана).</param>
        /// <param name="target">Куда лететь — иконка счётчика валюты.</param>
        /// <param name="onArrived">Вызывается по прилёте (и для возврата в пул).</param>
        public void Play(string amountText, Vector3 from, Transform target, Action onArrived)
        {
            Kill();

            _amount.text = amountText;

            Transform self = transform;
            self.position = from;
            self.localScale = Vector3.zero;

            float canvasScale = self.parent != null ? self.parent.lossyScale.y : 1f;
            float riseTo = from.y + _riseDistance * canvasScale;

            _sequence = DOTween.Sequence()
                .Append(self.DOScale(1f, _popDuration).SetEase(Ease.OutBack))
                .Join(self.DOMoveY(riseTo, _riseDuration).SetEase(Ease.OutCubic))
                .Append(self.DOMove(target.position, _flyDuration).SetEase(Ease.InQuad))
                .Join(self.DOScale(_endScale, _flyDuration).SetEase(Ease.InQuad))
                .OnComplete(() => onArrived?.Invoke())
                .SetLink(gameObject);
        }

        private void Kill()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        /// <summary>Пул наград. MonoMemoryPool сам включает объект при выдаче и выключает при возврате.</summary>
        public sealed class Pool : MonoMemoryPool<RewardFlyView>
        {
            protected override void OnSpawned(RewardFlyView item)
            {
                base.OnSpawned(item);
                item.transform.SetAsLastSibling(); // новая награда рисуется поверх предыдущих
            }

            protected override void OnDespawned(RewardFlyView item)
            {
                item.Kill();
                base.OnDespawned(item);
            }
        }
    }
}
