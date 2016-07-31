using BundtBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NString;

namespace BundtBotTest.BundtBot {
    [TestClass]
    public class CatDogTest {
        [TestMethod]
        public void Cat_Success() {
            var cat = CatDog.Cat().Result;
            Assert.IsNotNull(cat);
            Assert.IsFalse(cat.IsNullOrWhiteSpace());
            Assert.IsTrue(cat.StartsWith("http://random.cat/i/"));
        }
        [TestMethod]
        public void Dog_Success() {
            var dog = CatDog.Dog().Result;
            Assert.IsNotNull(dog);
            Assert.IsFalse(dog.IsNullOrWhiteSpace());
            Assert.IsTrue(dog.StartsWith("http://random.dog/"));
        }
    }
}
