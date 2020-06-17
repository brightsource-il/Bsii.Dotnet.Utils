using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
            TValue addValue,
            TValue updateValue)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = updateValue;
                return updateValue;
            }

            dictionary.Add(key, addValue);
            return addValue;
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
            TKey key, Func<TKey, TValue> addValueFactory, TValue updateValue)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = updateValue;
                return updateValue;
            }

            var addedValue = addValueFactory(key);
            dictionary.Add(key, addedValue);
            return addedValue;
        }

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
            TValue addValue,
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (dictionary.TryGetValue(key, out var valueToUpdate))
            {
                if (typeof(TValue).IsValueType)
                {
                    var updatedValue = updateValueFactory(key, valueToUpdate);
                    dictionary[key] = updatedValue;
                    return updatedValue;
                }

                updateValueFactory(key, valueToUpdate);
                return valueToUpdate;
            }

            dictionary.Add(key, addValue);
            return addValue;
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

        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
            TValue addValue,
            Action<TKey, TValue> updateAction)
        {
            if (dictionary.TryGetValue(key, out var valueToUpdate))
            {
                updateAction(key, valueToUpdate);
                return valueToUpdate;
            }

            dictionary.Add(key, addValue);
            return addValue;
        }

        public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, Task<TValue>> addValueFactory,
            Func<TKey, TValue, Task> updateValueFactory)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                await updateValueFactory(key, value);
                return value;
            }

            var addValue = await addValueFactory(key);
            dictionary.Add(key, addValue);
            return addValue;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue valueToAdd)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            dictionary[key] = valueToAdd;
            return valueToAdd;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            var valueToAdd = addValueFactory(key);
            dictionary[key] = valueToAdd;
            return valueToAdd;
        }
        
        
    }
}