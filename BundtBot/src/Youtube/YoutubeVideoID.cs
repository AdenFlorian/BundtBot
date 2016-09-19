using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using BundtBot.Utility;

namespace BundtBot.Youtube {
    public class YoutubeVideoID {
        public string resultVideoID { get; private set; }

        public async Task<string> Get(string searchString) {
            resultVideoID = null;

            MyLogger.WriteLine("Getting youtube video id...");

            var arguments = string.Format($"--get-id {searchString}");  //--ignore-errors

            // setup the process that will fire youtube-dl
            var youtubeDlProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = "youtube-dl.exe",
                    Arguments = arguments,
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            youtubeDlProcess.OutputDataReceived += (s, e) => {
                if (string.IsNullOrEmpty(e.Data)) return;
                MyLogger.WriteLine($"[{nameof(YoutubeVideoID)}.{nameof(Get)} {nameof(youtubeDlProcess.OutputDataReceived)}] {e.Data}");
                resultVideoID = e.Data;
            };
            youtubeDlProcess.ErrorDataReceived += (s, e) => {
                MyLogger.WriteLine($"[{nameof(YoutubeVideoID)}.{nameof(Get)} {nameof(youtubeDlProcess.ErrorDataReceived)}] {e.Data}");
            };
            youtubeDlProcess.Exited += (s, e) => {
                MyLogger.WriteLine($"[{ nameof(YoutubeVideoID)}.{ nameof(Get)} {nameof(youtubeDlProcess.Exited)}] youtube-dl Exited");
            };

            MyLogger.WriteLine("\n" + youtubeDlProcess.StartInfo.FileName + " " + youtubeDlProcess.StartInfo.Arguments + "\n");

            youtubeDlProcess.Start();
            youtubeDlProcess.BeginOutputReadLine();
            youtubeDlProcess.BeginErrorReadLine();
            MyLogger.Write("Waiting for Process to exit...");

            await Task.Run(() => { youtubeDlProcess.WaitForExit(); });

            MyLogger.WriteLine("Exited!");
            
            MyLogger.WriteLine("Youtube video ID get! " + resultVideoID, ConsoleColor.Green);

            return resultVideoID;
        }
    }
}
