using BundtBot.BundtBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundtBot.BundtBot {
    class SoundBoardArgs {
        #region Required
        public string actorName;
        public string soundName;
        public string soundPath;
        #endregion

        #region Optional
        public bool deleteAfterPlay = false;
        public bool reverb = false;
        public bool echo = false;
        public int echoLength = 0;
        public float echoFactor = 0;
        public float volume = 0f;
        public float length_seconds = 0f;
        #endregion

        public int length_ms {
            get { return (int)(length_seconds * 1000); }
        }

        public SoundBoardArgs() {

        }

        public SoundBoardArgs(string commandString) {
            // Command should have 3 words separated by spaces
            // 1. !owsb (or !sb)
            // 2. actor
            // 3. the sound name (can be multiple words)
            // So, if we split by spaces, we should have at least 3 parts
            commandString = commandString.Trim();
            List<string> words = new List<string>(commandString.Split(' '));

            // Filter out the arguments (words starting with '--')
            var args = ExtractArgs(words);

            if (args.Count() > 0) {
                ParseArgs(args);
            }

            args = null;
            
            var actorAndSoundNames = ParseActorAndSoundNames(words);

            actorName = actorAndSoundNames.Item1;
            soundName = actorAndSoundNames.Item2;
            
            if (words.Count > 3) {
                for (int i = 3; i < words.Count; i++) {
                    soundName += " " + words[i];
                }
            }
        }

        void ParseArgs(IEnumerable<string> args) {
            foreach (string arg in args) {
                if (arg.StartsWith("--length:") &&
                    arg.Length > 9) {
                    try {
                        length_seconds = float.Parse(arg.Substring(9));
                    } catch (Exception) {
                        throw new ArgumentException("badly formed length value");
                    }
                    if (length_seconds <= 0) {
                        throw new ArgumentException("invalid length value, must be a positive foat");
                    }
                    MyLogger.WriteLine("Parsed " + arg + " into " + length_seconds + " seconds");
                } else if (arg.StartsWith("--volume:") &&
                    arg.Length > 9) {
                    try {
                        var intVolume = int.Parse(arg.Substring(9));
                        if (intVolume < 1 || intVolume > 11) {
                            throw new ArgumentException("invalid volume, must be an integer from 1 to 10");
                        }
                        volume = (float)intVolume / 10f;
                        MyLogger.WriteLine("Parsed " + arg + " into " + volume);
                    } catch (Exception) {
                        throw new ArgumentException("badly formed volume value");
                    }
                } else if (arg.StartsWith("--echo")) {
                    echo = true;
                    if (arg == "--echo") {
                        MyLogger.WriteLine("Parsed " + arg);
                    } else {
                        var parts = arg.Split(':');
                        if (parts.Count() > 1) {
                            echoLength = (int)(float.Parse(parts[1]) * 1000);
                            if (echoLength <= 0 || echoLength > 50000) {
                                throw new ArgumentException("bad echo length");
                            }
                            if (parts.Count() > 2) {
                                echoFactor = (float)int.Parse(parts[2]) / 10;
                                if (echoFactor <= 0 || echoFactor > 1f) {
                                    throw new ArgumentException("bad echo factor");
                                }
                            }
                        }
                    }
                } else if (arg.StartsWith("--reverb")) {
                    reverb = true;
                    MyLogger.WriteLine("Parsed " + arg);
                } else {
                    throw new ArgumentException("argument not found");
                }
            }
        }

        static Tuple<string, string> ParseActorAndSoundNames(List<string> words) {
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

        static List<string> ExtractArgs(List<string> words) {
            words.Reverse();
            var args = words.TakeWhile(x => x.StartsWith("--")).ToList();
            words.RemoveRange(0, args.Count());
            words.Reverse();
            words = words.TakeWhile(x => !x.StartsWith("--")).ToList();
            return args;
        }
    }
}
