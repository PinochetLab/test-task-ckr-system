using TestTask.Core.Audio;
using TestTask.Features.Clicker.Config;
using UnityEngine;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// Эффекты нажатия кликера: звук и разлёт частиц из ParticleSystem на сцене.
    /// </summary>
    public sealed class ClickerEffectsPlayer
    {
        private readonly IAudioService _audio;
        private readonly ClickerConfig _config;

        public ClickerEffectsPlayer(IAudioService audio, ClickerConfig config)
        {
            _audio = audio;
            _config = config;
        }

        /// <param name="particles">Система частиц у кликабельного объекта: разлёт во все стороны.</param>
        public void PlayClick(ParticleSystem particles)
        {
            _audio.PlayOneShot(_config.ClickSound, _config.ClickVolume);

            if (particles != null && _config.ClickParticlesCount > 0)
                particles.Emit(_config.ClickParticlesCount);
        }
    }
}
