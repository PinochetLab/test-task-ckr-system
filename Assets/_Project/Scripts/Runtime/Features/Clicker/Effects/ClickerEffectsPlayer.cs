using TestTask.Core.Audio;
using TestTask.Features.Clicker.Config;
using UnityEngine;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// Собирает визуальные и звуковые эффекты кликера в одном месте.
    /// </summary>
    public sealed class ClickerEffectsPlayer
    {
        private const float CoinSpawnSpread = 60f;
        private const float CoinDelayStep = 0.05f;
        private const float ParticleDistanceMin = 120f;
        private const float ParticleDistanceMax = 260f;

        private static readonly Color[] ParticleColors =
        {
            new Color(1f, 0.82f, 0.2f),
            new Color(1f, 0.6f, 0.2f),
            Color.white
        };

        private readonly FlyingCurrencyView.Pool _coinPool;
        private readonly UiParticleView.Pool _particlePool;
        private readonly IAudioService _audio;
        private readonly ClickerConfig _config;

        public ClickerEffectsPlayer(
            FlyingCurrencyView.Pool coinPool,
            UiParticleView.Pool particlePool,
            IAudioService audio,
            ClickerConfig config)
        {
            _coinPool = coinPool;
            _particlePool = particlePool;
            _audio = audio;
            _config = config;
        }

        public void PlayClick(Vector3 origin, Vector3 currencyTarget)
        {
            _audio.PlayOneShot(_config.ClickSound, _config.ClickVolume);
            SpawnParticles(origin, _config.ClickParticlesCount);
            SpawnCoins(origin, currencyTarget, _config.FlyingCurrencyCount);
        }

        public void PlayAutoCollect(Vector3 origin, Vector3 currencyTarget)
        {
            SpawnCoins(origin, currencyTarget, 1);
        }

        private void SpawnParticles(Vector3 origin, int count)
        {
            for (int i = 0; i < count; i++)
            {
                UiParticleView particle = _particlePool.Spawn();
                particle.transform.SetAsLastSibling();

                Vector2 direction = Random.insideUnitCircle;
                if (direction.sqrMagnitude < 0.01f)
                    direction = Vector2.up;

                float distance = Random.Range(ParticleDistanceMin, ParticleDistanceMax);
                Color color = ParticleColors[Random.Range(0, ParticleColors.Length)];

                particle.Burst(origin, direction, distance, color, () => _particlePool.Despawn(particle));
            }
        }

        private void SpawnCoins(Vector3 origin, Vector3 target, int count)
        {
            for (int i = 0; i < count; i++)
            {
                FlyingCurrencyView coin = _coinPool.Spawn();
                coin.transform.SetAsLastSibling();

                float scale = coin.transform.parent != null ? coin.transform.parent.lossyScale.x : 1f;
                Vector3 start = origin + (Vector3)(Random.insideUnitCircle * CoinSpawnSpread * scale);

                coin.Fly(start, target, i * CoinDelayStep, () => _coinPool.Despawn(coin));
            }
        }
    }
}
