using System;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.Features.Dogs.View
{
    /// <summary>
    /// Поп-ап с описанием породы. Высота панели подстраивается под длину описания:
    /// короткий текст даёт компактную панель, длинный растягивает её до лимита,
    /// дальше описание прокручивается внутри панели.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BreedPopupView : MonoBehaviour, IBreedPopupView
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private ScrollRect _descriptionScroll;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backgroundButton;
        [SerializeField, Min(0.05f)] private float _animationDuration = 0.2f;

        [Header("Adaptive height")]
        [Tooltip("Минимальная высота панели, чтобы поп-ап с коротким текстом не выглядел обрубком.")]
        [SerializeField, Min(0f)] private float _minPanelHeight = 520f;

        [Tooltip("Максимальная высота панели как доля высоты области поп-апа.")]
        [SerializeField, Range(0.3f, 1f)] private float _maxPanelHeightRatio = 0.85f;

        private Sequence _sequence;

        public IObservable<Unit> CloseRequested =>
            _closeButton.OnClickAsObservable().Merge(_backgroundButton.OnClickAsObservable());

        public bool IsOpen => gameObject.activeSelf;

        private RectTransform DescriptionViewport =>
            _descriptionScroll.viewport != null ? _descriptionScroll.viewport : (RectTransform)_descriptionScroll.transform;

        private void Awake()
        {
            // Размер шрифта фиксированный: под длину текста подстраивается панель, а не шрифт.
            _description.enableAutoSizing = false;
            _description.overflowMode = TextOverflowModes.Overflow;
        }

        public void Show(string title, string description)
        {
            _title.text = title;
            _description.text = description;

            KillAnimation();
            gameObject.SetActive(true);
            FitPanelToContent();

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

        // Смена ориентации / safe area меняет доступную высоту — пересчитываем лимит.
        private void OnRectTransformDimensionsChange()
        {
            if (IsOpen && _panel != null && _descriptionScroll != null)
                FitPanelToContent();
        }

        /// <summary>
        /// Высота панели = «обвязка» (заголовок, кнопка, отступы) + высота текста,
        /// с ограничением сверху долей доступной высоты. Если текст не влез — включается прокрутка.
        /// </summary>
        private void FitPanelToContent()
        {
            RectTransform viewport = DescriptionViewport;
            RectTransform descriptionRect = _description.rectTransform;

            // Вьюпорт растянут по панели с фиксированными отступами, поэтому разница высот постоянна.
            float chromeHeight = _panel.rect.height - viewport.rect.height;
            float textHeight = Mathf.Ceil(_description.preferredHeight);

            var area = (RectTransform)_panel.parent;
            float maxHeight = Mathf.Max(_minPanelHeight, area.rect.height * _maxPanelHeightRatio);
            float panelHeight = Mathf.Clamp(chromeHeight + textHeight, _minPanelHeight, maxHeight);

            _panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
            descriptionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

            bool overflows = textHeight > viewport.rect.height + 0.5f;
            _descriptionScroll.vertical = overflows;
            _descriptionScroll.StopMovement();
            descriptionRect.anchoredPosition = Vector2.zero;
        }

        private void KillAnimation()
        {
            _sequence?.Kill();
            _sequence = null;
        }
    }
}
