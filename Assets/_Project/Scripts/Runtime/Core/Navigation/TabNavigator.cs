using System;
using System.Collections.Generic;
using System.Linq;
using TestTask.Core.Navigation.View;
using UniRx;
using Zenject;

namespace TestTask.Core.Navigation
{
    /// <summary>
    /// Переключает вкладки: Exit у текущей, Enter у новой.
    /// </summary>
    public sealed class TabNavigator : IInitializable, IDisposable
    {
        private const TabId StartTab = TabId.Clicker;

        private readonly Dictionary<TabId, ITabPresenter> _tabs;
        private readonly TabBarView _tabBar;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private ITabPresenter _current;

        public TabNavigator(List<ITabPresenter> tabs, TabBarView tabBar)
        {
            _tabs = tabs.ToDictionary(tab => tab.Id);
            _tabBar = tabBar;
        }

        public TabId? CurrentTab => _current?.Id;

        public void Initialize()
        {
            _tabBar.TabSelected
                .Subscribe(Open)
                .AddTo(_disposables);

            Open(StartTab);
        }

        public void Open(TabId id)
        {
            if (_current != null && _current.Id == id)
                return;

            if (!_tabs.TryGetValue(id, out ITabPresenter next))
                throw new InvalidOperationException($"Tab {id} is not registered");

            _current?.Exit();
            _current = next;
            _current.Enter();
            _tabBar.SetSelected(id);
        }

        public void Dispose()
        {
            // Exit не вызываем: при выгрузке сцены View уже могут быть уничтожены.
            // Презентеры сами отменяют свои операции в Dispose.
            _disposables.Dispose();
            _current = null;
        }
    }
}
