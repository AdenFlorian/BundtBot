using System;

namespace BundtBot.BundtBot.Utility {
    public static class ConsoleColorHelper {

        static int _roundRobinIndex;
        static readonly Random _random = new Random();

        static readonly ConsoleColor[] _colors = {
            ConsoleColor.Cyan,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Red,
            ConsoleColor.Yellow
        };

        public static ConsoleColor GetRandoColor() {
            var x = _random.Next(_colors.Length);
            return _colors[x];
        }

        public static ConsoleColor GetRoundRobinColor() {
            if (_roundRobinIndex == _colors.Length - 1) {
                _roundRobinIndex = 0;
            } else {
                _roundRobinIndex++;
            }
            
            return _colors[_roundRobinIndex];
        }

        public static void ResetRoundRobinToStart() {
            _roundRobinIndex = 0;
        }

        public static void ResetRoundRobinRandomly() {
            _roundRobinIndex = _random.Next(_colors.Length);
        }
    }
}
