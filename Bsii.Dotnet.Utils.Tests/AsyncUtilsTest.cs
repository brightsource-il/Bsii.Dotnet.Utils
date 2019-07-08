using System;
using System.Collections.Generic;
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
    }
}