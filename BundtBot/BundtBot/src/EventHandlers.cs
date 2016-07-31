using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BundtBot.Database;
using BundtBot.Extensions;
using BundtBot.Models;
using BundtBot.Sound;
using BundtBot.Utility;
using Discord;
using Discord.Net;

namespace BundtBot {
    class EventHandlers {
        public static void RegisterEventHandlers(DiscordClient _client, SoundBoard _soundBoard, SoundManager _soundManager) {
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

            _client.ChannelCreated += async (sender, e) => {
                try {
                    await e.Channel.SendMessageEx("less is more");
                    if (e.Channel.Name.ToLower().Contains("bundtbot")) {
                        if (BundtBot.TextChannelOverrides.ContainsKey(e.Server)) {
                            BundtBot.TextChannelOverrides[e.Server] = e.Channel;
                        } else {
                            BundtBot.TextChannelOverrides.Add(e.Server, e.Channel);
                        }
                    }
                } catch (Exception ex) {
                    MyLogger.WriteException(ex);
                }

            };
            _client.ChannelDestroyed += async (sender, e) => {
                try {
                    await e.Channel.SendMessageEx("RIP in pieces " + e.Channel.Name);
                    if (BundtBot.TextChannelOverrides.ContainsKey(e.Server)) {
                        if (BundtBot.TextChannelOverrides[e.Server] == e.Channel) {
                            BundtBot.TextChannelOverrides.Remove(e.Server);
                        }
                    }
                } catch (Exception ex) {
                    MyLogger.WriteException(ex);
                }

            };
            _client.ChannelUpdated += (sender, e) => {
                if (e.After.Name.ToLower().Contains("bundtbot")) {
                    if (BundtBot.TextChannelOverrides.ContainsKey(e.Server)) {
                        BundtBot.TextChannelOverrides[e.Server] = e.After;
                    } else {
                        BundtBot.TextChannelOverrides.Add(e.Server, e.After);
                    }
                }
            };
            #endregion

            #region ServerEvents
            _client.ServerAvailable += async (sender, e) => {
                MyLogger.Write("Server available! ");
                MyLogger.WriteLine(e.Server.Name, ConsoleColorHelper.GetRoundRobinColor());
                try {
                    await e.Server.CurrentUser.Edit(nickname: "bundtbot v" + BundtBot.Version);
                } catch (HttpException ex) {
                    MyLogger.WriteLine($"{ex.GetType()} thrown from trying to change the bot's nickname",
                        ConsoleColor.DarkYellow);
                    MyLogger.WriteLine("The bot might not have permission on that server to change it's nickname",
                        ConsoleColor.DarkYellow);
                } catch (Exception ex) {
                    MyLogger.WriteException(ex);
                }
                // Set override channel if exists
                foreach (var textChannel in e.Server.TextChannels) {
                    if (textChannel.Name.ToLower().Contains("bundtbot")) {
                        BundtBot.TextChannelOverrides.Add(e.Server, textChannel);
                        break;
                    }
                }
                // Register Users in DB
                foreach (var user in e.Server.Users) {
                    if (DB.Users.Exists(x => x.SnowflakeId == user.Id) == false) {
                        DB.Users.Insert(new Models.User {SnowflakeId = user.Id});
                    }
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
                await e.User.Server.DefaultChannel.SendMessageEx("welcome to server " + e.User.NicknameMention);
                await e.User.Server.DefaultChannel.SendMessageEx("beware of the airhorns...");
            };
            _client.UserUpdated += async (s, e) => {
                var voiceChannelBefore = e.Before.VoiceChannel;
                var voiceChannelAfter = e.After.VoiceChannel;
                if (voiceChannelBefore == voiceChannelAfter) return;
                if (voiceChannelBefore != null) {
                    await OnUserLeftVoiceChannel(new ChannelUserEventArgs(voiceChannelBefore, e.After), _soundManager);
                }
                if (voiceChannelAfter != null) {
                    OnUserJoinedVoiceChannel(new ChannelUserEventArgs(voiceChannelAfter, e.After), _soundBoard, _soundManager, new Random());
                }
            };
            _client.UserLeft += async (sender, e) => {
                // Can't send message to server if we just left it
                if (e.User.Id == _client.CurrentUser.Id) {
                    return;
                }
                await e.Server.DefaultChannel.SendMessageEx("RIP in pieces " + e.User.Nickname);
            };
            #endregion

            #region OtherEvents
            _client.Log.Message += (sender, eventArgs) => {
                Console.WriteLine($"[{eventArgs.Severity}] {eventArgs.Source}: {eventArgs.Message}");
            };
            #endregion

            MyLogger.WriteLine("Done!");
        }

        static void OnUserJoinedVoiceChannel(ChannelUserEventArgs e, SoundBoard soundBoard, SoundManager soundManager, Random random) {
            if (e.User.IsBot) {
                MyLogger.WriteLine("Bot joined a voice channel. Ignoring...");
                return;
            }
            if (e.Channel.IsAFK()) {
                MyLogger.WriteLine("User joined an AFK voice channel. Ignoring...");
                return;
            }
            if (soundManager.IsPlaying) {
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
            var i = random.Next(list.Length);
            var x = list[i];
            MyLogger.WriteLine("User joined a voice channel. Sending: " + x.Item1 + " " + x.Item2);
            FileInfo soundFile;
            if (soundBoard.TryGetSoundPath(x.Item1, x.Item2, out soundFile) == false) {
                MyLogger.WriteException(new FileNotFoundException("Couldn't Find Sound but should have"));
                return;
            }

            var audioClip = new AudioClip {
                Path = soundFile.FullName,
                Title = $"{x.Item1}: {x.Item2}"
            };

            var sound = new Sound.Sound(audioClip, e.Channel.Server.DefaultChannel, e.Channel) { TextUpdates = false };
            soundManager.EnqueueSound(sound);
        }

        static async Task OnUserLeftVoiceChannel(ChannelUserEventArgs e, SoundManager soundManager) {
            if (e.Channel != soundManager.VoiceChannel) return;
            if (e.Channel.Users.Count() > 1) return;
            if (soundManager.CurrentlyPlayingSound.TextUpdates) {
                await e.Channel.Server.DefaultChannel.SendMessageEx("sorry i bothered you with my 🎶");
            }
            MyLogger.WriteLine("[Program] OnUserLeftVoiceChannel - Telling SoundManager to stop," +
                               " because we are the last user in channel");
            soundManager.Stop();
        }
    }
}
