using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Loggers;
using FastMember;

namespace Bsii.Dotnet.Utils.Reflection
{
    /// <summary>
    /// </summary>
    public static class ObjectFlattener
    {
        private static Options DefaultOptions { get; } = new Options();
        private static ThreadLocal<StringBuilder> StringBuilderTls = new ThreadLocal<StringBuilder>(() => new StringBuilder());

        /// <summary>
        /// 
        /// </summary>
        public class Options
        {
            /// <summary>
            /// </summary>
            public bool ShouldFlattenDictionaries { get; set; } = true;
            /// <summary>
            /// </summary>
            public Func<Type, bool> IsPrimitivePredicate { get; set; } = IsTerminalType;
            /// <summary>
            /// In case the flattener encounters a type from this map - it will invoke the corresponding convert function prior to flattening the value. 
            /// </summary>
            public Dictionary<Type, Func<object, object>> CustomConverters { get; set; } = null;

            /// <summary>
            /// The maximum depth of nested properties to attempt flattening
            /// </summary>
            public int MaxDepth { get; set; } = 32;

            /// <summary>
            /// The threshold after which enumerable elements are discarded (default=0 - enumerable properties are skipped)
            /// </summary>
            public int MaxElementsInEnumerable { get; set; } = 0;

            /// <summary>
            /// The path separator to use when computing value's path
            /// </summary>
            public char PropertyPathSeparator { get; set; } = '.';
        }

        /// <summary>
        /// Flattens the given object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">An instance of the object</param>
        /// <param name="onData">callback on each resolved property value</param>
        /// <param name="options"></param>
        public static void Flatten<T>(object value, Action<string, object> onData, Options options = null)
        {
            Flatten(value, typeof(T), onData, options);
        }

        /// <summary>
        /// Flattens the given object and returns the dictionary of its properties values (including nested properties)
        /// </summary>
        public static Dictionary<string, object> FlattenToDictionary<T>(T value, Options options = null)
        {
            return FlattenToDictionary(value, typeof(T), options);
        }

        /// <summary>
        /// Flattens the given object and returns the dictionary of its properties values (including nested properties)
        /// </summary>
        public static Dictionary<string, object> FlattenToDictionary(object value, Type type, Options options = null)
        {
            var res = new Dictionary<string, object>();
            Flatten(value, type, (s, o) => res[s] = o, options);
            return res;
        }

        /// <summary>
        /// Flattens the given object
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="onData"></param>
        /// <param name="options"></param>
        public static void Flatten(object value, Type type, Action<string, object> onData, Options options = null)
        {
            var sb = StringBuilderTls.Value;
            sb.Clear();
            Flatten(StringBuilderTls.Value, value, type, onData, options ?? DefaultOptions, 0);
        }

        private static void Flatten(StringBuilder paths, object val, Type valueType, Action<string, object> onData, Options options, int currentRecursionLevel)
        {
            if (val == null)
            {
                return;
            }

            if (options.CustomConverters != null &&
                options.CustomConverters.TryGetValue(valueType, out var converter))
            {
                val = converter(val);
                if (val == null)
                {
                    return;
                }
                Flatten(paths, val, val.GetType(), onData, options, currentRecursionLevel);
                return;
            }

            if (currentRecursionLevel++ == options.MaxDepth)
            {
                throw new ArgumentOutOfRangeException(nameof(val),
                    $"The depth of nested values reached the maximum defined {nameof(Options.MaxDepth)}");
            }

            if (options.IsPrimitivePredicate(valueType))
            {
                onData(paths.ToString(), val);
                return;
            }
            if (paths.Length > 0)
            {
                paths.Append(options.PropertyPathSeparator);
            }
            switch (val)
            {
                case IDictionary valDictionary:
                    {
                        if (options.ShouldFlattenDictionaries)
                        {
                            foreach (DictionaryEntry token in valDictionary)
                            {
                                if (token.Value == null)
                                {
                                    continue;
                                }
                                var prevLen = paths.Length;
                                paths.Append(token.Key);
                                Flatten(paths, token.Value, token.Value.GetType(), onData,
                                    options, currentRecursionLevel);
                                paths.Length = prevLen;
                            }
                        }

                        return;
                    }
                case ICollection collection:
                    {
                        if (options.MaxElementsInEnumerable == 0 ||
                            collection.Count > options.MaxElementsInEnumerable)
                        {
                            return;
                        }

                        int i = 0;
                        foreach (var token in collection)
                        {
                            if (token == null)
                            {
                                continue;
                            }
                            var prevLen = paths.Length;
                            paths.Append(i++);
                            Flatten(paths, token, token.GetType(), onData, options,
                                currentRecursionLevel);
                            paths.Length = prevLen;
                        }

                        return;
                    }
                case IEnumerable _:
                    throw new NotImplementedException(
                        $"{nameof(ObjectFlattener)} doesn't support flattening of IEnumerable fields which are not ICollection!");
            }
            // if we got here, it means that the object type is complex (class/struct)
            var accessor = TypeAccessor.Create(valueType, false); // type-accessor caches itself
            var members = accessor.GetMembers(); // also cached
            foreach (var member in members)
            {
                if (!member.CanRead) //setter only properties for example
                {
                    continue;
                }

                var value = accessor[val, member.Name];
                if (value != null)
                {
                    var prevLen = paths.Length;
                    paths.Append(member.Name);
                    Flatten(paths, value, member.Type, onData, options, currentRecursionLevel);
                    paths.Length = prevLen;
                }
            }

        }

        /// <summary>
        /// Default primitives are defined as CLR primitives, strings, enums, TimeSpan, DateTime &amp; DateTimeOffset
        /// </summary>
        public static bool IsTerminalType(Type t)
        {
            return t.IsPrimitive
                   || t.IsEnum
                   || t == typeof(string)
                   || t == typeof(TimeSpan)
                   || t == typeof(DateTime)
                   || t == typeof(DateTimeOffset);
        }
        /// <summary>
        /// Flattens a dictionary to a dictionary of<key,<parameterName,parameterValue>>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict">the given dictionary to flatten</param>
        /// <param name="delimiter">delimiter string to input between parameters</param>
        /// <param name="shortNames">option to choose only the string after the last delimiter or show full string</param>
        /// <returns></returns>
        public static Dictionary<TKey, Dictionary<string, string>> FlattenDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict, string delimiter = ".", bool shortNames = false)
        {
            var dic = new Dictionary<TKey, Dictionary<string, string>>(dict.Count);
            foreach (var kvp in dict)
            {
                var hidRes = FlattenObjectToDictionary<TKey>
                    (kvp.Value, "", new Dictionary<string, string>(), delimiter, shortNames, new Dictionary<string, PropertyInfo[]>());
                dic.Add(kvp.Key, hidRes);
            }
            return dic;
        }

        private static Dictionary<string, string> FlattenObjectToDictionary<TKey>(object obj, string prefix,
            Dictionary<string, string> dic, string delimiter, bool shortNames,
            Dictionary<string, PropertyInfo[]> propertyDic)
        {
            if (obj == null)
            {
                return dic;
            }
            var objType = obj.GetType();
            if (!propertyDic.ContainsKey(prefix))
            {
                propertyDic[prefix] = objType.GetProperties();
            }
            var properties = propertyDic[prefix];
            var accessor = TypeAccessor.Create(objType);
            foreach (var property in properties)
            {
                object propValue = accessor[obj, property.Name];
                if (propValue == null)
                {
                    return dic;
                }

                var newPrefix = shortNames ? "" : prefix.Length == 0 ? prefix : prefix + delimiter;

                if (property.PropertyType.Assembly == objType.Assembly && !property.PropertyType.IsEnum)
                {
                    return FlattenObjectToDictionary<TKey>(propValue, $"{newPrefix}{property.Name}", dic, delimiter,
                        shortNames, propertyDic);
                }

                //now we have a primitive
                var name = shortNames ? property.Name : $"{newPrefix}{property.Name}";
                var camelCaseName = Char.ToLowerInvariant(name[0]) + name.Substring(1);
                dic[camelCaseName] = propValue.ToString();
                return dic;
            }

            return dic;
        }
    }
}
