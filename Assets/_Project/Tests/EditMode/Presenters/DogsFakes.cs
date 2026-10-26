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
        public bool IsVisible { get; private set; } = true;
        public bool IsLoading { get; private set; }
        public string ErrorText { get; private set; }

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;
        public void SetLoading(bool loading) => IsLoading = loading;
        public void SetError(string message) => ErrorText = message;
    }

    internal sealed class FakeBreedItemView : IBreedItemView
    {
        private readonly Subject<Unit> _clicked = new Subject<Unit>();

        public IObservable<Unit> Clicked => _clicked;
        public string Label { get; private set; }
        public bool IsLoading { get; private set; }
        public bool IsDespawned { get; set; }

        public void Click() => _clicked.OnNext(Unit.Default);
        public void SetLabel(string text) => Label = text;
        public void SetLoading(bool loading) => IsLoading = loading;
    }

    internal sealed class FakeBreedItemFactory : IBreedItemFactory
    {
        /// <summary>Все когда-либо созданные строки, по порядку.</summary>
        public List<FakeBreedItemView> Created { get; } = new List<FakeBreedItemView>();

        public IBreedItemView Spawn()
        {
            var item = new FakeBreedItemView();
            Created.Add(item);
            return item;
        }

        public void Despawn(IBreedItemView item) => ((FakeBreedItemView)item).IsDespawned = true;
    }

    internal sealed class FakeBreedPopupView : IBreedPopupView
    {
        private readonly Subject<Unit> _closeRequested = new Subject<Unit>();

        public IObservable<Unit> CloseRequested => _closeRequested;
        public bool IsOpen { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public int HideCount { get; private set; }
        public int HideImmediateCount { get; private set; }

        public void RequestClose() => _closeRequested.OnNext(Unit.Default);

        public void Show(string title, string description)
        {
            Title = title;
            Description = description;
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
