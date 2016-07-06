using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BundtBot.BundtBot {
    class SoundBoard {
        Random _random = new Random();

        const string BASE_PATH = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
        const char SLASH = '\\';

        /// <summary>
        /// Gets the path to a sound file by actor and sound names.
        /// </summary>
        public bool TryGetSoundPath(string actorName, string soundName, out FileInfo soundFile) {

            soundFile = null;

            if (CheckActorName(ref actorName) == false) {
                return false;
            }

            if (CheckSoundName(ref soundName, actorName) == false) {
                return false;
            }

            soundFile = new FileInfo(BASE_PATH + actorName + SLASH + soundName + ".mp3");

            Console.Write("looking for " + soundFile.FullName + "\t");

            if (soundFile.Exists == false) {
                MyLogger.WriteLine("didn't find it...", ConsoleColor.Red);
                soundFile = null;
                return false;
            }

            MyLogger.WriteLine("Found it!", ConsoleColor.Green);
            return true;
        }
        
        public static void ParseArgs(IEnumerable<string> args, ref Sound sound) {
            foreach (string arg in args) {
                if (arg.StartsWith("--length:") &&
                    arg.Length > 9) {
                    try {
                        sound.length_seconds = float.Parse(arg.Substring(9));
                    } catch (Exception) {
                        throw new ArgumentException("badly formed length value");
                    }
                    if (sound.length_seconds <= 0) {
                        throw new ArgumentException("invalid length value, must be a positive foat");
                    }
                    MyLogger.WriteLine("Parsed " + arg + " into " + sound.length_seconds + " seconds");
                } else if (arg.StartsWith("--volume:") &&
                    arg.Length > 9) {
                    try {
                        var intVolume = int.Parse(arg.Substring(9));
                        if (intVolume < 1 || intVolume > 11) {
                            throw new ArgumentException("invalid volume, must be an integer from 1 to 10");
                        }
                        sound.volume = (float)intVolume / 10f;
                        MyLogger.WriteLine("Parsed " + arg + " into " + sound.volume);
                    } catch (Exception) {
                        throw new ArgumentException("badly formed volume value");
                    }
                } else if (arg.StartsWith("--echo")) {
                    sound.echo = true;
                    if (arg == "--echo") {
                        MyLogger.WriteLine("Parsed " + arg);
                    } else {
                        var parts = arg.Split(':');
                        if (parts.Count() > 1) {
                            sound.echoLength = (int)(float.Parse(parts[1]) * 1000);
                            if (sound.echoLength <= 0 || sound.echoLength > 50000) {
                                throw new ArgumentException("bad echo length");
                            }
                            if (parts.Count() > 2) {
                                sound.echoFactor = (float)int.Parse(parts[2]) / 10;
                                if (sound.echoFactor <= 0 || sound.echoFactor > 1f) {
                                    throw new ArgumentException("bad echo factor");
                                }
                            }
                        }
                    }
                } else if (arg.StartsWith("--reverb")) {
                    sound.reverb = true;
                    MyLogger.WriteLine("Parsed " + arg);
                } else {
                    throw new ArgumentException("argument not found");
                }
            }
        }

        public static Tuple<string, string> ParseActorAndSoundNames(List<string> words) {
            if (words.Count == 1 &&
                words[0] == "!sb") {
                words.Add("#random");
                words.Add("#random");
            }

            if (words.Count == 2) {
                words.Add("#random");
            }

            return Tuple.Create(words[1], words[2]);
        }
        
        public static List<string> ExtractArgs(List<string> words) {
            words.Reverse();
            var args = words.TakeWhile(x => x.StartsWith("--")).ToList();
            words.RemoveRange(0, args.Count());
            words.Reverse();
            words = words.TakeWhile(x => !x.StartsWith("--")).ToList();
            return args;
        }

        /// <summary>Returns true if it found a match</summary>
        bool CheckActorName(ref string actorName) {
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
                //textChannel.SendMessage("i think you meant " + matchedCategory);
            }

            actorName = matchedCategory;
            return true;
        }

        /// <summary>Returns true if it found a match</summary>
        bool CheckSoundName(ref string soundName, string actorName) {
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
                //textChannel.SendMessage("i think you meant " + matchedSound);
            }

            soundName = matchedSound;
            return true;
        }
    }
}
