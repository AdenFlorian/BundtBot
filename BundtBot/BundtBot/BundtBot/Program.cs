using Discord;
using Discord.Audio;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WebSocketSharp;

namespace BundtBot.BundtBot {
    class Program {
        MessageReceivedProcessor msgRcvdProcessor = new MessageReceivedProcessor();
        SoundBoard soundBoard;
        Random random = new Random();
        DiscordClient _client;

        const string BOT_TOKEN_PATH = "keys/BotToken.txt";

        static void Main(string[] args) {
            new Program().Start();
        }

        void Start() {

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


            _client = new DiscordClient(x => {
                x.LogLevel = LogSeverity.Debug;
            });

            soundBoard = new SoundBoard(_client);

            WriteBundtBotASCIIArtToConsole();

            _client.Log.Message += (sender, eventArgs) => {
                Console.WriteLine($"[{eventArgs.Severity}] {eventArgs.Source}: {eventArgs.Message}");
            };

            RegisterEventHandlers(_client);
            
            _client.UsingAudio(x => {
                x.Mode = AudioMode.Outgoing;
            });

            _client.ExecuteAndWait(async () => {
                await _client.Connect(botToken);
            });

            _client.Disconnect();
            _client.Dispose();
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

        void RegisterEventHandlers(DiscordClient client) {
            MyLogger.Write("Registering Event Handlers...");

            #region ConnectedEvents
            _client.Ready += (sender, e) => {
                MyLogger.WriteLine("Client is Ready/Connected! ໒( ͡ᵔ ▾ ͡ᵔ )७", ConsoleColor.Green);
                MyLogger.WriteLine("Calling _client.DisconnectFromVoice()");
                //_client.DisconnectFromVoice();
                MyLogger.WriteLine("Calling _client.UpdateCurrentGame()");
                _client.SetGame("armada");
            };
            #endregion

            #region MessageEvents
            _client.MessageDeleted += (sender, e) => {

            };
            _client.MessageUpdated += (sender, e) => {

            };
            _client.MessageReceived += async (sender, e) => {
                try {
                    await msgRcvdProcessor.ProcessMessage(_client, soundBoard, e);
                } catch (Exception ex1) {
                    MyLogger.WriteLine("Caught Exception from msgRcvdProcessor.ProcessMessage(client, soundBoard, e)", ConsoleColor.Red);
                    MyLogger.WriteLine(ex1.Message, ConsoleColor.Red);
                    MyLogger.WriteLine(ex1.StackTrace, ConsoleColor.Yellow);
                    MyLogger.WriteLine("Going to wait a second then try to send a message to a text channel saying that we broke");
                    Thread.Sleep(1000);
                    try {
                        await e.Channel.SendMessage("bundtbot is brokebot");
                    } catch (Exception ex2) {
                        MyLogger.WriteLine("It really broke this time:");
                        MyLogger.WriteLine(ex2.Message, ConsoleColor.Red);
                    }
                }
            };
            #endregion

            #region ChannelEvents
            _client.ChannelCreated += (sender, e) => {
                e.Channel.SendMessage("less is more");
            };
            _client.ChannelDestroyed += (sender, e) => {
                e.Channel.SendMessage("RIP in pieces " + e.Channel.Name);
            };
            _client.ChannelUpdated += (sender, e) => {
            };
            #endregion

            #region GuildEvents
            _client.ServerAvailable += async (sender, e) => {
                MyLogger.Write("Server available! ");
                MyLogger.WriteLine(e.Server.Name, ConsoleColorHelper.GetRoundRobinColor());
                float version = 0.1f;

                Thread.Sleep(random.Next(0, 100));

                while (true) {
                    try {
                        await e.Server.CurrentUser.Edit(nickname: "bundtbot " + version);
                        version += 0.01f;
                        Thread.Sleep(10000);
                    } catch (Exception) {
                    }
                }
            };
            _client.JoinedServer += (sender, e) => {
                MyLogger.WriteLine("Joined Server! " + e.Server.Name);
            };
            #endregion

            #region GuildMemberEvents
            _client.UserBanned += (sender, e) => {
            };
            _client.UserUpdated += (sender, e) => {
            };
            #endregion

            #region UserEvents
            _client.UserJoined += (sender, e) => {
                e.User.Server.DefaultChannel.SendMessage("welcome to server " + e.User.NicknameMention);
                e.User.Server.DefaultChannel.SendMessage("beware of the airhorns...");
            };
            /*_client.UserJoinedVoiceChannel += (sender, e) => {
                if (e.User.IsBot) {
                    MyLogger.WriteLine("Bot joined a voice channel. Ignoring...");
                    return;
                }
                if (e.Channel.IsAFKChannel) {
                    MyLogger.WriteLine("User joined an AFK voice channel. Ignoring...");
                    return;
                }
                MyLogger.WriteLine("User joined a voice channel! " + e.User.Username + " : " + e.Channel.Name);
                e.Guild.ChangeMemberNickname(_client.Me, ":blue_heart: " + e.User.Username);
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
            };*/
            _client.UserLeft += (sender, e) => {
                e.Server.DefaultChannel.SendMessage("RIP in pieces " + e.User.Nickname);
            };
            #endregion

            MyLogger.WriteLine("Done!");
        }
    }
}
