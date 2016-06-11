using DiscordSharp;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace DiscordSharp_Starter {
    class MessageReceivedProcessor {

        public bool soundboardLocked = false;
        public string nextSoundPath { get; private set; } = null;

        private DiscordChannel lastChannel = null;
        private string desiredSoundName = null;

        public void ProcessMessage(DiscordClient client, object sender, DiscordMessageEventArgs eventArgs) {
            if (eventArgs.MessageText == "!admin") {
                bool isadmin = false;
                List<DiscordRole> roles = eventArgs.Author.Roles;
                foreach (DiscordRole role in roles) {
                    if (role.Name.Contains("Administrator")) {
                        isadmin = true;
                    }
                }
                if (isadmin) {
                    eventArgs.Channel.SendMessage("Yes, you are! :D");
                } else {
                    eventArgs.Channel.SendMessage("No, you aren't :c");
                }
            }
            if (eventArgs.MessageText == "!mod") {
                bool ismod = false;
                List<DiscordRole> roles = eventArgs.Author.Roles;
                foreach (DiscordRole role in roles) {
                    if (role.Name.Contains("mod")) {
                        ismod = true;
                    }
                }
                if (ismod) {
                    eventArgs.Channel.SendMessage("Yes, you are! :D");
                } else {
                    eventArgs.Channel.SendMessage("No, you aren't D:");
                }
            }
            if (eventArgs.MessageText == "!help") {
                eventArgs.Channel.SendMessage("!owsb <character name> <phrase>");
                eventArgs.Channel.SendMessage("created by @AdenFlorian");
                eventArgs.Channel.SendMessage("https://github.com/AdenFlorian/DiscordSharp_Starter");
                eventArgs.Channel.SendMessage("https://trello.com/b/VKqUgzwV/bundtbot#");
            }
            if (eventArgs.MessageText == "!cat") {
                Random rand = new Random();
                if (rand.NextDouble() >= 0.5) {
                    string s;
                    using (WebClient webclient = new WebClient()) {
                        s = webclient.DownloadString("http://random.cat/meow");
                        int pFrom = s.IndexOf("\\/i\\/") + "\\/i\\/".Length;
                        int pTo = s.LastIndexOf("\"}");
                        string cat = s.Substring(pFrom, pTo - pFrom);
                        Console.WriteLine("http://random.cat/i/" + cat);
                        eventArgs.Channel.SendMessage("I found a cat\nhttp://random.cat/i/" + cat);
                    }
                } else {
                    Dog(eventArgs, "how about a dog instead");
                }
            }
            if (eventArgs.MessageText == "!dog") {
                Dog(eventArgs, "i found a dog");
            }
            if (eventArgs.MessageText == "!stop") {
                if (client.GetVoiceClient() == null) {
                    eventArgs.Channel.SendMessage("stop what?");
                } else if (client.GetVoiceClient().Connected == false) {
                    eventArgs.Channel.SendMessage("stop what?");
                } else {
                    eventArgs.Channel.SendMessage("okay... :disappointed_relieved:");
                    client.DisconnectFromVoice();
                }
            }
            if (eventArgs.MessageText.StartsWith("!owsb ")) {
                if (eventArgs.MessageText.Length <= 8) {
                    eventArgs.Channel.SendMessage("you're doing it wrong");
                    return;
                }
                if (soundboardLocked) {
                    eventArgs.Channel.SendMessage("wait your turn...");
                    return;
                }
                lastChannel = eventArgs.Channel;
                var soundByteName = eventArgs.MessageText.Substring(6);
                desiredSoundName = soundByteName;

                DiscordMember author = eventArgs.Author;
                DiscordChannel channel = author.CurrentVoiceChannel;

                if (channel == null) {
                    eventArgs.Channel.SendMessage("you need to be in a voice channel to hear me roar");
                    return;
                }

                string soundFilePath = null;

                var spaceIndex = desiredSoundName.IndexOf(" ");
                if (spaceIndex < 1) {
                    lastChannel.SendMessage("you're doing it wrong");
                    client.DisconnectFromVoice();
                    soundboardLocked = false;
                    return;
                }
                var category = desiredSoundName.Substring(0, desiredSoundName.IndexOf(" "));
                var name = desiredSoundName.Substring(desiredSoundName.IndexOf(" ") + 1);
                var basePath = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
                var slash = '\\';

                // Check category
                {
                    var categories = Directory.GetDirectories(basePath);

                    if (categories.Length < 1) {
                        throw new Exception("Expected at least one directory in directory");
                    }

                    categories = categories.Select(str => str.Substring(str.LastIndexOf('\\') + 1)).ToArray();

                    var bestScore = Compute(category, categories[0]);
                    var matchedCategory = "";

                    foreach (string str in categories) {
                        var score = Compute(category, str);
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
                        soundboardLocked = false;
                        return;
                    }

                    if (bestScore > 0) {
                        lastChannel.SendMessage("i think you meant " + matchedCategory);
                    }

                    category = matchedCategory;
                }


                // Check name
                {
                    var soundNames = Directory.GetFiles(basePath + category);

                    if (soundNames.Length < 1) {
                        throw new Exception("Expected at least one file in directory");
                    }

                    //soundNames = soundNames.Select(str => str.Substring(str.LastIndexOf('\\') + 1)).ToArray();

                    for (int i = 0; i < soundNames.Length; i++) {
                        var newName = "";
                        var origName = soundNames[i];
                        newName = origName.Substring(origName.LastIndexOf('\\') + 1);
                        newName = newName.Substring(0, newName.LastIndexOf('.'));
                        soundNames[i] = newName;
                    }

                    var bestScore = Compute(name, soundNames[0]);
                    var matchedSound = "";

                    foreach (string str in soundNames) {
                        var score = Compute(name, str);
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
                        soundboardLocked = false;
                        return;
                    }

                    if (bestScore > 0) {
                        lastChannel.SendMessage("i think you meant " + matchedSound);
                    }

                    name = matchedSound;
                }




                soundFilePath = basePath + category + slash + name + ".mp3";

                Console.WriteLine("looking for " + soundFilePath);

                if (!File.Exists(soundFilePath)) {
                    Console.WriteLine("didn't find it...");
                    lastChannel.SendMessage("these are not the sounds you're looking for...");
                    client.DisconnectFromVoice();
                    soundboardLocked = false;
                    return;
                }
                Console.WriteLine("Found it!");
                nextSoundPath = soundFilePath;

                DiscordVoiceConfig voiceConfig = null;
                bool clientMuted = false;
                bool clientDeaf = false;
                client.ConnectToVoiceChannel(channel, voiceConfig, clientMuted, clientDeaf);
                soundboardLocked = true;
            }
        }

        private static void Dog(DiscordSharp.Events.DiscordMessageEventArgs eventArgs, string message) {
            try {
                string s;
                using (WebClient webclient = new MyWebClient()) {
                    s = webclient.DownloadString("http://random.dog/woof");
                    string dog = s;
                    Console.WriteLine("http://random.dog/" + dog);
                    eventArgs.Channel.SendMessage(message + "\nhttp://random.dog/" + dog);
                }
            } catch (Exception) {
                eventArgs.Channel.SendMessage("there are no dogs here, who let them out (random.dog is down :dog: :interrobang:)");
            }

        }

        /// <summary>
        /// Compute the distance between two strings.
        /// http://www.dotnetperls.com/levenshtein
        /// </summary>
        private static int Compute(string s, string t) {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0) {
                return m;
            }

            if (m == 0) {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) {
            }

            for (int j = 0; j <= m; d[0, j] = j++) {
            }

            // Step 3
            for (int i = 1; i <= n; i++) {
                //Step 4
                for (int j = 1; j <= m; j++) {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
