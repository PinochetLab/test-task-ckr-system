using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.Core.Navigation.View
{
    public sealed class TabButtonView : MonoBehaviour
    {
        [SerializeField] private TabId _id;
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private Text _label;
        [SerializeField] private Color _normalColor = new Color(0.22f, 0.22f, 0.26f);
        [SerializeField] private Color _selectedColor = new Color(0.25f, 0.55f, 0.95f);
        [SerializeField] private Color _normalTextColor = new Color(0.75f, 0.75f, 0.8f);
        [SerializeField] private Color _selectedTextColor = Color.white;

        public TabId Id => _id;

        public IObservable<TabId> Clicked => _button.OnClickAsObservable().Select(_ => _id);

        public void SetSelected(bool selected)
        {
            _background.color = selected ? _selectedColor : _normalColor;
            _label.color = selected ? _selectedTextColor : _normalTextColor;
        }
    }
}
