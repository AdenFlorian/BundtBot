using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BundtBot.Utility;
using NString;

namespace BundtBot.FFMPEG {
    public class FFMPEG {
        public async Task<FileInfo> FFMPEGConvertToWAVAsync(FileInfo fileToConvert) {
            return await FFMPEGConvert2Async(fileToConvert, ".wav");
        }

        public async Task<FileInfo> FFMPEGConvertToMP3Async(FileInfo fileToConvert) {
            return await FFMPEGConvert2Async(fileToConvert, ".mp3");
        }

        static async Task<FileInfo> FFMPEGConvert2Async(FileSystemInfo fileToConvert, string extension) {
			if (fileToConvert == null) throw new ArgumentNullException("fileToConvert");
			if (fileToConvert.Exists == false) throw new FileNotFoundException("file must exist", "fileToConvert");

            var output = fileToConvert.FullName.Substring(0, fileToConvert.FullName.LastIndexOf('.')) + extension;

            var ffmpegProcess = new Process {
                StartInfo = {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = @"ffmpeg.exe",
                    Arguments = "-y -i \"" + fileToConvert + "\" \"" + output + "\"",
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            ffmpegProcess.OutputDataReceived += (sender, ev) => {
                if (ev.Data.IsNullOrWhiteSpace()) return;
                MyLogger.Write("[FFMPEG (stdout)] ", ConsoleColor.Cyan);
                MyLogger.WriteLine(ev.Data);
            };
            ffmpegProcess.ErrorDataReceived += (sender, ev) => {
                if (ev.Data.IsNullOrWhiteSpace()) return;
                MyLogger.Write("[FFMPEG (stderr)] ", ConsoleColor.DarkMagenta);
                MyLogger.WriteLine(ev.Data);
            };

            MyLogger.WriteLine("\n" + ffmpegProcess.StartInfo.FileName + " " + ffmpegProcess.StartInfo.Arguments + "\n");

            ffmpegProcess.Start();

            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.BeginErrorReadLine();

            await Task.Run(() => { ffmpegProcess.WaitForExit(); });

            fileToConvert.Delete();
            return new FileInfo(output);
        }
    }
}
