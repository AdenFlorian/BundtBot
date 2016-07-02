using Discord;
using Discord.Audio;
using Discord.Commands;
using NString;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WrapYoutubeDl;

namespace BundtBot.BundtBot {
    class Program {
        SoundBoard _soundBoard;
        Random _random = new Random();
        DiscordClient _client;
        string version = "0.0";

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

            // Do version
            var versionPath = "version.txt";
            if (File.Exists(versionPath)) {
                var versionFloat = float.Parse(File.ReadAllText(versionPath));
                versionFloat += 0.01f;
                version = versionFloat.ToString("0.00");
            }
            File.WriteAllText(versionPath, version);

            var otherVersionPath = "../../version.txt";
            if (File.Exists(otherVersionPath)) {
                File.WriteAllText(otherVersionPath, version);
            }

            _client = new DiscordClient(x => {
                x.LogLevel = LogSeverity.Debug;
            });

            _soundBoard = new SoundBoard(_client);

            WriteBundtBotASCIIArtToConsole();
            MyLogger.WriteLine("v" + version, ConsoleColor.Cyan);
            MyLogger.NewLine();

            _client.UsingAudio(x => {
                x.Mode = AudioMode.Outgoing;
            });

            _client.UsingCommands(x => {
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Public;
            });

            SetupCommands();

            RegisterEventHandlers();

            _client.ExecuteAndWait(async () => {
                await _client.Connect(botToken);
            });

            _client.Disconnect();
            _client.Dispose();
        }

        void SetupCommands() {
            var commandService = _client.GetService<CommandService>();

            #region CommandEvents
            commandService.CommandErrored += async (s, e) => {
                MyLogger.WriteLine("[CommandErrored] " + e.Exception.Message, ConsoleColor.DarkMagenta);
                await e.Channel.SendMessage(e.Exception.Message);
            };
            commandService.CommandExecuted += (s, e) => {
                MyLogger.WriteLine("[CommandExecuted] " + e.Command.Text, ConsoleColor.DarkCyan);
            };
            #endregion

            #region boring commands
            commandService.CreateCommand("credits")
                .Alias(new string[] { "github" })
                .Description("Prints who made this thing.")
                .Do(async e => {
                    await e.Channel.SendMessage("!owsb <character name> <phrase>"
                        + "\n!yt <youtube search string>"
                        + "\ncreated by @AdenFlorian"
                        + "\nhttps://github.com/AdenFlorian/DiscordSharp_Starter"
                        + "\nhttps://trello.com/b/VKqUgzwV/bundtbot#");
                });
            commandService.CreateCommand("cat")
                .Alias(new string[] { "kitty", "feline", "Felis_catus", "kitten", "🐱", "🐈" })
                .Description("It's a secret.")
                .Do(async e => {
                    await Cat(e);
                });
            commandService.CreateCommand("dog")
                .Alias(new string[] { "doggy", "puppy", "Canis_lupus_familiaris", "🐶", "🐕" })
                .Description("The superior alternaitve to !cat.")
                .Do(async e => {
                    await Dog(e, "i found a dog");
                });
            commandService.CreateCommand("admin")
                .Alias(new string[] { "administrator" })
                .Description("Find out whose house you're in.")
                .Do(async e => {
                    string msg;
                    if (e.User.ServerPermissions.Administrator) {
                        msg = "Yes, you are! ┌( ಠ‿ಠ)┘";
                    } else {
                        msg = "No, you aren't (-_-｡), but these people are!";
                        var admins = e.Server.Users.Where(x => x.ServerPermissions.Administrator);
                        foreach (var admin in admins) {
                            msg += $" | {admin.Name} | ";
                        }
                    }
                    await e.Channel.SendMessage(msg);
                });
            commandService.CreateCommand("mod")
                .Alias(new string[] { "moderator" })
                .Description("Find out if you are a mod.")
                .Do(async e => {
                    if (e.User.Roles.Any(x => x.Name.Equals("mod"))) {
                        await e.Channel.SendMessage("Yes, you are! ┌( ಠ‿ಠ)┘");
                    } else {
                        await e.Channel.SendMessage("No, you aren't (-_-｡)");
                    }
                });
            commandService.CreateCommand("me")
                .Alias(new string[] { "mystatus" })
                .Description("Find out you status in life.")
                .Do(async e => {
                    await e.Channel.SendMessage("Voice channel: " + e.User.VoiceChannel?.Name);
                });
            #endregion

            #region SoundBoard
            commandService.CreateCommand("stop")
                .Alias(new string[] { "shutup", "stfu", "👎", "🚫🎶", "🚫 🎶" })
                .Description("Please don't stop the :notes:.")
                .Do(async e => {
                    var audioClient = e.Server.GetAudioClient();
                    if (audioClient == null) {
                        var msg = await e.Channel.SendMessage("stop what?");
                        await msg.Edit(msg.Text + " I unlocked the soundboard for you, hope that helps :grimacing: ");
                    } else {
                        var msg = await e.Channel.SendMessage("okay...");
                        audioClient.Clear();
                        await audioClient.Disconnect();
                        await msg.Edit(msg.Text + ":disappointed_relieved:");
                    }
                    _soundBoard.stop = true;
                    _soundBoard.locked = false;
                });
            commandService.CreateCommand("sb")
                .AddCheck((c, u, x) => u.VoiceChannel != null, Constants.NOT_IN_VOICE)
                .Alias(new string[] { "owsb" })
                .Description("Sound board. It plays sounds with its mouth.")
                .Parameter("sound args", ParameterType.Unparsed)
                .Do(async e => {
                    Sound soundBoardArgs = null;
                    try {
                        soundBoardArgs = new Sound(e.Message.Text);
                    } catch (Exception ex) {
                        await e.Channel.SendMessage("you're doing it wrong");
                        await e.Channel.SendMessage(ex.Message);
                    }

                    if (soundBoardArgs == null) {
                        await e.Channel.SendMessage("you're doing it wrong (or something broke)");
                        return;
                    }

                    await _soundBoard.Process(e, soundBoardArgs);
                });
            commandService.CreateCommand("youtube")
                // TODO These checks seem to be broken
                //.AddCheck((c, u, x) => _soundBoard.locked == false, Constants.SOUNDBOARD_LOCKED)
                //.AddCheck((c, u, x) => u.VoiceChannel != null, Constants.NOT_IN_VOICE)
                .Alias(new string[] { "yt" })
                .Description("It's a tube for you!")
                .Parameter("search string", ParameterType.Unparsed)
                .Do(async e => {
                    // First validate the command is correct
                    var ytSearchString = "";

                    var commandString = e.Message.Text.Trim();

                    if (commandString.StartsWith("!youtube ") &&
                        commandString.Length > 9) {
                        ytSearchString = commandString.Substring(9);
                    } else if (commandString.StartsWith("!yt ") &&
                        commandString.Length > 4) {
                        ytSearchString = commandString.Substring(4);
                    } else {
                        await e.Channel.SendMessage("you're doing it wrong (or something broke)");
                        return;
                    }

                    if (_soundBoard.locked) {
                        await e.Channel.SendMessage("wait your turn...or if you want to be mean, use !stop");
                        return;
                    }

                    var voiceChannel = e.User.VoiceChannel;

                    if (voiceChannel == null) {
                        await e.Channel.SendMessage("you need to be in a voice channel to hear me roar");
                        return;
                    }

                    await e.Channel.SendMessage("Searching youtube for: " + ytSearchString);

                    // Get video id
                    MyLogger.WriteLine("Getting youtube video id...");
                    var youtubeVideoID = new YoutubeVideoID().Get(ytSearchString);
                    MyLogger.WriteLine("Youtube video ID get! " + youtubeVideoID, ConsoleColor.Green);

                    MyLogger.WriteLine("Getting youtube video title...");
                    var youtubeVideoTitle = new YoutubeVideoName().Get(ytSearchString);
                    MyLogger.WriteLine("Youtube video title get! " + youtubeVideoTitle, ConsoleColor.Green);
                    await e.Channel.SendMessage("Found video: " + youtubeVideoTitle);

                    var mp3OutputFolder = "c:/@mp3/";

                    // See if file exists
                    var possiblePath = mp3OutputFolder + youtubeVideoID + ".wav";

                    string outputWAV;

                    if (File.Exists(possiblePath) == false) {
                        string youtubeOutput = await new YoutubeDownloader().YoutubeDownloadAndConvert(e, ytSearchString, mp3OutputFolder);
                        var msg = await e.Channel.SendMessage("Download finished! Converting audio...");
                        outputWAV = new FFMPEG().ffmpegConvert(youtubeOutput);
                        await msg.Edit(msg.Text + "finished! Sending data...");
                    } else {
                        MyLogger.WriteLine("WAV file exists already! " + possiblePath, ConsoleColor.Green);
                        outputWAV = possiblePath;
                        await e.Channel.SendMessage("Playing audio from cache...");
                    }

                    var args = new Sound();
                    args.soundPath = outputWAV;
                    args.deleteAfterPlay = false;

                    if (File.Exists(args.soundPath) == false) {
                        await e.Channel.SendMessage("that video doesn't work, sorry, try something else");
                        return;
                    }
                    _soundBoard.locked = true;

                    MyLogger.WriteLine("Connecting to voice channel:" + voiceChannel.Name);
                    MyLogger.WriteLine("\tOn server:  " + voiceChannel.Server.Name);
                    var audioService = _client.GetService<AudioService>();
                    var audioClient = await audioService.Join(voiceChannel);
                    new AudioStreamer().PlaySound(audioService, audioClient, args);
                });
            #endregion
        }

        void WriteBundtBotASCIIArtToConsole() {
            MyLogger.NewLine();
            MyLogger.WriteLine(Constants.BUNDTBOT_ASCII_ART, ConsoleColor.Red);
            MyLogger.NewLine();
        }

        string LoadBotToken() {
            string token = File.ReadLines(BOT_TOKEN_PATH).First();
            if (token.IsNullOrEmpty()) {
                throw new Exception("Bot token was empty or null after reading it from " + BOT_TOKEN_PATH);
            }
            return token;
        }

        void RegisterEventHandlers() {
            MyLogger.Write("Registering Event Handlers...");

            #region ConnectedEvents
            _client.Ready += (sender, e) => {
                MyLogger.WriteLine("Client is Ready/Connected! ໒( ͡ᵔ ▾ ͡ᵔ )७", ConsoleColor.Green);
                MyLogger.WriteLine("Setting game...");
                _client.SetGame("gniyalP");
            };
            #endregion

            #region MessageEvents
            _client.MessageDeleted += (sender, e) => {

            };
            _client.MessageUpdated += (sender, e) => {

            };
            _client.MessageReceived += (sender, e) => {
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

            #region ServerEvents
            _client.ServerAvailable += async (sender, e) => {
                MyLogger.Write("Server available! ");
                MyLogger.WriteLine(e.Server.Name, ConsoleColorHelper.GetRoundRobinColor());
                await e.Server.CurrentUser.Edit(nickname: "bundtbot v" + version);
            };
            _client.JoinedServer += (sender, e) => {
                MyLogger.WriteLine("Joined Server! " + e.Server.Name);
            };
            #endregion

            #region UserEvents
            _client.UserBanned += (s, e) => {
            };
            _client.UserUpdated += (s, e) => {
            };
            _client.UserJoined += (s, e) => {
                e.User.Server.DefaultChannel.SendMessage("welcome to server " + e.User.NicknameMention);
                e.User.Server.DefaultChannel.SendMessage("beware of the airhorns...");
            };
            _client.UserUpdated += async (s, e) => {
                var voiceChannelBefore = e.Before.VoiceChannel;
                var voiceChannelAfter = e.After.VoiceChannel;
                if (voiceChannelBefore != voiceChannelAfter) {
                    // Then user's voice channel changed
                    if (voiceChannelBefore != null) {
                        // OnUserLeaveVoiceChannel
                        MyLogger.WriteLine(e.After.Name + " left voice channel: " + voiceChannelBefore);
                    }
                    if (voiceChannelAfter != null) {
                        // OnUserJoinVoiceChannel
                        {
                            if (e.After.IsBot) {
                                MyLogger.WriteLine("Bot joined a voice channel. Ignoring...");
                                return;
                            }
                            // If AFK channel
                            /*if (voiceChannelAfter.) {
                                MyLogger.WriteLine("User joined an AFK voice channel. Ignoring...");
                                return;
                            }*/
                            MyLogger.WriteLine(e.After.Name + " joined voice channel: " + voiceChannelAfter);
                            var list = new[] {
                            Tuple.Create("reinhardt", "hello"),
                            Tuple.Create("genji", "hello"),
                            Tuple.Create("mercy", "hello"),
                            Tuple.Create("torbjorn", "hello"),
                            Tuple.Create("winston", "hi there"),
                            Tuple.Create("suhdude", "#random")
                        };
                            var i = _random.Next(list.Count());
                            var x = list[i];
                            MyLogger.WriteLine("User joined a voice channel. Sending: " + x.Item1 + " " + x.Item2);
                            await _soundBoard.Process(null, voiceChannelAfter, x.Item1, x.Item2);
                        }
                    }
                }
            };
            _client.UserLeft += (sender, e) => {
                e.Server.DefaultChannel.SendMessage("RIP in pieces " + e.User.Nickname);
            };
            #endregion

            #region OtherEvents
            _client.Log.Message += (sender, eventArgs) => {
                Console.WriteLine($"[{eventArgs.Severity}] {eventArgs.Source}: {eventArgs.Message}");
            };
            #endregion

            MyLogger.WriteLine("Done!");
        }

        async Task Cat(CommandEventArgs e) {
            Random rand = new Random();
            if (rand.NextDouble() >= 0.5) {
                using (var webclient = new HttpClient()) {
                    var s = await webclient.GetStringAsync("http://random.cat/meow");
                    int pFrom = s.IndexOf("\\/i\\/") + "\\/i\\/".Length;
                    int pTo = s.LastIndexOf("\"}");
                    string cat = s.Substring(pFrom, pTo - pFrom);
                    Console.WriteLine("http://random.cat/i/" + cat);
                    await e.Channel.SendMessage("I found a cat\nhttp://random.cat/i/" + cat);
                }
            } else {
                await Dog(e, "how about a dog instead");
            }
        }

        async Task Dog(CommandEventArgs e, string message) {
            try {
                using (var client = new HttpClient()) {
                    client.BaseAddress = new Uri("http://random.dog");
                    string dog = await client.GetStringAsync("woof");
                    Console.WriteLine("http://random.dog/" + dog);
                    await e.Channel.SendMessage(message + "\nhttp://random.dog/" + dog);
                }
            } catch (Exception) {
                await e.Channel.SendMessage("there are no dogs here, who let them out (random.dog is down :dog: :interrobang:)");
            }

        }
    }
}
