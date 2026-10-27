using System;
using TestTask.Features.Clicker.Effects;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestTask.Features.Clicker.View
{
    /// <summary>
    /// Кликабельный 2D-объект на сцене (не UI) с системой частиц разлёта.
    /// Клик приходит через EventSystem и Physics2DRaycaster на камере,
    /// поэтому UI, лежащий поверх, по-прежнему перехватывает нажатия.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ClickTargetView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ButtonPunchAnimator _animator;
        [SerializeField] private ParticleSystem _burst;

        private readonly Subject<Unit> _clicked = new Subject<Unit>();

        public IObservable<Unit> Clicked => _clicked;

        public ParticleSystem Burst => _burst;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void Punch()
        {
            _animator.Punch();
        }

        public void Shake()
        {
            _animator.Shake();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _clicked.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _clicked.OnCompleted();
            _clicked.Dispose();
        }
    }
}
