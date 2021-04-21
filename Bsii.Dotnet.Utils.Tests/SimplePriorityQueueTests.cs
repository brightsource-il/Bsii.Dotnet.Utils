using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class SimpleConcurrentPriorityQueueTests
    {
        [Theory]
        [InlineData(0, 10)]
        [InlineData(10, 11)]
        [InlineData(10, 10)]
        [InlineData(5, 11)]
        public void TestEnqueuedDequeuedInOrder(int minPriority, int maxPriority)
        {
            var q = new SimpleConcurrentPriorityQueue<int>(minPriority, maxPriority);
            var data = Enumerable.Range(minPriority, maxPriority - minPriority).Select(i => i).ToArray();
            foreach (var item in data)
            {
                q.Enqueue(item, item);
            }
            var dequeuedData = new List<int>(data.Length);
            foreach (var item in data.Reverse())
            {
                dequeuedData.Add(item);
                Assert.True(q.TryDequeue(out var queuedItem, out var queuedItemPriority));
                Assert.Equal(item, queuedItem);
                Assert.Equal(item, queuedItemPriority);
            }
            Assert.Equal(data.Reverse(), dequeuedData);
        }

    }
}
