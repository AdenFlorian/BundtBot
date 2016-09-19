using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BundtBot.Youtube {
    public class YoutubeVideoName {
        public object ProcessObject { get; set; }
        public bool Started { get; set; }
        public bool Finished { get; set; }
        public decimal Percentage { get; set; }
        public Process Process { get; set; }

        public string ConsoleLog { get; set; }

        public string resultVideoTitle { get; private set; }

        public async Task<string> Get(string searchString) {
            resultVideoTitle = null;
            Started = false;
            Finished = false;
            Percentage = 0;

            var arguments = string.Format($"--get-title {searchString}");  //--ignore-errors

            // setup the process that will fire youtube-dl
            Process = new Process();
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardError = true;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            Process.StartInfo.FileName = "youtube-dl.exe";
            Process.StartInfo.Arguments = arguments;
            Process.StartInfo.CreateNoWindow = false;
            Process.EnableRaisingEvents = true;

            Process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            Process.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceived);

            Process.Exited += Process_Exited;

            Console.WriteLine("\n" + Process.StartInfo.FileName + " " + Process.StartInfo.Arguments + "\n");

            Process.Start();
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
            Console.Write("Waiting for Process to exit...");

            await Task.Run(() => { Process.WaitForExit(); });

            Console.WriteLine("Exited!");

            return resultVideoTitle;
        }

        static void Process_Exited(object sender, EventArgs e) {
            Console.WriteLine("youtube-dl Exited");
        }

        public void ErrorDataReceived(object sendingprocess, DataReceivedEventArgs error) {
            Console.WriteLine(error.Data);
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
            if (string.IsNullOrEmpty(outLine.Data)) {
                return;
            }

            Console.WriteLine(outLine.Data);

            resultVideoTitle = outLine.Data;
        }
    }
}
