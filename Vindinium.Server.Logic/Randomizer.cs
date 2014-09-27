using System;

namespace Vindinium.Game.Logic
{
    public class Randomizer : IRandomizer
    {
        private readonly Random _random = new Random();

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}