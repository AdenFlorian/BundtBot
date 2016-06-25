using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp_Starter.BundtBot {
    public class Echo : IEffect {

        public int EchoLength { get; private set; }
        public float EchoFactor { get; private set; }

        Queue<float> samples;

        public Echo(int length = 5000, float factor = 0.5f) {
            this.EchoLength = length;
            this.EchoFactor = factor;
            this.samples = new Queue<float>();

            for (int i = 0; i < length; i++) {
                samples.Enqueue(0f);
            }
        }

        public float ApplyEffect(float sample) {
            samples.Enqueue(sample);
            return Math.Min(1, Math.Max(-1, sample + EchoFactor * samples.Dequeue()));
        }
    }
}
