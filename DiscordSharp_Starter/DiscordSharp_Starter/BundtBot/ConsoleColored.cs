using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp_Starter.BundtBot {
    public static class ConsoleColored {
        public static void Write(string message, ConsoleColor color) {
            var startingColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = startingColor;
        }
        public static void WriteLine(string message, ConsoleColor color) {
            var startingColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = startingColor;
        }
    }
}
