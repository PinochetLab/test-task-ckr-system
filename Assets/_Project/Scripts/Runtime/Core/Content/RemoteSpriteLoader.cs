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
    /// Загружает спрайты по url через общую очередь запросов и держит ограниченный кэш.
    /// Когда кэш переполнен, вытесняется спрайт, который дольше всех не запрашивали,
    /// а его текстура уничтожается. Спрайт на экране всегда самый свежий, поэтому вытеснение его не задевает.
    /// </summary>
    public sealed class RemoteSpriteLoader : ISpriteLoader, IDisposable
    {
        private readonly IRequestQueue _queue;
        private readonly IHttpClient _http;
        private readonly int _capacity;

        // Порядок использования: в начале — самый свежий, в конце — кандидат на вытеснение.
        private readonly LinkedList<KeyValuePair<string, Sprite>> _usage = new LinkedList<KeyValuePair<string, Sprite>>();
        private readonly Dictionary<string, LinkedListNode<KeyValuePair<string, Sprite>>> _cache =
            new Dictionary<string, LinkedListNode<KeyValuePair<string, Sprite>>>();

        public RemoteSpriteLoader(IRequestQueue queue, IHttpClient http, NetworkConfig config)
        {
            _queue = queue;
            _http = http;
            _capacity = Mathf.Max(1, config.SpriteCacheSize);
        }

        /// <summary>Сколько спрайтов сейчас в кэше.</summary>
        public int CachedCount => _cache.Count;

        public async UniTask<Sprite> LoadAsync(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            if (TryGetCached(url, out Sprite cached))
                return cached;

            Texture2D texture = await _queue.Enqueue(token => _http.GetTextureAsync(url, token), cancellationToken);

            // Пока ждали, эту же иконку мог загрузить другой вызов.
            if (TryGetCached(url, out cached))
            {
                DestroyObject(texture);
                return cached;
            }

            var rect = new Rect(0f, 0f, texture.width, texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = url;

            Add(url, sprite);
            return sprite;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<string, Sprite> entry in _usage)
                DestroySprite(entry.Value);

            _usage.Clear();
            _cache.Clear();
        }

        private bool TryGetCached(string url, out Sprite sprite)
        {
            if (!_cache.TryGetValue(url, out LinkedListNode<KeyValuePair<string, Sprite>> node))
            {
                sprite = null;
                return false;
            }

            _usage.Remove(node);
            _usage.AddFirst(node);
            sprite = node.Value.Value;
            return true;
        }

        private void Add(string url, Sprite sprite)
        {
            _cache[url] = _usage.AddFirst(new KeyValuePair<string, Sprite>(url, sprite));

            while (_cache.Count > _capacity)
            {
                LinkedListNode<KeyValuePair<string, Sprite>> oldest = _usage.Last;
                _usage.RemoveLast();
                _cache.Remove(oldest.Value.Key);
                DestroySprite(oldest.Value.Value);
            }
        }

        private static void DestroySprite(Sprite sprite)
        {
            if (sprite == null)
                return;

            DestroyObject(sprite.texture);
            DestroyObject(sprite);
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
                return;

            // Destroy работает только в Play Mode; в EditMode-тестах нужен DestroyImmediate.
            if (Application.isPlaying)
                Object.Destroy(target);
            else
                Object.DestroyImmediate(target);
        }
    }
}
