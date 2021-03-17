using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Asynchrnously provides <typeparamref name="T"/> values to all awaiting consumers<br/>
    /// Note: all awaiters will get a reference to the same values (in case that <typeparamref name="T"/> is reference type)
    /// </summary>
    public class AsyncValueSource<T>
    {
        private TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public void Next(T value)
        {
            var captured = _tcs;
            _tcs = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            captured.SetResult(value);
        }

        public Task<T> GetNextAsync()
        {
            return _tcs.Task;
        }
    }
}
