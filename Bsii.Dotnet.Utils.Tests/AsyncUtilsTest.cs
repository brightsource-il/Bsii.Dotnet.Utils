﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bsii.Dotnet.Utils.Collections;
using FluentAssertions;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class AsyncUtilsTest
    {
        [Fact]
        public void PollUntil()
        {
            bool conditionToWaitFor = false;

            Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    conditionToWaitFor = true;
                });

            var progCallbacks = new List<double>();
            Assert.True(PollingUtils.PollUntil(() => conditionToWaitFor, TimeSpan.FromSeconds(10),
                                               progCallbacks.Add, TimeSpan.FromMilliseconds(100)));
            Assert.True(conditionToWaitFor);
            Assert.True(progCallbacks.Count > 0);
            Assert.True(progCallbacks.IsSorted());
        }

        [Fact]
        public void TestPollUntilTimeout()
        {
            const bool conditionToWaitFor = false;
            var progCallbacks = new List<double>();
            Assert.False(PollingUtils.PollUntil(() => conditionToWaitFor,
                                                TimeSpan.FromSeconds(2),
                                                p =>
                                                    {
                                                        progCallbacks.Add(p);
                                                    },
                                                TimeSpan.FromMilliseconds(100))
                );
            Assert.False(conditionToWaitFor);
            progCallbacks.Count.Should().BeGreaterThan(2);
            Assert.True(progCallbacks.IsSorted());
            Assert.All(progCallbacks, p => p.Should().BeLessOrEqualTo(1).And.BeGreaterOrEqualTo(0));
        }

        [Fact]
        public void TestPollNullCondition()
        {
            Assert.Throws<ArgumentNullException>(() => PollingUtils.PollUntil(null, TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async Task TestPollAsync()
        {
            bool condition = false;
            var progCallbacks = new List<double>();

            var resTask = PollingUtils.PollUntilAsync(() => condition, TimeSpan.FromSeconds(20),
                progCallbacks.Add, TimeSpan.FromMilliseconds(200));

            async Task SetEventAfterDelay()
            {
                await Task.Delay(1000);
                condition = true;
            }

            await Task.WhenAll(SetEventAfterDelay(), resTask);

            Assert.True(condition);
            Assert.True(resTask.Result);
            Assert.True(progCallbacks.Count > 0);
            Assert.True(progCallbacks.IsSorted());
        }

        [Fact]
        public async Task TestPollAsyncCondition()
        {
            var condition = Task.FromResult(false);
            var progCallbacks = new List<double>();

            var resTask = PollingUtils.PollUntilAsync(() => condition, TimeSpan.FromSeconds(20),
                progCallbacks.Add, TimeSpan.FromMilliseconds(200));

            async Task SetEventAfterDelay()
            {
                await Task.Delay(1000);
                condition = Task.FromResult(true);
            }

            await Task.WhenAll(SetEventAfterDelay(), resTask);

            Assert.True(condition.Result);
            Assert.True(resTask.Result);
            Assert.True(progCallbacks.Count > 0);
            Assert.True(progCallbacks.IsSorted());
        }

        [Fact]
        public async Task TestTimeoutAfter()
        {
            await Assert.ThrowsAsync<TimeoutException>(() => Task.Delay(1000).TimeoutAfter(TimeSpan.FromMilliseconds(30)));
            // should not throw:
            await Task.Delay(50).TimeoutAfter(TimeSpan.FromMilliseconds(500));
            // pass cancellation token but do not cancel waiting:
            var cts = new CancellationTokenSource();
            await Task.Delay(50).TimeoutAfter(TimeSpan.FromMilliseconds(500), cts.Token);
            // cancel waiting:
            cts.CancelAfter(100);
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                Task.Delay(5000).TimeoutAfter(TimeSpan.FromMilliseconds(500), cts.Token));
        }

        [Fact]
        public async Task TestTimeoutAfterGeneric()
        {
            await Assert.ThrowsAsync<TimeoutException>(() =>
                Task.Delay(1000).ContinueWith(_ => true).TimeoutAfter(TimeSpan.FromMilliseconds(30)));
            // should not throw:
            var res = await Task.Delay(50).ContinueWith(_ => true).TimeoutAfter(TimeSpan.FromMilliseconds(500));
            Assert.True(res);
            // pass cancellation token but do not cancel waiting:
            var cts = new CancellationTokenSource();
            res = await Task.Delay(50).ContinueWith(_ => true)
                .TimeoutAfter(TimeSpan.FromMilliseconds(500), cts.Token);
            Assert.True(res);
            // cancel waiting:
            cts.CancelAfter(100);
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                Task.Delay(5000).ContinueWith(_ => true).TimeoutAfter(TimeSpan.FromMilliseconds(500), cts.Token));
        }

        [Fact]
        public async Task TestAsyncValueSource()
        {
            AsyncValueSource<bool> asyncValueSource = new();
            asyncValueSource.SetNext(false);
            IAsyncValueProvider<bool> asyncValueProvider = asyncValueSource;
            var getNext1 = asyncValueProvider.GetNextAsync();
            var getNext2 = asyncValueProvider.GetNextAsync();
            var getNext3 = asyncValueProvider.GetNextAsync();
            // we should be blocked because we are watching **next** value which isn't set yet...
            await Assert.ThrowsAsync<TimeoutException>(() =>
                Task.WhenAll(getNext1, getNext2, getNext3)
                    .TimeoutAfter(TimeSpan.FromMilliseconds(50)));
            asyncValueSource.SetNext(false);
            // we should be able to continue since because the next value was set since we were watching
            var (n1, n2, n3) = await AsyncUtils.ResolveAll(getNext1, getNext2, getNext3)
                .TimeoutAfter(TimeSpan.FromSeconds(1));
            Assert.False(n1);
            Assert.False(n2);
            Assert.False(n3);
        }

        [Fact]
        public async Task TestAsyncEventSource()
        {
            AsyncEventSource asyncEventSource = new();
            asyncEventSource.Signal();
            var awaiter1 = asyncEventSource.WaitAsync();
            var awaiter2 = asyncEventSource.WaitAsync();
            var awaiter3 = asyncEventSource.WaitAsync();
            // we started waiting after the previous signal, so we 'missed it'...
            await Assert.ThrowsAsync<TimeoutException>(() =>
                Task.WhenAll(awaiter1, awaiter2, awaiter3)
                    .TimeoutAfter(TimeSpan.FromMilliseconds(50)));
            asyncEventSource.Signal();
            // new signal received, we should be able to continue now...
            await Task.WhenAll(awaiter1, awaiter2, awaiter3)
                    .TimeoutAfter(TimeSpan.FromMilliseconds(50));
        }

        [Fact]
        public async Task TestAsyncCachingValueSource()
        {
            AsyncCachingValueSource<object> asyncValueSource = new();
            IAsyncCachingValueProvider<object> asyncValueProvider = asyncValueSource;

            // no last value
            await Assert.ThrowsAsync<TimeoutException>(async () =>
                await asyncValueProvider.GetAsync(
                    TimeSpan.FromMilliseconds(50),
                    timeOut: TimeSpan.FromMilliseconds(50)));

            var expectedValue = new object();
            asyncValueSource.SetNext(expectedValue);
            // check last value can be read:
            var lastValue = await asyncValueProvider.GetAsync(TimeSpan.FromMilliseconds(100));
            lastValue.Should().Be(expectedValue);
            await Task.Delay(200);
            // check last value can be read if max age not passed:
            lastValue = await asyncValueProvider.GetAsync(TimeSpan.FromMilliseconds(300));
            lastValue.Should().Be(expectedValue);
            // check last value is ignored if max age passed:
            await Assert.ThrowsAsync<TimeoutException>(async () =>
                await asyncValueProvider.GetAsync(
                    TimeSpan.FromMilliseconds(100),
                    timeOut: TimeSpan.FromMilliseconds(50)));
            // try to get a future value using 3 different tasks:
            var timeOut = TimeSpan.FromMilliseconds(500);
            var getNext1 = asyncValueProvider.GetAsync(TimeSpan.Zero, timeOut).AsTask();
            var getNext2 = asyncValueProvider.GetAsync(TimeSpan.Zero, timeOut).AsTask();
            var getNext3 = asyncValueProvider.GetAsync(TimeSpan.Zero, timeOut).AsTask();
            // we should be blocked because we are watching **next** value which isn't set yet...
            await Assert.ThrowsAsync<TimeoutException>(() =>
                Task.WhenAll(getNext1, getNext2, getNext3)
                    .TimeoutAfter(TimeSpan.FromMilliseconds(50)));
            var expectedValue2 = new object();
            asyncValueSource.SetNext(expectedValue2);
            // we should be able to continue since because the next value was set since we were watching
            var (n1, n2, n3) = await AsyncUtils.ResolveAll(getNext1, getNext2, getNext3);
            n1.Should().Be(expectedValue2);
            n2.Should().Be(expectedValue2);
            n3.Should().Be(expectedValue2);
        }



        [Fact]
        public async Task TestAsyncCachingEventSource()
        {
            AsyncCachingEventSource asyncEventSource = new();
            IAsyncCachingEventProvider asyncEventProvider = asyncEventSource;
            // no last event
            await Assert.ThrowsAsync<TimeoutException>(async () =>
                await asyncEventProvider.WaitAsync(
                    TimeSpan.FromMilliseconds(50),
                    timeOut: TimeSpan.FromMilliseconds(50)));

            asyncEventSource.Signal();
            // check last event received:
            await asyncEventProvider.WaitAsync(TimeSpan.FromMilliseconds(100));
            await Task.Delay(200);
            // check last event can be awaited if max age not passed:
            await asyncEventProvider.WaitAsync(TimeSpan.FromMilliseconds(300));
            // check last event is ignored if max age passed:
            await Assert.ThrowsAsync<TimeoutException>(async () =>
                await asyncEventProvider.WaitAsync(
                    TimeSpan.FromMilliseconds(100),
                    timeOut: TimeSpan.FromMilliseconds(50)));
            // try to get a future event using 3 different tasks:
            var timeOut = TimeSpan.FromMilliseconds(500);
            var getNext1 = asyncEventProvider.WaitAsync(TimeSpan.Zero, timeOut).AsTask();
            var getNext2 = asyncEventProvider.WaitAsync(TimeSpan.Zero, timeOut).AsTask();
            var getNext3 = asyncEventProvider.WaitAsync(TimeSpan.Zero, timeOut).AsTask();
            // we should be blocked because we are watching **next** event which isn't set yet...
            await Assert.ThrowsAsync<TimeoutException>(() =>
                Task.WhenAll(getNext1, getNext2, getNext3)
                    .TimeoutAfter(TimeSpan.FromMilliseconds(50)));
            asyncEventSource.Signal();
            // we should be able to continue since because the next value was set since we were watching
            await Task.WhenAll(getNext1, getNext2, getNext3);
        }
    }
}