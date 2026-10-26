using Cysharp.Threading.Tasks;

namespace TestTask.Core.Network
{
    /// <summary>
    /// Элемент очереди запросов (без generic-параметра, чтобы хранить разные запросы в одной очереди).
    /// </summary>
    internal interface IQueuedRequest
    {
        bool IsPending { get; }

        /// <summary>Выполняет запрос. Никогда не бросает исключений — результат уходит вызывающей стороне.</summary>
        UniTask ExecuteAsync();

        /// <summary>Отменяет запрос (используется при уничтожении очереди).</summary>
        void Cancel();
    }
}
