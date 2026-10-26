using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Features.Dogs.Data;
using TestTask.Features.Dogs.Service;
using TestTask.Features.Dogs.View;
using UniRx;

namespace TestTask.Tests
{
    internal sealed class FakeDogBreedsView : IDogBreedsView
    {
        private readonly Subject<string> _breedClicked = new Subject<string>();

        public IObservable<string> BreedClicked => _breedClicked;
        public bool IsVisible { get; private set; } = true;
        public bool IsLoading { get; private set; }
        public string ErrorText { get; private set; }
        public IReadOnlyList<BreedInfo> Breeds { get; private set; }
        public Dictionary<string, bool> ItemLoading { get; } = new Dictionary<string, bool>();

        public void ClickBreed(string id) => _breedClicked.OnNext(id);

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;

        public void Clear()
        {
            Breeds = null;
            ErrorText = null;
        }

        public void SetLoading(bool loading)
        {
            IsLoading = loading;
        }

        public void SetError(string message)
        {
            ErrorText = message;
        }

        public void SetBreeds(IReadOnlyList<BreedInfo> breeds)
        {
            Breeds = breeds;
        }

        public void SetItemLoading(string breedId, bool loading)
        {
            ItemLoading[breedId] = loading;
        }
    }

    internal sealed class FakeBreedPopupView : IBreedPopupView
    {
        private readonly Subject<Unit> _closeRequested = new Subject<Unit>();

        public IObservable<Unit> CloseRequested => _closeRequested;
        public bool IsOpen { get; private set; }
        public BreedDetails ShownDetails { get; private set; }
        public int HideCount { get; private set; }
        public int HideImmediateCount { get; private set; }

        public void RequestClose() => _closeRequested.OnNext(Unit.Default);

        public void Show(BreedDetails details)
        {
            ShownDetails = details;
            IsOpen = true;
        }

        public void Hide()
        {
            HideCount++;
            IsOpen = false;
        }

        public void HideImmediate()
        {
            HideImmediateCount++;
            IsOpen = false;
        }
    }

    /// <summary>Каждый запрос висит, пока тест сам не завершит его через Source.</summary>
    internal sealed class FakeDogBreedsService : IDogBreedsService
    {
        public sealed class Request<T>
        {
            public string Id;
            public UniTaskCompletionSource<T> Source;
            public CancellationToken Token;
        }

        public List<Request<IReadOnlyList<BreedInfo>>> ListRequests { get; } = new List<Request<IReadOnlyList<BreedInfo>>>();
        public List<Request<BreedDetails>> DetailsRequests { get; } = new List<Request<BreedDetails>>();

        public UniTask<IReadOnlyList<BreedInfo>> GetBreedsAsync(CancellationToken cancellationToken)
        {
            return Add(ListRequests, null, cancellationToken);
        }

        public UniTask<BreedDetails> GetBreedAsync(string id, CancellationToken cancellationToken)
        {
            return Add(DetailsRequests, id, cancellationToken);
        }

        private static UniTask<T> Add<T>(List<Request<T>> requests, string id, CancellationToken cancellationToken)
        {
            var source = new UniTaskCompletionSource<T>();
            cancellationToken.Register(() => source.TrySetCanceled(cancellationToken));
            requests.Add(new Request<T> { Id = id, Source = source, Token = cancellationToken });
            return source.Task;
        }
    }
}
