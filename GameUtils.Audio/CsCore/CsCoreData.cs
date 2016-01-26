using System;
using CSCore;
using CSCore.SoundOut;
using sOut = CSCore.SoundOut;

namespace GameUtils.Audio.CsCore
{
    internal class CsCoreData : SoundEffectData
    {
        readonly ISoundOut soundOut;
        readonly IWaveSource source;
        readonly object locker;

        public override TimeSpan Length
        {
            get { return source.GetLength(); }
        }

        public override TimeSpan Position
        {
            get { return source.GetPosition(); }
            set { source.SetPosition(value); }
        }

        public override float Volume
        {
            get { return soundOut.Volume; }
            set { soundOut.Volume = value; }
        }

        public override PlaybackState State
        {
            get { return (PlaybackState)soundOut.PlaybackState; }
        }

        internal CsCoreData(ISoundOut soundOut, IWaveSource source)
        {
            this.soundOut = soundOut;
            this.source = source;
            locker = new object();

            soundOut.Initialize(source);
        }

        public override void Play()
        {
            lock (locker)
            {
                switch (soundOut.PlaybackState)
                {
                    case sOut.PlaybackState.Stopped:
                        source.Position = 0;
                        soundOut.Play();
                        break;
                    case sOut.PlaybackState.Paused:
                        soundOut.Resume();
                        break;
                    case sOut.PlaybackState.Playing:
                        soundOut.Stop();
                        source.Position = 0;
                        soundOut.Play();
                        break;
                }
            }
        }

        public override void Pause()
        {
            lock (locker)
            {
                soundOut.Pause();
            }
        }

        public override void Stop()
        {
            lock (locker)
            {
                soundOut.Stop();
            }
        }

        protected override void Dispose(bool disposing)
        {
            soundOut.Dispose();
            source.Dispose();
        }
    }
}
