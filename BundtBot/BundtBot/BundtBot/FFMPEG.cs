using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using BundtBot.BundtBot.Utility;
using NString;

namespace BundtBot.BundtBot {
    public class FFMPEG {
        public async Task<FileInfo> FFMPEGConvertAsync(FileInfo outputPath) {
            Contract.Requires<ArgumentNullException>(outputPath != null);
            Contract.Requires<FileNotFoundException>(outputPath.Exists);
            var outputWAV = outputPath.FullName.Substring(0, outputPath.FullName.LastIndexOf('.')) + ".wav";
            
            var ffmpegProcess = new Process {
                StartInfo = {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = @"C:\Users\Bundt\Source\Repos\BundtBot\BundtBot\BundtBot\bin\Debug\ffmpeg.exe",
                    Arguments = "-y -i \"" + outputPath + "\" \"" + outputWAV + "\"",
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            ffmpegProcess.OutputDataReceived += (sender, ev) => {
                if (ev.Data.IsNullOrWhiteSpace()) return;
                MyLogger.Write("%%FFMPEG%% ", ConsoleColor.Cyan);
                MyLogger.WriteLine(ev.Data);
            };
            ffmpegProcess.ErrorDataReceived += (sender, ev) => {
                if (ev.Data.IsNullOrWhiteSpace()) return;
                MyLogger.Write("%%FFMPEG ERROR%% ", ConsoleColor.DarkMagenta);
                MyLogger.WriteLine(ev.Data);
            };

            MyLogger.WriteLine("\n" + ffmpegProcess.StartInfo.FileName + " " + ffmpegProcess.StartInfo.Arguments + "\n");

            ffmpegProcess.Start();
            
            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.BeginErrorReadLine();
            
            await Task.Run(() => { ffmpegProcess.WaitForExit(); });

            outputPath.Delete();
            return new FileInfo(outputWAV);
        }
    }
}
