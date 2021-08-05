using System;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Provides synchronization point for multiple awaiters on a single event source
    /// </summary>
    public class GracefulAsyncEventSource
    {
        private readonly GracefulAsyncValueSource<bool> _signaler;

        /// <summary>
        /// </summary>
        /// <param name="gracePeriod">If null, will block awaiters until next signal,</br>
        /// If positive, will return to awaiters if latest signal was set for so long since received,</br>
        /// If value is <see cref="System.Threading.Timeout.InfiniteTimeSpan">, will return without waiting (unless no signal was set).</param>
        public GracefulAsyncEventSource(TimeSpan? gracePeriod = default)
            => _signaler = new GracefulAsyncValueSource<bool>(gracePeriod);

        /// <summary>
        /// Sends a signal to current awaiters
        /// </summary>
        public void Signal() => _signaler.SetNext(true);

        /// <summary>
        /// Waits for a future signal
        /// </summary>
        public Task LatestWithGraceOrWaitAsync() => _signaler.GetLatestWithGraceOrNextAsync();
    }
}
