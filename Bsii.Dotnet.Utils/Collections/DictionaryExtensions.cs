﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils.Collections
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds or updates a value in a dictionary. If the key exists it calls the updateValueFactory and updates the value, else it inserts a new value from addValueFactory
        /// </summary>
        /// <returns>A value added or updated in dictionary</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
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

        /// <summary>
        /// Adds or updates a value in a dictionary. If the key exists it updates the value to updateValue, else it inserts to dictionary with value = addValue
        /// </summary>
        /// <returns>A value added or updated in dictionary</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
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

        /// <summary>
        /// Adds or updates a value in a dictionary. If the key exists it updates the value to updateValue, else it inserts a new value from addValueFactory
        /// </summary>
        /// <returns>A value added or updated in dictionary</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
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

        /// <summary>
        /// Adds or updates a value in a dictionary. If the key exists it calls the updateValueFactory and updates the value, else it inserts to dictionary with value = addValue
        /// </summary>
        /// <returns>A value added or updated in dictionary</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
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

        /// <summary>
        /// Adds or updates a value in a dictionary. If the key exists it calls the updateAction with key and ref to value and thn returns value,
        ///  else it inserts a new value from addValueFactory
        /// </summary>
        /// <returns>A value added or updated in dictionary</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
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

        /// <summary>
        /// Adds or updates a value in a dictionary. If the key exists it calls the updateAction with key and ref to value and thn returns value,else it inserts to dictionary with value = addValue
        /// </summary>
        /// <returns>A value added or updated in dictionary</returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
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

        /// <summary>
        /// Async Adds or updates a value in a dictionary. If the key exists it calls the async updateValueFactory and updates the value, else it inserts a new value from async addValueFactory
        /// </summary>
        /// <returns>A Task of value added or updated in dictionary</returns>
        public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
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

        /// <summary>
        /// Gets or Adds value to dictionary. If the key exists it returns it from the dictionary, else insert to dictionary with value=valueToAdd
        /// </summary>
        /// <returns>A value in dictionary or an added one given by the user</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue valueToAdd)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            dictionary[key] = valueToAdd;
            return valueToAdd;
        }

        /// <summary>
        /// Gets or Adds value to dictionary. If the key exists it returns it from the dictionary, else it inserts a new value from addValueFactory
        /// </summary>
        /// <returns>A value in dictionary or an added one given by the user</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TKey, TValue> addValueFactory)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            var valueToAdd = addValueFactory(key);
            dictionary[key] = valueToAdd;
            return valueToAdd;
        }

        /// <summary>
        /// Async Gets or Adds value to dictionary. If the key exists it returns it from the dictionary, else it inserts a new value from async addValueFactory
        /// </summary>
        /// <returns>Avalue in dictionary or an Task added value given by the user in async factory</returns>
        public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TKey, Task<TValue>> addValueFactory)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            var valueToAdd = await addValueFactory(key);
            dictionary[key] = valueToAdd;
            return valueToAdd;
        }
    }
}