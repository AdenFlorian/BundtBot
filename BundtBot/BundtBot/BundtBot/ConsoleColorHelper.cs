using System;
using System.Linq;

namespace BundtBot.BundtBot {
    public static class ConsoleColorHelper {

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
            if (roundRobinIndex == (colors.Length - 1)) {
                roundRobinIndex = 0;
            } else {
                roundRobinIndex++;
            }
            
            return colors[roundRobinIndex];
        }

        public static void ResetRoundRobinToStart() {
            roundRobinIndex = 0;
        }

        public static void ResetRoundRobinRandomly() {
            roundRobinIndex = random.Next(colors.Count());
        }
    }
}
