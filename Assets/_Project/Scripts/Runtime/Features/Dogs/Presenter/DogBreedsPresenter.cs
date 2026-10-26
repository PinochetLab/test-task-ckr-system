using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Core.Navigation;
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
    /// При входе на вкладку загружает список пород.
    /// По клику на породу загружает её описание; клик по другой породе отменяет предыдущий запрос.
    /// При уходе с вкладки отменяются все запросы вкладки.
    /// </summary>
    public sealed class DogBreedsPresenter : ITabPresenter, IInitializable, IDisposable
    {
        private const string ListErrorMessage = "Не удалось загрузить список пород";
        private const string DetailsErrorTitle = "Ошибка";
        private const string DetailsErrorMessage = "Не удалось загрузить описание породы";

        private readonly IDogBreedsView _view;
        private readonly IBreedPopupView _popup;
        private readonly IDogBreedsService _service;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private CancellationTokenSource _tabCts;
        private CancellationTokenSource _breedCts;

        public DogBreedsPresenter(IDogBreedsView view, IBreedPopupView popup, IDogBreedsService service)
        {
            _view = view;
            _popup = popup;
            _service = service;
        }

        public TabId Id => TabId.Dogs;

        public void Initialize()
        {
            _popup.HideImmediate();
            _view.SetLoading(false);
            _view.Hide();

            _view.BreedClicked
                .Subscribe(OnBreedClicked)
                .AddTo(_disposables);

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
        }

        private async UniTaskVoid LoadBreedsAsync(CancellationToken cancellationToken)
        {
            _view.Clear();
            _view.SetLoading(true);

            try
            {
                IReadOnlyList<BreedInfo> breeds = await _service.GetBreedsAsync(cancellationToken);
                _view.SetBreeds(breeds);
            }
            catch (OperationCanceledException)
            {
                // Ушли с вкладки — ничего не показываем.
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Dogs] {exception.Message}");
                _view.SetError(ListErrorMessage);
            }
            finally
            {
                // После отмены вкладка закрыта или сцена уже выгружается — view трогать нельзя.
                if (!cancellationToken.IsCancellationRequested)
                    _view.SetLoading(false);
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
            _view.SetItemLoading(breedId, true);

            try
            {
                BreedDetails details = await _service.GetBreedAsync(breedId, cancellationToken);
                _popup.Show(details);
            }
            catch (OperationCanceledException)
            {
                // Выбрали другую породу или ушли с вкладки.
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Dogs] {exception.Message}");
                _popup.Show(new BreedDetails(DetailsErrorTitle, DetailsErrorMessage));
            }
            finally
            {
                // Клик по другой породе отменяет этот запрос, но view жив — спиннер у пункта гасим.
                // А после ухода с вкладки или выгрузки сцены view трогать нельзя.
                if (!tabToken.IsCancellationRequested)
                    _view.SetItemLoading(breedId, false);
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
