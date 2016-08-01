using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NString;
using BundtBot.Youtube;

namespace BundtBotTest.Youtube {
    [TestClass]
    public class YoutubeHelperTests {
        #region IsYoutubeUrl
        #region True
        [TestMethod]
        public void IsYoutubeUrl_Https_True() {
            var result = YoutubeHelper.IsYoutubeUrl("https://www.youtube.com/watch?v=VL3jSgR9ySE");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsYoutubeUrl_Http_True() {
            var result = YoutubeHelper.IsYoutubeUrl("http://www.youtube.com/watch?v=VL3jSgR9ySE");
            Assert.IsTrue(result);
        }
        #endregion

        #region False
        [TestMethod]
        public void IsYoutubeUrl_Jibberish_False() {
            var result = YoutubeHelper.IsYoutubeUrl("jibberish");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsYoutubeUrl_NotYoutube_False() {
            var result = YoutubeHelper.IsYoutubeUrl("https://www.utube.com/watch?v=VL3jSgR9ySE");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsYoutubeUrl_TooShortVideoId_False() {
            var result = YoutubeHelper.IsYoutubeUrl("https://www.youtube.com/watch?v=VL5h3");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsYoutubeUrl_NoQuery_False() {
            var result = YoutubeHelper.IsYoutubeUrl("https://www.youtube.com/watch");
            Assert.IsFalse(result);
        }
        #endregion
        #endregion

        #region GetVideoIdFromUrl
        [TestMethod]
        public void GetVideoIdFromUrl_Https_Success() {
            var id = YoutubeHelper.GetVideoIdFromUrl("https://www.youtube.com/watch?v=VL3jSgR9ySE");
            Assert.IsNotNull(id);
            Assert.IsFalse(id.IsNullOrWhiteSpace());
            Assert.IsTrue(id.Length >= 11);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetVideoIdFromUrl_ShortId_Fail() {
            YoutubeHelper.GetVideoIdFromUrl("https://www.youtube.com/watch?v=VL3jSgR9yS");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetVideoIdFromUrl_BadUrl_Fail() {
            YoutubeHelper.GetVideoIdFromUrl("htts:/www.outue.com/wtchv=VL3jgR9yS");
        }
        #endregion
    }
}