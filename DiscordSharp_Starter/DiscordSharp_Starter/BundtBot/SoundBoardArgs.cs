using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp_Starter.BundtBot {
    class SoundBoardArgs {
        public string actorName;
        public string soundName;
        public string soundPath;
        public float volume;
        public float length_seconds;

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
                        throw new ArgumentException("badly formed argument value");
                    }
                    if (length_seconds <= 0) {
                        throw new ArgumentException("invalid argument value, must be a positive foat");
                    }
                    MyLogger.WriteLine("Parsed " + arg + " into " + length_seconds + " seconds");
                } else if (arg.StartsWith("--volume:") &&
                    arg.Length > 9) {
                    try {
                        volume = float.Parse(arg.Substring(9));
                    } catch (Exception) {
                        throw new ArgumentException("badly formed argument value");
                    }
                    if (volume <= 0 || volume > 1) {
                        throw new ArgumentException("invalid argument value, must be a positive foat (0 < volume <= 1)");
                    }
                    MyLogger.WriteLine("Parsed " + arg + " into " + volume);
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

            if (words.Count == 2 &&
                words[1] == "#random") {
                words.Add("#random");
            }

            if (words.Count < 3) {
                throw new Exception("Expected more than 2 parts in the command string");
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
