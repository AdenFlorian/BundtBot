using System;
using System.Collections.Generic;

namespace BundtBot.BundtBot.Effects {
    public class Reverb : IEffect {

        public int EchoLength { get; private set; }
        public float EchoFactor { get; }

        readonly Queue<float> _queue1 = new Queue<float>();

        public Reverb(int length = 3333, float factor = 0.5f) {
            EchoLength = length;
            EchoFactor = factor;

            for (var i = 0; i < length; i++) {
                _queue1.Enqueue(0f);
            }
        }

        public float ApplyEffect(float sample) {
            var x = Math.Min(1, Math.Max(-1, sample + EchoFactor * _queue1.Dequeue()));
            _queue1.Enqueue(x);
            return x;
        }
    }
}
