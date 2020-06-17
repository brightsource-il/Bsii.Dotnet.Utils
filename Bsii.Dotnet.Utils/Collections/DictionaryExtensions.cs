using System;
using System.Collections.Generic;

namespace Bsii.Dotnet.Utils.Collections
{
    public static class DictionaryExtensions
    {
        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
            Func<TKey, TValue> addValueFactory,
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                var updateValue = updateValueFactory(key, value);
                dictionary[key] = updateValue;
                return updateValue;
            }

            var addValue = addValueFactory(key);
            dictionary.Add(key, addValue);
            return addValue;
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey addKey, TValue addValue,
            TValue updateValue)
        {
            return dictionary.AddOrUpdate(addKey, (key) => addValue, (key, value) => updateValue);
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey addKey,
            Func<TKey, TValue> addValueFactory,
            TValue updateValue)
        {
            return dictionary.AddOrUpdate(addKey, addValueFactory, (key, value) => updateValue);
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue addValue,
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            return dictionary.AddOrUpdate(key, addValue, updateValueFactory);
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
            Func<TKey, TValue> addValueFactory,
            Action<TKey, TValue> updateAction)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                updateAction(key, value);
                return value;
            }

            var addValue = addValueFactory(key);
            dictionary.Add(key, addValue);
            return addValue;
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey addKey,
            Func<TKey, TValue> addValueFactory,
            Action updateAction)
        {
            return dictionary.AddOrUpdate(addKey, addValueFactory, (key, value) => updateAction());
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey addKey, TValue addValue,
            Action<TKey, TValue> updateAction)
        {
            return dictionary.AddOrUpdate(addKey, (key) => addValue, updateAction);
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey addKey, TValue addValue,
            Action updateAction)
        {
            return dictionary.AddOrUpdate(addKey, (key) => addValue, (key, value) => updateAction());
        }
    }
}