using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public class RateLimiter
    {
        private readonly SlidingWindowRateLimit _slidingWindow;
        private volatile int _concurrentItems;
        private readonly int? _maxConcurrency;

        public RateLimiter(int? maxPerWindow, TimeSpan? windowSize, int? maxConcurrency)
        {
            if (maxPerWindow.HasValue)
            {
                var windowSizeChecked = windowSize
                    ?? throw new ArgumentNullException(nameof(windowSize),
                            $"value can't be null when {nameof(maxPerWindow)} value was specified");
                _slidingWindow = new SlidingWindowRateLimit(maxPerWindow.Value, windowSizeChecked);
            }
            _maxConcurrency = maxConcurrency;
        }

        /// <summary>
        /// Verifies rate limits (tries to complete synchronously if possible)
        /// </summary>
        public async ValueTask Barrier(CancellationToken cancellationToken)
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

            if (_maxConcurrency != null)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_concurrentItems < _maxConcurrency.Value)
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
            Interlocked.Increment(ref _concurrentItems);

        public void PopLiveItem() =>
            Interlocked.Decrement(ref _concurrentItems);

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
