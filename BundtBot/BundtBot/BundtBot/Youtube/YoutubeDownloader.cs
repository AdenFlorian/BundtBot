using System;
using System.IO;
using System.Threading.Tasks;
using BundtBot.BundtBot.Extensions;
using BundtBot.BundtBot.Utility;
using Discord;
using Discord.Commands;

namespace BundtBot.BundtBot.Youtube {
    class YoutubeDownloader {

        Message _progressMessage;
        decimal _lastPercentage;

        public async Task<FileInfo> YoutubeDownloadAndConvertAsync(CommandEventArgs e, string ytSearchString, string mp3OutputFolder) {
            var urlToDownload = ytSearchString;
            var newFilename = Guid.NewGuid().ToString();

            var downloader = new AudioDownloader(urlToDownload, newFilename, mp3OutputFolder);
            downloader.ProgressDownload += async (sender, ev) => {
                MyLogger.WriteLine(ev.Percentage.ToString("0.0"), ConsoleColor.Green);
                if (ev.Percentage > _lastPercentage + 25) {
                    await _progressMessage.Edit("downloading: " + ev.Percentage.ToString("0") + "%");
                    _lastPercentage = ev.Percentage;
                }
            };
            downloader.FinishedDownload += async (sender, ev) => {
                Console.WriteLine("Finished Download!");
                await _progressMessage.Edit("downloading: :100: ");
            };
            downloader.ErrorDownload += downloader_ErrorDownload;
            downloader.StartedDownload += downloader_StartedDownload;

            _progressMessage = await e.Channel.SendMessageEx("downloading");
            FileInfo outputPath;
            try {
                outputPath = downloader.Download();
            }
            catch (Exception) {
                MyLogger.WriteLine("downloader.Download(); threw an exception :( possibly to big filesize", ConsoleColor.Yellow);
                await e.Channel.SendMessageEx("ummm...bad news...something broke...the video was probably too big to download, so try somethin else, k?");
                throw;
            }
            Console.WriteLine("downloader.Download() Finished! " + outputPath);

            return outputPath;
        }

        static void downloader_ErrorDownload(object sender, ProgressEventArgs e) {
            Console.WriteLine("error");
        }

        static void downloader_StartedDownload(object sender, DownloadEventArgs e) {
            Console.WriteLine("yotube-dl process started");
        }
    }
}
