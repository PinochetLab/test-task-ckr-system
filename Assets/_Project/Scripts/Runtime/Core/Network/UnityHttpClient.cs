using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TestTask.Core.Network
{
    /// <summary>
    /// HTTP-клиент на UnityWebRequest. При отмене токена запрос прерывается (Abort).
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

        private static async UniTask SendAsync(UnityWebRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // При отмене токена UniTask сразу вызывает request.Abort().
                await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken, cancelImmediately: true);
            }
            catch (UnityWebRequestException exception)
            {
                throw new HttpRequestException(request.url, exception.ResponseCode, exception.Error);
            }
        }
    }
}
