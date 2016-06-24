using DiscordSharp;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiscordSharp_Starter.BundtBot {
    class SoundBoard {

        public bool locked = false;
        public string nextSoundPath { get; private set; } = null;

        DiscordChannel lastChannel = null;
        DiscordClient client;
        Random random = new Random();

        string basePath = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
        char slash = '\\';

        public SoundBoard(DiscordClient client) {
            this.client = client;
        }

        public void Process(DiscordMessageEventArgs eventArgs, string actorName, string soundName, IEnumerable<string> args = null) {
            Process(eventArgs.Channel, eventArgs.Author.CurrentVoiceChannel, actorName, soundName, args);
        }

        public void Process(DiscordChannel textChannel, DiscordChannel voiceChannel, string actorName, string soundName, IEnumerable<string> args = null) {
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

            CheckActorName(ref actorName);

            CheckSoundName(ref soundName, actorName);
            
            soundFilePath = basePath + actorName + slash + soundName + ".mp3";

            Console.Write("looking for " + soundFilePath);

            if (!File.Exists(soundFilePath)) {
                ConsoleColored.WriteLine("didn't find it...", ConsoleColor.Red);
                lastChannel.SendMessage("these are not the sounds you're looking for...");
                client.DisconnectFromVoice();
                locked = false;
                return;
            }
            ConsoleColored.WriteLine("Found it!", ConsoleColor.Green);
            nextSoundPath = soundFilePath;

            DiscordVoiceConfig voiceConfig = null;
            bool clientMuted = false;
            bool clientDeaf = false;
            client.ConnectToVoiceChannel(voiceChannel, voiceConfig, clientMuted, clientDeaf);
            locked = true;
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
