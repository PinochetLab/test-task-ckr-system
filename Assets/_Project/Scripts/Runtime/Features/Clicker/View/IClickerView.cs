using System;
using TestTask.Core.UI;
using UniRx;

namespace TestTask.Features.Clicker.View
{
    public interface IClickerView : IScreenView
    {
        IObservable<Unit> Clicked { get; }

        void SetCurrency(long value);

        void SetEnergy(int current, int max);

        /// <summary>Фидбек успешного нажатия: «пульс» объекта и разлёт частиц.</summary>
        void PlayClickFeedback(int particleCount);

        /// <summary>Фидбек отказа (не хватило энергии).</summary>
        void PlayDenied();
    }
}
