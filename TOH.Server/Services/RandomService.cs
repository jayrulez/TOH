using System;
using System.Collections.Generic;

namespace TOH.Server.Services
{
    public class RandomService
    {
        private readonly Random _randomSource;

        public RandomService()
        {
            _randomSource = new Random(DateTime.UtcNow.Millisecond);
        }

        public T GetRandom<T>(List<T> sample)
        {
            var index = _randomSource.Next(sample.Count);

            return sample[index];
        }
    }
}
