using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BundtBot.BundtBot.Utility;

namespace BundtBot.BundtBot {
    class FFMPEG {
        public async Task<FileInfo> FFMPEGConvert(string outputPath) {
            var outputWAV = outputPath.Substring(0, outputPath.LastIndexOf('.')) + ".wav";

            var ffmpegProcess = new Process();

            var startinfo = new ProcessStartInfo {
                FileName = @"C:\Users\Bundt\Source\Repos\BundtBot\BundtBot\BundtBot\bin\Debug\ffmpeg.exe",
                Arguments = "-i \"" + outputPath + "\" \"" + outputWAV + "\"",
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            ffmpegProcess.StartInfo = startinfo;

            ffmpegProcess.OutputDataReceived += (sender, ev) => {
                MyLogger.WriteLine("%%FFMPEG%% " + ev.Data);
            };

            ffmpegProcess.Start();
            ffmpegProcess.BeginOutputReadLine();

            await Task.Run(() => { ffmpegProcess.WaitForExit(); });

            File.Delete(outputPath);
            return new FileInfo(outputWAV);
        }
    }
}
