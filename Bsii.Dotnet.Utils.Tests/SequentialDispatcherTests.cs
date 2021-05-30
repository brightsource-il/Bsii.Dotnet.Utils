using System.Threading.Tasks;
using Xunit;
using Bsii.Dotnet.Utils.Sequential;
using System.Linq;
using System;
using System.Threading;
using System.Diagnostics;

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
            dispatcher.Dispose();
        }

        [Fact]
        public async Task TestDispatchSingOperation()
        {
            using ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            var res = await dispatcher.Dispatch(async () =>
            {
                await Task.Delay(10);
                return 42;
            });
            Assert.Equal(42, res);
        }

        [Fact]
        public async Task TestDispatchSeveralOperations()
        {
            using ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            var results = await Task.WhenAll(Enumerable.Range(1, 10).Select(i => dispatcher.Dispatch(() => i)));
            Assert.Equal(Enumerable.Range(1, 10).Sum(), results.Sum());
        }

        [Fact]
        public async Task TestExtensions()
        {
            using ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            var t1 = dispatcher.Dispatch(() => Thread.Sleep(1)); //Sync void
            var t2 = dispatcher.Dispatch(() => 8); //Sync with result
            var t3 = dispatcher.Dispatch(async () => await Task.Delay(1)); //async void
            await Task.WhenAll(t1, t2, t3);
        }

        [Fact]
        public async Task TestOnlyLatestOperationDispatching()
        {
            using ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher(true);
            var t1 = dispatcher.Dispatch(() => Task.Delay(1000));
            var t2 = dispatcher.Dispatch(() => Task.Delay(1000));
            var t3 = dispatcher.Dispatch(() => Task.Delay(1000));
            dispatcher.Start();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await t1);
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await t2);
            await t3; //Should not throw

            ManualResetEvent mre = new ManualResetEvent(false);
            var t4 = dispatcher.Dispatch(() => 
            { 
                mre.Set();
                return Task.Delay(1000);
            }) ;
            mre.WaitOne(); //make sure t4 is running
            var t5 = dispatcher.Dispatch(() => Task.Delay(1000));
            var t6 = dispatcher.Dispatch(() => Task.Delay(1000));
            await t4; //Should not throw
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await t5);
            await t6; //Should be the one invoked
        }


        [Fact]
        public async Task TestActivityTraceIdCaptured()
        {
            using var activity = new Activity(nameof(TestActivityTraceIdCaptured)).Use();
            using ISequentialOperationsDispatcher dispatcher = new BufferBlockSequentialDispatcher();
            dispatcher.Start();
            var traceId = Activity.Current.TraceId.ToHexString();
            var (capturedActivityOperationName, capturedActivityId) = 
                await dispatcher.Dispatch(() => (Activity.Current.OperationName, Activity.Current.Id)); //Sync with result
            Assert.Equal(nameof(TestActivityTraceIdCaptured), capturedActivityOperationName);
            Assert.Contains(traceId, capturedActivityId);
            await dispatcher.Dispatch(() =>
            {
                capturedActivityOperationName = Activity.Current.OperationName + "2";
                capturedActivityId = Activity.Current.Id;
            }); //Sync void
            Assert.Equal(nameof(TestActivityTraceIdCaptured) + "2", capturedActivityOperationName);
            Assert.Contains(traceId, capturedActivityId);
            (capturedActivityOperationName, capturedActivityId) = 
                await dispatcher.Dispatch(() => Task.FromResult((Activity.Current.OperationName, Activity.Current.Id))); //Async with result
            Assert.Equal(nameof(TestActivityTraceIdCaptured), capturedActivityOperationName);
            Assert.Contains(traceId, capturedActivityId);
            await dispatcher.Dispatch(() =>
            {
                capturedActivityOperationName = Activity.Current.OperationName + "2";
                capturedActivityId = Activity.Current.Id;
                return Task.CompletedTask;
            }); //Async void
            Assert.Equal(nameof(TestActivityTraceIdCaptured) + "2", capturedActivityOperationName);
            Assert.Contains(traceId, capturedActivityId);
        }
    }
}
