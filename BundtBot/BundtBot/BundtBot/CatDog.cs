using System;
using System.Net.Http;
using System.Threading.Tasks;
using BundtBot.BundtBot.Extensions;
using BundtBot.BundtBot.Utility;
using Discord.Commands;

namespace BundtBot.BundtBot {
    public static class CatDog {
        public static async Task Cat(CommandEventArgs e) {
            var rand = new Random();
            if (rand.NextDouble() >= 0.5) {
                try {
                    using (var client = new HttpClient()) {
                        client.Timeout = TimeSpan.FromSeconds(2);
                        var s = await client.GetStringAsync("http://random.cat/meow");
                        var pFrom = s.IndexOf("\\/i\\/", StringComparison.Ordinal) + "\\/i\\/".Length;
                        var pTo = s.LastIndexOf("\"}", StringComparison.Ordinal);
                        var cat = s.Substring(pFrom, pTo - pFrom);
                        MyLogger.WriteLine("http://random.cat/i/" + cat);
                        await e.Channel.SendMessageEx("I found a cat\nhttp://random.cat/i/" + cat);
                    }
                } catch (Exception ex) {
                    MyLogger.WriteException(ex);
                    await e.Channel.SendMessageEx("there are no cats here, who let them out (random.cat is down :cat: :interrobang:)");
                }
            } else {
                await Dog(e, "how about a dog instead");
            }
        }
        public static async Task Dog(CommandEventArgs e, string message) {
            try {
                using (var client = new HttpClient()) {
                    client.Timeout = TimeSpan.FromSeconds(2);
                    var dog = await client.GetStringAsync("http://random.dog/woof");
                    MyLogger.WriteLine("http://random.dog/" + dog);
                    await e.Channel.SendMessageEx(message + "\nhttp://random.dog/" + dog);
                }
            } catch (Exception ex) {
                MyLogger.WriteException(ex);
                await e.Channel.SendMessageEx("there are no dogs here, who let them out (random.dog is down :dog: :interrobang:)");
            }
        }
    }
}
