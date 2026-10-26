namespace TestTask.Core.UI
{
    /// <summary>
    /// Экран вкладки с точки зрения презентера: показать, спрятать, узнать, виден ли он.
    /// </summary>
    public interface IScreenView
    {
        bool IsVisible { get; }

        void Show();

        void Hide();
    }
}
