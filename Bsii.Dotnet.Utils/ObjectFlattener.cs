using System;
using System.Collections;
using System.Collections.Generic;
using FastMember;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// </summary>
    public class ObjectFlattener
    {
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
            public Func<Type, bool> IsPrimitivePredicate { get; set; } = ObjectFlattener.IsTerminalType;
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
            Flatten(null, value, type, onData, options ?? new Options(), 0);
        }

        private static void Flatten(string prefix, object val, Type valueType, Action<string, object> onData, Options options, int currentRecursionLevel)
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
                Flatten(prefix, val, val.GetType(), onData, options, currentRecursionLevel);
                return;
            }

            if (currentRecursionLevel++ == options.MaxDepth)
            {
                throw new ArgumentOutOfRangeException(nameof(val),
                    $"The depth of nested values reached the maximum defined {nameof(Options.MaxDepth)}");
            }

            if (options.IsPrimitivePredicate(valueType))
            {
                onData(prefix ?? string.Empty, val);
                return;
            }
            prefix = prefix == null ? "" : (prefix + options.PropertyPathSeparator);
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

                                Flatten(prefix + token.Key, token.Value, token.Value.GetType(), onData,
                                    options, currentRecursionLevel);
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

                            Flatten(prefix + i++, token, token.GetType(), onData, options,
                                currentRecursionLevel);
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
                    Flatten(prefix + member.Name, value, member.Type, onData, options, currentRecursionLevel);
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
    }
}
