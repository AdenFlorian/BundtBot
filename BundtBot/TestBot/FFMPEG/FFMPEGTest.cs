using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BundtBotTest.FFMPEG {
    [TestClass]
    public class FFMPEGTest {

        readonly FileInfo _testOpusFile = new FileInfo(@"FFMPEG/TestFiles/test.opus");
        const string TempFolderName = "temp";
        static DirectoryInfo _tempFolder;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            // create temp folder
            _tempFolder = Directory.CreateDirectory(TempFolderName);
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            // delete temp folder
            _tempFolder.Delete(true);
        }

        [TestMethod]
        public void FFMPEGConvert_Success() {
            // copy opus file into tmp folder
            var opusTestFileInput = new FileInfo(_tempFolder.FullName + "/" + _testOpusFile.Name);
            var wavTestFileOutput = new FileInfo(_tempFolder.FullName + "/test.wav");
            _testOpusFile.CopyTo(opusTestFileInput.FullName, true);
            var ffmpeg = new global::BundtBot.FFMPEG();
            var task =  ffmpeg.FFMPEGConvertToWAVAsync(opusTestFileInput);
            task.Wait(TimeSpan.FromSeconds(3));
            Assert.IsTrue(wavTestFileOutput.Exists);
            Assert.IsTrue(wavTestFileOutput.Length > opusTestFileInput.Length);
        }
    }
}
