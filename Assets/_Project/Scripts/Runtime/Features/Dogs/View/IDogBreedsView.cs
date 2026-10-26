using System;
using System.Collections.Generic;
using TestTask.Core.UI;
using TestTask.Features.Dogs.Data;

namespace TestTask.Features.Dogs.View
{
    public interface IDogBreedsView : IScreenView
    {
        /// <summary>Клик по породе в списке, значение — id породы.</summary>
        IObservable<string> BreedClicked { get; }

        void Clear();

        void SetLoading(bool loading);

        void SetError(string message);

        void SetBreeds(IReadOnlyList<BreedInfo> breeds);

        void SetItemLoading(string breedId, bool loading);
    }
}
