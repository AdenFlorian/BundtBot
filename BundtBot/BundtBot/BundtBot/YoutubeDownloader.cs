using Discord;
using NString;
using System;
using System.Threading.Tasks;
using WrapYoutubeDl;

namespace BundtBot.BundtBot {
    class YoutubeDownloader {

        Message progressMessage;
        decimal lastPercentage = 0;

        public async Task<string> YoutubeDownloadAndConvert(MessageEventArgs e, string ytSearchString, string mp3OutputFolder) {
            var urlToDownload = "\"ytsearch1:"
                                + ytSearchString
                                + "\"";
            var newFilename = Guid.NewGuid().ToString();

            progressMessage = await e.Channel.SendMessage("downloading");

            var downloader = new AudioDownloader(urlToDownload, newFilename, mp3OutputFolder);
            downloader.ProgressDownload += async (object sender, ProgressEventArgs ev) => {
                Console.WriteLine(ev.Percentage);
                if (ev.Percentage > lastPercentage + 50) {
                    await progressMessage.Edit("downloading: " + ev.Percentage);
                }
                lastPercentage = ev.Percentage;
            };
            downloader.FinishedDownload += async (object sender, DownloadEventArgs ev) => {
                Console.WriteLine("Finished Download!");
                await progressMessage.Edit("yt download progress: :100: ");
            };
            downloader.ErrorDownload += downloader_ErrorDownload;
            downloader.StartedDownload += downloader_StartedDownload;

            await e.Channel.SendMessage("Searching youtube for: " + ytSearchString);

            var outputPath = downloader.Download();
            if (outputPath.IsNullOrEmpty()) {
                Console.WriteLine("output path is null :( possibly to big filesize", ConsoleColor.Yellow);
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
