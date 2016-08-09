using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using BundtBot.Extensions;
using BundtBot.Utility;
using Discord;
using Discord.Audio;

namespace BundtBot.Sound {
    class SoundManager {
        internal bool HasThingsInQueue => _soundQueue.Count > 0;
        internal bool IsPlaying { get; private set; }
        internal TrackRequest CurrentlyPlayingTrackRequest { get; private set; }
        /// <summary> The voice channel that we are currently streaming to, if any. </summary>
        internal Channel VoiceChannel { get; private set; }
        public bool Shutdown { get; set; } = false;

        ConcurrentQueue<TrackRequest> _soundQueue = new ConcurrentQueue<TrackRequest>();
        readonly SoundStreamer _audioStreamer = new SoundStreamer();

        public SoundManager() {
            new Thread(async() => {
                while (true) {
                    if (Shutdown) break;

                    // Pick something from queue
                    if (_soundQueue.Count == 0) {
                        CurrentlyPlayingTrackRequest = null;
                        IsPlaying = false;
                        Thread.Sleep(100);
                        continue;
                    }

                    IsPlaying = true;

                    TrackRequest trackRequest;
                    var result = _soundQueue.TryDequeue(out trackRequest);
                    if (result == false) { continue; }
                    CurrentlyPlayingTrackRequest = trackRequest;

                    MyLogger.WriteLine("Connecting to voice channel:" + trackRequest.VoiceChannel.Name);
                    MyLogger.WriteLine("\tOn server:  " + trackRequest.VoiceChannel.Server.Name);
                    var audioService = trackRequest.VoiceChannel.Client.GetService<AudioService>();
                    var audioClient = await audioService.Join(trackRequest.VoiceChannel);
                    VoiceChannel = trackRequest.VoiceChannel;
                    if (trackRequest.TextUpdates) {
                        var volumeOverride = _audioStreamer.GetVolumeOverride();
                        if (volumeOverride > 0) {
                            await trackRequest.TextChannel.SendMessageEx($"Playing **{trackRequest.Track.Title}** at *Override Volume* **{volumeOverride * 10}**");
                        }
                        else {
                            await trackRequest.TextChannel.SendMessageEx($"Playing **{trackRequest.Track.Title}** at Volume **{trackRequest.Volume * 10}**");
                        }
                    }

                    _audioStreamer.PlaySound(audioService, audioClient, trackRequest);

                    if (trackRequest.DeleteAfterPlay) {
                        MyLogger.WriteLine("Deleting TrackRequest file: " + trackRequest.Track, ConsoleColor.Yellow);
                        File.Delete(trackRequest.Track.Path);
                    }

                    // Check if next TrackRequest is in same channel
                    TrackRequest nextTrackRequest;
                    if (_soundQueue.TryPeek(out nextTrackRequest)) {
                        if (nextTrackRequest.VoiceChannel == trackRequest.VoiceChannel) {
                            continue;
                        }
                    }
                    
                    await audioService.Leave(trackRequest.VoiceChannel);
                    VoiceChannel = null;
                    Thread.Sleep(250);
                }
            }).Start();
        }

        public async void EnqueueSound(TrackRequest trackRequest) {
            Message msg = null;
            if (trackRequest.TextUpdates) {
                msg = await trackRequest.TextChannel.SendMessageEx($"Adding **{trackRequest.Track.Title}** to the queue...");
            }
            _soundQueue.Enqueue(trackRequest);
            MyLogger.WriteLine("[SoundManager] TrackRequest queued: " + trackRequest.Track.Title);
            if (trackRequest.TextUpdates && msg != null) {
                await msg.Edit(msg.Text + "done!");
            }
        }

        internal void Stop() {
            _soundQueue = new ConcurrentQueue<TrackRequest>();
            _audioStreamer.Stop = true;
            MyLogger.WriteLine("[SoundManager] Stopped");
        }

        internal void Skip() {
            _audioStreamer.Stop = true;
            MyLogger.WriteLine("[SoundManager] Skipped");
        }

        internal void SetVolumeOfCurrentTrack(float desiredVolume) {
            _audioStreamer.SetVolumeOfCurrentTrack(desiredVolume);
            MyLogger.WriteLine("[SoundManager] Volume set to " + desiredVolume);
        }

        internal void SetVolumeOverride(float desiredVolume) {
            _audioStreamer.SetVolumeOverride(desiredVolume);
            MyLogger.WriteLine("[SoundManager] Volume Override set to " + desiredVolume);
        }

        internal void ClearVolumeOverride() {
            _audioStreamer.ClearVolumeOverride();
            MyLogger.WriteLine("[SoundManager] Volume Override cleared");
        }

        internal TrackRequest PeekNext() {
            TrackRequest trackRequest;
            _soundQueue.TryPeek(out trackRequest);
            return trackRequest;
        }
    }
}
