using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Core.Network;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestTask.Core.Content
{
    /// <summary>
    /// Загружает спрайты по url через общую очередь запросов и кэширует их.
    /// </summary>
    public sealed class RemoteSpriteLoader : ISpriteLoader, IDisposable
    {
        private readonly IRequestQueue _queue;
        private readonly IHttpClient _http;
        private readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        public RemoteSpriteLoader(IRequestQueue queue, IHttpClient http)
        {
            _queue = queue;
            _http = http;
        }

        public async UniTask<Sprite> LoadAsync(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            if (_cache.TryGetValue(url, out Sprite cached))
                return cached;

            Texture2D texture = await _queue.Enqueue(token => _http.GetTextureAsync(url, token), cancellationToken);

            // Пока ждали, эту же иконку мог загрузить другой вызов.
            if (_cache.TryGetValue(url, out cached))
            {
                Object.Destroy(texture);
                return cached;
            }

            var rect = new Rect(0f, 0f, texture.width, texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = url;
            _cache[url] = sprite;
            return sprite;
        }

        public void Dispose()
        {
            foreach (Sprite sprite in _cache.Values)
            {
                if (sprite == null)
                    continue;

                Object.Destroy(sprite.texture);
                Object.Destroy(sprite);
            }

            _cache.Clear();
        }
    }
}
