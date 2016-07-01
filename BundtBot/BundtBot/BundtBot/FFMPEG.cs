using System.Diagnostics;
using System.IO;

namespace BundtBot.BundtBot {
    class FFMPEG {
        public string ffmpegConvert(string outputPath) {
            var outputWAV = outputPath.Substring(0, outputPath.LastIndexOf('.')) + ".wav";

            var ffmpegProcess = new Process();

            var startinfo = new ProcessStartInfo {
                FileName = @"C:\Users\Bundt\Source\Repos\BundtBot\BundtBot\BundtBot\bin\Debug\ffmpeg.exe",
                Arguments = "-i \"" + outputPath + "\" \"" + outputWAV + "\"",
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            ffmpegProcess.StartInfo = startinfo;

            ffmpegProcess.OutputDataReceived += (object sender, DataReceivedEventArgs ev) => {
                MyLogger.WriteLine("%%FFMPEG%% " + ev.Data);
            };

            ffmpegProcess.Start();
            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.WaitForExit();

            File.Delete(outputPath);
            return outputWAV;
        }
    }
}
