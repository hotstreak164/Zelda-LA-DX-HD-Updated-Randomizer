using System;
using Microsoft.Xna.Framework.Audio;

namespace GBSPlayer
{
    public class CDynamicEffectInstance
    {
        private DynamicSoundEffectInstance _instance;

        public SoundState State => _instance.State;

        public CDynamicEffectInstance(int sampleRate)
        {
            _instance = new DynamicSoundEffectInstance(
                sampleRate,
                AudioChannels.Mono
            );
        }

        public int GetPendingBufferCount()
        {
            return _instance.PendingBufferCount;
        }

        public void Play()
        {
            _instance.Play();
        }

        public void Pause()
        {
            _instance.Pause();
        }

        public void Resume()
        {
            if (_instance.State == SoundState.Paused)
                _instance.Resume();
        }

        public void Stop()
        {
            _instance.Stop();
        }

        public void SetVolume(float volume)
        {
            _instance.Volume = volume;
        }

        public void SubmitBuffer(byte[] buffer, int offset, int count)
        {
            _instance.SubmitBuffer(buffer, offset, count);
        }
    }
}