using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace BundtBot.Effects {
    public class EffectStream : WaveStream {
        public WaveStream SourceStream { get; set; }

        public List<IEffect> Effects { get; } = new List<IEffect>();

        public EffectStream(WaveStream sourceStream) {
            SourceStream = sourceStream;
        }

        public override long Length => SourceStream.Length;

        public override long Position {
            get { return SourceStream.Position; }
            set { SourceStream.Position = value; }
        }

        public override WaveFormat WaveFormat => SourceStream.WaveFormat;

        int _channel;

        public override int Read(byte[] buffer, int offset, int count) {
            var read = SourceStream.Read(buffer, offset, count);

            for (var i = 0; i < read / 4; i++) {
                var sample = BitConverter.ToSingle(buffer, i * 4);

                if (Effects.Count == WaveFormat.Channels) {
                    sample = Effects[_channel].ApplyEffect(sample);
                    _channel = (_channel + 1) % WaveFormat.Channels;
                }

                var bytes = BitConverter.GetBytes(sample);
                //bytes.CopyTo(buffer, i * 4);
                buffer[i * 4 + 0] = bytes[0];
                buffer[i * 4 + 1] = bytes[1];
                buffer[i * 4 + 2] = bytes[2];
                buffer[i * 4 + 3] = bytes[3];
            }

            return read;
        }
    }
}
