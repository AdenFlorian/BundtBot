using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBot.FFMPEG {
    [TestClass]
    public class FFMPEGTest {

        readonly FileInfo _testOpusFile = new FileInfo(@"FFMPEG/TestFiles/test.opus");
        readonly FileInfo _testWavFile = new FileInfo(@"FFMPEG/TestFiles/test.wav");
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
        public async Task FFMPEGConvert_Success() {
            // copy opus file into tmp folder
            var fileInfo = new FileInfo(_tempFolder.FullName + "/" + _testOpusFile.Name);
            _testOpusFile.CopyTo(fileInfo.FullName, true);
            var ffmpeg = new BundtBot.BundtBot.FFMPEG();
            await ffmpeg.FFMPEGConvertToWAVAsync(fileInfo);
            Assert.IsTrue(File.Exists(_tempFolder.FullName + "/test.wav"));
        }
    }
}
