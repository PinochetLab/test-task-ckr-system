using TestTask.Core.Audio;
using TestTask.Features.Clicker.Config;

namespace TestTask.Features.Clicker.Effects
{
    /// <summary>
    /// Звук нажатия кликера. Визуальные эффекты (пульс, частицы) живут во view.
    /// </summary>
    public sealed class ClickerEffectsPlayer : IClickerEffects
    {
        private readonly IAudioService _audio;
        private readonly ClickerConfig _config;

        public ClickerEffectsPlayer(IAudioService audio, ClickerConfig config)
        {
            _audio = audio;
            _config = config;
        }

        public void PlayClickSound()
        {
            _audio.PlayOneShot(_config.ClickSound, _config.ClickVolume);
        }
    }
}
