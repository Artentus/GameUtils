using System;

namespace GameUtils.Audio
{
    public abstract class SoundEffectData : IDisposable
    {
        public abstract TimeSpan Length { get; }

        public abstract TimeSpan Position { get; set; }

        public abstract float Volume { get; set; }

        public abstract PlaybackState State { get; }

        public abstract void Play();

        public abstract void Pause();

        public abstract void Stop();

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

        ~SoundEffectData()
        {
            Dispose(false);
        }
    }
}
