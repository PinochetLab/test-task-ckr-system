using TestTask.Features.Clicker.Config;
using TestTask.Features.Clicker.Effects;
using TestTask.Features.Clicker.Logic;
using TestTask.Features.Clicker.Model;
using TestTask.Features.Clicker.Presenter;
using TestTask.Features.Clicker.View;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Clicker.Installers
{
    public sealed class ClickerInstaller : MonoInstaller
    {
        [SerializeField] private ClickerView _view;
        [SerializeField] private RectTransform _effectsLayer;
        [SerializeField] private FlyingCurrencyView _flyingCurrencyPrefab;
        [SerializeField] private UiParticleView _particlePrefab;
        [SerializeField, Min(0)] private int _flyingCurrencyPoolSize = 10;
        [SerializeField, Min(0)] private int _particlePoolSize = 20;

        public override void InstallBindings()
        {
            Container.BindInstance(_view).AsSingle();

            Container.BindInterfacesAndSelfTo<ClickerModel>()
                .FromMethod(context =>
                {
                    var config = context.Container.Resolve<ClickerConfig>();
                    return new ClickerModel(config.MaxEnergy, config.StartEnergy);
                })
                .AsSingle();

            Container.Bind<ClickerRules>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClickerTicker>().AsSingle();
            Container.Bind<ClickerEffectsPlayer>().AsSingle();

            Container.BindMemoryPool<FlyingCurrencyView, FlyingCurrencyView.Pool>()
                .WithInitialSize(_flyingCurrencyPoolSize)
                .FromComponentInNewPrefab(_flyingCurrencyPrefab)
                .UnderTransform(_effectsLayer);

            Container.BindMemoryPool<UiParticleView, UiParticleView.Pool>()
                .WithInitialSize(_particlePoolSize)
                .FromComponentInNewPrefab(_particlePrefab)
                .UnderTransform(_effectsLayer);

            Container.BindInterfacesAndSelfTo<ClickerPresenter>().AsSingle();
        }
    }
}
