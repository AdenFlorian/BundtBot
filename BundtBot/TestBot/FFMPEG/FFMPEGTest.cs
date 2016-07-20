using BundtBot.BundtBot;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBot {
    [TestClass]
    public class FFMPEGTest {

        readonly FileInfo testOpusFile = new FileInfo(@"FFMPEG/TestFiles/test.opus");
        readonly FileInfo testWavFile = new FileInfo(@"FFMPEG/TestFiles/test.wav");
        const string tempFolderName = "temp";
        static DirectoryInfo tempFolder;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            // create temp folder
            tempFolder = Directory.CreateDirectory(tempFolderName);
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            // delete temp folder
            tempFolder.Delete(true);
        }

        [TestMethod]
        public async Task FFMPEGConvert_Success() {
            // copy opus file into tmp folder
            var fileInfo = new FileInfo(tempFolder.FullName + "/" + testOpusFile.Name);
            testOpusFile.CopyTo(fileInfo.FullName, true);
            var ffmpeg = new FFMPEG();
            await ffmpeg.FFMPEGConvertAsync(fileInfo);
            Assert.IsTrue(File.Exists(tempFolder.FullName + "/test.wav"));
        }
    }
}
