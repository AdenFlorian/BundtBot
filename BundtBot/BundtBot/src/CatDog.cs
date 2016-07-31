using System;
using System.Net.Http;
using System.Threading.Tasks;
using BundtBot.Utility;

namespace BundtBot {
    public static class CatDog {
        public static async Task<string> Cat() {
            using (var client = new HttpClient()) {
                client.Timeout = TimeSpan.FromSeconds(2);
                var s = await client.GetStringAsync("http://random.cat/meow");
                var pFrom = s.IndexOf("\\/i\\/", StringComparison.Ordinal) + "\\/i\\/".Length;
                var pTo = s.LastIndexOf("\"}", StringComparison.Ordinal);
                var cat = "http://random.cat/i/" + s.Substring(pFrom, pTo - pFrom);
                MyLogger.WriteLine(cat);
                return cat;
            }
        }
        public static async Task<string> Dog() {
            using (var client = new HttpClient()) {
                client.Timeout = TimeSpan.FromSeconds(2);
                var dog = "http://random.dog/" + await client.GetStringAsync("http://random.dog/woof");
                MyLogger.WriteLine(dog);
                return dog;
            }
        }
    }
}
