using System;
using System.Text;
using JetBrains.Annotations;

namespace BundtBot {
    class Program {
        [UsedImplicitly]
        static void Main() {
            // Allows stuff like ʘ ͜ʖ ʘ to show in the Console
            Console.OutputEncoding = Encoding.UTF8;
            Console.WindowHeight = (int)(Console.LargestWindowHeight * 0.9);
            Console.WindowTop = 0;

            new BundtBot().Start();
        }
    }
}
