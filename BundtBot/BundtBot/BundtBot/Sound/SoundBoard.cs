using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BundtBot.BundtBot.Utility;
using NString;

namespace BundtBot.BundtBot.Sound {
    class SoundBoard {
        readonly Random _random = new Random();

        const string BasePath = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
        const char Slash = '\\';

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

            soundFile = new FileInfo(BasePath + actorName + Slash + soundName + ".mp3");

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
            foreach (var arg in args) {
                if (arg.StartsWith("--length:") &&
                    arg.Length > 9) {
                    try {
                        sound.Length = (int)(float.Parse(arg.Substring(9)) * 1000);
                    } catch (Exception) {
                        throw new ArgumentException("badly formed length value");
                    }
                    if (sound.Length <= 0) {
                        throw new ArgumentException("invalid length value, must be a positive foat");
                    }
                    MyLogger.WriteLine("Parsed " + arg + " into " + sound.Length + " milliseconds");
                } else if ((arg.StartsWith("--volume:") &&
                    arg.Length > 9) ||
                    (arg.StartsWith("--vol:") &&
                    arg.Length > 6) ||
                    (arg.StartsWith("--v:") &&
                    arg.Length > 4)) {
                    try {
                        var intVolume = int.Parse(arg.Substring(9));
                        if (intVolume < 1 || intVolume > 11) {
                            throw new ArgumentException("invalid volume, must be an integer from 1 to 10");
                        }
                        sound.Volume = intVolume / 10f;
                        MyLogger.WriteLine("Parsed " + arg + " into " + sound.Volume);
                    } catch (Exception) {
                        throw new ArgumentException("badly formed volume value");
                    }
                } else if (arg.StartsWith("--echo")) {
                    sound.Echo = true;
                    if (arg == "--echo") {
                        MyLogger.WriteLine("Parsed " + arg);
                    } else {
                        var parts = arg.Split(':');
                        if (parts.Length <= 1) continue;
                        sound.EchoLength = (int)(float.Parse(parts[1]) * 1000);
                        if (sound.EchoLength <= 0 || sound.EchoLength > 50000) {
                            throw new ArgumentException("bad echo length");
                        }
                        if (parts.Length <= 2) continue;
                        sound.EchoFactor = (float)int.Parse(parts[2]) / 10;
                        if (sound.EchoFactor <= 0 || sound.EchoFactor > 1f) {
                            throw new ArgumentException("bad echo factor");
                        }
                    }
                } else if (arg.StartsWith("--reverb")) {
                    sound.Reverb = true;
                    MyLogger.WriteLine("Parsed " + arg);
                } else {
                    throw new ArgumentException("argument not found");
                }
            }
        }

        public static Tuple<string, string> ParseActorAndSoundNames(string actorAndSoundString) {
            var words = new List<string>(actorAndSoundString.Trim().Split(' '));

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (words.Count == 1 && words[0].IsNullOrWhiteSpace()) {
                words[0] = "#random";
                words.Add("#random");
            } else if (words.Count == 1) {
                words.Add("#random");
            } else if (words.Count > 2) {
                for (var i = 2; i < words.Count; i++) {
                    words[1] += " " + words[i];
                }
            }

            return Tuple.Create(words[0], words[1]);
        }
        
        /// <summary>
        /// Removes any args from <paramref name="unparsedParams"/> which start with <c>--</c>,
        /// and returns the args in a List.
        /// </summary>
        public static List<string> ExtractArgs(ref string unparsedParams) {
            var words = new List<string>(unparsedParams.Trim().Split(' '));
            words.Reverse();
            var args = words.TakeWhile(x => x.StartsWith("--")).ToList();
            words.RemoveRange(0, args.Count);
            words.Reverse();
            words = words.TakeWhile(x => !x.StartsWith("--")).ToList();
            var parsedParams = "";
            words.ForEach(x => parsedParams += x + " ");
            unparsedParams = parsedParams;
            return args;
        }

        /// <summary>Returns true if it found a match</summary>
        bool CheckActorName(ref string actorName) {
            var actorDirectories = Directory.GetDirectories(BasePath);

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

            foreach (var str in actorDirectories) {
                var score = ToolBox.Levenshtein(actorName, str);
                if (score >= bestScore) continue;
                bestScore = score;
                matchedCategory = str;
                if (bestScore == 0) {
                    break;
                }
            }

            const int highestScoreAllowed = 4;

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
        static bool CheckSoundName(ref string soundName, string actorName) {
            var soundNames = Directory.GetFiles(BasePath + actorName);

            if (soundNames.Length < 1) {
                throw new Exception("Expected at least one file in directory");
            }

            for (var i = 0; i < soundNames.Length; i++) {
                var origName = soundNames[i];
                var newName = origName.Substring(origName.LastIndexOf('\\') + 1);
                newName = newName.Substring(0, newName.LastIndexOf('.'));
                soundNames[i] = newName.ToLower();
            }

            // If Random
            if (soundName == "#random") {
                var rand = new Random();
                var num = rand.Next(0, soundNames.Length);
                soundName = soundNames[num];
                return true;
            }

            var bestScore = ToolBox.Levenshtein(soundName, soundNames[0]);
            var matchedSound = "";

            foreach (var str in soundNames) {
                var score = ToolBox.Levenshtein(soundName, str);
                if (score >= bestScore) continue;
                bestScore = score;
                matchedSound = str;
                if (bestScore == 0) {
                    break;
                }
            }

            const int highestScoreAllowed = 4;

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
