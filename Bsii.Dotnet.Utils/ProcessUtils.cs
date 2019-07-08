using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public static class ProcessExtensions
    {
        public static async Task<ProcessRunOutput> RunAsyncWithStandardStreamCapture(
            this Process process, TimeSpan? timeOut = null, Action<Process> afterProcessStart = null)
        {
            
            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();
            var exitCode = await process.RunAsync(timeOut,
                s => stdOut.Append(s),
                s => stdErr.Append(s), afterProcessStart);
            return new ProcessRunOutput
            {
                ExitCode = exitCode,
                StandardOutput = stdOut.ToString(),
                StandardError = stdErr.ToString()
            };
        }

        public static async Task<int> RunAsync(this Process process, TimeSpan? timeOut = null,
            Action<string> onStdOut = null, Action<string> onStdErr = null,
            Action<Process> afterProcessStart = null)
        {
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            if (onStdOut != null)
            {
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        onStdOut.Invoke(e.Data);
                    }
                };
            }

            if (onStdErr != null)
            {
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        onStdErr.Invoke(e.Data);
                    }
                };
            }

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            afterProcessStart?.Invoke(process);

            var processWaitTask = Task.Run(() =>
            {
                //Note: you must call wait for exit to perform correct async output redirection
                if (process.WaitForExit((int?) timeOut?.TotalMilliseconds ?? Timeout.Infinite))
                {
                    process.WaitForExit(); //Flush async output events http://blachniet.com/blog/flush-async-output-events/
                    return process.ExitCode;
                }

                try
                {
                    process.Kill();
                }
                catch (Exception e)
                {
                    throw new TimeoutException("Failed to kill process after timeout exceeded", e);
                }

                throw new TimeoutException("Process timed out");
            });

            return await processWaitTask;
        }

        public static bool SetProcessPrioritySafe(this Process p, ProcessPriorityClass priorityClass,
            Action<Exception> onException = null)
        {
            try
            {
                if (!p.HasExited)
                {
                    p.PriorityClass = priorityClass;
                    return true;
                }
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }

            return false;
        }
    }

    public class ProcessRunOutput
    {
        public int ExitCode { get; set; }

        public string StandardOutput { get; set; }

        public string StandardError { get; set; }
    }
}
