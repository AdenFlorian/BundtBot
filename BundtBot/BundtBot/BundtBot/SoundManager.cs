using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace BundtBot.BundtBot {
    class SoundManager {
        internal bool HasThingsInQueue { get { return _soundQueue.Count > 0; } }
        internal bool isPlaying { get; private set; }

        ConcurrentQueue<Sound> _soundQueue = new ConcurrentQueue<Sound>();
        AudioStreamer _audioStreamer = new AudioStreamer();

        public SoundManager() {
            new Thread(async() => {
                while (true) {
                    // Pick something from queue
                    if (_soundQueue.Count == 0) {
                        isPlaying = false;
                        Thread.Sleep(100);
                        continue;
                    }

                    isPlaying = true;

                    Sound sound;
                    var result = _soundQueue.TryDequeue(out sound);
                    if (result == false) { continue; }

                    MyLogger.WriteLine("Connecting to voice channel:" + sound.voiceChannel.Name);
                    MyLogger.WriteLine("\tOn server:  " + sound.voiceChannel.Server.Name);
                    var audioService = sound.voiceChannel.Client.GetService<AudioService>();
                    var audioClient = await audioService.Join(sound.voiceChannel);

                    _audioStreamer.PlaySound(audioService, audioClient, sound);

                    if (sound.deleteAfterPlay) {
                        MyLogger.WriteLine("Deleting sound file: " + sound.soundPath, ConsoleColor.Yellow);
                        File.Delete(sound.soundPath);
                    }

                    // Check if next sound is in same channel
                    Sound nextSound;
                    if (_soundQueue.TryPeek(out nextSound)) {
                        if (nextSound.voiceChannel == sound.voiceChannel) {
                            continue;
                        }
                    }
                    
                    await audioService.Leave(sound.voiceChannel);
                    Thread.Sleep(250);
                }
            }).Start();
        }

        public async void EnqueueSound(Sound sound, bool sendTextUpdates = true) {
            Message msg = null;
            if (sendTextUpdates) {
                msg = await sound.textChannel.SendMessage("Adding sound to the queue...");
            }
            _soundQueue.Enqueue(sound);
            if (sendTextUpdates) {
                await msg.Edit(msg.Text + "done!");
            }
        }

        internal void Stop() {
            _soundQueue = new ConcurrentQueue<Sound>();
            _audioStreamer.stop = true;
        }

        internal void Skip() {
            _audioStreamer.stop = true;
        }
    }
}
