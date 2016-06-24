using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp_Starter.BundtBot {
    public static class ConsoleColorHelper {

        static int roundRobinIndex = 0;

        static ConsoleColor[] colors = new[] {
            ConsoleColor.Cyan,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Red,
            ConsoleColor.Yellow
        };

        public static ConsoleColor GetRandoColor() {
            var random = new Random();
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
    }
}
