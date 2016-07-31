using System.Threading.Tasks;
using Discord;

namespace BundtBot.Extensions {
    static class ChannelExtensions {
        public static async Task<Message> SendMessageEx(this Channel channel, string msg) {
            // Check if server has override channel
            if (BundtBot.TextChannelOverrides.ContainsKey(channel.Server)) {
                if (BundtBot.TextChannelOverrides[channel.Server] != null) {
                    return await BundtBot.TextChannelOverrides[channel.Server].SendMessage(msg);
                }
            }

            return await channel.SendMessage(msg);
        }
    }
}
