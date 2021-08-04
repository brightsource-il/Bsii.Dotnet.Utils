using System;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Provides <typeparamref name="T"/> values to all asynchrnously awaiting consumers<br/>
    /// Note: all awaiters will get a reference to the same values (in case that <typeparamref name="T"/> is reference type)
    /// </summary>
    public class AsyncValueSource<T> : IAsyncValueProvider<T>
    {
        private TaskCompletionSource<T> _captured;
        private DateTime _captureTime;

        private TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly TimeSpan _gracePeriod;

        public AsyncValueSource(TimeSpan? gracePeriod = default)
            => _gracePeriod = gracePeriod ?? TimeSpan.Zero;

        public void SetNext(T value)
        {
            (_captured, _captureTime) = (_tcs, DateTime.UtcNow);
            _tcs = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _captured.SetResult(value);
        }

        public Task<T> GetNextAsync()
            => _captureTime + _gracePeriod < DateTime.UtcNow
                ? _tcs.Task
                : _captured.Task;
    }
}
