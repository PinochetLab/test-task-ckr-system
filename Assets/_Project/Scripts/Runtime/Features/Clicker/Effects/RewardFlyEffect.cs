using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// Слой «вылетающих наград» на экране кликера. Берёт строки «+N» из Zenject-пула
    /// (<see cref="RewardFlyView.Pool"/>, создаётся в ClickerInstaller под этим слоем)
    /// и «качает» иконку счётчика, когда награда в неё прилетает.
    /// Лежит внутри экрана кликера, поэтому на других вкладках не виден.
    /// </summary>
    public sealed class RewardFlyEffect : MonoBehaviour
    {
        [Tooltip("Иконка счётчика валюты, в которую летят награды")]
        [SerializeField] private RectTransform _target;
        [Tooltip("Насколько выше курсора появляется награда (в единицах Canvas)")]
        [SerializeField] private float _spawnOffsetY = 15f;
        [SerializeField] private float _targetPunch = 0.25f;
        [SerializeField, Min(0.05f)] private float _targetPunchDuration = 0.2f;

        private RewardFlyView.Pool _pool;
        private Tween _targetTween;
        private Vector3 _targetScale;
        private bool _targetScaleCached;

        [Inject]
        public void Construct(RewardFlyView.Pool pool)
        {
            _pool = pool;
        }

        /// <param name="from">Мировая позиция появления (для Overlay Canvas — пиксели экрана).</param>
        /// <param name="aboveFrom">true — чуть выше точки (над курсором), false — ровно в ней.</param>
        public void Play(string amountText, Vector3 from, bool aboveFrom)
        {
            RewardFlyView item = _pool.Spawn();

            if (aboveFrom)
                from.y += _spawnOffsetY * transform.lossyScale.y;
            from.z = 0f;

            item.Play(amountText, from, _target, () =>
            {
                PunchTarget();
                _pool.Despawn(item);
            });
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
