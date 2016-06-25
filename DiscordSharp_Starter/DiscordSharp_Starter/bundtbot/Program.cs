using DiscordSharp;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
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
            MyLogger.WriteLine(bundtbotASCIIART, ConsoleColor.Red);

            RegisterEventHandlers(client);

            // Now, try to connect to Discord.
            try {
                MyLogger.WriteLine("Calling client.Connect()");
                client.Connect();
            } catch (Exception e) {
                MyLogger.WriteLine("Something went wrong!", ConsoleColor.Yellow);
                MyLogger.WriteLine(e.Message, ConsoleColor.Red);
                MyLogger.WriteLine("Press any key to close this window.");
            }

            // Now to make sure the console doesnt close:
            Console.ReadKey(); // If the user presses a key, the bot will shut down.
            MyLogger.WriteLine("\nBuh Bye!");
            Environment.Exit(0); // Make sure all threads are closed.
        }

        private static void RegisterEventHandlers(DiscordClient client) {
            MyLogger.Write("Registering Event Handlers...");

            #region ConnectedEvents
            client.Connected += (sender, e) => {
                MyLogger.WriteLine("Connected!", ConsoleColor.Green);
                MyLogger.WriteLine("Calling client.DisconnectFromVoice()");
                client.DisconnectFromVoice();
                MyLogger.WriteLine("Calling client.UpdateCurrentGame()");
                client.UpdateCurrentGame("all of you");
            };
            client.VoiceClientConnected += (sender, e) => {
                DiscordVoiceClient voiceClient = client.GetVoiceClient();
                if (voiceClient == null) {
                    client.DisconnectFromVoice();
                    return;
                }

                soundBoard.OnConnectedToVoiceChannel(voiceClient);
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
                    Thread.Sleep(1000);
                    try {
                        e.Channel.SendMessage("bundtbot is brokebot");
                    } catch (Exception exception) {
                        MyLogger.WriteLine("It really broke this time:");
                        MyLogger.WriteLine(exception.Message, ConsoleColor.Red);
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
                    return;
                }
                MyLogger.WriteLine("User joined a voice channel! " + e.User.Username + " : " + e.Channel.Name);
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

            MyLogger.WriteLine("Done!");
        }
    }
}
