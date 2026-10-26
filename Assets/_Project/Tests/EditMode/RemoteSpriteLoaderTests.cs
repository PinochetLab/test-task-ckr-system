using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestTask.Core.Content;
using TestTask.Core.Network;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestTask.Tests
{
    public sealed class RemoteSpriteLoaderTests
    {
        private const int CacheSize = 2;

        private sealed class ImmediateQueue : IRequestQueue
        {
            public int PendingCount => 0;
            public bool IsBusy => false;

            public UniTask<T> Enqueue<T>(Func<CancellationToken, UniTask<T>> request, CancellationToken cancellationToken)
            {
                return request(cancellationToken);
            }
        }

        private sealed class FakeHttpClient : IHttpClient
        {
            public List<string> RequestedUrls { get; } = new List<string>();
            public List<Texture2D> Textures { get; } = new List<Texture2D>();

            public UniTask<string> GetTextAsync(string url, CancellationToken cancellationToken)
            {
                throw new NotSupportedException();
            }

            public UniTask<Texture2D> GetTextureAsync(string url, CancellationToken cancellationToken)
            {
                RequestedUrls.Add(url);
                var texture = new Texture2D(2, 2);
                Textures.Add(texture);
                return UniTask.FromResult(texture);
            }
        }

        private NetworkConfig _config;
        private FakeHttpClient _http;
        private RemoteSpriteLoader _loader;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<NetworkConfig>();
            SetCacheSize(_config, CacheSize);
            _http = new FakeHttpClient();
            _loader = new RemoteSpriteLoader(new ImmediateQueue(), _http, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _loader.Dispose();
            Object.DestroyImmediate(_config);
        }

        private static void SetCacheSize(NetworkConfig config, int size)
        {
            // Поле приватное и сериализуемое — так же, как его выставил бы инспектор.
            typeof(NetworkConfig)
                .GetField("_spriteCacheSize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(config, size);
        }

        private Sprite Load(string url) => _loader.LoadAsync(url, CancellationToken.None).GetAwaiter().GetResult();

        [Test]
        public void SameUrl_IsLoadedOnce()
        {
            Sprite first = Load("a");
            Sprite second = Load("a");

            Assert.AreSame(first, second);
            CollectionAssert.AreEqual(new[] { "a" }, _http.RequestedUrls);
        }

        [Test]
        public void CacheOverflow_EvictsLeastRecentlyUsed_AndDestroysItsTexture()
        {
            Load("a");
            Load("b");
            Load("a");      // «a» снова свежая, кандидат на вытеснение теперь «b»
            Load("c");

            Assert.AreEqual(CacheSize, _loader.CachedCount);
            Assert.IsTrue(_http.Textures[1] == null, "Текстура вытесненной иконки должна быть уничтожена");
            Assert.IsTrue(_http.Textures[0] != null);
            Assert.IsTrue(_http.Textures[2] != null);

            Load("b");
            Assert.AreEqual(4, _http.RequestedUrls.Count, "Вытесненная иконка загружается заново");
        }

        [Test]
        public void Dispose_DestroysAllCachedTextures()
        {
            Load("a");
            Load("b");

            _loader.Dispose();

            Assert.AreEqual(0, _loader.CachedCount);
            Assert.IsTrue(_http.Textures[0] == null);
            Assert.IsTrue(_http.Textures[1] == null);
        }
    }
}
