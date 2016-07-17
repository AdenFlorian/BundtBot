using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using BundtBot.BundtBot.Utility;

namespace BundtBot.BundtBot {
    class FFMPEG {
        public async Task<FileInfo> FFMPEGConvert(FileInfo outputPath) {
            Contract.Requires<ArgumentNullException>(outputPath != null);
            Contract.Requires<FileNotFoundException>(outputPath.Exists);
            var outputWAV = outputPath.FullName.Substring(0, outputPath.FullName.LastIndexOf('.')) + ".wav";

            var ffmpegProcess = new Process();

            var startinfo = new ProcessStartInfo {
                FileName = @"C:\Users\Bundt\Source\Repos\BundtBot\BundtBot\BundtBot\bin\Debug\ffmpeg.exe",
                Arguments = "-y -i \"" + outputPath + "\" \"" + outputWAV + "\"",
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

            Console.WriteLine("\n" + ffmpegProcess.StartInfo.FileName + " " + ffmpegProcess.StartInfo.Arguments + "\n");

            ffmpegProcess.Start();
            ffmpegProcess.BeginOutputReadLine();

            await Task.Run(() => { ffmpegProcess.WaitForExit(); });

            outputPath.Delete();
            return new FileInfo(outputWAV);
        }
    }
}
