using System;
using System.Threading.Tasks;
using BundtBot.BundtBot.Utility;
using Discord;
using Discord.Commands;
using NString;
using WrapYoutubeDl;

namespace BundtBot.BundtBot {
    class YoutubeDownloader {

        Message _progressMessage;
        decimal _lastPercentage;

        public async Task<string> YoutubeDownloadAndConvert(CommandEventArgs e, string ytSearchString, string mp3OutputFolder) {
            var urlToDownload = "\"ytsearch1:"
                                + ytSearchString
                                + "\"";
            var newFilename = Guid.NewGuid().ToString();

            var downloader = new AudioDownloader(urlToDownload, newFilename, mp3OutputFolder);
            downloader.ProgressDownload += async (sender, ev) => {
                Console.WriteLine(ev.Percentage);
                if (ev.Percentage > _lastPercentage + 50) {
                    await _progressMessage.Edit("downloading: " + ev.Percentage);
                }
                _lastPercentage = ev.Percentage;
            };
            downloader.FinishedDownload += async (sender, ev) => {
                Console.WriteLine("Finished Download!");
                await _progressMessage.Edit("downloading: :100: ");
            };
            downloader.ErrorDownload += downloader_ErrorDownload;
            downloader.StartedDownload += downloader_StartedDownload;

            _progressMessage = await e.Channel.SendMessage("downloading");

            var outputPath = downloader.Download();
            if (outputPath.IsNullOrEmpty()) {
                MyLogger.WriteLine("output path is null :( possibly to big filesize", ConsoleColor.Yellow);
                await e.Channel.SendMessage("ummm...bad news...something broke...the video was probably too big to download, so try somethin else, k?");
                throw new Exception();
            }
            Console.WriteLine("downloader.Download() Finished! " + outputPath);

            return outputPath;
        }

        void downloader_ErrorDownload(object sender, ProgressEventArgs e) {
            Console.WriteLine("error");
        }

        void downloader_StartedDownload(object sender, DownloadEventArgs e) {
            Console.WriteLine("yotube-dl process started");
        }
    }
}
