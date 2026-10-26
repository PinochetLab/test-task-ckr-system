using DG.Tweening;
using UnityEngine;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// Анимации кнопки: «пульс» при клике и тряска, когда не хватает энергии.
    /// </summary>
    public sealed class ButtonPunchAnimator : MonoBehaviour
    {
        [SerializeField] private float _punchStrength = -0.12f;
        [SerializeField, Min(0.05f)] private float _punchDuration = 0.2f;
        [SerializeField] private float _shakeStrength = 20f;
        [SerializeField, Min(0.05f)] private float _shakeDuration = 0.3f;

        private Tween _tween;
        private Vector3 _initialScale;
        private Vector3 _initialPosition;

        private void Awake()
        {
            _initialScale = transform.localScale;
            _initialPosition = transform.localPosition;
        }

        public void Punch()
        {
            ResetState();
            _tween = transform
                .DOPunchScale(Vector3.one * _punchStrength, _punchDuration, 8, 0.5f)
                .SetLink(gameObject);
        }

        public void Shake()
        {
            ResetState();
            float scale = transform.parent != null ? transform.parent.lossyScale.x : 1f;
            _tween = transform
                .DOShakePosition(_shakeDuration, new Vector3(_shakeStrength * scale, 0f, 0f), 20, 0f)
                .SetLink(gameObject);
        }

        private void OnDisable()
        {
            ResetState();
        }

        private void ResetState()
        {
            _tween?.Kill();
            _tween = null;
            transform.localScale = _initialScale;
            transform.localPosition = _initialPosition;
        }
    }
}
