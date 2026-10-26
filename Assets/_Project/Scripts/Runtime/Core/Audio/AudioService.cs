using UnityEngine;

namespace TestTask.Core.Audio
{
    public sealed class AudioService : IAudioService
    {
        private readonly AudioSource _source;

        public AudioService(AudioSource source)
        {
            _source = source;
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _source == null)
                return;

            _source.pitch = Random.Range(0.95f, 1.05f);
            _source.PlayOneShot(clip, volume);
        }
    }
}
