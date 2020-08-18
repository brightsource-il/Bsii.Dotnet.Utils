using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    public class ProcessExtensionsTests
    {
        [Fact]
        public async Task TestRunAsync()
        {
            var cmd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which";
            var process = new Process { StartInfo = new ProcessStartInfo(cmd, cmd) };
            string stdOut = null, stdErr = null;
            bool callback = false;
            var exitCode = await process.RunAsync(onStdOut: s => stdOut = s, onStdErr: s => stdErr = s,
                afterProcessStart: p => callback = true);
            Assert.Equal(0, exitCode);
            Assert.True(callback);
            Assert.Contains(cmd, stdOut);
            Assert.Null(stdErr);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(1024, false)]
        [InlineData(3, true)]
        public async Task TestRunAsyncWithStandardStreamCapture(int? maxStdOutLength, bool shouldThrow)
        {
            var cmd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which";
            bool callback = false;
            
            Task<ProcessRunOutput> Execute()
            {
                var process = new Process { StartInfo = new ProcessStartInfo(cmd, cmd) };
                return process.RunAsyncWithStandardStreamCapture(afterProcessStart: p => callback = true,
                    maxStdOutLength: maxStdOutLength);
            }

            if (shouldThrow)
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(Execute);
            }
            else
            {
                var processRunOutput = await Execute();
                Assert.Equal(0, processRunOutput.ExitCode);
                Assert.True(callback);
                Assert.Contains(cmd, processRunOutput.StandardOutput);
                Assert.True(string.IsNullOrEmpty(processRunOutput.StandardError));
            }
        }
    }
}
