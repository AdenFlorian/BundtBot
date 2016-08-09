using System;
using System.Threading.Tasks;
using BundtBot.Utility;
using Discord;
using Message = Discord.Message;

namespace BundtBot.Extensions {
    static class UserExtensions {
        public static async Task<Message> SendMessageEx(this User user, string msg) {
            MyLogger.Info($"Sending message `{msg}` to User `{user.Name}` for Server `{user.Server}`", ConsoleColor.Cyan);
            return await user.SendMessage(msg);
        }
    }
}
