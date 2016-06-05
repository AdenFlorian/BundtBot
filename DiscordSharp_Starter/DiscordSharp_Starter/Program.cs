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
                    eventArgs.Channel.SendMessage("...no");
                    // Because this is a public message, 
                    // the bot should send a message to the channel the message was received.
                }
                if (eventArgs.MessageText == "!cat") {
                    Thread t = new Thread(new ParameterizedThreadStart(randomcat));
                    t.Start(eventArgs.Channel);
                    string s;
                    using (WebClient webclient = new WebClient()) {
                        s = webclient.DownloadString("http://random.cat/meow");
                        int pFrom = s.IndexOf("\\/i\\/") + "\\/i\\/".Length;
                        int pTo = s.LastIndexOf("\"}");
                        string cat = s.Substring(pFrom, pTo - pFrom);
                        var newCatPngName = "cat_" + Guid.NewGuid() + ".png";
                        Console.WriteLine(newCatPngName);
                        webclient.DownloadFile("http://random.cat/i/" + cat, newCatPngName);
                        client.AttachFile(eventArgs.Channel, "i found a cat:", "cat.png");
                    }
                }
                if (eventArgs.MessageText == "!highnoon") {
                    if (soundboardLocked) {
                        eventArgs.Channel.SendMessage("wait your turn...");
                        return;
                    }
                    lastChannel = eventArgs.Channel;
                    desiredSoundName = "!highnoon";

                    DiscordMember author = eventArgs.Author;
                    DiscordChannel channel = author.CurrentVoiceChannel;

                    if (channel == null) {
                        eventArgs.Channel.SendMessage("you need to be in a voice channel to hear me roar");
                        return;
                    }

                    DiscordVoiceConfig voiceConfig = null;
                    bool clientMuted = false;
                    bool clientDeaf = false;
                    client.ConnectToVoiceChannel(channel, voiceConfig, clientMuted, clientDeaf);
                    soundboardLocked = true;
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

                    if (desiredSoundName == "!highnoon") {
                        Console.WriteLine("!highnoon");
                        soundFilePath = @"C:\Users\Bundt\Desktop\high noon.mp3";
                    } else {
                        desiredSoundName.IndexOf(" ");
                        var category = desiredSoundName.Substring(0, desiredSoundName.IndexOf(" "));
                        var name = desiredSoundName.Substring(desiredSoundName.IndexOf(" ") + 1);

                        var basePath = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
                        var slash = '\\';

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
                    }

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
                //var rand = new Random();
                //var bytes = new byte[32000];
                //rand.NextBytes(bytes);

                /*byte[] sampleBuffer = null;

                var soundFile = @"C:\Users\Bundt\Desktop\high noon.mp3";

                using (var wfr = new Mp3FileReader(soundFile)){
                    int offset = 0;
                    long numBytes = wfr.Length;
                    sampleBuffer = new byte[numBytes];
                    wfr.Read(sampleBuffer, offset, (int)numBytes);
                };*/

                string soundFilePath = null;


                if (desiredSoundName == "!highnoon") {
                    Console.WriteLine("!highnoon");
                    soundFilePath = @"C:\Users\Bundt\Desktop\high noon.mp3";
                } else {
                    desiredSoundName.IndexOf(" ");
                    var category = desiredSoundName.Substring(0, desiredSoundName.IndexOf(" "));
                    var name = desiredSoundName.Substring(desiredSoundName.IndexOf(" ") + 1);

                    var basePath = @"C:\Users\Bundt\Desktop\All sound files\!categorized\";
                    var slash = '\\';

                    soundFilePath = basePath + category + slash + name + ".mp3";


                    //Console.WriteLine("looking for " + soundFilePath);

                    if (!File.Exists(soundFilePath)) {
                        Console.WriteLine("didn't find it...");
                        lastChannel.SendMessage("these are not the sounds you're looking for...");
                        client.DisconnectFromVoice();
                        soundboardLocked = false;
                        return;
                    }
                    //Console.WriteLine("Found it!");
                }


                

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





                //voiceClient.SendVoice(sampleBuffer);
                Thread.Sleep(waitTimeMS + 1500);
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

            //  Don't want messages to be removed? this piece of code will
            //  Keep messages for you. Remove if unused :)
            //client.MessageDeleted += (sender, e) =>
            //    {
            //        e.Channel.SendMessage("Removing messages has been disabled on this server!");
            //        e.Channel.SendMessage("<@" + e.DeletedMessage.Author.ID + "> sent: " +e.DeletedMessage.Content.ToString());
            //    };



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

        public static void randomcat(object channel) {

        }
    }
}
