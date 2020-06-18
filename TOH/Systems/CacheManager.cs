using System;
using System.Collections.Generic;
using System.Text;

namespace TOH.Systems
{
    public sealed class CacheManager
    {
        private static readonly Lazy<CacheManager> lazy = new Lazy<CacheManager>(() => new CacheManager(), true);

        public static CacheManager Instance { get { return lazy.Value; } }

        public T Get<T>(string key) where T : IConvertible
        {
            return default;
        }

        public void Set<T>(string key, T value)
        {
            var stringValue = Convert.ToString(value);

            
        }
    }
}
