using DiscordSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WebSocketSharp;

namespace BundtBot.BundtBot {
    class Program {
        static MessageReceivedProcessor msgRcvdProcessor = new MessageReceivedProcessor();
        static SoundBoard soundBoard;
        static Random random = new Random();

        const string BOT_TOKEN_PATH = "keys/BotToken.txt";

        static void Main(string[] args) {
            // Allows stuff like ʘ ͜ʖ ʘ to show in the Console
            Console.OutputEncoding = Encoding.UTF8;

            // Load Bot Token
            string botToken = null;

            try {
                botToken = LoadBotToken();
            } catch (Exception ex) {
                MyLogger.WriteException(ex);
                MyLogger.WriteExitMessageAndReadKey();
                return;
            }

            DiscordClient client = new DiscordClient(botToken, true, true);
            soundBoard = new SoundBoard(client);

            WriteBundtBotASCIIArtToConsole();

            RegisterEventHandlers(client);

            // Now, try to connect to Discord.
            try {
                MyLogger.WriteLine("Calling client.Connect()");
                client.Connect();
            } catch (Exception ex) {
                MyLogger.WriteException(ex);
                MyLogger.WriteExitMessageAndReadKey();
            }

            // Now to make sure the console doesnt close:
            //Console.ReadKey(); // If the user presses a key, the bot will shut down.
            //Console.TreatControlCAsInput = true;

            // I did this because Console.ReadKey() was giving me trouble when starting a Process
            while (true) {
                Thread.Sleep(100);
            }
            //MyLogger.WriteLine("\nBuh Bye!");
            //Environment.Exit(0); // Make sure all threads are closed.
        }

        private static void WriteBundtBotASCIIArtToConsole() {
            MyLogger.NewLine();
            MyLogger.WriteLineMultiColored(Constants.BUNDTBOT_ASCII_ART);
            MyLogger.NewLine();
        }

        private static string LoadBotToken() {
            string token = File.ReadLines(BOT_TOKEN_PATH).First();
            if (token.IsNullOrEmpty()) {
                throw new Exception("Bot token was empty or null after reading it from " + BOT_TOKEN_PATH);
            }
            return token;
        }

        private static void RegisterEventHandlers(DiscordClient client) {
            MyLogger.Write("Registering Event Handlers...");

            client.TextClientDebugMessageReceived += (sender, e) => {
                //MyLogger.WriteLine("***TextClientDebugLog*** " + e.message.Message);
            };

            client.VoiceClientDebugMessageReceived += (sender, e) => {
                MyLogger.WriteLine("+++VoiceClientDebugLog+++ " + e.message.Message);
            };

            #region ConnectedEvents
            client.Connected += (sender, e) => {
                MyLogger.WriteLine("Client is Connected! ໒( ͡ᵔ ▾ ͡ᵔ )७", ConsoleColor.Green);
                MyLogger.WriteLine("Calling client.DisconnectFromVoice()");
                client.DisconnectFromVoice();
                MyLogger.WriteLine("Calling client.UpdateCurrentGame()");
                client.UpdateCurrentGame("all of you");
            };
            client.VoiceClientConnected += (sender, e) => {
                MyLogger.WriteLine("Voice Client is Connected! ( ͡↑ ͜ʖ ͡↑)", ConsoleColor.Green);
                DiscordVoiceClient voiceClient = client.GetVoiceClient();
                if (voiceClient == null) {
                    client.DisconnectFromVoice();
                    return;
                }

                var defaultTextChannel = voiceClient.Channel.Parent
                    .Channels.First();

                try {
                    soundBoard.OnConnectedToVoiceChannel(voiceClient);
                } catch (Exception ex) {
                    MyLogger.WriteLine(ex.Message, ConsoleColor.Red);
                    MyLogger.WriteLine(ex.StackTrace, ConsoleColor.Yellow);
                    soundBoard.locked = false;
                    defaultTextChannel.SendMessage("bundtbot is brokebot");
                    client.DisconnectFromVoice();
                }
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
                } catch (Exception ex1) {
                    MyLogger.WriteLine("Caught Exception from msgRcvdProcessor.ProcessMessage(client, soundBoard, e)", ConsoleColor.Red);
                    MyLogger.WriteLine(ex1.Message, ConsoleColor.Red);
                    MyLogger.WriteLine(ex1.StackTrace, ConsoleColor.Yellow);
                    MyLogger.WriteLine("Going to wait a second then try to send a message to a text channel saying that we broke");
                    Thread.Sleep(1000);
                    try {
                        e.Channel.SendMessage("bundtbot is brokebot");
                    } catch (Exception ex2) {
                        MyLogger.WriteLine("It really broke this time:");
                        MyLogger.WriteLine(ex2.Message, ConsoleColor.Red);
                    }
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
                MyLogger.Write("Guild available! ");
                MyLogger.WriteLine(e.Server.Name, ConsoleColorHelper.GetRoundRobinColor());
                e.Server.ChangeMemberNickname(client.Me, "bundtbot");
            };
            client.GuildCreated += (sender, e) => {
                MyLogger.WriteLine("Guild created!");
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
                    MyLogger.WriteLine("Bot joined a voice channel. Ignoring...");
                    return;
                }
                if (e.Channel.IsAFKChannel) {
                    MyLogger.WriteLine("User joined an AFK voice channel. Ignoring...");
                    return;
                }
                MyLogger.WriteLine("User joined a voice channel! " + e.User.Username + " : " + e.Channel.Name);
                e.Guild.ChangeMemberNickname(client.Me, ":blue_heart: " + e.User.Username);
                var list = new[] {
                    Tuple.Create("reinhardt", "hello"),
                    Tuple.Create("genji", "hello"),
                    Tuple.Create("mercy", "hello"),
                    Tuple.Create("torbjorn", "hello"),
                    Tuple.Create("winston", "hi there"),
                    Tuple.Create("suhdude", "#random")
                };
                var i = random.Next(list.Count());
                var x = list[i];
                MyLogger.WriteLine("User joined a voice channel. Sending: " + x.Item1 + " " + x.Item2);
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

            MyLogger.WriteLine("Done!");
        }
    }
}
