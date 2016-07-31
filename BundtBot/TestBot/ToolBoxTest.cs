using BundtBot.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BundtBotTest {
    [TestClass]
    public class ToolBoxTest {
        [TestMethod]
        public void LevenshteinTest1() {
            Assert.AreEqual(0, ToolBox.Levenshtein("x", "x"));
        }
    }
}
