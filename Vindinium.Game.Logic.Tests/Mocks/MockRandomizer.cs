using System;

namespace Vindinium.Game.Logic.Tests.Mocks
{
    public class MockRandomizer : IRandomizer
    {
        private readonly Random _random;

        public MockRandomizer(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}