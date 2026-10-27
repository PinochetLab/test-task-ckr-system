using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// Слой «вылетающих наград» на экране кликера. Держит небольшой пул строк «+N»
    /// и «качает» иконку счётчика, когда награда в неё прилетает.
    /// Лежит внутри экрана кликера, поэтому на других вкладках не виден.
    /// </summary>
    public sealed class RewardFlyEffect : MonoBehaviour
    {
        [SerializeField] private RewardFlyView _prefab;
        [Tooltip("Иконка счётчика валюты, в которую летят награды")]
        [SerializeField] private RectTransform _target;
        [Tooltip("Насколько выше курсора появляется награда (в единицах Canvas)")]
        [SerializeField] private float _spawnOffsetY = 15f;
        [SerializeField] private float _targetPunch = 0.25f;
        [SerializeField, Min(0.05f)] private float _targetPunchDuration = 0.2f;

        private readonly Stack<RewardFlyView> _free = new Stack<RewardFlyView>();
        private Tween _targetTween;
        private Vector3 _targetScale;
        private bool _targetScaleCached;

        /// <param name="from">Мировая позиция появления (для Overlay Canvas — пиксели экрана).</param>
        /// <param name="aboveFrom">true — чуть выше точки (над курсором), false — ровно в ней.</param>
        public void Play(string amountText, Vector3 from, bool aboveFrom)
        {
            RewardFlyView item = _free.Count > 0 ? _free.Pop() : Instantiate(_prefab, transform);
            item.gameObject.SetActive(true);
            item.transform.SetAsLastSibling();

            if (aboveFrom)
                from.y += _spawnOffsetY * transform.lossyScale.y;
            from.z = 0f;

            item.Play(amountText, from, _target, () =>
            {
                PunchTarget();
                Release(item);
            });
        }

        private void Release(RewardFlyView item)
        {
            if (item == null)
                return;

            item.gameObject.SetActive(false);
            _free.Push(item);
        }

        private void PunchTarget()
        {
            if (_target == null)
                return;

            if (!_targetScaleCached)
            {
                _targetScale = _target.localScale;
                _targetScaleCached = true;
            }

            _targetTween?.Kill();
            _target.localScale = _targetScale;
            _targetTween = _target
                .DOPunchScale(_targetScale * _targetPunch, _targetPunchDuration, 6, 0.5f)
                .SetLink(_target.gameObject);
        }
    }
}
