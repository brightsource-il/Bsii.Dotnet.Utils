using System.Collections.Concurrent;
using System.Linq;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// A super simple, concurrent, priority queue.
    /// Dequeues from the highest priority to the lowest.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleConcurrentPriorityQueue<T>
    {
        private readonly ConcurrentQueue<T>[] _priorityQueues;
        private readonly int _minPriority;
        public SimpleConcurrentPriorityQueue(int minPriority, int maxPriority)
        {
            _minPriority = minPriority;
            _priorityQueues = Enumerable.Range(minPriority, maxPriority - minPriority + 1)
                .Select(_ => new ConcurrentQueue<T>()).ToArray();
        }

        public bool TryDequeue(
#if !NETSTANDARD2_0
            [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)]
#endif
        out T item, out int priority)
        {
            for (var i = _priorityQueues.Length - 1; i > -1; i--)
            {
                if (_priorityQueues[i].TryDequeue(out item))
                {
                    priority = i + _minPriority;
                    return true;
                }
            }
            item = default;
            priority = 0;
            return false;
        }

        public void Enqueue(int priority, T item) =>
            _priorityQueues[priority - _minPriority].Enqueue(item);
    }
}
