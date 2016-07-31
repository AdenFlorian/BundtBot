using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BundtBotTest.BundtBot {
    [TestClass]
    public class BundtBotTest {

        class StubDiscordClient : DiscordClient {
            
        }

        [TestMethod]
        public void Start_Success() {
            //var discordClient = new StubDiscordClient();

            var bundtBot = new global::BundtBot.BundtBot();
            bundtBot.Start();
        }
    }
}
