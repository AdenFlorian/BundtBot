using System.Threading.Tasks;
using Discord;

namespace BundtBot.BundtBot.Extensions {
    static class ChannelExtensions {
        public static async Task<Message> SendMessageEx(this Channel channel, string msg) {
            // Check if server has override channel
            if (Program.TextChannelOverrides.ContainsKey(channel.Server)) {
                if (Program.TextChannelOverrides[channel.Server] != null) {
                    return await Program.TextChannelOverrides[channel.Server].SendMessage(msg);
                }
            }

            return await channel.SendMessage(msg);
        }
    }
}
