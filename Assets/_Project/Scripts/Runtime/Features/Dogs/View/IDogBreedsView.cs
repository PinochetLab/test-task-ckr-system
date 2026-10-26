using TestTask.Core.UI;

namespace TestTask.Features.Dogs.View
{
    /// <summary>
    /// Экран списка пород. Строки списка создаёт и наполняет презентер через <see cref="IBreedItemFactory"/>,
    /// сама view только показывает загрузку и ошибку.
    /// </summary>
    public interface IDogBreedsView : IScreenView
    {
        void SetLoading(bool loading);

        /// <param name="message">Текст ошибки; пустая строка убирает её.</param>
        void SetError(string message);
    }
}
