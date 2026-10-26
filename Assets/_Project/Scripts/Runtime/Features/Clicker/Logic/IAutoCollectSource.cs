using System;
using UniRx;

namespace TestTask.Features.Clicker.Logic
{
    /// <summary>Источник событий успешного автосбора.</summary>
    public interface IAutoCollectSource
    {
        IObservable<Unit> AutoCollected { get; }
    }
}
