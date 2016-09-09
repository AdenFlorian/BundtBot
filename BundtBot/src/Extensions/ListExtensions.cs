using System;
using System.Collections.Generic;

namespace BundtBot.Extensions {
    static class ListExtensions {
        static readonly Random _random = new Random();

        public static T GetRandom<T>(this List<T> list) {
            return list[_random.Next(0, list.Count)];
        }
    }
}
