using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Provides <typeparamref name="T"/> values to all asynchronously awaiting consumers<br/>
    /// Note: all awaiters will get a reference to the same values (in case that <typeparamref name="T"/> is reference type)
    /// </summary>
    public class AsyncValueSource<T> : IAsyncValueProvider<T>
    {
        private class AsyncValue<TValue>
        {
            public TaskCompletionSource<TValue> TaskCompletionSource { get; }
            public DateTime ValueTime { get; }

            public AsyncValue(TaskCompletionSource<TValue> taskCompletionSource, DateTime valueTime)
            {
                TaskCompletionSource = taskCompletionSource;
                ValueTime = valueTime;
            }
        }

        private AsyncValue<T> _captured;

        private TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly TimeSpan? _gracePeriod;

        /// <summary>
        /// </summary>
        /// <param name="gracePeriod">If null, will block awaiters until next value provided,</br>
        /// If positive, latest value will be provided to awaiters for so long since received,</br>
        /// If value is <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>, latest available value will be provided without waiting (unless no latest value available).</param>
        public AsyncValueSource(TimeSpan? gracePeriod = default)
        {
            if (gracePeriod.HasValue &&
                gracePeriod <= TimeSpan.Zero &&
                gracePeriod != Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentException("gracePeriod must be positive or System.Threading.Timeout.InfiniteTimeSpan", nameof(gracePeriod));
            }
            _gracePeriod = gracePeriod;
        }

        public void SetNext(T value)
        {
            _captured = new AsyncValue<T>(_tcs, DateTime.UtcNow);
            _tcs = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _captured.TaskCompletionSource.SetResult(value);
        }

        public Task<T> GetNextAsync()
        {
            if (_gracePeriod.HasValue && _captured != default)
            {
                if (_gracePeriod == Timeout.InfiniteTimeSpan ||
                    _captured?.ValueTime + _gracePeriod.Value > DateTime.UtcNow)
                {
                    return _captured?.TaskCompletionSource.Task ?? _tcs.Task;
                }
                else
                {
                    _captured = default;
                }
            }

            return _tcs.Task;
        }
    }
}
