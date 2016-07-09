using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Discord;
using Discord.Audio;
using Discord.Commands;
using NString;
using WrapYoutubeDl;

namespace BundtBot.BundtBot {
    class Program {
        readonly SoundBoard _soundBoard = new SoundBoard();
        readonly Random _random = new Random();
        readonly SoundManager _soundManager = new SoundManager();
        DiscordClient _client;
        string _version = "0.0";

        const string BotTokenPath = "keys/BotToken.txt";

        static void Main() {
            new Program().Start();
        }

        void Start() {
            // Allows stuff like ʘ ͜ʖ ʘ to show in the Console
            Console.OutputEncoding = Encoding.UTF8;

            // Load Bot Token
            string botToken;

            try {
                botToken = LoadBotToken();
            } catch (Exception ex) {
                MyLogger.WriteException(ex);
                MyLogger.WriteExitMessageAndReadKey();
                return;
            }
            
            const string versionPath = "version.txt";
            if (File.Exists(versionPath)) {
                var versionFloat = float.Parse(File.ReadAllText(versionPath));
                versionFloat += 0.01f;
                _version = versionFloat.ToString("0.00");
            }
            File.WriteAllText(versionPath, _version);

            const string otherVersionPath = "../../version.txt";
            if (File.Exists(otherVersionPath)) {
                File.WriteAllText(otherVersionPath, _version);
            }

            _client = new DiscordClient(x => {
                x.LogLevel = LogSeverity.Debug;
            });

            WriteBundtBotASCIIArtToConsole();
            MyLogger.WriteLine("v" + _version, ConsoleColor.Cyan);
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
        }

        void SetupCommands() {
            var commandService = _client.GetService<CommandService>();

            #region CommandEvents
            commandService.CommandErrored += async (s, e) => {
                if (e.Exception == null) {
                    return;
                }
                MyLogger.WriteLine("[CommandErrored] " + e.Exception.Message, ConsoleColor.DarkMagenta);
                await e.Channel.SendMessage(e.Exception.Message);
            };
            commandService.CommandExecuted += (s, e) => {
                MyLogger.WriteLine("[CommandExecuted] " + e.Command.Text, ConsoleColor.DarkCyan);
            };
            #endregion

            #region boring commands
            commandService.CreateCommand("credits")
                .Alias("github")
                .Description("Prints who made this thing.")
                .Do(async e => {
                    await e.Channel.SendMessage("!owsb <character name> <phrase>"
                        + "\n!yt <youtube search string>"
                        + "\ncreated by @AdenFlorian"
                        + "\nhttps://github.com/AdenFlorian/DiscordSharp_Starter"
                        + "\nhttps://trello.com/b/VKqUgzwV/bundtbot#");
                });
            commandService.CreateCommand("cat")
                .Alias("kitty", "feline", "Felis_catus", "kitten", "🐱", "🐈")
                .Description("It's a secret.")
                .Do(async e => {
                    await CatDog.Cat(e);
                });
            commandService.CreateCommand("dog")
                .Alias("doggy", "puppy", "Canis_lupus_familiaris", "🐶", "🐕")
                .Description("The superior alternaitve to !cat.")
                .Do(async e => {
                    await CatDog.Dog(e, "i found a dog");
                });
            commandService.CreateCommand("admin")
                .Alias("administrator")
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
                .Alias("moderator")
                .Description("Find out if you are a mod.")
                .Do(async e => {
                    if (e.User.Roles.Any(x => x.Name.Equals("mod"))) {
                        await e.Channel.SendMessage("Yes, you are! ┌( ಠ‿ಠ)┘");
                    } else {
                        await e.Channel.SendMessage("No, you aren't (-_-｡)");
                    }
                });
            commandService.CreateCommand("me")
                .Alias("mystatus")
                .Description("Find out you status in life.")
                .Do(async e => {
                    await e.Channel.SendMessage("Voice channel: " + e.User.VoiceChannel?.Name);
                });
            commandService.CreateCommand("invite")
                .Description("Why wasn't I invited?.")
                .Do(async e => {
                    await e.Channel.SendMessage("Click this link to invite me to your server: "
                        + Constants.InviteLink);
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
            commandService.CreateCommand("stop")
                .Alias("shutup", "stfu", "👎", "🚫🎶", "🚫 🎶")
                .Description("Please don't stop the :notes:.")
                .Do(async e => {
                    var msg = await e.Channel.SendMessage("okay...");
                    _soundManager.Stop();
                    await msg.Edit(msg.Text + ":disappointed_relieved:");
                });
            commandService.CreateCommand("next")
                .Alias("skip")
                .Description("Play the next sound.")
                .Do(async e => {
                    if (_soundManager.IsPlaying == false) {
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
                .AddCheck((c, u, x) => u.VoiceChannel != null, Constants.NotInVoice)
                .Alias("owsb")
                .Description("Sound board. It plays sounds with its mouth.")
                .Parameter("sound args", ParameterType.Unparsed)
                .Do(async e => {
                    // Command should have 3 words separated by spaces
                    // 1. !owsb (or !sb)
                    // 2. actor
                    // 3. the sound name (can be multiple words)
                    // So, if we split by spaces, we should have at least 3 parts
                    var actorAndSoundString = e.Args[0];

                    List<string> args;
                    try {
                        // Filter out the arguments (words starting with '--')
                        args = SoundBoard.ExtractArgs(ref actorAndSoundString);
                    } catch (Exception ex) {
                        await e.Channel.SendMessage($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    var actorAndSoundNames = SoundBoard.ParseActorAndSoundNames(actorAndSoundString);

                    var actorName = actorAndSoundNames.Item1;
                    var soundName = actorAndSoundNames.Item2;

                    FileInfo soundFile;
                    
                    if (_soundBoard.TryGetSoundPath(actorName, soundName, out soundFile) == false) {
                        await e.Channel.SendMessage("these are not the sounds you're looking for...");
                        return;
                    }

                    var sound = new Sound(soundFile, e.Channel, e.User.VoiceChannel);

                    try {
                        if (args.Count > 0) {
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
                //.AddCheck((c, u, x) => u.VoiceChannel != null, Constants.NOT_IN_VOICE)
                .Alias("yt", "ytr")
                .Description("It's a tube for you!")
                .Parameter("search string", ParameterType.Unparsed)
                .Do(async e => {
                    var unparsedArgsString = e.Args[0];

                    if (unparsedArgsString.IsNullOrWhiteSpace()) {
                        await e.Channel.SendMessage("http://i1.kym-cdn.com/photos/images/original/000/614/523/644.jpg"
                            + "\ndo it liek dis `!yt This Is Gangsta Rap`");
                        return;
                    }

                    List<string> args;
                    try {
                        // Filter out the arguments (words starting with '--')
                        args = SoundBoard.ExtractArgs(ref unparsedArgsString);
                    } catch (Exception ex) {
                        await e.Channel.SendMessage($"you're doing it wrong ({ex.Message})");
                        return;
                    }
                    
                    var ytSearchString = unparsedArgsString;
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

                    const string mp3OutputFolder = "c:/@mp3/";

                    // See if file exists
                    var possibleSoundFile = new FileInfo(mp3OutputFolder + youtubeVideoID + ".wav");

                    FileInfo outputWAVFile;

                    if (possibleSoundFile.Exists == false) {
                        var youtubeOutput = await new YoutubeDownloader().YoutubeDownloadAndConvert(e, ytSearchString, mp3OutputFolder);
                        var msg = await e.Channel.SendMessage("Download finished! Converting audio...");
                        outputWAVFile = await new FFMPEG().FFMPEGConvert(youtubeOutput);
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

                    var sound = new Sound(outputWAVFile, e.Channel, voiceChannel) {
                        DeleteAfterPlay = false
                    };

                    try {
                        if (args.Count > 0) {
                            SoundBoard.ParseArgs(args, ref sound);
                        }
                    } catch (Exception ex) {
                        await e.Channel.SendMessage($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    _soundManager.EnqueueSound(sound);
                });
            commandService.CreateCommand("volume")
                .Alias("vol", "🔉")
                .Description("turn down fer wut (1 to 10, 1 being down, 10 being fer wut).")
                .Parameter("desired volume")
                .Do(async e => {
                    try {
                        var desiredVolume = float.Parse(e.Args[0]) / 10f;
                        _soundManager.SetVolume(desiredVolume);
                        await e.Channel.SendMessage("is dat betta?");
                    } catch (Exception) {
                        await e.Channel.SendMessage("wat did u doo to dah volumez");
                        throw;
                    }
                });
            #endregion

            #region AliasCommands
            // TODO
            /*commandService.CreateCommand("potg")
                .Description("gratz on your play of the game")
                .Do(async e => {
                    
                });*/
            #endregion
        }

        void WriteBundtBotASCIIArtToConsole() {
            MyLogger.NewLine();
            MyLogger.WriteLine(Constants.BundtbotASCIIArt, ConsoleColor.Red);
            MyLogger.NewLine();
        }

        string LoadBotToken() {
            var token = File.ReadLines(BotTokenPath).First();
            if (token.IsNullOrEmpty()) {
                throw new Exception("Bot token was empty or null after reading it from " + BotTokenPath);
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
                await e.Server.CurrentUser.Edit(nickname: "bundtbot v" + _version);
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
                if (e.Channel.IsAFK) {
                    MyLogger.WriteLine("User joined an AFK voice channel. Ignoring...");
                    return;
                }
                if (_soundManager.IsPlaying) {
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
                var i = _random.Next(list.Length);
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
