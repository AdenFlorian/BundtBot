using BundtBot.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BundtBotTest.Utility {
    [TestClass]
    public class ToolBoxTest {
        [TestMethod]
        public void Levenshtein() {
            Assert.AreEqual(0, ToolBox.Levenshtein("x", "x"));
        }
    }
}
