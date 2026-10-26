using TestTask.Core.Audio;
using TestTask.Core.Content;
using TestTask.Core.Navigation;
using TestTask.Core.Navigation.View;
using TestTask.Core.Network;
using UnityEngine;
using Zenject;

namespace TestTask.Core.Installers
{
    /// <summary>
    /// Инфраструктура: сеть, загрузка контента, звук, навигация по вкладкам.
    /// </summary>
    public sealed class CoreInstaller : MonoInstaller
    {
        // Навигатор инициализируется после презентеров вкладок.
        private const int NavigatorExecutionOrder = 100;

        [SerializeField] private TabBarView _tabBar;
        [SerializeField] private AudioSource _audioSource;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<RequestQueue>().AsSingle();
            Container.BindInterfacesTo<UnityHttpClient>().AsSingle();
            Container.BindInterfacesTo<NewtonsoftJsonParser>().AsSingle();
            Container.BindInterfacesTo<RemoteSpriteLoader>().AsSingle();

            Container.BindInstance(_audioSource).AsSingle();
            Container.BindInterfacesTo<AudioService>().AsSingle();

            Container.BindInstance(_tabBar).AsSingle();
            Container.BindInterfacesAndSelfTo<TabNavigator>().AsSingle();
            Container.BindExecutionOrder<TabNavigator>(NavigatorExecutionOrder);
        }
    }
}
