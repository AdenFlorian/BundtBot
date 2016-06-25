using DiscordSharp;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DiscordSharp_Starter.BundtBot {
    class SoundBoard {

        public bool locked = false;
        SoundBoardArgs nextSound;

        DiscordChannel lastChannel = null;
        DiscordClient client;
        Random random = new Random();

        string basePath = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
        char slash = '\\';

        public SoundBoard(DiscordClient client) {
            this.client = client;
        }

        public void Process(DiscordMessageEventArgs eventArgs, SoundBoardArgs soundBoardArgs) {
            Process(eventArgs.Channel, eventArgs.Author.CurrentVoiceChannel, soundBoardArgs);
        }

        public void Process(DiscordChannel textChannel, DiscordChannel voiceChannel, string actorName, string soundName) {
            var soundBoardArgs = new SoundBoardArgs {
                actorName = actorName,
                soundName = soundName
            };
            Process(textChannel, voiceChannel, soundBoardArgs);
        }

        public void Process(DiscordChannel textChannel, DiscordChannel voiceChannel, SoundBoardArgs soundBoardArgs) {
            if (textChannel == null) {
                textChannel = voiceChannel.Parent.Channels.First(x => x.Type == ChannelType.Text);
                if (textChannel == null) {
                    Console.WriteLine("somebody broke me :(");
                    return;
                }
            }

            if (locked) {
                textChannel.SendMessage("wait your turn...");
                return;
            }

            lastChannel = textChannel;

            if (voiceChannel == null) {
                textChannel.SendMessage("you need to be in a voice channel to hear me roar");
                return;
            }

            string soundFilePath = null;

            CheckActorName(ref soundBoardArgs.actorName);

            CheckSoundName(ref soundBoardArgs.soundName, soundBoardArgs.actorName);
            
            soundFilePath = basePath + soundBoardArgs.actorName + slash + soundBoardArgs.soundName + ".mp3";

            Console.Write("looking for " + soundFilePath + "\t");

            if (!File.Exists(soundFilePath)) {
                MyLogger.WriteLine("didn't find it...", ConsoleColor.Red);
                lastChannel.SendMessage("these are not the sounds you're looking for...");
                client.DisconnectFromVoice();
                locked = false;
                return;
            }

            MyLogger.WriteLine("Found it!", ConsoleColor.Green);
            soundBoardArgs.soundPath = soundFilePath;
            nextSound = soundBoardArgs;

            DiscordVoiceConfig voiceConfig = null;
            bool clientMuted = false;
            bool clientDeaf = false;
            client.ConnectToVoiceChannel(voiceChannel, voiceConfig, clientMuted, clientDeaf);
            locked = true;
        }

        public void OnConnectedToVoiceChannel(DiscordVoiceClient voiceClient) {

            string soundFilePath = nextSound.soundPath;

            int ms = voiceClient.VoiceConfig.FrameLengthMs;
            int channels = 1;
            int sampleRate = 48000;
            int waitTimeMS = 0;

            int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
            byte[] buffer = new byte[blockSize];
            var outFormat = new WaveFormat(sampleRate, 16, channels);
            voiceClient.SetSpeaking(true);
            using (var mp3Reader = new MediaFoundationReader(soundFilePath)) {
                // Just an extra check to keep the bot from blowing people ears out
                if (nextSound.volume > 1) {
                    throw new ArgumentException("Voluem should never be greater than 1!");
                }
                using (var waveChannel32 = new WaveChannel32(mp3Reader, nextSound.volume, 0f) { PadWithZeroes = false }) {
                    using (var resampler = new MediaFoundationResampler(waveChannel32, outFormat) { ResamplerQuality = 60 }) {
                        int byteCount;
                        while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) {
                            waitTimeMS += ms;

                            // Limit sound length
                            if (nextSound.length_ms > 0 &&
                                waitTimeMS > nextSound.length_ms) {
                                break;
                            }
                            if (voiceClient.Connected) {
                                voiceClient.SendVoice(buffer);
                            } else
                                break;
                        }
                        MyLogger.WriteLine("Voice finished enqueuing", ConsoleColor.Yellow);
                        resampler.Dispose();
                        mp3Reader.Close();
                    }
                }
            }

            int paddingMS = 1000;
            var totalWaitTimeMS = waitTimeMS + paddingMS;

            MyLogger.WriteLine("Waiting for " + totalWaitTimeMS + "ms");
            Thread.Sleep(totalWaitTimeMS);
            client.DisconnectFromVoice();
            locked = false;
        }

        void CheckActorName(ref string actorName) {
            var actorDirectories = Directory.GetDirectories(basePath);

            if (actorDirectories.Length < 1) {
                throw new Exception("Expected at least one directory in directory");
            }

            actorDirectories = actorDirectories.Select(str => str.Substring(str.LastIndexOf('\\') + 1)).ToArray();

            if (actorName == "#random") {
                var num = random.Next(0, actorDirectories.Length - 1);
                actorName = actorDirectories[num];
            } else {
                var bestScore = ToolBox.Compute(actorName, actorDirectories[0]);
                var matchedCategory = "";

                foreach (string str in actorDirectories) {
                    var score = ToolBox.Compute(actorName, str);
                    if (score < bestScore) {
                        bestScore = score;
                        matchedCategory = str;
                        if (bestScore == 0) {
                            break;
                        }
                    }
                }

                var highestScoreAllowed = 5;

                if (bestScore > highestScoreAllowed) {
                    // Score not good enough
                    Console.WriteLine("Matching score not good enough");
                    // no match
                    lastChannel.SendMessage("these are not the sounds you're looking for...");
                    client.DisconnectFromVoice();
                    locked = false;
                    return;
                }

                if (bestScore > 0) {
                    lastChannel.SendMessage("i think you meant " + matchedCategory);
                }

                actorName = matchedCategory;
            }
        }

        void CheckSoundName(ref string soundName, string actorName) {
            var soundNames = Directory.GetFiles(basePath + actorName);

            if (soundNames.Length < 1) {
                throw new Exception("Expected at least one file in directory");
            }

            for (int i = 0; i < soundNames.Length; i++) {
                var newName = "";
                var origName = soundNames[i];
                newName = origName.Substring(origName.LastIndexOf('\\') + 1);
                newName = newName.Substring(0, newName.LastIndexOf('.'));
                soundNames[i] = newName;
            }

            // If Random
            if (soundName == "#random") {
                Random rand = new Random();
                var num = rand.Next(0, soundNames.Length - 1);
                soundName = soundNames[num];
            } else {
                var bestScore = ToolBox.Compute(soundName, soundNames[0]);
                var matchedSound = "";

                foreach (string str in soundNames) {
                    var score = ToolBox.Compute(soundName, str);
                    if (score < bestScore) {
                        bestScore = score;
                        matchedSound = str;
                        if (bestScore == 0) {
                            break;
                        }
                    }
                }

                var highestScoreAllowed = 5;

                if (bestScore > highestScoreAllowed) {
                    // Score not good enough
                    Console.WriteLine("Matching score not good enough");
                    // no match
                    lastChannel.SendMessage("these are not the sounds you're looking for...");
                    client.DisconnectFromVoice();
                    locked = false;
                    return;
                }

                if (bestScore > 0) {
                    lastChannel.SendMessage("i think you meant " + matchedSound);
                }

                soundName = matchedSound;
            }
        }
    }
}
