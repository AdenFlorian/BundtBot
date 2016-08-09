using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BundtBotTest.Youtube {
    [TestClass]
    public class YoutubeTests {
        [TestMethod]
        public void GetVideoId_SearchTest_Success() {
            const string ytSearchString = "test";
            var task = new global::BundtBot.Youtube.Youtube().GetVideoId($"\"ytsearch1:{ytSearchString}\"");
            task.Wait();
            var result = task.Result;
            Assert.AreEqual("2a4Uxdy9TQY", result);
        }

        [TestMethod]
        public void GetVideoTitle_SearchTest_Success() {
            const string ytSearchString = "test";
            var task = new global::BundtBot.Youtube.Youtube().GetVideoTitle($"\"ytsearch1:{ytSearchString}\"");
            task.Wait();
            var result = task.Result;
            Assert.AreEqual("Idiot Test - 90% fail", result);
        }

        [TestMethod]
        public void GetVideoId_NullInput_Fail() {
            try {
                var task = new global::BundtBot.Youtube.Youtube().GetVideoId(null);
                task.Wait();
                Assert.Fail("Expected an Exception");
            }
            catch (AggregateException ae) {
                Assert.AreEqual(typeof(ArgumentNullException), ae.InnerException.GetType());
            }
        }

        [TestMethod]
        public void GetVideoTitle_NullInput_Fail() {
            try {
                var task = new global::BundtBot.Youtube.Youtube().GetVideoTitle(null);
                task.Wait();
                Assert.Fail("Expected an Exception");
            } catch (AggregateException ae) {
                Assert.AreEqual(typeof(ArgumentNullException), ae.InnerException.GetType());
            }
        }
    }
}