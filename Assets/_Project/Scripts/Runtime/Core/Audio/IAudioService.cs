using UnityEngine;

namespace TestTask.Core.Audio
{
    public interface IAudioService
    {
        void PlayOneShot(AudioClip clip, float volume = 1f);
    }
}
