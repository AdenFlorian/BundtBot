using System;
using System.Linq;

namespace DiscordSharp_Starter.BundtBot {
    public static class ConsoleColorHelper {

        static bool initialized = false;

        static int roundRobinIndex = 0;
        static Random random = new Random();

        static ConsoleColor[] colors = new[] {
            ConsoleColor.Cyan,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Red,
            ConsoleColor.Yellow
        };

        public static ConsoleColor GetRandoColor() {
            var x = random.Next(colors.Count());
            return colors[x];
        }

        public static ConsoleColor GetRoundRobinColor() {
            if (initialized == false) {
                roundRobinIndex = random.Next(colors.Count());
                initialized = true;
            }

            if (roundRobinIndex == (colors.Length - 1)) {
                roundRobinIndex = 0;
            } else {
                roundRobinIndex++;
            }
            
            return colors[roundRobinIndex];
        }
    }
}
