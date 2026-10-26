using System;
using UniRx;

namespace TestTask.Features.Dogs.View
{
    public interface IBreedPopupView
    {
        IObservable<Unit> CloseRequested { get; }

        bool IsOpen { get; }

        void Show(string title, string description);

        void Hide();

        void HideImmediate();
    }
}
