using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Provides <typeparamref name="T"/> values to all asynchrnously awaiting consumers<br/>
    /// Note: all awaiters will get a reference to the same values (in case that <typeparamref name="T"/> is reference type)
    /// </summary>
    public class AsyncCachingValueSource<T> : IAsyncCachingValueProvider<T>
    {
        private class LastValueCache { public T Value; public DateTime Created; }

        private TaskCompletionSource<LastValueCache> _tcs = new TaskCompletionSource<LastValueCache>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private LastValueCache _lastValue;

        public void SetNext(T value)
        {
            lock (_tcs)
            {
                var captured = _tcs;
                _tcs = new TaskCompletionSource<LastValueCache>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _lastValue = new LastValueCache { Value = value, Created = DateTime.UtcNow };
                captured.SetResult(_lastValue);
            }
        }

        /// <summary>
        /// Gets a value - if the last value that was set is younger than the <paramref name="maxAge"/> - returns it,  <br/>
        /// Otherwise, will wait until a new value was set, or will throw <see cref="TimeoutException"/> if <paramref name="timeOut"/> was set and was exceeded.
        /// </summary>
        public async ValueTask<T> GetAsync(TimeSpan maxAge, TimeSpan? timeOut = default)
        {
            var captured = _lastValue;
            if (captured != default)
            {
                if (captured.Created >= DateTime.UtcNow - maxAge)
                {
                    return captured.Value;
                }
            }
            if (timeOut != default && timeOut.Value > TimeSpan.Zero)
            {
                return (await _tcs.Task.TimeoutAfter(timeOut.Value)).Value;
            }
            return (await _tcs.Task).Value;
        }
    }
}
