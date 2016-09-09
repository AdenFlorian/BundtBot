using System;
using System.Collections.Generic;

namespace BundtBot.Effects {
    public class Echo : IEffect {

        public int EchoLength { get; }
        public float EchoFactor { get; }

        readonly Queue<float> _samples = new Queue<float>();

        public Echo(int length = 5000, float factor = 0.5f) {
            EchoLength = length;
            EchoFactor = factor;

            for (var i = 0; i < length; i++) {
                _samples.Enqueue(0f);
            }
        }

        public float ApplyEffect(float sample) {
            sample *= 0.5f;
            _samples.Enqueue(sample);
            return Math.Min(1, Math.Max(-1, sample + EchoFactor * _samples.Dequeue()));
        }
    }
}
