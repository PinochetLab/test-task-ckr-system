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

        [Header("Вылетающие награды")]
        [SerializeField] private RewardFlyView _rewardPrefab;
        [Tooltip("Слой внутри экрана кликера, куда складываются награды из пула")]
        [SerializeField] private RectTransform _rewardLayer;
        [SerializeField, Min(0)] private int _rewardPoolSize = 4;

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

            Container.BindMemoryPool<RewardFlyView, RewardFlyView.Pool>()
                .WithInitialSize(_rewardPoolSize)
                .FromComponentInNewPrefab(_rewardPrefab)
                .UnderTransform(_rewardLayer);

            Container.BindInterfacesAndSelfTo<ClickerPresenter>().AsSingle();
        }
    }
}
