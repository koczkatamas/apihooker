using System;
using System.Collections.Generic;

namespace ApiHooker.Utils.ExtensionMethods
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> itemFactory)
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
                dict[key] = value = itemFactory(key);
            return value;
        }
    }
}
