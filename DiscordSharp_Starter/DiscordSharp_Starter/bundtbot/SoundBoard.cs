using DiscordSharp;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.IO;
using System.Linq;

namespace DiscordSharp_Starter.bundtbot {
    class SoundBoard {

        public bool locked = false;
        public string nextSoundPath { get; private set; } = null;

        DiscordChannel lastChannel = null;

        public void Process(DiscordClient client, DiscordMessageEventArgs eventArgs, string actor, string soundName) {
            if (locked) {
                eventArgs.Channel.SendMessage("wait your turn...");
                return;
            }

            lastChannel = eventArgs.Channel;
            DiscordMember author = eventArgs.Author;
            DiscordChannel channel = author.CurrentVoiceChannel;

            if (channel == null) {
                eventArgs.Channel.SendMessage("you need to be in a voice channel to hear me roar");
                return;
            }

            string soundFilePath = null;
            var basePath = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
            var slash = '\\';

            // Check category
            {
                var categories = Directory.GetDirectories(basePath);

                if (categories.Length < 1) {
                    throw new Exception("Expected at least one directory in directory");
                }

                categories = categories.Select(str => str.Substring(str.LastIndexOf('\\') + 1)).ToArray();

                if (actor == "#random") {
                    Random rand = new Random();
                    var num = rand.Next(0, categories.Length - 1);
                    actor = categories[num];
                } else {
                    var bestScore = ToolBox.Compute(actor, categories[0]);
                    var matchedCategory = "";

                    foreach (string str in categories) {
                        var score = ToolBox.Compute(actor, str);
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

                    actor = matchedCategory;
                }
            }
            
            // Check name
            {
                var soundNames = Directory.GetFiles(basePath + actor);

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




            soundFilePath = basePath + actor + slash + soundName + ".mp3";

            Console.WriteLine("looking for " + soundFilePath);

            if (!File.Exists(soundFilePath)) {
                Console.WriteLine("didn't find it...");
                lastChannel.SendMessage("these are not the sounds you're looking for...");
                client.DisconnectFromVoice();
                locked = false;
                return;
            }
            Console.WriteLine("Found it!");
            nextSoundPath = soundFilePath;

            DiscordVoiceConfig voiceConfig = null;
            bool clientMuted = false;
            bool clientDeaf = false;
            client.ConnectToVoiceChannel(channel, voiceConfig, clientMuted, clientDeaf);
            locked = true;
        }
    }
}
