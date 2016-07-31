using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using BundtBot.BundtBot.Sound;
using BundtBot.BundtBot.Utility;
using Discord;
using Discord.Audio;
using Discord.Commands;
using NString;

namespace BundtBot.BundtBot {
    class Program {
        internal static Dictionary<Server, Channel> TextChannelOverrides = new Dictionary<Server, Channel>();
        internal static string Version { get; private set; } = "0.0";

        readonly SoundBoard _soundBoard = new SoundBoard();
        readonly SoundManager _soundManager = new SoundManager();
        DiscordClient _client;

        static readonly string _songCachePath = ConfigurationManager.AppSettings["SongCacheFolder"];
        static readonly string _botTokenPath = ConfigurationManager.AppSettings["BotTokenPath"];

        static void Main() {
            new Program().Start();
        }

        void Start() {
            InitConsole();
            InitVersion();
            InitClient();

            WriteBundtBotASCIIArtToConsole();
            MyLogger.WriteLine("v" + Version, ConsoleColor.Cyan);
            MyLogger.NewLine();
            
            var commandService = _client.GetService<CommandService>();
            Commands.Register(commandService, _soundManager, _soundBoard, _songCachePath);

            EventHandlers.RegisterEventHandlers(_client, _soundBoard, _soundManager);

            while (true) {
                try {
                    _client.ExecuteAndWait(async () => await _client.Connect(LoadBotToken()));
                } catch (Exception ex) {
                    MyLogger.WriteLine("***CAUGHT TOP LEVEL EXCEPTION***", ConsoleColor.DarkMagenta);
                    MyLogger.WriteException(ex);
                }
            }
        }

        void InitClient() {
            _client = new DiscordClient(x => { x.LogLevel = LogSeverity.Debug; });

            _client.UsingAudio(x => { x.Mode = AudioMode.Outgoing; });

            _client.UsingCommands(x => {
                x.PrefixChar = ConfigurationManager.AppSettings["CommandPrefix"][0];
                x.HelpMode = HelpMode.Public;
            });
        }

        static void InitConsole() {
            // Allows stuff like ʘ ͜ʖ ʘ to show in the Console
            Console.OutputEncoding = Encoding.UTF8;
            Console.WindowHeight = (int) (Console.LargestWindowHeight * 0.9);
            Console.WindowTop = 0;
        }

        static void InitVersion() {
            const string versionPath = "version.txt";
            if (File.Exists(versionPath)) {
                var versionFloat = float.Parse(File.ReadAllText(versionPath));
                versionFloat += 0.01f;
                Version = versionFloat.ToString("0.00");
            }
            File.WriteAllText(versionPath, Version);
            const string otherVersionPath = "../../version.txt";
            if (File.Exists(otherVersionPath)) {
                File.WriteAllText(otherVersionPath, Version);
            }
        }
        
        static void WriteBundtBotASCIIArtToConsole() {
            MyLogger.NewLine();
            MyLogger.WriteLine(Constants.BundtbotASCIIArt, ConsoleColor.Red);
            MyLogger.NewLine();
        }

        static string LoadBotToken() {
            try {
                var token = File.ReadLines(_botTokenPath).First();
                if (token.IsNullOrEmpty()) {
                    throw new Exception("Bot token was empty or null after reading it from " + _botTokenPath);
                }
                return token;
            }
            catch (Exception ex) {
                MyLogger.WriteException(ex);
                throw;
            }
        }
    }
}
