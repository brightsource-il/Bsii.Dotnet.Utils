using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class TimeExecutionUtilsTest
    {
        // Task.Delay(X) takes a bit more then X ms due to task-scheduling and low end CPU available at GitHub Actions:
        const int TestPrecisionMilliseconds = 100;

        [Fact]
        public async Task VoidAsyncFuncUtilTest()
        {
            var res = await TimeExecutionUtils.TimeExecutionAsync(() => Task.Delay(200));
            AssertResult(TimeSpan.FromMilliseconds(200), res);
        }

        [Fact]
        public async Task AsyncFuncUtilTest()
        {
            static async Task<int> AsyncFunc()
            {
                await Task.Delay(200);
                return 1;
            }

            var (res, time) = await TimeExecutionUtils.TimeExecutionAsync(AsyncFunc);
            AssertResult(TimeSpan.FromMilliseconds(200), time);
            Assert.Equal(1, res);
        }

        [Fact]
        public async Task VoidTaskUtilTest()
        {
            var res = await Task.Delay(200).TimeExecutionAsync();
            AssertResult(TimeSpan.FromMilliseconds(200), res);
        }

        [Fact]
        public async Task TaskUtilTest()
        {
            static async Task<int> AsyncFunc()
            {
                await Task.Delay(200);
                return 1;
            }

            var (res, time) = await AsyncFunc().TimeExecutionAsync();
            AssertResult(TimeSpan.FromMilliseconds(200), time);
            Assert.Equal(1, res);
        }

        [Fact]
        public void VoidSyncFuncUtilTest()
        {
            var res = TimeExecutionUtils.TimeExecution(() => Thread.Sleep(200));
            AssertResult(TimeSpan.FromMilliseconds(200), res);
        }

        [Fact]
        public void SyncFuncUtilTest()
        {
            static int Func()
            {
                Thread.Sleep(200);
                return 1;
            }

            var (res, time) = TimeExecutionUtils.TimeExecution(Func);
            AssertResult(TimeSpan.FromMilliseconds(200), time);
            Assert.Equal(1, res);
        }

        private static void AssertResult(TimeSpan expected, TimeSpan actual) =>
            Assert.True(Math.Abs(expected.TotalMilliseconds - actual.TotalMilliseconds) < TestPrecisionMilliseconds);
    }
}
