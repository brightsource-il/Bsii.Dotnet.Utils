using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Provides a rate limiting mechanism with two parameters:<br/>
    /// 1. Max passes through the <see cref="BarrierAsync(CancellationToken)"/> in a time window<br/>
    /// 2. Max live items at any time (controlled externally by calling the <see cref="PushLiveItem"/> &amp; <see cref="PopLiveItem"/> methods
    /// </summary>
    /// <remarks>
    /// No concurrent calls to the <see cref="BarrierAsync(CancellationToken)"/> method should be made as it is not thread safe, <br/>
    /// However, the methods <see cref="PushLiveItem"/> &amp; <see cref="PopLiveItem"/> are thread-safe and can be called concurrently from multiple threads.
    /// </remarks>
    public class RateLimiter
    {
        private readonly SlidingWindowRateLimit _slidingWindow;
        private volatile int _liveItemsCount;
        private readonly int? _maxLiveItems;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxPassesPerWindow">If not null - specifies the max allowed passes through the <see cref="BarrierAsync(CancellationToken)"/> within a time window</param>
        /// <param name="windowSize">If not null - specifies the size of the sliding time window</param>
        /// <param name="maxLiveItems">If not null - specifies the amount of lives items allowed without blocking the caller of the <see cref="BarrierAsync(CancellationToken)"/> method</param>
        public RateLimiter(int? maxPassesPerWindow, TimeSpan? windowSize, int? maxLiveItems)
        {
            if (maxPassesPerWindow.HasValue)
            {
                var windowSizeChecked = windowSize
                    ?? throw new ArgumentNullException(nameof(windowSize),
                            $"value can't be null when {nameof(maxPassesPerWindow)} value was specified");
                _slidingWindow = new SlidingWindowRateLimit(maxPassesPerWindow.Value, windowSizeChecked);
            }
            _maxLiveItems = maxLiveItems;
        }

        /// <summary>
        /// Verifies rate limits (tries to complete synchronously if possible)
        /// </summary>
        public async ValueTask BarrierAsync(CancellationToken cancellationToken)
        {
            if (_slidingWindow != null)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_slidingWindow.TryGetSlot(out var timeUntilNextSlot))
                    {
                        break;
                    }
                    await Task.Delay(timeUntilNextSlot, cancellationToken);
                }
            }

            if (_maxLiveItems != null)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_liveItemsCount < _maxLiveItems.Value)
                    {
                        break;
                    }
                    
                    await Task.Delay(
                        15, // any value less than 15ms is meaningless (at Windows at least, probably at Linux as well),
                            // see remarks at https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.delay?view=net-5.0
                            // and https://twitter.com/nick_craver/status/1021003343777452032?lang=en
                        cancellationToken);
                }
            }
        }

        public void PushLiveItem() =>
            Interlocked.Increment(ref _liveItemsCount);

        public void PopLiveItem() =>
            Interlocked.Decrement(ref _liveItemsCount);

        private class SlidingWindowRateLimit
        {
            private readonly int _requestLimit;
            private readonly CircularBuffer<DateTime> _timeStamps;
            private readonly TimeSpan _requestInterval;

            public SlidingWindowRateLimit(int requestLimit, TimeSpan requestInterval)
            {
                if (requestLimit < 1)
                {
                    throw new ArgumentException($"Received invalid value for {nameof(requestLimit)}, {requestLimit}. Input must be one or greater.");
                }
                if (requestInterval <= TimeSpan.Zero)
                {
                    throw new ArgumentException($"Received invalid value for {nameof(requestInterval)}, {requestInterval}. Input must be greater than zero.");
                }
                _timeStamps = new CircularBuffer<DateTime>(requestLimit);
                _requestLimit = requestLimit;
                _requestInterval = requestInterval;
            }

            public bool TryGetSlot(out TimeSpan timeUntilNextSlot)
            {
                var now = DateTime.UtcNow;
                var windowStartTime = now - _requestInterval;
                while (!_timeStamps.IsEmpty && _timeStamps.Front() < windowStartTime)
                {
                    _timeStamps.PopFront();
                }
                if (_timeStamps.Size < _requestLimit)
                {
                    _timeStamps.PushBack(now);
                    timeUntilNextSlot = default;
                    return true;
                }
                timeUntilNextSlot = _requestInterval - (now - _timeStamps.Front());
                return false;
            }
        }
    }
}
