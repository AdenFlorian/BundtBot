using Discord;
using Discord.Audio;
using NAudio.Wave;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BundtBot.BundtBot {
    class AudioStreamer {
        public void PlaySound(AudioService audioService, IAudioClient audioClient, Sound sound) {
            string soundFilePath = sound.soundPath;
            int ms = audioService.Config.BufferLength;
            int channels = audioService.Config.Channels;
            int sampleRate = 48000;
            int waitTimeMS = 0;
            var outFormat = new WaveFormat(sampleRate, 16, channels);

            // Just an extra check to keep the bot from blowing people's ears out
            if (sound.volume > 1.1f) {
                throw new ArgumentException("Volume should never be greater than 1!");
            } else if (sound.volume == 0) {
                sound.volume = 1;
            }

            using (var audioFileStream = new MediaFoundationReader(soundFilePath))
            using (var waveChannel32 = new WaveChannel32(audioFileStream, sound.volume * 0.25f, 0f) { PadWithZeroes = false })
            using (var effectStream = new EffectStream(waveChannel32))
            using (var blockAlignmentStream = new BlockAlignReductionStream(effectStream))
            using (var resampler = new MediaFoundationResampler(blockAlignmentStream, outFormat)) {
                resampler.ResamplerQuality = 60;
                ApplyEffects(waveChannel32, effectStream, sound);

                // Establish the size of our AudioBuffer
                int blockSize = outFormat.AverageBytesPerSecond / 50;
                byte[] buffer = new byte[blockSize];
                int byteCount;

                // Read audio into our buffer, and keep a loop open while data is present
                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) {
                    // Limit play length (--length)
                    waitTimeMS += ms;
                    if (sound.length_ms > 0 && waitTimeMS > sound.length_ms) {
                        break;
                    }

                    if (byteCount < blockSize) {
                        // Incomplete Frame
                        for (int i = byteCount; i < blockSize; i++)
                            buffer[i] = 0;
                    }

                    if (audioClient.State == ConnectionState.Disconnected) {
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

            audioClient.Wait();
        }

        void ApplyEffects(WaveChannel32 waveChannel32, EffectStream effectStream, Sound sound) {
            for (int i = 0; i < waveChannel32.WaveFormat.Channels; i++) {
                if (sound.echo) {
                    if (sound.echoLength > 0) {
                        if (sound.echoFactor > 0) {
                            effectStream.Effects.Add(new Echo(sound.echoLength, sound.echoFactor));
                        } else {
                            effectStream.Effects.Add(new Echo(sound.echoLength));
                        }
                    } else {
                        effectStream.Effects.Add(new Echo());
                    }
                } else if (sound.reverb) {
                    effectStream.Effects.Add(new Reverb());
                }
            }
        }
    }
}
