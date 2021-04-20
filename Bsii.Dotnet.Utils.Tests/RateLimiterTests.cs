using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class RateLimiterTests
    {
        // TODO - write thorough tests
        [Theory]
        [InlineData(1000, "00:00:00.1")]
        [InlineData(300, "00:00:00.3")]
        public async Task TestWindow(int maxItemsPerWindow, string windowSizeStr)
        {
            var windowSize = TimeSpan.Parse(windowSizeStr);
            var limiter = new RateLimiter(maxItemsPerWindow, windowSize, null);
            int executions = 0;
            var manualResetEvent = new ManualResetEvent(false);
            var backgroundTask = Task.Run(async () =>
            {
                manualResetEvent.Set();
                for (var i = 0; i < maxItemsPerWindow * 2; i++)
                {
                    await limiter.Barrier(CancellationToken.None);
                    Interlocked.Increment(ref executions);
                }
            });
            manualResetEvent.WaitOne();
            await Task.Delay(windowSize / 2);
            Assert.Equal(maxItemsPerWindow, executions);
            await Task.Delay(windowSize);
            Assert.NotEqual(maxItemsPerWindow, executions);
            await Task.Delay(windowSize + windowSize / 2);
            Assert.Equal(maxItemsPerWindow * 2, executions);
        }
    }
}
