using TestTask.Core.Network;
using TestTask.Features.Clicker.Config;
using TestTask.Features.Dogs.Config;
using TestTask.Features.Weather.Config;
using UnityEngine;
using Zenject;

namespace TestTask.App.Installers
{
    [CreateAssetMenu(menuName = "TestTask/Installers/Configs", fileName = "ConfigsInstaller")]
    public sealed class ConfigsInstaller : ScriptableObjectInstaller<ConfigsInstaller>
    {
        [SerializeField] private NetworkConfig _network;
        [SerializeField] private ClickerConfig _clicker;
        [SerializeField] private WeatherConfig _weather;
        [SerializeField] private DogsConfig _dogs;

        public override void InstallBindings()
        {
            Container.BindInstance(_network).AsSingle();
            Container.BindInstance(_clicker).AsSingle();
            Container.BindInstance(_weather).AsSingle();
            Container.BindInstance(_dogs).AsSingle();
        }
    }
}
