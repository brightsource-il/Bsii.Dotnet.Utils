using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Circular buffer implementation based on <see href="https://github.com/joaoportela/CircularBuffer-CSharp"/> 
    /// which in turn based on <see href="https://boost.org"/>'s implementation (C++)<br/>
    /// When writing to a full buffer:<br/>
    /// <see cref="PushBack"/> -> removes this[0] / <see cref="Front()"/><br/>
    /// <see cref="PushFront"/> -> removes this[<see cref="Size"/>-1] / <see cref="Back()"/><br/>
    /// </summary>
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;

        /// <summary>
        /// The _start. Index of the first element in buffer.
        /// </summary>
        private int _start;

        /// <summary>
        /// The _end. Index after the last element in the buffer.
        /// </summary>
        private int _end;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
        /// 
        /// </summary>
        /// <param name='capacity'>
        /// Buffer capacity. Must be positive.
        /// </param>
        public CircularBuffer(int capacity) : this(capacity, Array.Empty<T>()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
        /// 
        /// </summary>
        /// <param name='capacity'>
        /// Buffer capacity. Must be positive.
        /// </param>
        /// <param name='items'>
        /// Items to fill buffer with. Items length must be less than capacity.
        /// Suggestion: use Skip(x).Take(y).ToArray() to build this argument from
        /// any enumerable.
        /// </param>
        public CircularBuffer(int capacity, T[] items)
        {
            if (capacity < 1)
            {
                throw new ArgumentException(
                    "Circular buffer cannot have negative or zero capacity.", nameof(capacity));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (items.Length > capacity)
            {
                throw new ArgumentException(
                    "Too many items to fit circular buffer", nameof(items));
            }

            _buffer = new T[capacity];
            Array.Copy(items, _buffer, items.Length);
            Size = items.Length;
            _start = 0;
            _end = Size == capacity ? 0 : Size;
        }

        /// <summary>
        /// Maximum capacity of the buffer. Elements pushed into the buffer after
        /// maximum capacity is reached (IsFull = true), will remove an element.
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// Boolean indicating if Circular is at full capacity.
        /// Adding more elements when the buffer is full will
        /// cause elements to be removed from the other end
        /// of the buffer.
        /// </summary>
        public bool IsFull => Size == Capacity;

        /// <summary>
        /// True if has no elements.
        /// </summary>
        public bool IsEmpty => Size == 0;

        /// <summary>
        /// Current buffer size (the number of elements that the buffer has).
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Element at the front of the buffer - this[0].
        /// </summary>
        /// <returns>The value of the element of type T at the front of the buffer.</returns>
        public T Front()
        {
            ThrowIfEmpty();
            return _buffer[_start];
        }

        /// <summary>
        /// Element at the back of the buffer - this[Size - 1].
        /// </summary>
        /// <returns>The value of the element of type T at the back of the buffer.</returns>
        public T Back()
        {
            ThrowIfEmpty();
            return _buffer[(_end != 0 ? _end : Capacity) - 1];
        }

        /// <summary>
        /// Index access to elements in buffer.
        /// Index does not loop around like when adding elements,
        /// valid interval is [0;Size[
        /// </summary>
        /// <param name="index">Index of element to access.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when index is outside of [; Size[ interval.</exception>
        public T this[int index]
        {
            get
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
                }
                if (index >= Size)
                {
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {Size}");
                }
                var actualIndex = InternalIndex(index);
                return _buffer[actualIndex];
            }
            set
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
                }
                if (index >= Size)
                {
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {Size}");
                }
                var actualIndex = InternalIndex(index);
                _buffer[actualIndex] = value;
            }
        }

        /// <summary>
        /// Pushes a new element to the back of the buffer. Back()/this[Size-1]
        /// will now return this element.
        /// 
        /// When the buffer is full, the element at Front()/this[0] will be 
        /// popped to allow for this new element to fit.
        /// </summary>
        /// <param name="item">Item to push to the back of the buffer</param>
        public void PushBack(T item)
        {
            if (IsFull)
            {
                _buffer[_end] = item;
                Increment(ref _end);
                _start = _end;
            }
            else
            {
                _buffer[_end] = item;
                Increment(ref _end);
                ++Size;
            }
        }

        /// <summary>
        /// Pushes a new element to the front of the buffer. Front()/this[0]
        /// will now return this element.
        /// 
        /// When the buffer is full, the element at Back()/this[Size-1] will be 
        /// popped to allow for this new element to fit.
        /// </summary>
        /// <param name="item">Item to push to the front of the buffer</param>
        public void PushFront(T item)
        {
            if (IsFull)
            {
                Decrement(ref _start);
                _end = _start;
                _buffer[_start] = item;
            }
            else
            {
                Decrement(ref _start);
                _buffer[_start] = item;
                ++Size;
            }
        }

        /// <summary>
        /// Removes the element at the back of the buffer. Decreasing the 
        /// Buffer size by 1.
        /// </summary>
        public void PopBack()
        {
            ThrowIfEmpty();
            Decrement(ref _end);
            --Size;
        }

        /// <summary>
        /// Removes the element at the front of the buffer. Decreasing the 
        /// Buffer size by 1.
        /// </summary>
        public void PopFront()
        {
            ThrowIfEmpty();
            Increment(ref _start);
            --Size;
        }

        /// <summary>
        /// Copies the buffer contents to an array, according to the logical
        /// contents of the buffer (i.e. independent of the internal 
        /// order/contents)
        /// </summary>
        /// <returns>A new array with a copy of the buffer contents.</returns>
        public T[] ToArray()
        {
            var newArray = new T[Size];
            var firstSpan = GetFirstSegment();
            Array.Copy(firstSpan.Array, firstSpan.Offset, newArray, 0, firstSpan.Count);
            if (Size > firstSpan.Count)
            {
                var secondSpan = GetSecondSegment();
                Array.Copy(secondSpan.Array, secondSpan.Offset, newArray, firstSpan.Count, secondSpan.Count);
            }
            return newArray;
        }

        #region IEnumerable<T> implementation
        /// <summary>
        /// Returns an enumerator that iterates through this buffer.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate this collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (IsEmpty)
            {
                yield break;
            }
            var firstSpan = GetFirstSegment();
            for (var i = 0; i < firstSpan.Count; i++)
            {
                yield return firstSpan.Array[firstSpan.Offset + i];
            }
            var secondSpan = GetSecondSegment();
            for (var i = 0; i < secondSpan.Count; i++)
            {
                yield return secondSpan.Array[secondSpan.Offset + i];
            }
        }
        #endregion
        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfEmpty()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Cannot access an empty buffer.");
            }
        }

        /// <summary>
        /// Increments the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Increment(ref int index)
        {
            if (++index == Capacity)
            {
                index = 0;
            }
        }

        /// <summary>
        /// Decrements the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Decrement(ref int index)
        {
            if (index == 0)
            {
                index = Capacity;
            }
            index--;
        }

        /// <summary>
        /// Converts the index in the argument to an index in <code>_buffer</code>
        /// </summary>
        /// <returns>
        /// The transformed index.
        /// </returns>
        /// <param name='index'>
        /// External index.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int InternalIndex(int index)
        {
            return _start + (index < Capacity - _start ? index : index - _buffer.Length);
        }

        #region Array items easy access.
        // The array is composed by at most two non-contiguous segments, 
        // the next two methods allow easy access to those.

        private ArraySegment<T> GetFirstSegment() =>
            _start < _end
                ? new ArraySegment<T>(_buffer, _start, _end - _start)
                : new ArraySegment<T>(_buffer, _start, Capacity - _start);

        private ArraySegment<T> GetSecondSegment() =>
            _start < _end
                ? new ArraySegment<T>(Array.Empty<T>())
                : new ArraySegment<T>(_buffer, 0, _end);

        #endregion
    }
}
