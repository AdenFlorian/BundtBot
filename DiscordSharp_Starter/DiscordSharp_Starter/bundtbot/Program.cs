using DiscordSharp;
using NAudio.Wave;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace DiscordSharp_Starter.bundtbot {
    class Program {

        public static bool isBot = true;
        
        readonly static string botToken = ConfigurationManager.AppSettings["botToken"];
        static MessageReceivedProcessor messageRcvdProcessor = new MessageReceivedProcessor();

        static SoundBoard soundBoard = new SoundBoard();

        static void Main(string[] args) {
            // First of all, a DiscordClient will be created, and the email and password will be defined.
            Console.WriteLine("Defining variables");
            
            DiscordClient client = new DiscordClient(botToken, isBot, true);

            // Then, we are going to set up our events before connecting to discord, to make sure nothing goes wrong.
            Console.WriteLine("Defining Events");

            // Client is connected to Discord
            client.Connected += (sender, e) =>  {
                Console.WriteLine("oh, look it's " + e.User.Username + " again");
                // If the bot is connected, this message will show.
                // Changes to client, like playing game should be called when the client is connected,
                // just to make sure nothing goes wrong.
                //client.UpdateCurrentGame("DS_starter!", true, "https://github.com/NaamloosDT/DiscordSharp_Starter");
                client.UpdateCurrentGame("all of you"); // This will display at "Playing: "
                //Whoops! i messed up here. (original: Bot online!\nPress any key to close this window.)
            };

            // Private message has been received
            client.PrivateMessageReceived += (sender, e) =>  {
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

            // Channel message has been received
            client.MessageReceived += (sender, eventArgs) => {
                messageRcvdProcessor.ProcessMessage(client, soundBoard, eventArgs);
            };

            client.VoiceClientConnected += (sender, e) => {
                DiscordVoiceClient voiceClient = client.GetVoiceClient();
                if (voiceClient == null) {
                    client.DisconnectFromVoice();
                    return;
                }

                if (String.IsNullOrEmpty(soundBoard.nextSoundPath)) {

                }

                string soundFilePath = soundBoard.nextSoundPath;
                
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
                soundBoard.locked = false;
            };
            
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
            
            // Now to make sure the console doesnt close:
            Console.ReadKey(); // If the user presses a key, the bot will shut down.
            Environment.Exit(0); // Make sure all threads are closed.
        }
    }
}
