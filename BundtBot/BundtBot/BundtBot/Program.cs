using Discord;
using Discord.Audio;
using Discord.Commands;
using NString;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WrapYoutubeDl;

namespace BundtBot.BundtBot {
    class Program {
        SoundBoard _soundBoard = new SoundBoard();
        Random _random = new Random();
        DiscordClient _client;
        SoundManager _soundManager = new SoundManager();
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
                    await CatDog.Cat(e);
                });
            commandService.CreateCommand("dog")
                .Alias(new string[] { "doggy", "puppy", "Canis_lupus_familiaris", "🐶", "🐕" })
                .Description("The superior alternaitve to !cat.")
                .Do(async e => {
                    await CatDog.Dog(e, "i found a dog");
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
            commandService.CreateCommand("invite")
                .Description("Why wasn't I invited?.")
                .Do(async e => {
                    await e.Channel.SendMessage("Click this link to invite me to your server: "
                        + Constants.INVITE_LINK);
                });
            commandService.CreateCommand("bot")
                .Description("Why wasn't I invited?.")
                .Do(async e => {
                    Thread.Sleep(1000);
                    var msg = await e.Channel.SendMessage("bundtbot");
                    Thread.Sleep(2000);
                    await msg.Edit(msg.Text + " is");
                    Thread.Sleep(3000);
                    var msg2 = await e.Channel.SendMessage(":back:");
                    Thread.Sleep(333);
                    await msg2.Edit(msg2.Text + ":on:");
                    Thread.Sleep(333);
                    await msg2.Edit(msg2.Text + ":on:" + ":top:");
                });
            #endregion

            #region SoundBoard
            // TODO Fix !stop
            commandService.CreateCommand("stop")
                .Alias(new string[] { "shutup", "stfu", "👎", "🚫🎶", "🚫 🎶" })
                .Description("Please don't stop the :notes:.")
                .Do(async e => {
                    var msg = await e.Channel.SendMessage("okay...");
                    _soundManager.Stop();
                    await msg.Edit(msg.Text + ":disappointed_relieved:");
                });
            commandService.CreateCommand("next")
                .Alias(new string[] { "skip" })
                .Description("Play the next sound.")
                .Do(async e => {
                    if (_soundManager.isPlaying == false) {
                        await e.Channel.SendMessage("there's nothing to skip");
                        return;
                    }
                    if (_soundManager.HasThingsInQueue == false) {
                        var msg = await e.Channel.SendMessage("end of line");
                        _soundManager.Skip();
                        await msg.Edit(msg.Text + " :stop_button:");
                    } else {
                        var msg = await e.Channel.SendMessage("sure thing boss...");
                        _soundManager.Skip();
                        await msg.Edit(msg.Text + "is this what you wanted?");
                    }
                });
            commandService.CreateCommand("sb")
                .AddCheck((c, u, x) => u.VoiceChannel != null, Constants.NOT_IN_VOICE)
                .Alias(new string[] { "owsb" })
                .Description("Sound board. It plays sounds with its mouth.")
                .Parameter("sound args", ParameterType.Unparsed)
                .Do(async e => {
                    // Command should have 3 words separated by spaces
                    // 1. !owsb (or !sb)
                    // 2. actor
                    // 3. the sound name (can be multiple words)
                    // So, if we split by spaces, we should have at least 3 parts
                    var commandString = e.Message.Text.Trim();
                    List<string> words = new List<string>(commandString.Split(' '));

                    List<string> args;
                    try {
                        // Filter out the arguments (words starting with '--')
                        args = SoundBoard.ExtractArgs(words);
                    } catch (Exception ex) {
                        await e.Channel.SendMessage($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    var actorAndSoundNames = SoundBoard.ParseActorAndSoundNames(words);

                    var actorName = actorAndSoundNames.Item1;
                    var soundName = actorAndSoundNames.Item2;

                    if (words.Count > 3) {
                        for (int i = 3; i < words.Count; i++) {
                            soundName += " " + words[i];
                        }
                    }

                    FileInfo soundFile;
                    
                    if (_soundBoard.TryGetSoundPath(actorName, soundName, out soundFile) == false) {
                        await e.Channel.SendMessage("these are not the sounds you're looking for...");
                        return;
                    }

                    Sound sound = null;
                    sound = new Sound(soundFile, e.Channel, e.User.VoiceChannel);
                    
                    if (sound == null) {
                        await e.Channel.SendMessage("you're doing it wrong (or something broke)");
                        return;
                    }

                    try {
                        if (args.Count() > 0) {
                            SoundBoard.ParseArgs(args, ref sound);
                        }
                    } catch (Exception ex) {
                        await e.Channel.SendMessage($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    _soundManager.EnqueueSound(sound);
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

                    var voiceChannel = e.User.VoiceChannel;

                    if (voiceChannel == null) {
                        await e.Channel.SendMessage("you need to be in a voice channel to hear me roar");
                        return;
                    }

                    await e.Channel.SendMessage("Searching youtube for: " + ytSearchString);

                    // Get video id
                    MyLogger.WriteLine("Getting youtube video id...");
                    var youtubeVideoID = await new YoutubeVideoID().Get(ytSearchString);
                    MyLogger.WriteLine("Youtube video ID get! " + youtubeVideoID, ConsoleColor.Green);

                    // Get video name
                    MyLogger.WriteLine("Getting youtube video title...");
                    var youtubeVideoTitle = await new YoutubeVideoName().Get(ytSearchString);
                    MyLogger.WriteLine("Youtube video title get! " + youtubeVideoTitle, ConsoleColor.Green);
                    await e.Channel.SendMessage("Found video: " + youtubeVideoTitle);

                    var mp3OutputFolder = "c:/@mp3/";

                    // See if file exists
                    var possibleSoundFile = new FileInfo(mp3OutputFolder + youtubeVideoID + ".wav");

                    FileInfo outputWAVFile;

                    if (possibleSoundFile.Exists == false) {
                        string youtubeOutput = await new YoutubeDownloader().YoutubeDownloadAndConvert(e, ytSearchString, mp3OutputFolder);
                        var msg = await e.Channel.SendMessage("Download finished! Converting audio...");
                        outputWAVFile = await new FFMPEG().ffmpegConvert(youtubeOutput);
                        await msg.Edit(msg.Text + "finished! Sending data...");
                    } else {
                        MyLogger.WriteLine("WAV file exists already! " + possibleSoundFile.FullName, ConsoleColor.Green);
                        outputWAVFile = possibleSoundFile;
                        await e.Channel.SendMessage("Playing audio from cache...");
                    }

                    if (outputWAVFile.Exists == false) {
                        await e.Channel.SendMessage("that video doesn't work, sorry, try something else");
                        return;
                    }

                    var sound = new Sound(outputWAVFile, e.Channel, voiceChannel);
                    sound.deleteAfterPlay = false;
                    
                    _soundManager.EnqueueSound(sound);
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
            _client.UserUpdated += (s, e) => {
            };
            _client.UserJoinedVoiceChannel += (s, e) => {
                if (e.User.IsBot) {
                    MyLogger.WriteLine("Bot joined a voice channel. Ignoring...");
                    return;
                }
                // If AFK channel
                /*if (voiceChannelAfter.) {
                    MyLogger.WriteLine("User joined an AFK voice channel. Ignoring...");
                    return;
                }*/
                if (_soundManager.isPlaying) {
                    MyLogger.WriteLine("_soundManager.HasThingsInQueue() is true. Ignoring...");
                    return;
                }
                MyLogger.WriteLine(e.User.Name + " joined voice channel: " + e.Channel);
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
                FileInfo soundFile;
                if (_soundBoard.TryGetSoundPath(x.Item1, x.Item2, out soundFile) == false) {
                    MyLogger.WriteException(new FileNotFoundException("Couldn't Find Sound but should have"));
                    return;
                }
                var sound = new Sound(soundFile, e.Channel.Server.DefaultChannel, e.Channel);
                _soundManager.EnqueueSound(sound, false);
            };
            _client.UserLeftVoiceChannel += (s, e) => {
                MyLogger.WriteLine(e.User.Name + " left voice channel: " + e.Channel);
            };
            _client.UserLeft += (sender, e) => {
                // Can't send message to server if we just left it
                if (e.User.Id == _client.CurrentUser.Id) {
                    return;
                }
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
    }
}
