using TestTask.Features.Weather.Presenter;
using TestTask.Features.Weather.Service;
using TestTask.Features.Weather.View;
using UniRx;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Weather.Installers
{
    public sealed class WeatherInstaller : MonoInstaller
    {
        [SerializeField] private WeatherView _view;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<WeatherView>().FromInstance(_view).AsSingle();

            // Таймер опроса погоды: главный поток, время игры (timeScale) не влияет.
            Container.Bind<IScheduler>()
                .FromInstance(Scheduler.MainThreadIgnoreTimeScale)
                .WhenInjectedInto<WeatherPresenter>();

            Container.BindInterfacesTo<WeatherService>().AsSingle();
            Container.BindInterfacesAndSelfTo<WeatherPresenter>().AsSingle();
        }
    }
}
