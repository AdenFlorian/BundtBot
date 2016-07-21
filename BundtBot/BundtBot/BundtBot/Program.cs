using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.BundtBot.Sound;
using BundtBot.BundtBot.Utility;
using Discord;
using Discord.Audio;
using Discord.Commands;
using NString;
using WrapYoutubeDl;
using Octokit;
using BundtBot.BundtBot.Extensions;
using BundtBot.BundtBot.Models;
using BundtBot.BundtBot.Reddit;
using Discord.Net;
using LiteDB;

namespace BundtBot.BundtBot {
    class Program {
        readonly SoundBoard _soundBoard = new SoundBoard();
        readonly Random _random = new Random();
        readonly SoundManager _soundManager = new SoundManager();
        DiscordClient _client;
        string _version = "0.0";

        const string Mp3OutputFolder = "c:/@mp3/";
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

            while (true) {
                try {
                    _client.ExecuteAndWait(async () => {
                        await _client.Connect(botToken);
                    });
                    //break;
                } catch (Exception ex) {
                    MyLogger.WriteLine("***CAUGHT TOP LEVEL EXCEPTION***", ConsoleColor.DarkMagenta);
                    MyLogger.WriteException(ex);
                }
            }
            
        }

        void SetupCommands() {
            var commandService = _client.GetService<CommandService>();

            #region CommandEvents
            commandService.CommandErrored += async (s, e) => {
                if (e.Exception == null) return;
                MyLogger.WriteException(e.Exception, "[CommandErrored]");
                await e.Channel.SendMessage($"bundtbot is brokebot, something broke while processing someone's `{e.Command.Text}` command :(");
            };
            commandService.CommandExecuted += (s, e) => {
                MyLogger.WriteLine("[CommandExecuted] " + e.Command.Text, ConsoleColor.DarkCyan);
            };
            #endregion

            #region boring commands
            commandService.CreateCommand("credits")
                .Description("Prints who made this thing.")
                .Do(async e => {
                    await e.Channel.SendMessage("!owsb <character name> <phrase>"
                        + "\n!yt <youtube search string>"
                        + "\ncreated by @AdenFlorian"
                        + "\nhttps://github.com/AdenFlorian/DiscordSharp_Starter"
                        + "\nhttps://trello.com/b/VKqUgzwV/bundtbot#");
                });
            commandService.CreateCommand("github")
                .Alias("git", "🐙 🐱", "🐙🐱")
                .Description("people tell me i need ot get help.")
                .Do(async e => {
                    await e.Channel.SendMessage("https://github.com/AdenFlorian/DiscordSharp_Starter");
                });
            commandService.CreateCommand("changelog")
                .Alias("what's new")
                .Description("cha cha cha chaaangeeeessss.")
                .Parameter("number of commits to pull", ParameterType.Optional)
                .Do(async e => {
                    var arg1 = e.GetArg("number of commits to pull");
                    if (arg1.IsNullOrWhiteSpace()) arg1 = "5";
                    var numOfCommitsToPull = int.Parse(arg1);
                    if (numOfCommitsToPull > 42) numOfCommitsToPull = 42;
                    if (numOfCommitsToPull < 1) numOfCommitsToPull = 5;
                    // Get last 5 commit messages from the AdenFlorian/BundtBot github project
                    var client = new GitHubClient(new ProductHeaderValue("AdenFlorian-BundtBot"));
                    var commits = await client.Repository.Commit.GetAll("AdenFlorian", "BundtBot");
                    var fiveCommits = commits.Take(numOfCommitsToPull).ToList();
                    var msg = "";
                    fiveCommits.ForEach(x => msg += "🔹 " + x.Commit.Message + "\n");
                    var xx = numOfCommitsToPull.ToString().Select(x => x.ToString()).ToList();
                    var numberEmojiString = "";
                    xx.ForEach(num => numberEmojiString += ":" + int.Parse(num).ToVerbal() + ":");
                    await e.Channel.SendMessage($"Last {numberEmojiString} commits from `AdenFlorian/BundtBot` on github:");
                    await e.Channel.SendMessage(msg);
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
                        var admins = e.Server.Users.Where(x => x.ServerPermissions.Administrator).ToList();
                        admins.ForEach(x => msg += $" | {x.Name} | ");
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
                    await msg.Edit(msg.Text + ":zipper_mouth:");
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
                        var msg = await e.Channel.SendMessage("standby...");
                        _soundManager.Skip();
                        await msg.Edit(msg.Text + "Clip has been terminated, and it's parents have been notified. The next clip in line has taken its place. How do you sleep at night.");
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
                    var actorAndSoundString = e.Args[0].ToLower();

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

                    var sound = new Sound.Sound(soundFile, e.Channel, e.User.VoiceChannel, $"{soundFile.Directory?.Name}: {soundFile.Name}");

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

                    AudioClip clip;

                    // Check DB for clip where title matches search string
                    // Open database (or create if not exits)
                    using (var db = new LiteDatabase(@"MyData.db")) {
                        // Get customer collection
                        var searchStrings = db.GetCollection<YoutubeSearchString>("YoutubeSearchStrings");
                        var clips = db.GetCollection<AudioClip>("AudioClips");

                        clips.EnsureIndex(x => x.Id);
                        searchStrings.EnsureIndex(x => x.Text);

                        var clipId = searchStrings.FindOne(x => x.Text == ytSearchString)?.AudioClipId;

                        // Use Linq to query documents
                        clip = clips.FindOne(x => x.Id == clipId);

                        //var allclips = clips.FindAll();
                        //foreach (var audioClip in allclips) {
                        //    MyLogger.WriteLine(audioClip.ToString());
                        //}
                        //var allsearchstrings = searchStrings.FindAll();
                        //foreach (var searchstring in allsearchstrings) {
                        //    MyLogger.WriteLine(searchstring.ToString());
                        //}
                    }

                    FileInfo outputWAVFile;

                    if (clip == null) {
                        await e.Channel.SendMessage("Searching youtube for: " + ytSearchString);

                        // Get video id
                        MyLogger.WriteLine("Getting youtube video id...");
                        var youtubeVideoID = await new YoutubeVideoID().Get($"\"ytsearch1:{ytSearchString}\"");
                        MyLogger.WriteLine("Youtube video ID get! " + youtubeVideoID, ConsoleColor.Green);

                        // Get video name
                        MyLogger.WriteLine("Getting youtube video title...");
                        var youtubeVideoTitle = await new YoutubeVideoName().Get($"\"ytsearch1:{ytSearchString}\"");
                        MyLogger.WriteLine("Youtube video title get! " + youtubeVideoTitle, ConsoleColor.Green);
                        await e.Channel.SendMessage("Found video: " + youtubeVideoTitle);
                        
                        FileInfo youtubeOutput;
                        if (ytSearchString.Contains("youtube.com/watch?")) {
                            youtubeOutput =
                                await
                                    new YoutubeDownloader().YoutubeDownloadAndConvertAsync(e, ytSearchString,
                                        Mp3OutputFolder);
                        }
                        else {
                            youtubeOutput =
                                await
                                    new YoutubeDownloader().YoutubeDownloadAndConvertAsync(e,
                                        $"\"ytsearch1:{ytSearchString}\"", Mp3OutputFolder);
                        }
                        var msg = await e.Channel.SendMessage("Download finished! Converting audio...");
                        outputWAVFile = await new FFMPEG().FFMPEGConvertAsync(youtubeOutput);
                        await msg.Edit(msg.Text + "finished!");

                        if (outputWAVFile.Exists == false) {
                            await e.Channel.SendMessage("that video doesn't work, sorry, try something else");
                            return;
                        }
                        
                        using (var db = new LiteDatabase(@"MyData.db")) {
                            // Get customer collection
                            var clips = db.GetCollection<AudioClip>("AudioClips");
                            clip = null;
                            clips.EnsureIndex(x => x.YoutubeID);
                            clip = clips.FindOne(x => x.YoutubeID == youtubeVideoID);

                            if (clip == null) {
                                clip = new AudioClip {
                                    Title = youtubeVideoTitle,
                                    Path = outputWAVFile.FullName,
                                    YoutubeID = youtubeVideoID
                                };
                                clip = clips.FindById(clips.Insert(clip));
                            }

                            var searchStrings = db.GetCollection<YoutubeSearchString>("YoutubeSearchStrings");
                            if (searchStrings.Exists(x => x.Text == ytSearchString) == false) {
                                searchStrings.Insert(new YoutubeSearchString {
                                    Text = ytSearchString,
                                    AudioClipId = clip.Id
                                });
                            }
                        }
                    }
                    else {
                        outputWAVFile = new FileInfo(clip.Path);
                    }

                    var sound = new Sound.Sound(outputWAVFile, e.Channel, voiceChannel, clip.Title) {
                        DeleteAfterPlay = false
                    };

                    try {
                        // Defaulting youtube volume to 5 because they are long
                        sound.Volume = 0.5f;
                        if (args.Count > 0) {
                            SoundBoard.ParseArgs(args, ref sound);
                        }
                    } catch (Exception ex) {
                        await e.Channel.SendMessage($"you're doing it wrong ({ex.Message})");
                        return;
                    }
                    _soundManager.EnqueueSound(sound);
                });
            commandService.CreateCommand("youtube_haiku")
                .Alias("ythaiku", "ythk")
                .Description("It's snowing on mt fuji")
                .Parameter("search string", ParameterType.Unparsed)
                .Do(async e => {
                    List<string> args;
                    try {
                        // Filter out the arguments (words starting with '--')
                        args = SoundBoard.ExtractArgs(ref e.Args[0]);
                    } catch (Exception ex) {
                        await e.Channel.SendMessage($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    var voiceChannel = e.User.VoiceChannel;

                    if (voiceChannel == null) {
                        await e.Channel.SendMessage("you need to be in a voice channel to hear me roar");
                        return;
                    }

                    var haikuMsg = await e.Channel.SendMessage("☢HAIKU INCOMING☢");
                    
                    var haikuUrl = await RedditManager.GetYoutubeHaikuUrlAsync();

                    await haikuMsg.Edit(haikuMsg.Text + $": {haikuUrl.AbsoluteUri}");

                    var youtubeOutput = await new YoutubeDownloader().YoutubeDownloadAndConvertAsync(e, haikuUrl.AbsoluteUri, Mp3OutputFolder);
                    var msg = await e.Channel.SendMessage("Download finished! Converting audio...");
                    var outputWAVFile = await new FFMPEG().FFMPEGConvertAsync(youtubeOutput);
                    await msg.Edit(msg.Text + "finished!");

                    if (outputWAVFile.Exists == false) {
                        await e.Channel.SendMessage("that haiku didn't work, sorry, try something else");
                        return;
                    }

                    var youtubeVideoTitle = await new YoutubeVideoName().Get(haikuUrl.AbsoluteUri);
                    
                    var sound = new Sound.Sound(outputWAVFile, e.Channel, voiceChannel, youtubeVideoTitle) {
                        DeleteAfterPlay = true
                    };

                    try {
                        // Defaulting haikus volume to 8 because they are short
                        sound.Volume = 0.8f;
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
                        _soundManager.SetVolumeOverride(desiredVolume);
                        await e.Channel.SendMessage($"global volume set to {desiredVolume * 10}");
                    } catch (Exception) {
                        await e.Channel.SendMessage("wat did u doo to dah volumez");
                        throw;
                    }
                });
            commandService.CreateCommand("tempvolume")
                .Alias("tempvol", "voltemp", "🔉")
                .Description("turn down fer wut (1 to 10, 1 being down, 10 being fer wut).")
                .Parameter("desired volume")
                .Do(async e => {
                    try {
                        var desiredVolume = float.Parse(e.Args[0]) / 10f;
                        _soundManager.SetVolumeOfCurrentClip(desiredVolume);
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

        static void WriteBundtBotASCIIArtToConsole() {
            MyLogger.NewLine();
            MyLogger.WriteLine(Constants.BundtbotASCIIArt, ConsoleColor.Red);
            MyLogger.NewLine();
        }

        static string LoadBotToken() {
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
                try {
                    await e.Server.CurrentUser.Edit(nickname: "bundtbot v" + _version);
                }
                catch (HttpException ex) {
                    MyLogger.WriteLine($"{ex.GetType()} thrown from trying to change the bot's nickname",
                        ConsoleColor.DarkYellow);
                    MyLogger.WriteLine("The bot might not have permission on that server to change it's nickname",
                        ConsoleColor.DarkYellow);
                }
                catch (Exception ex) {
                    MyLogger.WriteException(ex);
                }
            };
            _client.JoinedServer += (sender, e) => {
                MyLogger.WriteLine("Joined Server! " + e.Server.Name);
            };
            #endregion

            #region UserEvents
            _client.UserBanned += (s, e) => {
            };
            _client.UserJoined += async (s, e) => {
                await e.User.Server.DefaultChannel.SendMessage("welcome to server " + e.User.NicknameMention);
                await e.User.Server.DefaultChannel.SendMessage("beware of the airhorns...");
            };
            _client.UserUpdated += async (s, e) => {
                var voiceChannelBefore = e.Before.VoiceChannel;
                var voiceChannelAfter = e.After.VoiceChannel;
                if (voiceChannelBefore == voiceChannelAfter) return;
                if (voiceChannelBefore != null) {
                    await OnUserLeftVoiceChannel(new ChannelUserEventArgs(voiceChannelBefore, e.After));
                }
                if (voiceChannelAfter != null) {
                    OnUserJoinedVoiceChannel(new ChannelUserEventArgs(voiceChannelAfter, e.After));
                }
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

        async Task OnUserLeftVoiceChannel(ChannelUserEventArgs e) {
            if (e.Channel != _soundManager.VoiceChannel) return;
            if (e.Channel.Users.Count() > 1) return;
            if (_soundManager.CurrentlyPlayingSound.TextUpdates) {
                await e.Channel.Server.DefaultChannel.SendMessage("sorry i bothered you with my 🎶");
            }
            MyLogger.WriteLine("[Program] OnUserLeftVoiceChannel - Telling SoundManager to stop," +
                               " because we are the last user in channel");
            _soundManager.Stop();
        }

        void OnUserJoinedVoiceChannel(ChannelUserEventArgs e) {
            if (e.User.IsBot) {
                MyLogger.WriteLine("Bot joined a voice channel. Ignoring...");
                return;
            }
            if (e.Channel.IsAFK()) {
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
            var sound = new Sound.Sound(soundFile, e.Channel.Server.DefaultChannel, e.Channel, $"{x.Item1}: {x.Item2}") {TextUpdates = false};
            _soundManager.EnqueueSound(sound);
        }
    }
}
