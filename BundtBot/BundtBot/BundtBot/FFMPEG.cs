using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.BundtBot.Utility;
using NString;

namespace BundtBot.BundtBot {
    public class FFMPEG {
        public async Task<FileInfo> FFMPEGConvertAsync(FileInfo outputPath) {
            Contract.Requires<ArgumentNullException>(outputPath != null);
            Contract.Requires<FileNotFoundException>(outputPath.Exists);
            var outputWAV = outputPath.FullName.Substring(0, outputPath.FullName.LastIndexOf('.')) + ".wav";

            // setup the process that will fire youtube-dl
            var ffmpegProcess = new Process {
                StartInfo = {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    //WorkingDirectory = GetDirectoryName(fullPathToEXE),
                    FileName = @"C:\Users\Bundt\Source\Repos\BundtBot\BundtBot\BundtBot\bin\Debug\ffmpeg.exe",
                    Arguments = "-y -i \"" + outputPath + "\" \"" + outputWAV + "\"",
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            //ffmpegProcess.OutputDataReceived += (sender, ev) => {
            //    MyLogger.WriteLine("%%FFMPEG%% " + ev.Data);
            //};
            ffmpegProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceived);
            ffmpegProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);

            Console.WriteLine("\n" + ffmpegProcess.StartInfo.FileName + " " + ffmpegProcess.StartInfo.Arguments + "\n");

            ffmpegProcess.Start();

            //
            // Read in all the text from the process with the StreamReader.
            //
            /*using (StreamReader reader = ffmpegProcess.StandardOutput)
            using (StreamReader readerE = ffmpegProcess.StandardError) {
                string result = reader.ReadToEnd();
                Console.Write(result);
                result = readerE.ReadToEnd();
                Console.Write(result);
            }*/

            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.BeginErrorReadLine();

            ffmpegProcess.WaitForExit();
            //await Task.Run(() => { ffmpegProcess.WaitForExit(); });

            //for (int i = 0; i < 10; i++) {
            //    Thread.Sleep(300);
            //}

            outputPath.Delete();
            return new FileInfo(outputWAV);
        }

        public void ErrorDataReceived(object sendingprocess, DataReceivedEventArgs error) {
            if (error.Data.IsNullOrWhiteSpace()) return;
            MyLogger.Write("%%FFMPEG ERROR%% ", ConsoleColor.DarkMagenta);
            MyLogger.WriteLine(error.Data);
        }
        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
            if (outLine.Data.IsNullOrWhiteSpace()) return;
            MyLogger.Write("%%FFMPEG%% ", ConsoleColor.Cyan);
            MyLogger.WriteLine(outLine.Data);
        }
    }
}
