using DiscordSharp;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using WebSocketSharp;
using WrapYoutubeDl;

namespace DiscordSharp_Starter.BundtBot {
    class MessageReceivedProcessor {

        public void ProcessMessage(DiscordClient client, SoundBoard soundBoard, DiscordMessageEventArgs eventArgs) {
            if (eventArgs.MessageText == "!admin") {
                var admin = eventArgs.Author.Roles.Find(x => x.Name.Contains("Administrator"));
                string msg;
                if (admin != null) {
                    msg = "Yes, you are! :D";
                } else {
                    msg = "No, you aren't :c";
                }
                eventArgs.Channel.SendMessage(msg);
            }
            if (eventArgs.MessageText == "!mod") {
                bool ismod = false;
                List<DiscordRole> roles = eventArgs.Author.Roles;
                foreach (DiscordRole role in roles) {
                    if (role.Name.Contains("mod")) {
                        ismod = true;
                    }
                }
                if (ismod) {
                    eventArgs.Channel.SendMessage("Yes, you are! :D");
                } else {
                    eventArgs.Channel.SendMessage("No, you aren't D:");
                }
            }
            if (eventArgs.MessageText == "!help") {
                eventArgs.Channel.SendMessage("!owsb <character name> <phrase>");
                eventArgs.Channel.SendMessage("created by @AdenFlorian");
                eventArgs.Channel.SendMessage("https://github.com/AdenFlorian/DiscordSharp_Starter");
                eventArgs.Channel.SendMessage("https://trello.com/b/VKqUgzwV/bundtbot#");
            }
            if (eventArgs.MessageText == "!cat") {
                Random rand = new Random();
                if (rand.NextDouble() >= 0.5) {
                    string s;
                    using (WebClient webclient = new WebClient()) {
                        s = webclient.DownloadString("http://random.cat/meow");
                        int pFrom = s.IndexOf("\\/i\\/") + "\\/i\\/".Length;
                        int pTo = s.LastIndexOf("\"}");
                        string cat = s.Substring(pFrom, pTo - pFrom);
                        Console.WriteLine("http://random.cat/i/" + cat);
                        eventArgs.Channel.SendMessage("I found a cat\nhttp://random.cat/i/" + cat);
                    }
                } else {
                    Dog(eventArgs, "how about a dog instead");
                }
            }
            if (eventArgs.MessageText == "!dog") {
                Dog(eventArgs, "i found a dog");
            }

            #region SoundBoard

            if (eventArgs.MessageText == "!stop") {
                if (client.GetVoiceClient() == null) {
                    eventArgs.Channel.SendMessage("stop what? (client.GetVoiceClient() returned null)");
                } else if (client.GetVoiceClient().Connected == false) {
                    eventArgs.Channel.SendMessage("stop what? (client.GetVoiceClient().Connected == false)");
                } else {
                    eventArgs.Channel.SendMessage("okay... :disappointed_relieved:");
                    soundBoard.stop = true;
                }
            }

            if (eventArgs.MessageText.StartsWith("!owsb ") ||
                eventArgs.MessageText.StartsWith("!sb")) {
                SoundBoardArgs soundBoardArgs = null;
                try {
                    soundBoardArgs = new SoundBoardArgs(eventArgs.MessageText);
                } catch (Exception e) {
                    eventArgs.Channel.SendMessage("you're doing it wrong");
                    eventArgs.Channel.SendMessage(e.Message);
                }

                if (soundBoardArgs == null) {
                    eventArgs.Channel.SendMessage("you're doing it wrong (or something broke)");
                    return;
                }

                soundBoard.Process(eventArgs, soundBoardArgs);
            }

            if (eventArgs.MessageText.StartsWith("!youtube ") ||
                eventArgs.MessageText.StartsWith("!yt ")) {

                var ytSearchString = "";

                var commandString = eventArgs.MessageText.Trim();

                if (commandString.StartsWith("!youtube ") &&
                    commandString.Length > 9) {
                    ytSearchString = commandString.Substring(9);
                } else if (commandString.StartsWith("!yt ") &&
                    commandString.Length > 4) {
                    ytSearchString = commandString.Substring(4);
                } else {
                    eventArgs.Channel.SendMessage("you're doing it wrong (or something broke)");
                    return;
                }

                if (eventArgs.Author.CurrentVoiceChannel == null) {
                    eventArgs.Channel.SendMessage("you need to be in a voice channel to hear me roar");
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
                    outputWAV = YoutubeDownloadAndConvert(eventArgs, ytSearchString, mp3OutputFolder);
                } else {
                    MyLogger.WriteLine("WAV file exists already! " + possiblePath, ConsoleColor.Green);
                    outputWAV = possiblePath;
                    eventArgs.Channel.SendMessage("Wait for it...");
                }

                var args = new SoundBoardArgs();
                args.soundPath = outputWAV;
                args.deleteAfterPlay = false;

                if (File.Exists(args.soundPath) == false) {
                    eventArgs.Channel.SendMessage("that video doesn't work, sorry, try something else");
                    return;
                }

                if (soundBoard.locked) {
                    eventArgs.Channel.SendMessage("wait your turn...");
                    return;
                }
                soundBoard.locked = true;

                soundBoard.nextSound = args;

                DiscordVoiceConfig voiceConfig = null;
                bool clientMuted = false;
                bool clientDeaf = false;
                client.ConnectToVoiceChannel(eventArgs.Author.CurrentVoiceChannel, voiceConfig, clientMuted, clientDeaf);
            }
            #endregion
        }

        private static string YoutubeDownloadAndConvert(DiscordMessageEventArgs eventArgs, string ytSearchString, string mp3OutputFolder) {
            var urlToDownload = "\"ytsearch1:"
                                + ytSearchString
                                + "\"";
            var newFilename = Guid.NewGuid().ToString();

            var downloader = new AudioDownloader(urlToDownload, newFilename, mp3OutputFolder);
            downloader.ProgressDownload += downloader_ProgressDownload;
            downloader.FinishedDownload += downloader_FinishedDownload;
            downloader.ErrorDownload += downloader_ErrorDownload;
            downloader.StartedDownload += downloader_StartedDownload;

            eventArgs.Channel.SendMessage("Searching youtube for: " + ytSearchString);

            var outputPath = downloader.Download();
            if (outputPath.IsNullOrEmpty()) {
                Console.WriteLine("output path is null :( possibly to big filesize", ConsoleColor.Yellow);
                eventArgs.Channel.SendMessage("ummm...bad news...something broke...the video was probably too big to download, so try somethin else, k?");
                throw new Exception();
            }
            Console.WriteLine("downloader.Download() Finished! " + outputPath);

            eventArgs.Channel.SendMessage("Download finished! Converting then streaming...");

            // Convert to WAV
            var outputWAV = outputPath.Substring(0, outputPath.LastIndexOf('.')) + ".wav";

            var ffmpegProcess = new Process();

            var startinfo = new ProcessStartInfo {
                FileName = @"C:\Users\Bundt\Source\Repos\DiscordSharp_Starter\DiscordSharp_Starter\DiscordSharp_Starter\bin\Debug\ffmpeg.exe",
                Arguments = "-i \"" + outputPath + "\" \"" + outputWAV + "\"",
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            ffmpegProcess.StartInfo = startinfo;

            ffmpegProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                MyLogger.WriteLine("%%FFMPEG%% " + e.Data);
            };

            ffmpegProcess.Start();
            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.WaitForExit();
            
            File.Delete(outputPath);

            return outputWAV;
        }

        private static void Dog(DiscordSharp.Events.DiscordMessageEventArgs eventArgs, string message) {
            try {
                string s;
                using (WebClient webclient = new MyWebClient()) {
                    s = webclient.DownloadString("http://random.dog/woof");
                    string dog = s;
                    Console.WriteLine("http://random.dog/" + dog);
                    eventArgs.Channel.SendMessage(message + "\nhttp://random.dog/" + dog);
                }
            } catch (Exception) {
                eventArgs.Channel.SendMessage("there are no dogs here, who let them out (random.dog is down :dog: :interrobang:)");
            }

        }

        static void downloader_FinishedDownload(object sender, DownloadEventArgs e) {
            Console.WriteLine("Finished Download!");
        }

        static void downloader_ProgressDownload(object sender, ProgressEventArgs e) {
            Console.WriteLine(e.Percentage);
        }

        static void downloader_ErrorDownload(object sender, ProgressEventArgs e) {
            Console.WriteLine("error");
        }

        static void downloader_StartedDownload(object sender, DownloadEventArgs e) {
            Console.WriteLine("yotube-dl process started");
        }
    }
}
