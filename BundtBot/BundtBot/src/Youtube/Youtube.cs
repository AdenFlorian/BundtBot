using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BundtBot.Utility;

namespace BundtBot.Youtube {
    public class Youtube {
        public async Task<string> GetVideoId(string searchString) {
            if (searchString == null) throw new ArgumentNullException();
            return await GetOutputFromYoutubeDlSingleLineCommand($"--get-id {searchString}");
        }

        public async Task<string> GetVideoTitle(string searchString) {
            if (searchString == null) throw new ArgumentNullException();
            return await GetOutputFromYoutubeDlSingleLineCommand($"--get-title {searchString}");
        }

        async Task<string> GetOutputFromYoutubeDlSingleLineCommand(string args) {
            string output = null;
            var youtubeDlProcess = YoutubeDlProcess(args);

            youtubeDlProcess.OutputDataReceived += (s, e) => {
                if (string.IsNullOrEmpty(e.Data)) return;
                MyLogger.Info($"[{nameof(youtubeDlProcess.OutputDataReceived)}] {e.Data}");
                output = e.Data;
            };
            youtubeDlProcess.ErrorDataReceived += (s, e) => {
                MyLogger.Info($"[{nameof(youtubeDlProcess.ErrorDataReceived)}] {e.Data}");
            };
            youtubeDlProcess.Exited += (s, e) => {
                MyLogger.Info($"[{nameof(youtubeDlProcess.Exited)}] youtube-dl Exited");
            };

            MyLogger.WriteLine("\n" + youtubeDlProcess.StartInfo.FileName + " " + youtubeDlProcess.StartInfo.Arguments + "\n");

            youtubeDlProcess.Start();
            youtubeDlProcess.BeginOutputReadLine();
            youtubeDlProcess.BeginErrorReadLine();

            MyLogger.WriteLine("Waiting for Process to exit...");

            await Task.Run(() => { youtubeDlProcess.WaitForExit(); });

            MyLogger.WriteLine("Exited!");

            return output;
        }

        static Process YoutubeDlProcess(string arguments) {
            return new Process {
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
        }
    }
}
