using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Core.Navigation;
using TestTask.Features.Dogs.Config;
using TestTask.Features.Dogs.Data;
using TestTask.Features.Dogs.Service;
using TestTask.Features.Dogs.View;
using UniRx;
using UnityEngine;
using Zenject;
using CancellationToken = System.Threading.CancellationToken;

namespace TestTask.Features.Dogs.Presenter
{
    /// <summary>
    /// При входе на вкладку загружает список пород и строит его строки.
    /// По клику на породу загружает её описание; клик по другой породе отменяет предыдущий запрос.
    /// При уходе с вкладки отменяются все запросы вкладки.
    /// </summary>
    public sealed class DogBreedsPresenter : ITabPresenter, IInitializable, IDisposable
    {
        private sealed class BreedItem
        {
            public string BreedId;
            public IBreedItemView View;
        }

        private readonly IDogBreedsView _view;
        private readonly IBreedPopupView _popup;
        private readonly IBreedItemFactory _itemFactory;
        private readonly IDogBreedsService _service;
        private readonly DogsConfig _config;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly CompositeDisposable _itemSubscriptions = new CompositeDisposable();
        private readonly List<BreedItem> _items = new List<BreedItem>();

        private CancellationTokenSource _tabCts;
        private CancellationTokenSource _breedCts;

        public DogBreedsPresenter(
            IDogBreedsView view,
            IBreedPopupView popup,
            IBreedItemFactory itemFactory,
            IDogBreedsService service,
            DogsConfig config)
        {
            _view = view;
            _popup = popup;
            _itemFactory = itemFactory;
            _service = service;
            _config = config;
        }

        public TabId Id => TabId.Dogs;

        public void Initialize()
        {
            _popup.HideImmediate();
            _view.SetLoading(false);
            _view.Hide();

            _popup.CloseRequested
                .Subscribe(_ => _popup.Hide())
                .AddTo(_disposables);
        }

        public void Enter()
        {
            CancelAll();

            _view.Show();
            _tabCts = new CancellationTokenSource();
            LoadBreedsAsync(_tabCts.Token).Forget();
        }

        public void Exit()
        {
            CancelAll();
            _popup.HideImmediate();
            _view.Hide();
        }

        public void Dispose()
        {
            CancelAll();
            _disposables.Dispose();

            // Строки не возвращаем в пул: при выгрузке сцены они уже уничтожаются вместе с ней.
            _itemSubscriptions.Dispose();
            _items.Clear();
        }

        private async UniTaskVoid LoadBreedsAsync(CancellationToken cancellationToken)
        {
            ClearItems();
            _view.SetError(string.Empty);
            _view.SetLoading(true);

            try
            {
                IReadOnlyList<BreedInfo> breeds = await _service.GetBreedsAsync(cancellationToken);
                ShowBreeds(breeds);
            }
            catch (OperationCanceledException)
            {
                // Ушли с вкладки — ничего не показываем.
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Dogs] {exception.Message}");
                _view.SetError(_config.ListErrorMessage);
            }
            finally
            {
                // После отмены вкладка закрыта или сцена уже выгружается — view трогать нельзя.
                if (!cancellationToken.IsCancellationRequested)
                    _view.SetLoading(false);
            }
        }

        private void ShowBreeds(IReadOnlyList<BreedInfo> breeds)
        {
            ClearItems();

            for (int i = 0; i < breeds.Count; i++)
            {
                BreedInfo breed = breeds[i];
                IBreedItemView itemView = _itemFactory.Spawn();
                itemView.SetLabel(string.Format(_config.ItemFormat, i + 1, breed.Name));
                itemView.SetLoading(false);

                itemView.Clicked
                    .Subscribe(_ => OnBreedClicked(breed.Id))
                    .AddTo(_itemSubscriptions);

                _items.Add(new BreedItem { BreedId = breed.Id, View = itemView });
            }
        }

        private void ClearItems()
        {
            _itemSubscriptions.Clear();

            foreach (BreedItem item in _items)
                _itemFactory.Despawn(item.View);

            _items.Clear();
        }

        private void SetItemLoading(string breedId, bool loading)
        {
            foreach (BreedItem item in _items)
            {
                if (item.BreedId == breedId)
                    item.View.SetLoading(loading);
            }
        }

        private void OnBreedClicked(string breedId)
        {
            if (_tabCts == null || string.IsNullOrEmpty(breedId))
                return;

            CancelBreedRequest();
            _breedCts = CancellationTokenSource.CreateLinkedTokenSource(_tabCts.Token);
            LoadBreedAsync(breedId, _breedCts.Token, _tabCts.Token).Forget();
        }

        private async UniTaskVoid LoadBreedAsync(string breedId, CancellationToken cancellationToken, CancellationToken tabToken)
        {
            SetItemLoading(breedId, true);

            try
            {
                BreedDetails details = await _service.GetBreedAsync(breedId, cancellationToken);
                string description = string.IsNullOrEmpty(details.Description) ? _config.EmptyDescription : details.Description;
                _popup.Show(details.Name, description);
            }
            catch (OperationCanceledException)
            {
                // Выбрали другую породу или ушли с вкладки.
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Dogs] {exception.Message}");
                _popup.Show(_config.DetailsErrorTitle, _config.DetailsErrorMessage);
            }
            finally
            {
                // Клик по другой породе отменяет этот запрос, но view жив — спиннер у пункта гасим.
                // А после ухода с вкладки или выгрузки сцены view трогать нельзя.
                if (!tabToken.IsCancellationRequested)
                    SetItemLoading(breedId, false);
            }
        }

        private void CancelBreedRequest()
        {
            if (_breedCts == null)
                return;

            _breedCts.Cancel();
            _breedCts.Dispose();
            _breedCts = null;
        }

        private void CancelAll()
        {
            CancelBreedRequest();

            if (_tabCts == null)
                return;

            _tabCts.Cancel();
            _tabCts.Dispose();
            _tabCts = null;
        }
    }
}
