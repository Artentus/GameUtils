using System;
using System.IO;

namespace GameUtils.Audio
{
    public abstract class AudioEngine : IEngineComponent
    {
        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return !(component is AudioEngine);
        }

        public object Tag { get; set; }

        public abstract float Volume { get; set; }

        public abstract bool Muted { get; set; }

        public abstract int ChannelCount { get; }

        public abstract float GetChannelVolume(int channel);
        public abstract void SetChannelVolume(int channel, float value);

        protected internal abstract SoundEffectData CreateSoundEffect(Stream stream);
        protected internal abstract SoundEffectData CreateSoundEffect(string file);

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        { }

        ~AudioEngine()
        {
            Dispose(false);
        }
    }
}
