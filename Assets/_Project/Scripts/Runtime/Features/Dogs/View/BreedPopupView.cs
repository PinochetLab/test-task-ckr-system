using System;
using DG.Tweening;
using TestTask.Features.Dogs.Data;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.Features.Dogs.View
{
    /// <summary>Поп-ап с описанием породы.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BreedPopupView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backgroundButton;
        [SerializeField, Min(0.05f)] private float _animationDuration = 0.2f;

        private Sequence _sequence;

        public IObservable<Unit> CloseRequested =>
            _closeButton.OnClickAsObservable().Merge(_backgroundButton.OnClickAsObservable());

        public bool IsOpen => gameObject.activeSelf;

        public void Show(BreedDetails details)
        {
            _title.text = details.Name;
            _description.text = string.IsNullOrEmpty(details.Description) ? "Описание отсутствует" : details.Description;

            KillAnimation();
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;
            _panel.localScale = Vector3.one * 0.85f;

            _sequence = DOTween.Sequence()
                .Append(DOTween.To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value, 1f, _animationDuration))
                .Join(_panel.DOScale(1f, _animationDuration).SetEase(Ease.OutBack))
                .SetLink(gameObject);
        }

        public void Hide()
        {
            if (!IsOpen)
                return;

            KillAnimation();
            _sequence = DOTween.Sequence()
                .Append(DOTween.To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value, 0f, _animationDuration))
                .Join(_panel.DOScale(0.85f, _animationDuration).SetEase(Ease.InQuad))
                .OnComplete(() => gameObject.SetActive(false))
                .SetLink(gameObject);
        }

        public void HideImmediate()
        {
            KillAnimation();
            gameObject.SetActive(false);
        }

        private void KillAnimation()
        {
            _sequence?.Kill();
            _sequence = null;
        }
    }
}
