using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TestTask.Core.Network
{
    /// <summary>
    /// Последовательная очередь запросов. Следующий запрос стартует только после завершения предыдущего.
    /// </summary>
    public sealed class RequestQueue : IRequestQueue, IDisposable
    {
        private readonly LinkedList<IQueuedRequest> _pending = new LinkedList<IQueuedRequest>();
        private readonly CancellationTokenSource _disposeCts = new CancellationTokenSource();

        private bool _isProcessing;
        private bool _isDisposed;

        public int PendingCount => _pending.Count;

        public bool IsBusy { get; private set; }

        public UniTask<T> Enqueue<T>(Func<CancellationToken, UniTask<T>> request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (_isDisposed)
                return UniTask.FromCanceled<T>();

            if (cancellationToken.IsCancellationRequested)
                return UniTask.FromCanceled<T>(cancellationToken);

            var item = new QueuedRequest<T>(request, cancellationToken, _disposeCts.Token, RemovePending);
            item.Node = _pending.AddLast(item);
            item.ListenCancellation();

            if (!_isProcessing)
                ProcessAsync().Forget();

            return item.Task;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _disposeCts.Cancel();

            foreach (IQueuedRequest item in _pending)
                item.Cancel();

            _pending.Clear();
        }

        private void RemovePending<T>(QueuedRequest<T> item)
        {
            if (item.Node != null && item.Node.List == _pending)
                _pending.Remove(item.Node);

            item.Node = null;
        }

        private async UniTaskVoid ProcessAsync()
        {
            _isProcessing = true;

            try
            {
                while (!_isDisposed && _pending.First != null)
                {
                    IQueuedRequest next = _pending.First.Value;
                    _pending.RemoveFirst();

                    if (!next.IsPending)
                        continue;

                    IsBusy = true;
                    await next.ExecuteAsync();
                    IsBusy = false;
                }
            }
            finally
            {
                IsBusy = false;
                _isProcessing = false;
            }
        }
    }
}
