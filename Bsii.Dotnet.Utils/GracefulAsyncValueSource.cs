using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Provides <typeparamref name="T"/> values to all asynchronously awaiting consumers<br/>
    /// Note: all awaiters will get a reference to the same values (in case that <typeparamref name="T"/> is reference type)
    /// </summary>
    public class GracefulAsyncValueSource<T> : IAsyncValueProvider<T>
    {
        private class TaskCompletionSourceWithTime<TValue> : TaskCompletionSource<TValue>
        {
            public DateTime ValueTime { get; set; }

            public TaskCompletionSourceWithTime(DateTime valueTime, TaskCreationOptions taskCreationOptions)
                : base(taskCreationOptions)
            {
                ValueTime = valueTime;
            }
        }

        private TaskCompletionSourceWithTime<T> _current = new TaskCompletionSourceWithTime<T>(
            DateTime.MinValue, TaskCreationOptions.RunContinuationsAsynchronously);

        private TaskCompletionSourceWithTime<T> _captured = new TaskCompletionSourceWithTime<T>(
            DateTime.MinValue, TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly TimeSpan? _gracePeriod;

        /// <summary>
        /// </summary>
        /// <param name="gracePeriod">If null, will block awaiters until next value provided,</br>
        /// If positive, latest value will be provided to awaiters for so long since received,</br>
        /// If value is <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>, latest available value will be provided without waiting (unless no latest value available).</param>
        public GracefulAsyncValueSource(TimeSpan? gracePeriod = default)
        {
            if (gracePeriod.HasValue &&
                gracePeriod <= TimeSpan.Zero &&
                gracePeriod != Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentException($"{nameof(gracePeriod)} must be positive or System.Threading.Timeout.InfiniteTimeSpan", nameof(gracePeriod));
            }
            _gracePeriod = gracePeriod;
        }

        public void SetNext(T value)
        {
            _captured = _current;
            _current = new TaskCompletionSourceWithTime<T>(DateTime.UtcNow,
                TaskCreationOptions.RunContinuationsAsynchronously);
            _captured.SetResult(value);
        }

        public Task<T> GetLatestWithGraceOrNextAsync()
        {
            var captured = _captured;
            if (_gracePeriod.HasValue && captured != default)
            {
                if (_gracePeriod == Timeout.InfiniteTimeSpan ||
                    captured.ValueTime + _gracePeriod.Value > DateTime.UtcNow)
                {
                    return captured.Task ?? _current.Task;
                }
            }

            return _current.Task;
        }
    }
}
