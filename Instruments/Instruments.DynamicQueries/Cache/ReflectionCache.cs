using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Instruments.DynamicQueries.Cache
{
    public sealed class ReflectionCache
    {
        private static Lazy<ReflectionCache> _instance = new Lazy<ReflectionCache>(() => new ReflectionCache());
        public static ReflectionCache Instance => _instance.Value;

        private ConcurrentDictionary<string, PropertyInfo> _cache;

        private ReflectionCache()
        {
            _cache = new ConcurrentDictionary<string, PropertyInfo>();
        }

        public PropertyInfo GetPropertyInfo(string propertyName, Type objectType)
        {
            var key = GetPropertyKey(propertyName, objectType);

            if (_cache.ContainsKey(key))
                return _cache[key];

            return null;
        }

        public void SetPropertyInfo(string propertyName, Type objectType, PropertyInfo propertyInfo)
        {
            var key = GetPropertyKey(propertyName, objectType);

            _cache.TryAdd(key, propertyInfo);
        }
        
        private string GetPropertyKey(string propertyName, Type objectType)
        {
            return $"{objectType.FullName}.{propertyName}";
        }
    }
}
