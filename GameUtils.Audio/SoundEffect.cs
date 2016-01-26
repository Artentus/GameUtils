using System;
using System.IO;

namespace GameUtils.Audio
{
    public sealed class SoundEffect : IDisposable
    {
        readonly SoundEffectData data;

        public TimeSpan Length
        {
            get { return data.Length; }
        }

        public TimeSpan Position
        {
            get { return data.Position; }
            set { data.Position = value; }
        }

        public float Volume
        {
            get { return data.Volume; }
            set { data.Volume = value; }
        }

        public PlaybackState State
        {
            get { return data.State; }
        }

        public SoundEffect(Stream stream)
        {
            try
            {
                data = GameEngine.QueryComponent<AudioEngine>().CreateSoundEffect(stream);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    "A sound effect can only be created if a sound engine ist registered.",
                    ex);
            }
        }

        public SoundEffect(string file)
        {
            try
            {
                data = GameEngine.QueryComponent<AudioEngine>().CreateSoundEffect(file);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    "A sound effect can only be created if a sound engine ist registered.",
                    ex);
            }
        }

        public void Play()
        {
            data.Play();
        }

        public void Pause()
        {
            data.Pause();
        }

        public void Stop()
        {
            data.Stop();
        }

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

        private void Dispose(bool disposing)
        {
            data.Dispose();
        }

        ~SoundEffect()
        {
            Dispose(false);
        }
    }
}
