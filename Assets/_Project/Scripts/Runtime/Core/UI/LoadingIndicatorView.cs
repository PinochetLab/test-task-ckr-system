using DG.Tweening;
using UnityEngine;

namespace TestTask.Core.UI
{
    /// <summary>
    /// Индикатор загрузки: вращающийся квадрат.
    /// </summary>
    public sealed class LoadingIndicatorView : MonoBehaviour
    {
        [SerializeField] private RectTransform _spinner;
        [SerializeField, Min(0.1f)] private float _turnDuration = 0.8f;

        private Tween _tween;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void OnEnable()
        {
            _spinner.localRotation = Quaternion.identity;
            _tween = _spinner
                .DOLocalRotate(new Vector3(0f, 0f, -360f), _turnDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        private void OnDisable()
        {
            _tween?.Kill();
            _tween = null;
        }
    }
}
