using DiscordSharp;
using DiscordSharp.Objects;
using NAudio.Wave;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace DiscordSharp_Starter.BundtBot {
    class Program {
        static MessageReceivedProcessor msgRcvdProcessor = new MessageReceivedProcessor();
        static SoundBoard soundBoard;
        static Random random = new Random();

        static void Main(string[] args) {
            var botToken = ConfigurationManager.AppSettings["botToken"];
            DiscordClient client = new DiscordClient(botToken, true, true);
            soundBoard = new SoundBoard(client);

            var bundtbotASCIIART = "";
            bundtbotASCIIART += @" ______                  _       ______             " + "\n";
            bundtbotASCIIART += @"(____  \                | |  _  (____  \        _   " + "\n";
            bundtbotASCIIART += @" ____)  )_   _ ____   __| |_| |_ ____)  ) ___ _| |_ " + "\n";
            bundtbotASCIIART += @"|  __  (| | | |  _ \ / _  (_   _)  __  ( / _ (_   _)" + "\n";
            bundtbotASCIIART += @"| |__)  ) |_| | | | ( (_| | | |_| |__)  ) |_| || |_ " + "\n";
            bundtbotASCIIART += @"|______/|____/|_| |_|\____|  \__)______/ \___/  \__)";
            ConsoleColored.WriteLine(bundtbotASCIIART, ConsoleColor.Red);

            RegisterEventHandlers(client);

            // Now, try to connect to Discord.
            try {
                Console.WriteLine("Calling client.Connect()");
                client.Connect();
            } catch (Exception e) {
                Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
            }

            // Now to make sure the console doesnt close:
            Console.ReadKey(); // If the user presses a key, the bot will shut down.
            Console.WriteLine("\nBuh Bye!");
            Environment.Exit(0); // Make sure all threads are closed.
        }

        private static void RegisterEventHandlers(DiscordClient client) {
            Console.Write("Registering Event Handlers...");

            #region ConnectedEvents
            client.Connected += (sender, e) => {
                ConsoleColored.WriteLine("Connected!", ConsoleColor.Green);
                Console.WriteLine("Calling client.DisconnectFromVoice()");
                client.DisconnectFromVoice();
                Console.WriteLine("Calling client.UpdateCurrentGame()");
                client.UpdateCurrentGame("all of you");
            };
            client.VoiceClientConnected += (sender, e) => {
                DiscordVoiceClient voiceClient = client.GetVoiceClient();
                if (voiceClient == null) {
                    client.DisconnectFromVoice();
                    return;
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
                        ConsoleColored.WriteLine("Voice finished enqueuing", ConsoleColor.Yellow);
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
            #endregion

            #region MessageEvents
            client.PrivateMessageDeleted += (sender, e) => {

            };
            client.PrivateMessageReceived += (sender, e) => {
                if (e.Message == "!help") {
                    e.Author.SendMessage("this is a private message, what did you expect");
                } else if (e.Message.StartsWith("join")) {
                    e.Author.SendMessage("Please use this url instead!" +
                        "https://discordapp.com/oauth2/authorize?client_id=[CLIENT_ID]&scope=bot&permissions=0");
                }
            };
            client.MessageDeleted += (sender, e) => {

            };
            client.MessageEdited += (sender, e) => {

            };
            client.MessageReceived += (sender, e) => {
                try {
                    msgRcvdProcessor.ProcessMessage(client, soundBoard, e);
                } catch (Exception) {
                    e.Channel.SendMessage("bundtbot is brokebot");
                }
            };
            #endregion

            #region ChannelEvents
            client.ChannelCreated += (sender, e) => {
                e.ChannelCreated.SendMessage("less is more");
            };
            client.ChannelDeleted += (sender, e) => {
                e.ChannelDeleted.SendMessage("RIP in pieces " + e.ChannelDeleted.Name);
            };
            client.ChannelUpdated += (sender, e) => {
            };
            #endregion

            #region GuildEvents
            client.GuildAvailable += (sender, e) => {
                Console.Write("Guild available! ");
                ConsoleColored.WriteLine(e.Server.Name, ConsoleColorHelper.GetRoundRobinColor());
                e.Server.ChangeMemberNickname(client.Me, "bundtbot");
            };
            client.GuildCreated += (sender, e) => {
                Console.WriteLine("Guild created!");
            };
            client.GuildDeleted += (sender, e) => {
            };
            client.GuildUpdated += (sender, e) => {
            };
            #endregion

            #region GuildMemberEvents
            client.GuildMemberBanned += (sender, e) => {
            };
            client.GuildMemberUpdated += (sender, e) => {
            };
            #endregion

            #region UserEvents
            client.UserAddedToServer += (sender, e) => {
                e.AddedMember.SendMessage("welcome to server");
                e.AddedMember.SendMessage("beware of the airhorns...");
            };
            client.UserJoinedVoiceChannel += (sender, e) => {
                if (e.User.IsBot) {
                    return;
                }
                Console.WriteLine("User joined a voice channel! " + e.User.Username + " : " + e.Channel.Name);
                e.Guild.ChangeMemberNickname(client.Me, ":blue_heart: " + e.User.Username);
                var list = new[] {
                    Tuple.Create("reinhardt", "hello"),
                    Tuple.Create("genji", "hello"),
                    Tuple.Create("mercy", "hello"),
                    Tuple.Create("torbjorn", "hello"),
                    Tuple.Create("winston", "hi there")
                };
                var i = random.Next(list.Count());
                var x = list[i];
                soundBoard.Process(null, e.Channel, x.Item1, x.Item2);
            };
            client.UserLeftVoiceChannel += (sender, e) => {
            };
            client.UserRemovedFromServer += (sender, e) => {
                e.Server.Channels.First().SendMessage("RIP in pieces " + e.MemberRemoved.Username);
            };
            client.UserSpeaking += (sender, e) => {
            };
            client.UserTypingStart += (sender, e) => {
            };
            client.UserUpdate += (sender, e) => {
            };
            #endregion

            Console.WriteLine("Done!");
        }
    }
}
