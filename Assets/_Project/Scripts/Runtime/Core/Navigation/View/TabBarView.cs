using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace TestTask.Core.Navigation.View
{
    /// <summary>Нижняя панель навигации.</summary>
    public sealed class TabBarView : MonoBehaviour
    {
        [SerializeField] private TabButtonView[] _buttons = Array.Empty<TabButtonView>();

        public IObservable<TabId> TabSelected => _buttons.Select(button => button.Clicked).Merge();

        public void SetSelected(TabId id)
        {
            foreach (TabButtonView button in _buttons)
                button.SetSelected(button.Id == id);
        }
    }
}
