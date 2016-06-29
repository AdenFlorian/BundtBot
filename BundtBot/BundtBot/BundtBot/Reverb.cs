using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundtBot.BundtBot {
    public class Reverb : IEffect {

        public int EchoLength { get; private set; }
        public float EchoFactor { get; private set; }

        Queue<float> queue1;

        public Reverb(int length = 3333, float factor = 0.5f) {
            this.EchoLength = length;
            this.EchoFactor = factor;
            this.queue1 = new Queue<float>();

            for (int i = 0; i < length; i++) {
                queue1.Enqueue(0f);
            }
        }

        public float ApplyEffect(float sample) {
            var x = Math.Min(1, Math.Max(-1, sample + EchoFactor * queue1.Dequeue()));
            queue1.Enqueue(x);
            return x;
        }
    }
}
