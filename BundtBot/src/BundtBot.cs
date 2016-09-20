using BundtBot.Sound;
using BundtBot.Utility;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Net;
using NString;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;

namespace BundtBot {
	public class BundtBot {
        internal static Dictionary<Server, Channel> TextChannelOverrides = new Dictionary<Server, Channel>();
        internal static string Version { get; private set; } = "0.0";

        readonly SoundBoard _soundBoard = new SoundBoard();
        readonly SoundManager _soundManager = new SoundManager();
        readonly DiscordClient _client;

        static readonly DirectoryInfo _songCacheFolder = new DirectoryInfo(ConfigurationManager.AppSettings["SongCacheFolder"]);
		static readonly string _botTokenPath = ConfigurationManager.AppSettings["BotTokenPath"];
		static readonly string _userEmail = ConfigurationManager.AppSettings["UserEmail"];
		static readonly string _password = ConfigurationManager.AppSettings["Password"];
		static readonly string _accountType = ConfigurationManager.AppSettings["AccountType"];

		public BundtBot() {
            _client = new DiscordClient(x => { x.LogLevel = LogSeverity.Debug; });
        }

        public BundtBot(DiscordClient discordClient) {
            _client = discordClient;
        }

        public void Start() {
            InitVersion();
            InitClient();

            WriteBundtBotASCIIArtToConsole();
            MyLogger.WriteLine("v" + Version, ConsoleColor.Cyan);
            MyLogger.NewLine();

            var commandService = _client.GetService<CommandService>();
            Commands.Register(commandService, _soundManager, _soundBoard, _songCacheFolder);

            EventHandlers.RegisterEventHandlers(_client, _soundBoard, _soundManager);

            while (true) {
				try {
					ConnectAsUserOrBot();
				} catch (HttpException httpException) {
					MyLogger.WriteLine("***Caught HttpException***", ConsoleColor.Red);
					MyLogger.WriteException(httpException);
					MyLogger.WriteLine("***Breaking out of main loop***", ConsoleColor.Yellow);
					break;
				} catch (Exception ex) {
                    MyLogger.WriteLine("***CAUGHT TOP LEVEL EXCEPTION***", ConsoleColor.DarkMagenta);
                    MyLogger.WriteException(ex);
					MyLogger.WriteLine("Sleeping for 1 second then retrying connect");
					Thread.Sleep(1000);
                }
            }
        }

		void ConnectAsUserOrBot() {
			switch (_accountType) {
				case "user":
					_client.ExecuteAndWait(async () => await _client.Connect(_userEmail, _password));
					break;
				case "bot":
					_client.ExecuteAndWait(async () => await _client.Connect(LoadBotToken()));
					break;
				default:
					throw new ConfigurationErrorsException("AccountType must be user or bot");
			}
		}

		void InitClient() {
            _client.UsingAudio(x => { x.Mode = AudioMode.Outgoing; });

            _client.UsingCommands(x => {
                x.PrefixChar = ConfigurationManager.AppSettings["CommandPrefix"][0];
                x.HelpMode = HelpMode.Public;
            });
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
            } catch (Exception ex) {
                MyLogger.WriteException(ex);
                throw;
            }
        }
    }
}
