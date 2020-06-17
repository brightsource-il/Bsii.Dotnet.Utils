using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Bsii.Dotnet.Utils.Collections
{
    /// <summary>
    ///     Provides useful extension methods for generic collections
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Returns a string representation of this IDictionary collection
        /// </summary>
        /// <typeparam name="TKey">Type of Keys</typeparam>
        /// <typeparam name="TValue">Type of Values</typeparam>
        /// <param name="dictionary">the collection to be stringified</param>
        /// <param name="maxElements">The limit to the maximal number of values to return, 0 for unlimited</param>
        /// <returns>The string representation of dictionary according to the provided formatting options</returns>
        public static string AsString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, int maxElements = 0)
        {
            return AsString(dictionary, null, null, ", ", ": ", maxElements);
        }

        /// <summary>
        ///     Returns a string representation of this IDictionary collection
        /// </summary>
        /// <typeparam name="TKey">Type of Keys</typeparam>
        /// <typeparam name="TValue">Type of Values</typeparam>
        /// <param name="dictionary">the collection to be stringified</param>
        /// <param name="keyFormat">The format of the pair key</param>
        /// <param name="maxElements">The limit to the maximal number of values to return, 0 for unlimited</param>
        /// <returns>The string representation of dictionary according to the provided formatting options</returns>
        public static string AsString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, string keyFormat,
            int maxElements = 0)
        {
            return AsString(dictionary, keyFormat, null, ", ", ": ", maxElements);
        }

        /// <summary>
        ///     Returns a string representation of this IDictionary collection
        /// </summary>
        /// <typeparam name="TKey">Type of Keys</typeparam>
        /// <typeparam name="TValue">Type of Values</typeparam>
        /// <param name="dictionary">the collection to be stringified</param>
        /// <param name="keyFormat">The format of the pair key</param>
        /// <param name="valueFormat">The format of the pair value</param>
        /// <param name="maxElements">The limit to the maximal number of values to return, 0 for unlimited</param>
        /// <returns>The string representation of dictionary according to the provided formatting options</returns>
        public static string AsString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, string keyFormat,
            string valueFormat, int maxElements = 0)
        {
            return AsString(dictionary, keyFormat, valueFormat, ", ", ": ", maxElements);
        }

        /// <summary>
        ///     Returns a string representation of this IDictionary collection
        /// </summary>
        /// <typeparam name="TKey">Type of Keys</typeparam>
        /// <typeparam name="TValue">Type of Values</typeparam>
        /// <param name="dictionary">the collection to be stringified</param>
        /// <param name="keyFormat">The format of the pair key</param>
        /// <param name="valueFormat">The format of the pair value</param>
        /// <param name="memberDelimiter">Separator between pairs</param>
        /// <param name="maxElements">The limit to the maximal number of values to return, 0 for unlimited</param>
        /// <returns>The string representation of dictionary according to the provided formatting options</returns>
        public static string AsString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, string keyFormat,
            string valueFormat, string memberDelimiter, int maxElements = 0)
        {
            return AsString(dictionary, keyFormat, valueFormat, memberDelimiter, ": ", maxElements);
        }

        /// <summary>
        ///     Returns a string representation of this IDictionary collection
        /// </summary>
        /// <typeparam name="TKey">Type of Keys</typeparam>
        /// <typeparam name="TValue">Type of Values</typeparam>
        /// <param name="dictionary">the collection to be stringified</param>
        /// <param name="keyFormat">The format of the pair key</param>
        /// <param name="valueFormat">The format of the pair value</param>
        /// <param name="memberDelimiter">Separator between pairs</param>
        /// <param name="pairDelimiter">Separator between key and value</param>
        /// <param name="maxElements">The limit to the maximal number of values to return, 0 for unlimited</param>
        /// <returns>The string representation of dictionary according to the provided formatting options</returns>
        public static string AsString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, string keyFormat,
            string valueFormat, string memberDelimiter,
            string pairDelimiter, int maxElements = 0)
        {
            var formatString = $"{{0:{keyFormat}}}{{1}}{{2:{valueFormat}}}";
            return AsStringImpl(dictionary, maxElements, memberDelimiter, '{', '}',
                kvp => string.Format(formatString, kvp.Key, pairDelimiter, kvp.Value));
        }

        /// <summary>
        ///     Returns a string representation of this IEnumerable collection separated by commas
        /// </summary>
        /// <typeparam name="T">The type of the collection members</typeparam>
        /// <param name="list">The list to be stringified</param>
        /// <param name="maxElements">The limit to the maximal number of values to return, 0 for unlimited</param>
        /// <returns>The string representation of list according to the provided formatting options</returns>
        public static string AsString<T>(this IEnumerable<T> list, int maxElements = 0)
        {
            return AsString(list, null, maxElements);
        }

        /// <summary>
        ///     Returns a string representation of this IEnumerable collection separated by commas
        /// </summary>
        /// <typeparam name="T">The type of the collection members</typeparam>
        /// <param name="list">The list to be stringified</param>
        /// <param name="format">The format of the members of the list</param>
        /// <param name="maxElements">The max number of elements to print</param>
        /// <returns>The string representation of list according to the provided formatting options</returns>
        public static string AsString<T>(this IEnumerable<T> list, string format, int maxElements = 0)
        {
            return AsString(list, format, ", ", maxElements);
        }

        /// <summary>
        ///     Returns a string representation of this IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The type of the collection members</typeparam>
        /// <param name="list">The list to be stringified</param>
        /// <param name="format">The format of the members of the list</param>
        /// <param name="delimiter">Separator between members of the collection</param>
        /// <param name="maxElements">The max number of elements to print, zero for unbound</param>
        /// <returns>The string representation of list according to the provided formatting options</returns>
        public static string AsString<T>(this IEnumerable<T> list, string format, string delimiter, int maxElements = 0)
        {
            var fmt = format == null
                ? new Func<T, string>(e => e.ToString())
                : e => string.Format($"{{0:{format}}}", e);
            return AsStringImpl(list, maxElements, delimiter, '[', ']', fmt);
        }

        private static string AsStringImpl<T>(
            IEnumerable<T> list,
            int maxElements,
            string delimiter,
            char openChar,
            char closeChar,
            Func<T, string> formatter)
        {
            void AppendElement(IEnumerator<T> en, StringBuilder stringBuilder)
            {
                stringBuilder.Append(en.Current != null ? formatter(en.Current) : "null");
            }

            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var sb = new StringBuilder(maxElements * 3 + 2); //Just some heuristics, maybe cache this?
            sb.Append(openChar);
            using (var en = list.GetEnumerator())
            {
                if (en.MoveNext()) //At least one element
                {
                    AppendElement(en, sb);

                    var count = 1;
                    while (en.MoveNext())
                    {
                        sb.Append(delimiter);
                        if (count == maxElements)
                        {
                            if (list is ICollection<T> c)
                            {
                                sb.Append($"...({c.Count - maxElements} more)");
                            }
                            else
                            {
                                sb.Append("...");
                            }

                            break;
                        }

                        AppendElement(en, sb);
                        ++count;
                    }
                }
            }

            sb.Append(closeChar);
            return sb.ToString();
        }

        /// <summary>
        ///     Represents an integer range as a string batched by ranges, i.e. new[]{1,2,5,6,7,8,11} --> [1-2,5-8,11]
        /// </summary>
        /// <param name="collection">The data to print</param>
        /// <param name="delimiter">between clauses</param>
        /// <param name="rangeDelimiter">within range clause</param>
        /// <param name="openChar"></param>
        /// <param name="closeChar"></param>
        /// <param name="maxClauses">Maximal number of elements/ranges to output, 0 for unlimited</param>
        /// <returns></returns>
        public static string AsConsecutiveRangesString(this IEnumerable<int> collection, string delimiter = ",",
            string rangeDelimiter = "-", char openChar = '[', char closeChar = ']', int maxClauses = 0)
        {
            var sb = new StringBuilder();
            sb.Append(openChar);
            var rangeMode = false;
            var nClauses = 0;

            using (var en = collection.GetEnumerator())
            {
                if (en.MoveNext()) //At least one element
                {
                    var prev = en.Current;
                    var delimiterNeeded = false;
                    while (en.MoveNext())
                    {
                        if (delimiterNeeded)
                        {
                            if (nClauses == maxClauses)
                            {
                                break;
                            }

                            sb.Append(delimiter);
                        }

                        if (en.Current - prev == 1) //Consecutive numbers
                        {
                            if (!rangeMode) //entering range mode
                            {
                                rangeMode = true;
                                sb.Append(prev);
                                sb.Append(rangeDelimiter);
                            }

                            delimiterNeeded = false;
                        }
                        else //Not range mode
                        {
                            sb.Append(prev); //Append the previous element
                            rangeMode = false;
                            delimiterNeeded = true;
                            ++nClauses;
                        }

                        prev = en.Current;
                    }

                    if (delimiterNeeded)
                    {
                        sb.Append(delimiter);
                    }

                    if (maxClauses > 0 && nClauses == maxClauses
                    ) //If there is a clause limit and there are potentially more clauses
                    {
                        sb.Append("...");
                    }
                    else
                    {
                        sb.Append(prev); //Last element
                    }
                }
            }

            sb.Append(closeChar);
            return sb.ToString();
        }


        /// <summary>
        ///     Represents an unsigned integer range as a string batched by ranges, i.e. new[]{1,2,5,6,7,8,11} --> [1-2,5-8,11]
        /// </summary>
        /// <param name="collection">The data to print</param>
        /// <param name="delimiter">between clauses</param>
        /// <param name="rangeDelimiter">within range clause</param>
        /// <param name="openChar"></param>
        /// <param name="closeChar"></param>
        /// <param name="maxClauses">Maximal number of elements/ranges to output, 0 for unlimited</param>
        /// <returns></returns>
        public static string AsConsecutiveRangesString(this IEnumerable<uint> collection, string delimiter = ",",
            string rangeDelimiter = "-", char openChar = '[', char closeChar = ']', int maxClauses = 0)
        {
            var sb = new StringBuilder();
            sb.Append(openChar);
            var rangeMode = false;
            var nClauses = 0;

            using (var en = collection.GetEnumerator())
            {
                if (en.MoveNext()) //At least one element
                {
                    var prev = en.Current;
                    var delimiterNeeded = false;
                    while (en.MoveNext())
                    {
                        if (delimiterNeeded)
                        {
                            if (nClauses == maxClauses)
                            {
                                break;
                            }

                            sb.Append(delimiter);
                        }

                        if (en.Current - prev == 1) //Consecutive numbers
                        {
                            if (!rangeMode) //entering range mode
                            {
                                rangeMode = true;
                                sb.Append(prev);
                                sb.Append(rangeDelimiter);
                            }

                            delimiterNeeded = false;
                        }
                        else //Not range mode
                        {
                            sb.Append(prev); //Append the previous element
                            rangeMode = false;
                            delimiterNeeded = true;
                            ++nClauses;
                        }

                        prev = en.Current;
                    }

                    if (delimiterNeeded)
                    {
                        sb.Append(delimiter);
                    }

                    if (maxClauses > 0 && nClauses == maxClauses
                    ) //If there is a clause limit and there are potentially more clauses
                    {
                        sb.Append("...");
                    }
                    else
                    {
                        sb.Append(prev); //Last element
                    }
                }
            }

            sb.Append(closeChar);
            return sb.ToString();
        }


        /// <summary>
        ///     Adds the specified range to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection members.</typeparam>
        /// <param name="collection">The collection to add the range to.</param>
        /// <param name="range">A range of elements to add to collection.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> range)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            if (collection is List<T> listCollection)
            {
                listCollection.AddRange(range);
                return;
            }

            foreach (var e in range)
            {
                collection.Add(e);
            }
        }

        /// <summary>
        ///     Checks whether a collection is sorted in an ascending order
        /// </summary>
        /// <typeparam name="T">Member type of the collection</typeparam>
        /// <param name="list">The collection to check</param>
        /// <returns>
        ///     <b>true</b> if list is sorted in ascending order; <b>false</b>, otherwise.
        /// </returns>
        public static bool IsSorted<T>(this IEnumerable<T> list) where T : IComparable<T>
        {
            return list.IsSorted((t1, t2) => t1.CompareTo(t2));
        }

        /// <summary>
        ///     Checks whether a collection is sorted according to the specified comparison in ascending order.
        /// </summary>
        /// <typeparam name="T">Member type of the collection</typeparam>
        /// <param name="list">The collection to check</param>
        /// <param name="comparison">The comparator for the lists's elements.</param>
        /// <returns>
        ///     <b>true</b> if list is sorted in ascending order according to comparison; <b>false</b>, otherwise.
        /// </returns>
        public static bool IsSorted<T>(this IEnumerable<T> list, Comparison<T> comparison)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (comparison == null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            using (var iter = list.GetEnumerator())
            {
                iter.MoveNext();
                var previous = iter.Current;
                while (iter.MoveNext())
                {
                    if (comparison(previous, iter.Current) > 0)
                    {
                        return false;
                    }

                    previous = iter.Current;
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns a merged and sorted collection.
        /// </summary>
        /// <typeparam name="T">Member type of the collection.</typeparam>
        /// <param name="list">The first collection to merge with. This collection is assumed to be sorted in ascending order.</param>
        /// <param name="other">The second collection to merge with. This collection is assumed to be sorted in ascending order.</param>
        /// <returns>An sorted enumeration consisting of the elements of the two specified collections.</returns>
        public static IEnumerable<T> MergeSorted<T>(this IEnumerable<T> list, IEnumerable<T> other)
            where T : IComparable<T>
        {
            return list.MergeSorted(other, (t1, t2) => t1.CompareTo(t2));
        }

        /// <summary>
        ///     Returns a merged and sorted collection.
        /// </summary>
        /// <typeparam name="T">Member type of the collection.</typeparam>
        /// <param name="list">
        ///     The first collection to merge with. This collection is assumed to be sorted in ascending order
        ///     according to the specified comparison.
        /// </param>
        /// <param name="other">
        ///     The second collection to merge with. This collection is assumed to be sorted in ascending order
        ///     according to the specified comparison.
        /// </param>
        /// <param name="comparison">The comparison according to which the collections are supposed to be sorted.</param>
        /// <returns>An sorted enumeration consisting of the elements of the two specified collections.</returns>
        public static IEnumerable<T> MergeSorted<T>(this IEnumerable<T> list, IEnumerable<T> other,
            Comparison<T> comparison)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            using (var firstEnumerator = list.GetEnumerator())
            {
                using (var secondEnumerator = other.GetEnumerator())
                {
                    var elementsLeftInFirst = firstEnumerator.MoveNext();
                    var elementsLeftInSecond = secondEnumerator.MoveNext();
                    while (elementsLeftInFirst || elementsLeftInSecond)
                    {
                        if (!elementsLeftInFirst)
                        {
                            do
                            {
                                yield return secondEnumerator.Current;
                            } while (secondEnumerator.MoveNext());

                            yield break;
                        }

                        if (!elementsLeftInSecond)
                        {
                            do
                            {
                                yield return firstEnumerator.Current;
                            } while (firstEnumerator.MoveNext());

                            yield break;
                        }

                        if (comparison(firstEnumerator.Current, secondEnumerator.Current) < 0)
                        {
                            yield return firstEnumerator.Current;
                            elementsLeftInFirst = firstEnumerator.MoveNext();
                        }
                        else
                        {
                            yield return secondEnumerator.Current;
                            elementsLeftInSecond = secondEnumerator.MoveNext();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets a value from a dictionary, throws a custom exception which contains info about the key
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        /// <exception cref="InfoKeyNotFoundException"></exception>
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out var res))
            {
                return res;
            }

            throw new InfoKeyNotFoundException(key);
        }

        /// <summary>
        ///     Modifies a collection in-place by removing items from the collection that match
        ///     a given <see cref="T:Predicate[T]" />.
        /// </summary>
        /// <remarks>
        ///     The type of collection passed in will affect how the method performs. For collections
        ///     with a built-in method to remove in-place (such as sets) the existing implementation
        ///     will be used. For collections implementing IList[T], the method will perform better
        ///     because the collection can be enumerated more efficiently. For all other collections,
        ///     the items to remove will be buffered and Remove will be called individually which,
        ///     depending on the collection type, can be very slow resulting in an O(n) scan to
        ///     determine the items to remove, then a separate O(n) scan for each item that matched.
        /// </remarks>
        /// <typeparam name="T">The type of item in the collection.</typeparam>
        /// <param name="collection">The collection to remove from.</param>
        /// <param name="match">The predicate that determines if the item will be removed.</param>
        /// <returns>The number of items removed.</returns>
        /// <example>
        ///     <![CDATA[
        ///     var numbers = new Collection<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        ///     numbers.RemoveAll( x => x % 2 == 0 );  // remove even numbers
        /// ]]>
        /// </example>
        public static int RemoveAll<T>(this ICollection<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (collection.IsReadOnly)
            {
                throw new NotSupportedException("The collection is read-only.");
            }

            // Defer to existing implementation...
            if (collection is HashSet<T> hashSetOfT)
            {
                return hashSetOfT.RemoveWhere(match);
            }

            // Defer to existing implementation...
            if (collection is SortedSet<T> sortedSetOfT)
            {
                return sortedSetOfT.RemoveWhere(match);
            }

            // Defer to existing implementation...
            if (collection is List<T> listOfT)
            {
                return listOfT.RemoveAll(match);
            }

            // Have to use our own implementation.

            var removed = 0;

            // IList<T> is pretty efficient because we only have to enumerate
            // the list once and if a match, we remove at that position.
            // Enumerate backwards so that the indexes don't shift out from under us.
            if (collection is IList<T> list)
            {
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var item = list[i];
                    if (match(item))
                    {
                        list.RemoveAt(i);
                        removed++;
                    }
                }

                return removed;
            }

            // For ICollection<T> it isn't as efficient because we have to first
            // buffer all the items to remove in a temporary collection.
            // Then we enumerate that temp collection removing each individually
            // from the ICollection<T> which could be potentially O(n).

            var itemsToRemove = new List<T>(collection.Where(i => match(i)));
            foreach (var item in itemsToRemove)
            {
                if (collection.Remove(item))
                {
                    removed++;
                }
            }

            return removed;
        }

        /// <summary>
        ///     Removes all items from the source list which exist in the itemsToRemove collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="itemsToRemove"></param>
        /// <returns>The number of removed items</returns>
        public static int RemoveAll<T>(this ICollection<T> list, IEnumerable<T> itemsToRemove)
        {
            if (itemsToRemove == null)
            {
                throw new ArgumentNullException(nameof(itemsToRemove));
            }

            var hashset = new HashSet<T>(itemsToRemove);
            return RemoveAll(list, hashset);
        }


        /// <summary>
        ///     Removes all items from the source list which exist in the itemsToRemove hashset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="itemsToRemove"></param>
        /// <returns>The number of removed items</returns>
        public static int RemoveAll<T>(this ICollection<T> list, HashSet<T> itemsToRemove)
        {
            if (itemsToRemove == null)
            {
                throw new ArgumentNullException(nameof(itemsToRemove));
            }

            return list.RemoveAll(itemsToRemove.Contains);
        }


        /// <summary>
        ///     Retrieves several elements from the blocking collection based on a timeout provided
        ///     Throws the same exception as the Take method of blocking collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="blockingCollection">The collection to consume elements from</param>
        /// <param name="waitTime">The time to allocate for the operation</param>
        /// <param name="maxElements">Optional max of elements to consume</param>
        /// <returns>A collection that contains the elements obtained</returns>
        public static ICollection<T> TakeBatch<T>(this BlockingCollection<T> blockingCollection, TimeSpan waitTime,
            int? maxElements = null)
        {
            ValidateBatchParameters(waitTime, maxElements);
            return GetBatch(blockingCollection, waitTime, maxElements);
        }

        private static void ValidateBatchParameters(TimeSpan waitTime, int? maxElements)
        {
            if (waitTime <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(waitTime), "Only positive wait time is allowed");
            }

            if (maxElements <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxElements),
                    "Only positive max number of elements allowed");
            }
        }

        private static ICollection<T> GetBatch<T>(BlockingCollection<T> blockingCollection, TimeSpan waitTime,
            int? maxElements)
        {
            var res = new List<T>();
            var timeLeft = waitTime;
            var sw = Stopwatch.StartNew();
            while (!blockingCollection.IsAddingCompleted && timeLeft > TimeSpan.Zero)
            {
                if (blockingCollection.TryTake(out var data, timeLeft))
                {
                    res.Add(data);
                }

                if (res.Count >= maxElements)
                {
                    break;
                }

                timeLeft = waitTime - sw.Elapsed;
            }

            return res;
        }

        /// <summary>
        ///     Allows enumeration of blocking collection created by acquiring items over a period of time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="blockingCollection"></param>
        /// <param name="waitTime"></param>
        /// <param name="maxElements"></param>
        /// <returns></returns>
        public static IEnumerable<ICollection<T>> GetBatchConsumingEnumerable<T>(
            this BlockingCollection<T> blockingCollection, TimeSpan waitTime, int? maxElements = null)
        {
            ValidateBatchParameters(waitTime, maxElements);
            while (!blockingCollection.IsAddingCompleted)
            {
                ICollection<T> batch;
                try
                {
                    batch = GetBatch(blockingCollection, waitTime, maxElements);
                }
                catch (InvalidOperationException)
                {
                    yield break;
                }

                if (batch.Count > 0)
                {
                    yield return batch;
                }
            }
        }


        public static IDisposable AsDisposable<T>(this IEnumerable<T> collectionIn)
            where T : IDisposable
        {
            return new DisposableAction(() =>
            {
                var exceptions = new List<Exception>();
                foreach (var i in collectionIn)
                {
                    try
                    {
                        i.Dispose();
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }

                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
            });
        }

        public static IDisposable AsDisposable<T>(this T collectionIn, out T collectionOut)
            where T : IEnumerable<IDisposable>
        {
            collectionOut = collectionIn;
            return AsDisposable(collectionIn);
        }
    }
}