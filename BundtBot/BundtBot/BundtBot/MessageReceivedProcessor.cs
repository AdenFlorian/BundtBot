using Discord;
using Discord.Audio;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WrapYoutubeDl;

namespace BundtBot.BundtBot {
    class MessageReceivedProcessor {
        public async Task ProcessMessage(DiscordClient client, SoundBoard soundBoard, MessageEventArgs e) {
            #region boring commands
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
                soundBoard.locked = true;
                
                MyLogger.WriteLine("Connecting to voice channel:" + voiceChannel.Name);
                MyLogger.WriteLine("\tOn server:  " + voiceChannel.Server.Name);
                var audioService = client.GetService<AudioService>();
                var audioClient = await audioService.Join(voiceChannel);
                new AudioStreamer().PlaySound(audioService, audioClient, args);
            }
            #endregion
        }
    }
}
