using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestTask.Core.Network
{
    /// <summary>
    /// Транспорт HTTP. Сам по себе очередь не использует — запросы ставят в очередь сервисы.
    /// </summary>
    public interface IHttpClient
    {
        UniTask<string> GetTextAsync(string url, CancellationToken cancellationToken);
        UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken);
    }
}
