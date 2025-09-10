using System;
using System.Collections.Generic;

namespace ALWTTT.Extentions
{
    public static class ListExtensions
    {
        // TODO: Centralize random seed to all extensions? Use IRandomNumber interface?
        private static System.Random rng = new System.Random();

        /// <summary>
        /// Set a custom seed for deterministic results.
        /// </summary>
        public static void SetSeed(int seed)
        {
            rng = new System.Random(seed);
        }

        /// <summary>
        /// Returns a random item from inside the <typeparam name="T">List</typeparam>
        /// </summary>
        public static T RandomItem<T>(this List<T> list)
        {
            if (list.Count == 0)
                throw new IndexOutOfRangeException("List is Empty");

            var randomIndex = rng.Next(0, list.Count);
            return list[randomIndex];
        }
    }
}