using System.Threading.Tasks;
using Xunit;
using Bsii.Dotnet.Utils.Sequential;
using System.Linq;
using System;
using System.Threading;

namespace Bsii.Dotnet.Utils.Tests
{
    public class SequentialDispatcherTests
    {
        [Fact]
        public async Task TestStartStop()
        {
            ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            await dispatcher.StopAsync();
        }

        [Fact]
        public async Task TestDoubleStartAndStopThrow()
        {
            ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            Assert.Throws<InvalidOperationException>(() => dispatcher.Start());
            await dispatcher.StopAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.StopAsync());
        }

        [Fact]
        public async Task TestDispatchSingOperation()
        {
            ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            var res = await dispatcher.Dispatch(async () =>
            {
                await Task.Delay(10);
                return 42;
            });
            Assert.Equal(42, res);
            await dispatcher.StopAsync();
        }

        [Fact]
        public async Task TestDispatchSeveralOperations()
        {
            ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            var results = await Task.WhenAll(Enumerable.Range(1, 10).Select(i => dispatcher.Dispatch(() => i)));
            Assert.Equal(Enumerable.Range(1, 10).Sum(), results.Sum());
            await dispatcher.StopAsync();
        }

        [Fact]
        public async Task TestExtensions()
        {
            ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            var t1 = dispatcher.Dispatch(() => Thread.Sleep(1)); //Sync void
            var t2 = dispatcher.Dispatch(() => 8); //Sync with result
            var t3 = dispatcher.Dispatch(async () => await Task.Delay(1)); //async void
            await Task.WhenAll(t1, t2, t3);
            await dispatcher.StopAsync();
        }
    }
}
