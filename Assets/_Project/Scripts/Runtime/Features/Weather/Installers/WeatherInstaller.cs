using TestTask.Features.Weather.Presenter;
using TestTask.Features.Weather.Service;
using TestTask.Features.Weather.View;
using UnityEngine;
using Zenject;

namespace TestTask.Features.Weather.Installers
{
    public sealed class WeatherInstaller : MonoInstaller
    {
        [SerializeField] private WeatherView _view;

        public override void InstallBindings()
        {
            Container.BindInstance(_view).AsSingle();
            Container.BindInterfacesTo<WeatherService>().AsSingle();
            Container.BindInterfacesAndSelfTo<WeatherPresenter>().AsSingle();
        }
    }
}
