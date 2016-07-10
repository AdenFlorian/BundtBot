using Discord;

namespace BundtBot.BundtBot.Extensions {
    static class DiscordNetExtensions {
        public static bool IsAFK(this Channel channel) {
            return channel == channel.Server.AFKChannel;
        }
    }
}
