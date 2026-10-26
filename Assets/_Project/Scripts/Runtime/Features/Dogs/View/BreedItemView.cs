using System;
using TestTask.Core.UI;
using TestTask.Features.Dogs.Data;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TestTask.Features.Dogs.View
{
    /// <summary>Строка списка пород: «1 - Akita» и индикатор загрузки.</summary>
    public sealed class BreedItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _label;
        [SerializeField] private LoadingIndicatorView _loader;

        public string BreedId { get; private set; }

        public IObservable<string> Clicked => _button.OnClickAsObservable().Select(_ => BreedId);

        public void Setup(int number, BreedInfo breed)
        {
            BreedId = breed.Id;
            _label.text = $"{number} - {breed.Name}";
            SetLoading(false);
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
                item.BreedId = null;
                base.OnDespawned(item);
            }
        }
    }
}
