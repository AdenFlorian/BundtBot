//  If you have any questions or just want to talk, join my server!
//  https://discord.gg/0oZpaYcAjfvkDuE4
using DiscordSharp;
using DiscordSharp.Objects;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordSharp_Starter {
    class Program {

        public static DiscordSharp.Objects.DiscordChannel lastchannel;
        public static bool isBot = true;
        private static string desiredSoundName = null;
        private static string nextSoundPath = null;
        private static bool soundboardLocked = false;
        private static DiscordChannel lastChannel = null;

        const string botToken = "*************************************";

        static void Main(string[] args) {
            // First of all, a DiscordClient will be created, and the email and password will be defined.
            Console.WriteLine("Defining variables");
            
            DiscordClient client = new DiscordClient(botToken, isBot, true);
            // Then, we are going to set up our events before connecting to discord, to make sure nothing goes wrong.

            Console.WriteLine("Defining Events");
            // find that one you interested in 

            client.Connected += (sender, e) => // Client is connected to Discord
            {
                Console.WriteLine("oh, look it's " + e.User.Username + " again");
                // If the bot is connected, this message will show.
                // Changes to client, like playing game should be called when the client is connected,
                // just to make sure nothing goes wrong.
                //client.UpdateCurrentGame("DS_starter!", true, "https://github.com/NaamloosDT/DiscordSharp_Starter");
                client.UpdateCurrentGame("all of you"); // This will display at "Playing: "
                //Whoops! i messed up here. (original: Bot online!\nPress any key to close this window.)
            };


            client.PrivateMessageReceived += (sender, e) => // Private message has been received
            {
                if (e.Message == "!help") {
                    e.Author.SendMessage("this is a private message, what did you expect");
                    // Because this is a private message, the bot should send a private message back
                    // A private message does NOT have a channel
                }
                if (e.Message.StartsWith("join")) {
                    if (!isBot) {
                        string inviteID = e.Message.Substring(e.Message.LastIndexOf('/') + 1);
                        // Thanks to LuigiFan (Developer of DiscordSharp) for this line of code!
                        client.AcceptInvite(inviteID);
                        e.Author.SendMessage("Joined your discord server!");
                        Console.WriteLine("Got join request from " + inviteID);
                    } else {
                        e.Author.SendMessage("Please use this url instead!" +
                            "https://discordapp.com/oauth2/authorize?client_id=[CLIENT_ID]&scope=bot&permissions=0");
                    }
                }
            };


            client.MessageReceived += (sender, eventArgs) => // Channel message has been received
            {
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
                        Thread t = new Thread(new ParameterizedThreadStart(randomcat));
                        t.Start(eventArgs.Channel);
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
            };

            client.VoiceClientConnected += (sender, e) => {
                
                DiscordVoiceClient voiceClient = client.GetVoiceClient();
                if (voiceClient == null) {
                    client.DisconnectFromVoice();
                    return;
                }

                if (String.IsNullOrEmpty(nextSoundPath)) {

                }

                string soundFilePath = nextSoundPath;
                
                int ms = voiceClient.VoiceConfig.FrameLengthMs;
                int channels = 1;
                int sampleRate = 48000;
                int waitTimeMS = 0;

                int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
                byte[] buffer = new byte[blockSize];
                var outFormat = new WaveFormat(sampleRate, 16, channels);
                voiceClient.SetSpeaking(true);
                using (var mp3Reader = new MediaFoundationReader(soundFilePath)) {
                    using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat) { ResamplerQuality = 60 }) {
                        int byteCount;
                        while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) {
                            waitTimeMS += ms;
                            if (voiceClient.Connected) {
                                voiceClient.SendVoice(buffer);
                            } else
                                break;
                        }
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Voice finished enqueuing");
                        Console.ForegroundColor = ConsoleColor.White;
                        resampler.Dispose();
                        mp3Reader.Close();
                    }
                }
                var totalWaitTimeMS = waitTimeMS + 1500;
                Console.WriteLine("Waiting for " + totalWaitTimeMS + "ms");
                Thread.Sleep(totalWaitTimeMS);
                client.DisconnectFromVoice();
                soundboardLocked = false;
            };

            //  Below: some things that might be nice?

            //  This sends a message to every new channel on the server
            client.ChannelCreated += (sender, e) => {
                e.ChannelCreated.SendMessage("less is more");
            };

            //  When a user joins the server, send a message to them.
            client.UserAddedToServer += (sender, e) => {
                e.AddedMember.SendMessage("welcome to server");
                e.AddedMember.SendMessage("beware of the airhorns...");
            };

            client.Connected += (sender, e) => {
                Console.WriteLine("Client connected for realz this time!");
            };

            client.GuildCreated += (sender, e) => {
                Console.WriteLine("Guild created!");
                e.Server.Channels.First().SendMessage("i am bundtbot destroyer of cakes");
            };

            client.GuildAvailable += (sender, e) => {
                Console.WriteLine("Guild available! " + e.Server.Name);
                e.Server.ChangeMemberNickname(client.Me, "bundtbot");
            };

            // Now, try to connect to Discord.
            try {
                // Make sure that IF something goes wrong, the user will be notified.
                // The SendLoginRequest should be called after the events are defined, to prevent issues.
                Console.WriteLine("Sending login request");
                client.SendLoginRequest();
                Console.WriteLine("Connecting client in separate thread");
                // Cannot convert from 'method group' to 'ThreadStart', so i removed threading
                // Pass argument 'true' to use .Net sockets.
                client.Connect();
                // Login request, and then connect using the discordclient i just made.
                Console.WriteLine("Client connected!");
                client.DisconnectFromVoice();
            } catch (Exception e) {
                Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
            }

            // Done! your very own Discord bot is online!


            // Now to make sure the console doesnt close:
            Console.ReadKey(); // If the user presses a key, the bot will shut down.
            Environment.Exit(0); // Make sure all threads are closed.
        }

        private static void Dog(DiscordSharp.Events.DiscordMessageEventArgs eventArgs, string message) {
            Thread t = new Thread(new ParameterizedThreadStart(randomcat));
            t.Start(eventArgs.Channel);
            string s;
            using (WebClient webclient = new WebClient()) {
                s = webclient.DownloadString("http://random.dog/woof");
                string dog = s;
                Console.WriteLine("http://random.dog/" + dog);
                eventArgs.Channel.SendMessage(message + "\nhttp://random.dog/" + dog);
            }
        }

        public static void randomcat(object channel) {

        }

        /// <summary>
        /// Compute the distance between two strings.
        /// http://www.dotnetperls.com/levenshtein
        /// </summary>
        public static int Compute(string s, string t) {
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
