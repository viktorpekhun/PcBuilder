using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace PcBuilder.SharedKernel.Caching
{
    public class CacheService : ICacheInvalidator
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, byte> _keys = new();

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryGet<T>(string key, out T? value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public void Set<T>(string key, T value, TimeSpan duration)
        {
            _keys.TryAdd(key, 0);
            _cache.Set(key, value, duration);
        }

        public void InvalidateByPrefix(string prefix)
        {
            var keysToRemove = _keys.Keys
                .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.TryRemove(key, out _);
            }
        }
    }
}
