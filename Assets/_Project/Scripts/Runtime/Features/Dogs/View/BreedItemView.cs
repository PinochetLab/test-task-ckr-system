using System;
using TestTask.Core.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TestTask.Features.Dogs.View
{
    /// <summary>Строка списка пород: подпись и индикатор загрузки.</summary>
    public sealed class BreedItemView : MonoBehaviour, IBreedItemView
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _label;
        [SerializeField] private LoadingIndicatorView _loader;

        public IObservable<Unit> Clicked => _button.OnClickAsObservable();

        public void SetLabel(string text)
        {
            _label.text = text;
        }

        public void SetLoading(bool loading)
        {
            _loader.SetVisible(loading);
        }

        public sealed class Pool : MonoMemoryPool<BreedItemView>
        {
            protected override void OnDespawned(BreedItemView item)
            {
                item.SetLoading(false);
                base.OnDespawned(item);
            }
        }
    }
}
