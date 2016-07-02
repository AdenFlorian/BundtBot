using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BundtBot.BundtBot {
    class SoundBoard {
        public bool locked = false;
        public bool stop = false;
        
        DiscordClient _client;
        Random _random = new Random();

        const string BASE_PATH = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
        const char SLASH = '\\';

        public SoundBoard(DiscordClient client) {
            _client = client;
        }

        public async Task Process(CommandEventArgs e, Sound sound) {
            await Process(e.Channel, e.User.VoiceChannel, sound);
        }

        public async Task Process(Channel textChannel, Channel voiceChannel, string actorName, string soundName) {
            var sound = new Sound {
                actorName = actorName,
                soundName = soundName
            };
            await Process(textChannel, voiceChannel, sound);
        }

        /// <summary>
        /// Processes the sound board args,
        /// plays the sound to the voice channel,
        /// and sends updates via the text channel
        /// </summary>
        public async Task Process(Channel textChannel, Channel voiceChannel, Sound sound) {
            if (textChannel == null) {
                textChannel = voiceChannel.Server.DefaultChannel;
                if (textChannel == null) {
                    Console.WriteLine("somebody broke me :(");
                    return;
                }
            }

            if (voiceChannel == null) {
                await textChannel.SendMessage("you need to be in a voice channel to hear me roar");
                return;
            }

            if (locked) {
                await textChannel.SendMessage("wait your turn...or if you want to be mean, use !stop");
                return;
            }

            locked = true;

            string soundFilePath = null;

            if (CheckActorName(ref sound.actorName, textChannel) == false) {
                await textChannel.SendMessage("these are not the sounds you're looking for...");
            }

            if (CheckSoundName(ref sound.soundName, sound.actorName, textChannel) == false) {
                await textChannel.SendMessage("these are not the sounds you're looking for...");
            }

            soundFilePath = BASE_PATH + sound.actorName + SLASH + sound.soundName + ".mp3";

            Console.Write("looking for " + soundFilePath + "\t");

            if (!File.Exists(soundFilePath)) {
                MyLogger.WriteLine("didn't find it...", ConsoleColor.Red);
                await textChannel.SendMessage("these are not the sounds you're looking for...");
                locked = false;
                return;
            }

            MyLogger.WriteLine("Found it!", ConsoleColor.Green);
            sound.soundPath = soundFilePath;

            MyLogger.WriteLine("Connecting to voice channel:" + voiceChannel.Name);
            MyLogger.WriteLine("\tOn server:  " + voiceChannel.Server.Name);
            var audioService = _client.GetService<AudioService>();
            var audioClient = await audioService.Join(voiceChannel);
            new AudioStreamer().PlaySound(audioService, audioClient, sound);
            await audioClient.Disconnect();
            locked = false;

            if (sound.deleteAfterPlay) {
                MyLogger.WriteLine("Deleting sound file: " + sound.soundPath, ConsoleColor.Yellow);
                File.Delete(soundFilePath);
            }
        }

        /// <summary>Returns true if it found a match</summary>
        bool CheckActorName(ref string actorName, Channel textChannel) {
            var actorDirectories = Directory.GetDirectories(BASE_PATH);

            if (actorDirectories.Length < 1) {
                throw new Exception("Expected at least one directory in directory");
            }

            actorDirectories = actorDirectories.Select(str => str.Substring(str.LastIndexOf('\\') + 1)).ToArray();

            if (actorName == "#random") {
                var num = _random.Next(0, actorDirectories.Length);
                actorName = actorDirectories[num];
                return true;
            }

            var bestScore = ToolBox.Levenshtein(actorName, actorDirectories[0]);
            var matchedCategory = "";

            foreach (string str in actorDirectories) {
                var score = ToolBox.Levenshtein(actorName, str);
                if (score < bestScore) {
                    bestScore = score;
                    matchedCategory = str;
                    if (bestScore == 0) {
                        break;
                    }
                }
            }

            var highestScoreAllowed = 4;

            if (bestScore > highestScoreAllowed) {
                // Score not good enough
                Console.WriteLine("Matching score not good enough");
                // no match
                return false;
            }

            if (bestScore > 0) {
                textChannel.SendMessage("i think you meant " + matchedCategory);
            }

            actorName = matchedCategory;
            return true;
        }

        /// <summary>Returns true if it found a match</summary>
        bool CheckSoundName(ref string soundName, string actorName, Channel textChannel) {
            var soundNames = Directory.GetFiles(BASE_PATH + actorName);

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
                var num = rand.Next(0, soundNames.Length);
                soundName = soundNames[num];
                return true;
            }

            var bestScore = ToolBox.Levenshtein(soundName, soundNames[0]);
            var matchedSound = "";

            foreach (string str in soundNames) {
                var score = ToolBox.Levenshtein(soundName, str);
                if (score < bestScore) {
                    bestScore = score;
                    matchedSound = str;
                    if (bestScore == 0) {
                        break;
                    }
                }
            }

            var highestScoreAllowed = 4;

            if (bestScore > highestScoreAllowed) {
                // Score not good enough
                Console.WriteLine("Matching score not good enough");
                // no match
                return false;
            }

            if (bestScore > 0) {
                textChannel.SendMessage("i think you meant " + matchedSound);
            }

            soundName = matchedSound;
            return true;
        }
    }
}
