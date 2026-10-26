using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TestTask.Core.Network
{
    internal sealed class QueuedRequest<T> : IQueuedRequest
    {
        private enum State
        {
            Pending,
            Running,
            Finished
        }

        private readonly Func<CancellationToken, UniTask<T>> _request;
        private readonly CancellationToken _callerToken;
        private readonly CancellationToken _queueToken;
        private readonly Action<QueuedRequest<T>> _onCancelledWhilePending;
        private readonly UniTaskCompletionSource<T> _completion = new UniTaskCompletionSource<T>();

        private CancellationTokenRegistration _registration;
        private State _state = State.Pending;

        public QueuedRequest(
            Func<CancellationToken, UniTask<T>> request,
            CancellationToken callerToken,
            CancellationToken queueToken,
            Action<QueuedRequest<T>> onCancelledWhilePending)
        {
            _request = request;
            _callerToken = callerToken;
            _queueToken = queueToken;
            _onCancelledWhilePending = onCancelledWhilePending;
        }

        public UniTask<T> Task => _completion.Task;

        public bool IsPending => _state == State.Pending;

        internal LinkedListNode<IQueuedRequest> Node { get; set; }

        /// <summary>
        /// Подписка на отмену делается после добавления в очередь,
        /// чтобы колбэк мог безопасно удалить элемент.
        /// </summary>
        public void ListenCancellation()
        {
            if (_callerToken.CanBeCanceled)
                _registration = _callerToken.Register(OnCallerCancelled);
        }

        public async UniTask ExecuteAsync()
        {
            if (_state != State.Pending)
                return;

            _state = State.Running;

            // Пока запрос выполняется, отмена вызывающей стороны приходит через связанный токен.
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(_callerToken, _queueToken);

            try
            {
                linked.Token.ThrowIfCancellationRequested();
                T result = await _request(linked.Token);
                _completion.TrySetResult(result);
            }
            catch (OperationCanceledException)
            {
                _completion.TrySetCanceled(_callerToken.IsCancellationRequested ? _callerToken : linked.Token);
            }
            catch (Exception exception)
            {
                _completion.TrySetException(exception);
            }
            finally
            {
                _state = State.Finished;
                _registration.Dispose();
            }
        }

        public void Cancel()
        {
            if (_state == State.Finished)
                return;

            _state = State.Finished;
            _registration.Dispose();
            _completion.TrySetCanceled(_queueToken);
        }

        private void OnCallerCancelled()
        {
            if (_state != State.Pending)
                return;

            _state = State.Finished;
            _onCancelledWhilePending(this);
            _completion.TrySetCanceled(_callerToken);
        }
    }
}
