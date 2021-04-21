using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class CircularBufferTests
    {
        [Theory]
        [InlineData(0, 1, false)]
        [InlineData(0, 1, true)]
        [InlineData(0, 2, false)]
        [InlineData(0, 2, true)]
        [InlineData(0, 5, false)]
        [InlineData(0, 5, true)]
        [InlineData(1, 5, false)]
        [InlineData(1, 5, true)]
        [InlineData(2, 5, false)]
        [InlineData(2, 5, true)]
        [InlineData(3, 5, false)]
        [InlineData(3, 5, true)]
        [InlineData(4, 5, false)]
        [InlineData(4, 5, true)]
        public void TestNotFull(int count, int capacity, bool pushBack)
        {
            var cb = new CircularBuffer<int>(capacity);
            Push(count, pushBack, cb);
            Assert.Equal(count, cb.Size);
            var expectedSequence = CreateSequence(0, count, !pushBack);
            Assert.True(expectedSequence.SequenceEqual(cb));
        }

        [Theory]
        [InlineData(5, 5, false)]
        [InlineData(5, 5, true)]
        [InlineData(3, 3, false)]
        [InlineData(3, 3, true)]
        [InlineData(1, 1, false)]
        [InlineData(1, 1, true)]
        public void TestFull(int count, int capacity, bool pushBack)
        {
            var cb = new CircularBuffer<int>(capacity);
            Push(count, pushBack, cb);
            Assert.Equal(count, cb.Size);
            var expectedSequence = CreateSequence(0, count, !pushBack);
            Assert.True(expectedSequence.SequenceEqual(cb));
        }

        [Theory]
        [InlineData(6, 5, false)]
        [InlineData(6, 5, true)]
        [InlineData(4, 3, false)]
        [InlineData(4, 3, true)]
        [InlineData(2, 1, false)]
        [InlineData(2, 1, true)]
        public void TestOverflow(int count, int capacity, bool pushBack)
        {
            var cb = new CircularBuffer<int>(capacity);
            Push(count, pushBack, cb);
            Assert.Equal(capacity, cb.Size);
            Assert.NotEqual(count, cb.Size);
            var overflow = count - capacity;
            var expectedSequence = CreateSequence(overflow, count - overflow, !pushBack);
            Assert.True(expectedSequence.SequenceEqual(cb));
        }

        private static IEnumerable<int> CreateSequence(int start, int count, bool reversed)
        {
            var sequence = Enumerable.Range(start, count);
            return reversed ? sequence.Reverse() : sequence;
        }

        private static void Push(int count, bool pushBack, CircularBuffer<int> cb)
        {
            for (int i = 0; i < count; i++)
            {
                if (pushBack)
                {
                    cb.PushBack(i);
                }
                else
                {
                    cb.PushFront(i);
                }
            }
        }
    }
}
