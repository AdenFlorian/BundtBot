using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BundtBot.Utility;
using Discord;
using Message = Discord.Message;

namespace BundtBot.Extensions {
    static class ChannelExtensions {
        public static async Task<Message> SendMessageEx(this Channel channel, string msg) {
            MyLogger.Info($"Sending message `{msg}`", ConsoleColor.Cyan);
            if (ChannelHasOverride(channel)) {
                MyLogger.Info($"To Override Channel `{channel.Name}` on Server `{channel.Server}`", ConsoleColor.Cyan);
                return await BundtBot.TextChannelOverrides[channel.Server].SendMessage(msg);
            }
            MyLogger.Info($"To Channel `{channel.Name}` on Server `{channel.Server}`", ConsoleColor.Cyan);
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
