using System;
using TestTask.Features.Dogs.Data;
using UniRx;

namespace TestTask.Features.Dogs.View
{
    public interface IBreedPopupView
    {
        IObservable<Unit> CloseRequested { get; }

        bool IsOpen { get; }

        void Show(BreedDetails details);

        void Hide();

        void HideImmediate();
    }
}
