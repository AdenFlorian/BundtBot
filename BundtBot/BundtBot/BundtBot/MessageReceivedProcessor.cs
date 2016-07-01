using BundtBot.BundtBot;
using Discord;
using Discord.Audio;
using NString;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using WrapYoutubeDl;

namespace BundtBot.BundtBot {
    class MessageReceivedProcessor {

        public async Task ProcessMessage(DiscordClient client, SoundBoard soundBoard, MessageEventArgs e) {
            #region boring commands
            if (e.Message.Text == "!admin") {
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
            }
            if (e.Message.Text == "!mod") {
                bool ismod = false;
                var roles = e.User.Roles;
                foreach (var role in roles) {
                    if (role.Name.Contains("mod")) {
                        ismod = true;
                    }
                }
                if (ismod) {
                    await e.Channel.SendMessage("Yes, you are! :D");
                } else {
                    await e.Channel.SendMessage("No, you aren't D:");
                }
            }
            if (e.Message.Text == "!help") {
                await e.Channel.SendMessage("!owsb <character name> <phrase>");
                await e.Channel.SendMessage("created by @AdenFlorian");
                await e.Channel.SendMessage("https://github.com/AdenFlorian/DiscordSharp_Starter");
                await e.Channel.SendMessage("https://trello.com/b/VKqUgzwV/bundtbot#");
            }
            if (e.Message.Text == "!cat") {
                await Cat(e);
            }
            if (e.Message.Text == "!dog") {
                await Dog(e, "i found a dog");
            }
            #endregion

            #region SoundBoard

            if (e.Message.Text == "!stop") {
                var audioClient = e.Server.GetAudioClient();
                if (audioClient == null) {
                    await e.Channel.SendMessage("stop what? (client.GetVoiceClient() returned null)");
                } else {
                    var msg = await e.Channel.SendMessage("okay...");
                    audioClient.Clear();
                    await audioClient.Disconnect();
                    soundBoard.stop = true;
                    soundBoard.locked = false;
                    await msg.Edit(msg.Text + ":disappointed_relieved:");
                }
            }

            if (e.Message.Text.StartsWith("!owsb ") ||
                e.Message.Text.StartsWith("!sb")) {
                SoundBoardArgs soundBoardArgs = null;
                try {
                    soundBoardArgs = new SoundBoardArgs(e.Message.Text);
                } catch (Exception ex) {
                    await e.Channel.SendMessage("you're doing it wrong");
                    await e.Channel.SendMessage(ex.Message);
                }

                if (soundBoardArgs == null) {
                    await e.Channel.SendMessage("you're doing it wrong (or something broke)");
                    return;
                }

                await soundBoard.Process(e, soundBoardArgs);
            }

            if (e.Message.Text.StartsWith("!youtube ") ||
                e.Message.Text.StartsWith("!yt ")) {

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
                
                if (soundBoard.locked) {
                    await e.Channel.SendMessage("wait your turn...or if you want to be mean, use !stop");
                    return;
                }
                
                if (e.User.VoiceChannel == null) {
                    await e.Channel.SendMessage("you need to be in a voice channel to hear me roar");
                    return;
                }

                // Get video id
                MyLogger.WriteLine("Getting youtube video id...");
                var youtubeVideoID = youtube_dl.GetVideoID(ytSearchString);
                MyLogger.WriteLine("Youtube video ID get! " + youtubeVideoID, ConsoleColor.Green);

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
                    await e.Channel.SendMessage("Wait for it...");
                }

                var args = new SoundBoardArgs();
                args.soundPath = outputWAV;
                args.deleteAfterPlay = false;

                if (File.Exists(args.soundPath) == false) {
                    await e.Channel.SendMessage("that video doesn't work, sorry, try something else");
                    return;
                }
                soundBoard.locked = true;

                soundBoard.nextSound = args;

                var voiceChannel = e.User.VoiceChannel;



                MyLogger.WriteLine("Connecting to voice channel:" + voiceChannel.Name);
                MyLogger.WriteLine("\tOn server:  " + voiceChannel.Server.Name);
                var audioService = client.GetService<AudioService>();
                var audioClient = await audioService.Join(voiceChannel);
                await soundBoard.OnConnectedToVoiceChannel(audioService, audioClient);
            }
            #endregion
        }

        private static async Task Cat(MessageEventArgs e) {
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

        private static async Task Dog(MessageEventArgs e, string message) {
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
