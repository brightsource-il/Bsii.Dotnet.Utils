using System;
using System.Threading.Tasks;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class TimeExecutionUtilsTest
    {
        const double TestPrecisionMilliseconds = 20;    // Task.Delay(X) takes a bit more then X ms due to multi threading 

        [Fact]
        public async Task VoidAsyncFuncUtilTest()
        {
            static Task AsyncFunc() => Task.Delay(200);
            var res  = await TimeExecutionUtils.TimeExecutionAsync(AsyncFunc);
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

            var (res, time)  = await TimeExecutionUtils.TimeExecutionAsync(AsyncFunc);
            AssertResult(TimeSpan.FromMilliseconds(200), time);
            Assert.Equal(1, res);
        }

        [Fact]
        public async Task VoidTaskUtilTest()
        {
            var res  = await Task.Delay(200).TimeExecutionAsync();
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

            var (res, time)  = await AsyncFunc().TimeExecutionAsync();
            AssertResult(TimeSpan.FromMilliseconds(200), time);
            Assert.Equal(1, res);
        }

        [Fact]
        public void VoidSyncFuncUtilTest()
        {
            static void Func() => Task.Delay(200).WaitContextless();
            var res = TimeExecutionUtils.TimeExecution(Func);
            AssertResult(TimeSpan.FromMilliseconds(200), res);
        }

        [Fact]
        public void SyncFuncUtilTest()
        {
            static int Func()
            {
                Task.Delay(200).WaitContextless();
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
