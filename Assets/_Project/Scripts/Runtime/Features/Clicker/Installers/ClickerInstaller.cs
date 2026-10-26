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

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ClickerView>().FromInstance(_view).AsSingle();

            Container.BindInterfacesAndSelfTo<ClickerModel>()
                .FromMethod(context =>
                {
                    var config = context.Container.Resolve<ClickerConfig>();
                    return new ClickerModel(config.MaxEnergy, config.StartEnergy);
                })
                .AsSingle();

            Container.Bind<ClickerRules>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClickerTicker>().AsSingle();
            Container.BindInterfacesTo<ClickerEffectsPlayer>().AsSingle();

            Container.BindInterfacesAndSelfTo<ClickerPresenter>().AsSingle();
        }
    }
}
