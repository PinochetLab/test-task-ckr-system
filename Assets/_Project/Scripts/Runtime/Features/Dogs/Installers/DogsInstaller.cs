using TestTask.Features.Dogs.Presenter;
using TestTask.Features.Dogs.Service;
using TestTask.Features.Dogs.View;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Dogs.Installers
{
    public sealed class DogsInstaller : MonoInstaller
    {
        [SerializeField] private DogBreedsView _view;
        [SerializeField] private BreedPopupView _popup;
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private BreedItemView _itemPrefab;
        [SerializeField, Min(0)] private int _itemPoolSize = 10;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DogBreedsView>().FromInstance(_view).AsSingle();
            Container.BindInterfacesAndSelfTo<BreedPopupView>().FromInstance(_popup).AsSingle();

            Container.BindMemoryPool<BreedItemView, BreedItemView.Pool>()
                .WithInitialSize(_itemPoolSize)
                .FromComponentInNewPrefab(_itemPrefab)
                .UnderTransform(_listContent);

            Container.BindInterfacesTo<BreedItemFactory>().AsSingle();

            Container.BindInterfacesTo<DogBreedsService>().AsSingle();
            Container.BindInterfacesAndSelfTo<DogBreedsPresenter>().AsSingle();
        }
    }
}
