using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public static class ProcessExtensions
    {
        /// <summary>
        /// Asynchronously runs the process and captures its outputs.
        /// <para>The task is completed once the process has exited.</para>
        /// </summary>
        /// <param name="process">The process to run</param>
        /// <param name="timeOut">Optional timeout for the process execution, after which the process will get terminated and <see cref="TimeoutException"/> will be thrown</param>
        /// <param name="afterProcessStart">A callback that gets called once the process has started</param>
        /// <param name="maxStdOutLength">Optional hard limit for the standard output length.
        /// <para>
        ///     (!) If exceeded - the processes will be killed and an <see cref="ArgumentOutOfRangeException"/> exception will be thrown
        ///</para>
        /// </param>
        /// <param name="maxStdErrLength">Optional hard limit for the standard error length.
        /// <para>
        ///     (!) If exceeded - the processes will be killed and an <see cref="ArgumentOutOfRangeException"/> exception will be thrown
        ///</para>
        /// </param>
        /// <returns><see cref="Task"></see>&lt;<see cref="ProcessRunOutput"/>&gt; containing the outputs and exit code of the process</returns>
        public static async Task<ProcessRunOutput> RunAsyncWithStandardStreamCapture(
            this Process process, TimeSpan? timeOut = null, Action<Process> afterProcessStart = null,
            int? maxStdOutLength = null, int? maxStdErrLength = null)
        {
            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();
            var stdOutLimitReached = false;
            var stdErrLimitReached = false;

            void TryKillAndSetFlag(out bool flag)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    // noop
                }

                flag = true;
            }

            var onStdOut = CreateStringBuilderAppender(stdOut, maxStdOutLength,
                () => TryKillAndSetFlag(out stdOutLimitReached));
            var onStdErr = CreateStringBuilderAppender(stdErr, maxStdErrLength,
                () => TryKillAndSetFlag(out stdErrLimitReached));
            var exitCode = await process.RunAsync(timeOut, onStdOut, onStdErr, afterProcessStart);
            if (stdOutLimitReached)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStdOutLength));
            }

            if (stdErrLimitReached)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStdErrLength));
            }

            return new ProcessRunOutput
            {
                ExitCode = exitCode,
                StandardOutput = stdOut.ToString(),
                StandardError = stdErr.ToString()
            };
        }

        /// <summary>
        /// Asynchronously runs the process and optionally registers to output events.
        /// <para>The task is completed once the process has exited.</para>
        /// </summary>
        /// <param name="process">The process to run</param>
        /// <param name="timeOut">Optional timeout for the process execution, after which the process will get terminated and <see cref="TimeoutException"/> will be thrown</param>
        /// <param name="onStdOut">Event handler to call once <see cref="Process.OutputDataReceived"/> is raised with meaningful value (not null or empty)</param>
        /// <param name="onStdErr">Event handler to call once <see cref="Process.ErrorDataReceived"/> is raised with meaningful value (not null or empty)</param>
        /// <param name="afterProcessStart">A callback that gets called once the process has started</param>
        /// <returns></returns>
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
                if (process.WaitForExit((int?)timeOut?.TotalMilliseconds ?? Timeout.Infinite))
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

        /// <summary>
        /// Try to set the priority for the process
        /// </summary>
        /// <param name="p"></param>
        /// <param name="priorityClass">The priority to set</param>
        /// <param name="onException">Option exception handler</param>
        /// <returns><c>true</c> if priority was set successfully, otherwise <c>false</c> </returns>
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

        private static Action<string> CreateStringBuilderAppender(
            StringBuilder sb, int? limit, Action onLimitReached)
        {
            if (limit == null)
            {
                return s => sb.AppendLine(s);
            }

            return s =>
            {
                if (s.Length + sb.Length > limit.Value)
                {
                    onLimitReached?.Invoke();
                }
                else
                {
                    sb.AppendLine(s);
                }
            };
        }
    }
}