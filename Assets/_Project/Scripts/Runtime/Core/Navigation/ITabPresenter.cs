namespace TestTask.Core.Navigation
{
    /// <summary>
    /// Презентер вкладки. Всё, что вкладка запускает в <see cref="Enter"/>
    /// (запросы, таймеры), должно быть остановлено в <see cref="Exit"/>.
    /// </summary>
    public interface ITabPresenter
    {
        TabId Id { get; }

        void Enter();

        void Exit();
    }
}
