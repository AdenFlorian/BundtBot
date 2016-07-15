using System;
using NString;

namespace BundtBot.BundtBot.Utility {
    public static class MyLogger {

        public static bool EnableTimestamps = false;

        public static void Write(string message, ConsoleColor color) {
            var startingColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Write(message);
            Console.ForegroundColor = startingColor;
        }

        public static void WriteLine(string message, ConsoleColor color) {
            var startingColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            WriteLine(message);
            Console.ForegroundColor = startingColor;
        }

        public static void WriteLineMultiColored(string message, bool randomSequence = false) {
            ConsoleColorHelper.ResetRoundRobinToStart();
            var startingColor = Console.ForegroundColor;

            foreach (var item in message) {
                if (item == '\n') {
                    ConsoleColorHelper.ResetRoundRobinToStart();
                    Write(item.ToString());
                    continue;
                }

                ConsoleColor nextColor;

                if (randomSequence) {
                    nextColor = ConsoleColorHelper.GetRandoColor();
                } else {
                    nextColor = ConsoleColorHelper.GetRoundRobinColor();
                }

                Write(item.ToString(), nextColor);
            }

            NewLine();
            Console.ForegroundColor = startingColor;
        }

        public static void Write(string message) {
            if (EnableTimestamps) {
                Console.Write(DateTime.Now + " | " + message);
            } else {
                Console.Write(message);
            }
        }

        public static void WriteLine(string message) {
            if (EnableTimestamps) {
                Console.WriteLine(DateTime.Now + " | " + message);
            } else {
                Console.WriteLine(message);
            }
        }

        public static void NewLine() {
            Console.WriteLine();
        }

        public static void WriteExitMessageAndReadKey() {
            WriteLine("Press the any key to exit...");
            Console.ReadKey(true);
        }

        public static void WriteException(Exception exception, string messagePrefix = "") {
            if (messagePrefix.IsNullOrWhiteSpace() == false) {
                WriteLine(messagePrefix, ConsoleColor.Magenta);
            }
            WriteLine(exception.Message, ConsoleColor.Red);
            WriteLine(exception.StackTrace, ConsoleColor.Red);
        }
    }
}
