using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Database;
using BundtBot.Extensions;
using BundtBot.Models;
using BundtBot.Reddit;
using BundtBot.Sound;
using BundtBot.Utility;
using BundtBot.Youtube;
using Discord.Commands;
using NString;
using Octokit;

namespace BundtBot {
    class Commands {
        public static void Register(CommandService commandService, SoundManager soundManager, SoundBoard soundBoard, string songCachePath) {

            #region CommandEvents

            commandService.CommandErrored += async (s, e) => {
                if (e.Exception == null) return;
                MyLogger.WriteException(e.Exception, "[CommandErrored]");
                await
                    e.Channel.SendMessageEx(
                        $"bundtbot is brokebot, something broke while processing someone's `{e.Command.Text}` command :(");
            };
            commandService.CommandExecuted += (s, e) => {
                MyLogger.WriteLine("[CommandExecuted] " + e.Command.Text, ConsoleColor.DarkCyan);
            };

            #endregion

            #region boring commands

            commandService.CreateCommand("credits")
                .Description("Prints who made this thing.")
                .Do(async e => {
                    await e.Channel.SendMessageEx("!owsb <character name> <phrase>"
                                                  + "\n!yt <youtube search string>"
                                                  + "\ncreated by @AdenFlorian"
                                                  + "\nhttps://github.com/AdenFlorian/DiscordSharp_Starter"
                                                  + "\nhttps://trello.com/b/VKqUgzwV/bundtbot#");
                });
            commandService.CreateCommand("github")
                .Alias("git", "🐙 🐱", "🐙🐱")
                .Description("people tell me i need ot get help.")
                .Do(async e => {
                    await e.Channel.SendMessageEx("https://github.com/AdenFlorian/DiscordSharp_Starter");
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
                    await
                        e.Channel.SendMessageEx(
                            $"Last {numberEmojiString} commits from `AdenFlorian/BundtBot` on github:");
                    await e.Channel.SendMessageEx(msg);
                });
            commandService.CreateCommand("cat")
                .Alias("kitty", "feline", "Felis_catus", "kitten", "🐱", "🐈")
                .Description("It's a secret.")
                .Do(async e => {
                    var rand = new Random();
                    if (rand.NextDouble() >= 0.5) {
                        try {
                            var cat = await CatDog.Cat();
                            await e.Channel.SendMessageEx("I found a cat\n" + cat);
                        } catch (Exception ex) {
                            MyLogger.WriteException(ex);
                            await e.Channel.SendMessageEx("there are no cats here, who let them out (random.cat is down :cat: :interrobang:)");
                        }
                    } else {
                        var dog = await CatDog.Dog();
                        await e.Channel.SendMessageEx("how about a dog instead" + dog);
                    }
                });
            commandService.CreateCommand("dog")
                .Alias("doggy", "puppy", "Canis_lupus_familiaris", "🐶", "🐕")
                .Description("The superior alternaitve to !cat.")
                .Do(async e => {
                    try {
                        var dog = await CatDog.Dog();
                        await e.Channel.SendMessageEx("i found a dog" + dog);
                    } catch (Exception ex) {
                        MyLogger.WriteException(ex);
                        await e.Channel.SendMessageEx("there are no dogs here, who let them out (random.dog is down :dog: :interrobang:)");
                    }
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
                    await e.Channel.SendMessageEx(msg);
                });
            commandService.CreateCommand("mod")
                .Alias("moderator")
                .Description("Find out if you are a mod.")
                .Do(async e => {
                    if (e.User.Roles.Any(x => x.Name.Equals("mod"))) {
                        await e.Channel.SendMessageEx("Yes, you are! ┌( ಠ‿ಠ)┘");
                    } else {
                        await e.Channel.SendMessageEx("No, you aren't (-_-｡)");
                    }
                });
            commandService.CreateCommand("me")
                .Alias("mystatus")
                .Description("Find out you status in life.")
                .Do(async e => {
                    await e.Channel.SendMessageEx("Voice channel: " + e.User.VoiceChannel?.Name);
                });
            commandService.CreateCommand("invite")
                .Description("Why wasn't I invited?.")
                .Do(async e => {
                    await e.Channel.SendMessageEx("Click this link to invite me to your server: "
                                                  + Constants.InviteLink);
                });
            commandService.CreateCommand("bot")
                .Description("Why wasn't I invited?.")
                .Do(async e => {
                    Thread.Sleep(1000);
                    var msg = await e.Channel.SendMessageEx("bundtbot");
                    Thread.Sleep(2000);
                    await msg.Edit(msg.Text + " is");
                    Thread.Sleep(3000);
                    var msg2 = await e.Channel.SendMessageEx(":back:");
                    Thread.Sleep(333);
                    await msg2.Edit(msg2.Text + ":on:");
                    Thread.Sleep(333);
                    await msg2.Edit(msg2.Text + ":on:" + ":top:");
                });

            #endregion

            #region DBCommands

            commandService.CreateCommand("users")
                .Do(async e => {
                    await e.Channel.SendMessageEx($"I have {DB.Users.FindAll().Count()} users registered");
                });

            #endregion

            #region SoundBoard

            commandService.CreateCommand("stop")
                .Alias("shutup", "stfu", "👎", "🚫🎶", "🚫 🎶")
                .Description("Please don't stop the :notes:.")
                .Do(async e => {
                    var msg = await e.Channel.SendMessageEx("okay...");
                    soundManager.Stop();
                    await msg.Edit(msg.Text + ":zipper_mouth:");
                });
            commandService.CreateCommand("next")
                .Alias("skip")
                .Description("Play the next Track.")
                .Do(async e => {
                    if (soundManager.IsPlaying == false) {
                        await e.Channel.SendMessageEx("there's nothing to skip");
                        return;
                    }
                    if (soundManager.HasThingsInQueue == false) {
                        var msg = await e.Channel.SendMessageEx("end of line");
                        soundManager.Skip();
                        await msg.Edit(msg.Text + " :stop_button:");
                    } else {
                        var msg = await e.Channel.SendMessageEx("standby...");
                        soundManager.Skip();
                        await
                            msg.Edit(msg.Text +
                                     "Track has been terminated, and it's parents have been notified. The next track in line has taken its place. How do you sleep at night.");
                    }
                });
            commandService.CreateCommand("upnext")
                .Alias("whatsnext", "peek")
                .Description("look at next track")
                .Do(async e => {
                    if (soundManager.IsPlaying == false) {
                        await e.Channel.SendMessageEx("there's nothing up next, because nothing is even playing...");
                        return;
                    }
                    if (soundManager.HasThingsInQueue == false) {
                        await e.Channel.SendMessageEx("nuthin");
                    } else {
                        var nextSound = soundManager.PeekNext();
                        if (nextSound == null) {
                            await e.Channel.SendMessageEx("i thought there was something up next, " +
                                                          "but i may have been wrong, so, umm, sorry?");
                        } else {
                            await e.Channel.SendMessageEx($"Up Next: **{nextSound.Track.Title}**");
                        }
                    }
                });
            commandService.CreateCommand("nowplaying")
                .Do(async e => {
                    if (soundManager.IsPlaying == false) {
                        await e.Channel.SendMessageEx("nothing");
                        return;
                    }
                    await e.Channel.SendMessageEx($"**{soundManager.CurrentlyPlayingTrackRequest.Track.Title}**");
                });
            commandService.CreateCommand("like")
                .Alias("thumbsup", "upvote", "👍")
                .Description("bundtbot for president 2020")
                .Do(async e => {
                    // Find out what song is playing
                    var currentSound = soundManager.CurrentlyPlayingTrackRequest;

                    if (currentSound == null) {
                        await e.Channel.SendMessageEx($"what's to like? nothing is playing...");
                        return;
                    }

                    var user = DB.Users.FindOne(x => x.SnowflakeId == e.User.Id);
                    var track = DB.Tracks.FindById(currentSound.Track.Id);
                    var likes = DB.Likes.Find(x => x.UserId == user.Id);

                    if (likes.Any(x => x.TrackId == track.Id) == false) {
                        DB.Likes.Insert(new Like {
                            UserId = user.Id,
                            TrackId = track.Id
                        });
                        await e.Channel.SendMessageEx($"i like **{track.Title}** too 😎");
                    } else {
                        await e.Channel.SendMessageEx($"i already know that you like **{track.Title}**");
                    }
                });
            commandService.CreateCommand("mylikes")
                .Do(async e => {
                    var user = DB.Users.FindOne(x => x.SnowflakeId == e.User.Id);
                    var likes = DB.Likes
                        .Find(x => x.UserId == user.Id);
                    var msg = "**Your Likes:**\n";
                    likes.ToList().ForEach(x => msg += DB.Tracks.FindById(x.TrackId).Title + "\n");
                    await e.User.SendMessageEx(msg);
                });
            commandService.CreateCommand("sb")
                .Alias("owsb")
                .Description("Sound board. It plays sounds with its mouth.")
                .Parameter("Sound args", ParameterType.Unparsed)
                .Do(async e => {
                    // Command should have 3 words separated by spaces
                    // 1. !owsb (or !sb)
                    // 2. actor
                    // 3. the Sound name (can be multiple words)
                    // So, if we split by spaces, we should have at least 3 parts
                    var actorAndSoundString = e.Args[0].ToLower();

                    List<string> args;
                    try {
                        // Filter out the arguments (words starting with '--')
                        args = SoundBoard.ExtractArgs(ref actorAndSoundString);
                    } catch (Exception ex) {
                        await e.Channel.SendMessageEx($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    if (e.User.VoiceChannel == null) {
                        await e.Channel.SendMessageEx(Constants.NotInVoice);
                        return;
                    }

                    var actorAndSoundNames = SoundBoard.ParseActorAndSoundNames(actorAndSoundString);

                    var actorName = actorAndSoundNames.Item1;
                    var soundName = actorAndSoundNames.Item2;

                    FileInfo soundFile;

                    if (soundBoard.TryGetSoundPath(actorName, soundName, out soundFile) == false) {
                        await e.Channel.SendMessageEx("these are not the sounds you're looking for...");
                        return;
                    }

                    var track = new Track {
                        Path = soundFile.FullName,
                        Title = $"{soundFile.Directory?.Name}: {soundFile.Name}"
                    };

                    var sound = new Sound.TrackRequest(track, e.Channel, e.User.VoiceChannel);

                    try {
                        if (args.Count > 0) {
                            SoundBoard.ParseArgs(args, ref sound);
                        }
                    } catch (Exception ex) {
                        await e.Channel.SendMessageEx($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    soundManager.EnqueueSound(sound);
                });
            commandService.CreateCommand("youtube")
                .Alias("yt", "ytr")
                .Description("It's a tube for you!")
                .Parameter("search string", ParameterType.Unparsed)
                .Do(async e => {
                    var unparsedArgsString = e.Args[0];

                    if (unparsedArgsString.IsNullOrWhiteSpace()) {
                        await e.Channel.SendMessageEx("http://i1.kym-cdn.com/photos/images/original/000/614/523/644.jpg"
                                                      + "\ndo it liek dis `!yt This Is Gangsta Rap`");
                        return;
                    }

                    List<string> args;
                    try {
                        args = SoundBoard.ExtractArgs(ref unparsedArgsString);
                    } catch (Exception ex) {
                        await e.Channel.SendMessageEx($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    var ytSearchString = unparsedArgsString;
                    var voiceChannel = e.User.VoiceChannel;

                    if (voiceChannel == null) {
                        await e.Channel.SendMessageEx(Constants.NotInVoice);
                        return;
                    }

                    Track track;

                    string youtubeVideoID;
                    
                    if (YoutubeHelper.IsYoutubeUrl(ytSearchString)) {
                        youtubeVideoID = YoutubeHelper.GetVideoIdFromUrl(ytSearchString);
                    } else {
                        youtubeVideoID = await GetYoutubeVideoIdBySearchString(ytSearchString);
                    }
                    
                    if (Track.TryGetTrackByYoutubeId(youtubeVideoID, out track)) {
                        track.AddSearchString(ytSearchString);
                    }

                    if (track == null) {
                        track = await GetTrackByYoutubeId(e, youtubeVideoID, songCachePath);
                        if (track == null) return;
                        track.AddSearchString(ytSearchString);
                    } else if (File.Exists(track.Path) == false) {
                        await RedownloadTrack(e, track, songCachePath);
                    }

                    var sound = new Sound.TrackRequest(track, e.Channel, voiceChannel) {
                        DeleteAfterPlay = false
                    };

                    try {
                        // Defaulting youtube volume to 5 because they are long
                        sound.Volume = 0.5f;
                        if (args.Count > 0) SoundBoard.ParseArgs(args, ref sound);
                    } catch (Exception ex) {
                        await e.Channel.SendMessageEx($"you're doing it wrong ({ex.Message})");
                        return;
                    }
                    soundManager.EnqueueSound(sound);
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
                        await e.Channel.SendMessageEx($"you're doing it wrong ({ex.Message})");
                        return;
                    }

                    var voiceChannel = e.User.VoiceChannel;

                    if (voiceChannel == null) {
                        await e.Channel.SendMessageEx("you need to be in a voice channel to hear me roar");
                        return;
                    }

                    var haikuMsg = await e.Channel.SendMessageEx("☢HAIKU INCOMING☢");

                    var haikuUrl = await RedditManager.GetYoutubeHaikuUrlAsync();

                    await haikuMsg.Edit(haikuMsg.Text + $": {haikuUrl.AbsoluteUri}");

                    var youtubeOutput =
                        await
                            new YoutubeDownloader().YoutubeDownloadAndConvertAsync(e, haikuUrl.AbsoluteUri,
                                songCachePath);
                    var msg = await e.Channel.SendMessageEx("Download finished! Converting audio...");
                    var outputWAVFile = await new FFMPEG.FFMPEG().FFMPEGConvertToWAVAsync(youtubeOutput);
                    await msg.Edit(msg.Text + "finished!");

                    if (outputWAVFile.Exists == false) {
                        await e.Channel.SendMessageEx("that haiku didn't work, sorry, try something else");
                        return;
                    }

                    var youtubeVideoTitle = await new Youtube.Youtube().GetVideoTitle(haikuUrl.AbsoluteUri);

                    var track = new Track {
                        Title = youtubeVideoTitle,
                        Path = outputWAVFile.FullName
                    };

                    var sound = new Sound.TrackRequest(track, e.Channel, voiceChannel) {
                        DeleteAfterPlay = true
                    };

                    try {
                        // Defaulting haikus volume to 8 because they are short
                        sound.Volume = 0.8f;
                        if (args.Count > 0) {
                            SoundBoard.ParseArgs(args, ref sound);
                        }
                    } catch (Exception ex) {
                        await e.Channel.SendMessageEx($"you're doing it wrong ({ex.Message})");
                        return;
                    }
                    soundManager.EnqueueSound(sound);
                });
            commandService.CreateCommand("volume")
                .Alias("vol", "🔉")
                .Description("turn down fer wut (1 to 10, 1 being down, 10 being fer wut).")
                .Parameter("desired volume")
                .Do(async e => {
                    try {
                        var desiredVolume = float.Parse(e.Args[0]) / 10f;
                        soundManager.SetVolumeOverride(desiredVolume);
                        await e.Channel.SendMessageEx($"global volume set to {desiredVolume * 10}");
                    } catch (Exception) {
                        await e.Channel.SendMessageEx("wat did u doo to dah volumez");
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
                        soundManager.SetVolumeOfCurrentTrack(desiredVolume);
                        await e.Channel.SendMessageEx("is dat betta?");
                    } catch (Exception) {
                        await e.Channel.SendMessageEx("wat did u doo to dah volumez");
                        throw;
                    }
                });
            #endregion
        }

        static async Task<string> GetYoutubeVideoIdBySearchString(string ytSearchString) {
            Track track;

            if (Track.TryGetTrackByYoutubeSearchString(ytSearchString, out track))
                return track.YoutubeId;

            return await new Youtube.Youtube().GetVideoId($"\"ytsearch1:{ytSearchString}\"");
        }

        static async Task<Track> GetTrackByYoutubeId(CommandEventArgs e, string youtubeVideoID, string songCachePath) {
            var youtubeUrl = "https://www.youtube.com/watch?v=" + youtubeVideoID;

            // Get video title
            MyLogger.WriteLine("Getting youtube video title...");
            var youtubeVideoTitle = await new Youtube.Youtube().GetVideoTitle(youtubeUrl);
            MyLogger.WriteLine("Youtube video title get! " + youtubeVideoTitle, ConsoleColor.Green);

            await e.Channel.SendMessageEx($"Found video: **{youtubeVideoTitle}**");

            var youtubeOutput = await new YoutubeDownloader().YoutubeDownloadAndConvertAsync(e, youtubeUrl, songCachePath);

            var msg = await e.Channel.SendMessageEx("Download finished! Converting audio...");

            FileInfo trackPath;

            if (youtubeOutput.Extension != "mp3") {
                trackPath = await new FFMPEG.FFMPEG().FFMPEGConvertToMP3Async(youtubeOutput);
            } else {
                trackPath = youtubeOutput;
            }

            await msg.Edit(msg.Text + "finished!");

            if (trackPath.Exists == false) {
                await e.Channel.SendMessageEx("that video doesn't work, sorry, try something else");
                return null;
            }

            return Track.NewTrack(youtubeVideoTitle, trackPath, youtubeVideoID);
        }

        static async Task RedownloadTrack(CommandEventArgs e, Track track, string songCachePath) {
            var youtubeUrl = "https://www.youtube.com/watch?v=" + track.YoutubeId;

            await e.Channel.SendMessageEx($"Redownloading video: **{track.Title}**");

            var youtubeOutput = await new YoutubeDownloader().YoutubeDownloadAndConvertAsync(e, youtubeUrl, songCachePath);

            var msg = await e.Channel.SendMessageEx("Download finished! Converting audio...");

            FileInfo trackPath;

            if (youtubeOutput.Extension != "mp3") {
                trackPath = await new FFMPEG.FFMPEG().FFMPEGConvertToMP3Async(youtubeOutput);
            } else {
                trackPath = youtubeOutput;
            }

            await msg.Edit(msg.Text + "finished!");

            if (trackPath.Exists == false) {
                await e.Channel.SendMessageEx("that video doesn't work, sorry, try something else");
                throw new YoutubeException("TrackRequest file didn't exist after download and conversion");
            }

            track.Path = trackPath.FullName;

            track.Save();
        }
    }
}
