using System;
using System.IO;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;

namespace GameUtils.Audio.CsCore
{
    public class CsCoreEngine : AudioEngine
    {
        readonly MMDevice device;
        readonly AudioEndpointVolume endpointVolume;

        public override float Volume
        {
            get { return endpointVolume.MasterVolumeLevelScalar; }
            set { endpointVolume.MasterVolumeLevelScalar = value; }
        }

        public override bool Muted
        {
            get { return endpointVolume.GetMute(); }
            set { endpointVolume.SetMute(value, Guid.Empty); }
        }

        public override int ChannelCount
        {
            get { return (int)endpointVolume.ChannelCount; }
        }

        public override float GetChannelVolume(int channel)
        {
            return endpointVolume.GetChannelVolumeLevelScalar((uint)channel);
        }

        public override void SetChannelVolume(int channel, float value)
        {
            endpointVolume.SetChannelVolumeLevelScalar((uint)channel, value, Guid.Empty);
        }

        public CsCoreEngine()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            }
            endpointVolume = AudioEndpointVolume.FromDevice(device);
        }

        ISoundOut CreateSoundOut(ref IWaveSource source)
        {
            ISoundOut soundOut;
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                soundOut = new WasapiOut(true, AudioClientShareMode.Shared, 50);
            else
            {
                soundOut = new DirectSoundOut() { Latency = 100 };
                if (source.WaveFormat.BitsPerSample > 16)
                    source = source.ToSampleSource().ToWaveSource(16);
            }
            return soundOut;
        }

        protected internal override SoundEffectData CreateSoundEffect(Stream stream)
        {
            IWaveSource source = CodecFactory.Instance.GetCodec(null, stream);
            ISoundOut soundOut = this.CreateSoundOut(ref source);

            return new CsCoreData(soundOut, source);
        }

        protected internal override SoundEffectData CreateSoundEffect(string file)
        {
            IWaveSource source = CodecFactory.Instance.GetCodec(file);
            ISoundOut soundOut = this.CreateSoundOut(ref source);

            return new CsCoreData(soundOut, source);
        }

        protected override void Dispose(bool disposing)
        {
            endpointVolume.Dispose();
            device.Dispose();

            base.Dispose(disposing);
        }
    }
}
