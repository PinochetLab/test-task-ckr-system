using System;
using System.Collections.Generic;
using TestTask.Core.UI;
using TestTask.Features.Dogs.Data;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TestTask.Features.Dogs.View
{
    public sealed class DogBreedsView : TabScreenView
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private LoadingIndicatorView _loader;
        [SerializeField] private Text _errorText;

        private readonly List<BreedItemView> _items = new List<BreedItemView>();
        private readonly Subject<string> _breedClicked = new Subject<string>();
        private readonly CompositeDisposable _itemSubscriptions = new CompositeDisposable();

        private BreedItemView.Pool _itemPool;

        public IObservable<string> BreedClicked => _breedClicked;

        [Inject]
        public void Construct(BreedItemView.Pool itemPool)
        {
            _itemPool = itemPool;
        }

        public void SetLoading(bool loading)
        {
            _loader.SetVisible(loading);
        }

        public void SetError(string message)
        {
            _errorText.text = message;
        }

        public void SetBreeds(IReadOnlyList<BreedInfo> breeds)
        {
            Clear();

            for (int i = 0; i < breeds.Count; i++)
            {
                BreedItemView item = _itemPool.Spawn();
                item.transform.SetAsLastSibling();
                item.Setup(i + 1, breeds[i]);

                item.Clicked
                    .Subscribe(_breedClicked.OnNext)
                    .AddTo(_itemSubscriptions);

                _items.Add(item);
            }
        }

        public void SetItemLoading(string breedId, bool loading)
        {
            foreach (BreedItemView item in _items)
            {
                if (item.BreedId == breedId)
                    item.SetLoading(loading);
            }
        }

        public void Clear()
        {
            _itemSubscriptions.Clear();

            foreach (BreedItemView item in _items)
                _itemPool.Despawn(item);

            _items.Clear();
            _errorText.text = string.Empty;
        }

        private void OnDestroy()
        {
            _itemSubscriptions.Dispose();
            _breedClicked.Dispose();
        }
    }
}
