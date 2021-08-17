using System;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public class AsyncCachingEventSource : IAsyncCachingEventProvider
    {
        AsyncCachingValueSource<bool> _internalValueSource = new AsyncCachingValueSource<bool>();

        public void Signal() =>
            _internalValueSource.SetNext(false);

        /// <summary>
        /// Gets a value - if the last value that was set is younger than the <paramref name="maxAge"/> - returns it,  <br/>
        /// Otherwise, will wait until a new value was set, or will throw <see cref="TimeoutException"/> if <paramref name="timeOut"/> was set and was exceeded.
        /// </summary>
        public async ValueTask WaitAsync(TimeSpan maxAge, TimeSpan? timeOut = default)
        {
            await _internalValueSource.GetAsync(maxAge, timeOut);
        }
    }
}
