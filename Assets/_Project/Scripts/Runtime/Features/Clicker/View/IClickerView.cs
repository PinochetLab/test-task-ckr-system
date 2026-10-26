using System;
using TestTask.Core.UI;
using UniRx;

namespace TestTask.Features.Clicker.View
{
    public interface IClickerView : IScreenView
    {
        IObservable<Unit> Clicked { get; }

        void SetCurrency(string text);

        /// <param name="fill">Заполнение шкалы, 0..1.</param>
        /// <param name="isLow">Энергии почти не осталось — шкала подсвечивается.</param>
        void SetEnergy(string text, float fill, bool isLow);

        /// <summary>Фидбек успешного нажатия: «пульс» объекта и разлёт частиц.</summary>
        void PlayClickFeedback(int particleCount);

        /// <summary>Фидбек отказа (не хватило энергии).</summary>
        void PlayDenied();
    }
}
