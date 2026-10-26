using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TestTask.Core.Network
{
    /// <summary>
    /// HTTP-клиент на UnityWebRequest. При отмене токена запрос прерывается (Abort),
    /// а request уничтожается только после того, как Unity завершила операцию.
    /// </summary>
    public sealed class UnityHttpClient : IHttpClient
    {
        private readonly NetworkConfig _config;

        public UnityHttpClient(NetworkConfig config)
        {
            _config = config;
        }

        public async UniTask<string> GetTextAsync(string url, CancellationToken cancellationToken)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            Configure(request);
            request.SetRequestHeader("Accept", "application/json");

            await SendAsync(request, cancellationToken);
            return request.downloadHandler.text;
        }

        public async UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            Configure(request);

            await SendAsync(request, cancellationToken);
            return DownloadHandlerTexture.GetContent(request);
        }

        private void Configure(UnityWebRequest request)
        {
            request.timeout = _config.TimeoutSeconds;

            if (!string.IsNullOrEmpty(_config.UserAgent))
                request.SetRequestHeader("User-Agent", _config.UserAgent);
        }

        /// <summary>
        /// Отправляет запрос и ждёт его настоящего завершения.
        /// Отмену обрабатываем сами: по токену вызываем Abort, но продолжаем ждать операцию,
        /// пока Unity её не закроет. С cancelImmediately UniTask завершал ожидание сразу,
        /// request уничтожался через using, а на следующем кадре UniTask снова вызывал Abort()
        /// у уже уничтоженного request — отсюда NullReferenceException в UnityWebRequest.Abort.
        /// Заодно очередь не запускает следующий запрос, пока прерванный реально не остановился.
        /// </summary>
        private static async UniTask SendAsync(UnityWebRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            try
            {
                using (cancellationToken.Register(() => AbortIfRunning(request, operation)))
                    await operation;
            }
            catch (UnityWebRequestException exception)
            {
                // Прерванный по нашей отмене запрос — это отмена, а не сетевая ошибка.
                if (cancellationToken.IsCancellationRequested)
                    throw new System.OperationCanceledException(cancellationToken);

                throw new HttpRequestException(request.url, exception.ResponseCode, exception.Error);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private static void AbortIfRunning(UnityWebRequest request, UnityWebRequestAsyncOperation operation)
        {
            if (!operation.isDone)
                request.Abort();
        }
    }
}
