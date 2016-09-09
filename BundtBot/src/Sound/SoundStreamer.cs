using System;
using BundtBot.Effects;
using BundtBot.Utility;
using Discord;
using Discord.Audio;
using NAudio.Wave;

namespace BundtBot.Sound {
    class SoundStreamer {
        public volatile bool Stop;

        volatile float _volume;
        volatile float _volumeOverride;

        public void SetVolumeOfCurrentTrack(float newVolume) {
            // Just an extra check to keep the bot from blowing people's ears out
            if (newVolume > 1.1f) {
                throw new ArgumentException("Volume should never be greater than 1!");
            }
            _volume = newVolume;
        }

        public float GetVolumeOverride() {
            return _volumeOverride;
        }

        public void SetVolumeOverride(float newVolume) {
            // Just an extra check to keep the bot from blowing people's ears out
            if (newVolume > 1.1f) {
                throw new ArgumentException("Volume should never be greater than 1!");
            }
            _volumeOverride = newVolume;
        }

        public void ClearVolumeOverride() {
            _volumeOverride = -1;
        }

        public void PlaySound(AudioService audioService, IAudioClient audioClient, TrackRequest trackRequest) {
            var channels = audioService.Config.Channels;
            var timePlayed = 0;
            var outFormat = new WaveFormat(48000, 16, channels);

            SetVolumeOfCurrentTrack(trackRequest.Volume);

            using (var audioFileStream = new MediaFoundationReader(trackRequest.Track.Path))
            using (var waveChannel32 = new WaveChannel32(audioFileStream, (_volumeOverride > 0 ? _volumeOverride : _volume) * 0.2f, 0f) { PadWithZeroes = false })
            using (var effectStream = new EffectStream(waveChannel32))
            using (var blockAlignmentStream = new BlockAlignReductionStream(effectStream))
            using (var resampler = new MediaFoundationResampler(blockAlignmentStream, outFormat)) {
                resampler.ResamplerQuality = 60;
                ApplyEffects(waveChannel32, effectStream, trackRequest);

                // Establish the size of our AudioBuffer
                var blockSize = outFormat.AverageBytesPerSecond / 50;
                var buffer = new byte[blockSize];
                int byteCount;

                // Read audio into our buffer, and keep a loop open while data is present
                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) {
                    waveChannel32.Volume = (_volumeOverride > 0 ? _volumeOverride : _volume) * 0.2f;

                    // Limit play length (--length)
                    timePlayed += byteCount * 1000 / outFormat.AverageBytesPerSecond;
                    if (trackRequest.TimeLimit > 0 && timePlayed > trackRequest.TimeLimit) {
                        break;
                    }

                    if (byteCount < blockSize) {
                        // Incomplete Frame
                        for (var i = byteCount; i < blockSize; i++) {
                            buffer[i] = 0;
                        }
                    }

                    if (audioClient.State == ConnectionState.Disconnected || Stop) {
                        break;
                    }

                    // Send the buffer to Discord
                    try {
                        audioClient.Send(buffer, 0, blockSize);
                    } catch (Exception) {
                        break;
                    }
                }
                MyLogger.WriteLine("Voice finished enqueuing", ConsoleColor.Green);
            }

            if (Stop) {
                audioClient.Clear();
                Stop = false;
            }

            audioClient.Wait();
        }

        static void ApplyEffects(IWaveProvider waveChannel32, EffectStream effectStream, TrackRequest trackRequest) {
            for (var i = 0; i < waveChannel32.WaveFormat.Channels; i++) {
                if (trackRequest.Echo) {
                    if (trackRequest.EchoLength > 0) {
                        if (trackRequest.EchoFactor > 0) {
                            effectStream.Effects.Add(new Echo(trackRequest.EchoLength, trackRequest.EchoFactor));
                        } else {
                            effectStream.Effects.Add(new Echo(trackRequest.EchoLength));
                        }
                    } else {
                        effectStream.Effects.Add(new Echo());
                    }
                } else if (trackRequest.Reverb) {
                    effectStream.Effects.Add(new Reverb());
                }
            }
        }
    }
}
