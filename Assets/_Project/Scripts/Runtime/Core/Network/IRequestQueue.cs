using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TestTask.Core.Network
{
    /// <summary>
    /// Очередь сетевых запросов: запросы выполняются строго по одному в порядке постановки.
    /// </summary>
    public interface IRequestQueue
    {
        /// <summary>
        /// Ставит запрос в очередь.
        /// Отмена <paramref name="cancellationToken"/>:
        /// если запрос ещё ожидает — он удаляется из очереди;
        /// если уже выполняется — отменяется токен, переданный в <paramref name="request"/>.
        /// </summary>
        UniTask<T> Enqueue<T>(Func<CancellationToken, UniTask<T>> request, CancellationToken cancellationToken);

        /// <summary>Количество запросов, ожидающих выполнения (без текущего).</summary>
        int PendingCount { get; }

        /// <summary>Выполняется ли сейчас какой-либо запрос.</summary>
        bool IsBusy { get; }
    }
}
