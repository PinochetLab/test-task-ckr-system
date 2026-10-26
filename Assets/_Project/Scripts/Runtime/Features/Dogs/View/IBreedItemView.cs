using System;
using UniRx;

namespace TestTask.Features.Dogs.View
{
    /// <summary>Строка списка пород: подпись, клик и индикатор загрузки.</summary>
    public interface IBreedItemView
    {
        IObservable<Unit> Clicked { get; }

        void SetLabel(string text);

        void SetLoading(bool loading);
    }
}
