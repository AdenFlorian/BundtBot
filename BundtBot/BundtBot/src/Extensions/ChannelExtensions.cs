using System.Threading.Tasks;
using Discord;

namespace BundtBot.Extensions {
    static class ChannelExtensions {
        public static async Task<Message> SendMessageEx(this Channel channel, string msg) {
            if (ChannelHasOverride(channel)) {
                return await BundtBot.TextChannelOverrides[channel.Server].SendMessage(msg);
            }
            return await channel.SendMessage(msg);
        }

        static bool ChannelHasOverride(Channel channel) {
            if (channel.Server == null) return false;
            if (BundtBot.TextChannelOverrides.ContainsKey(channel.Server) == false) return false;
            if (BundtBot.TextChannelOverrides[channel.Server] == null) return false;
            return true;
        }
    }
}
