using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public static class PollingUtils
    {
        #region PollUntilAsync

        /// <summary>
        ///     Waits asynchronously until a condition occurs or until a timeout
        ///     Implemented as polling the condition at 1s frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static Task<bool> PollUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
        {
            return PollUntilAsync(condition, timeout, null, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        ///     Waits asynchronously until a condition occurs or until a timeout
        ///     Implemented as polling the condition at 1s frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static Task<bool> PollUntilAsync(Func<bool> condition, TimeSpan timeout)
        {
            return PollUntilAsync(condition, timeout, null, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        ///     Waits asynchronously until a condition occurs or until a timeout
        ///     Implemented as polling the condition at 1s frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <param name="progress">A progress callback, will be called with values of 0..1</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static Task<bool> PollUntilAsync(Func<Task<bool>> condition, TimeSpan timeout, Action<double> progress)
        {
            return PollUntilAsync(condition, timeout, progress, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        ///     Waits asynchronously until a condition occurs or until a timeout
        ///     Implemented as polling the condition at 1s frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <param name="progress">A progress callback, will be called with values of 0..1</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static Task<bool> PollUntilAsync(Func<bool> condition, TimeSpan timeout, Action<double> progress)
        {
            return PollUntilAsync(condition, timeout, progress, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        ///     Waits asynchronously until a condition occurs or until a timeout
        ///     Implemented as polling the condition at 1s frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <param name="progress">A progress callback, will be called with values of 0..1</param>
        /// <param name="queryFrequency">The polling frequency, defaults to 1 second</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static async Task<bool> PollUntilAsync(Func<bool> condition, TimeSpan timeout, Action<double> progress,
            TimeSpan queryFrequency)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var sw = Stopwatch.StartNew();
            do
            {
                if (condition())
                {
                    progress?.Invoke(1.0); // Done

                    return true;
                }

                await Task.Delay(queryFrequency);
                progress?.Invoke(Math.Min(sw.ElapsedMilliseconds / timeout.TotalMilliseconds, 1));
            } while (sw.Elapsed < timeout);

            return false;
        }

        /// <summary>
        ///     Waits asynchronously until a condition occurs or until a timeout
        ///     Implemented as polling the condition at 1s frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <param name="progress">A progress callback, will be called with values of 0..1</param>
        /// <param name="queryFrequency">The polling frequency, defaults to 1 second</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static async Task<bool> PollUntilAsync(Func<Task<bool>> condition, TimeSpan timeout,
            Action<double> progress,
            TimeSpan queryFrequency)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var sw = Stopwatch.StartNew();
            do
            {
                if (await condition())
                {
                    progress?.Invoke(1.0); // Done

                    return true;
                }

                await Task.Delay(queryFrequency);
                progress?.Invoke(Math.Min(sw.ElapsedMilliseconds / timeout.TotalMilliseconds, 1));
            } while (sw.Elapsed < timeout);

            return false;
        }

        #endregion PollUntilAsync

        #region PollUntil

        /// <summary>
        ///     Waits until a condition occurs or until a timeout
        ///     Implemented as polling the condition at 1s frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static bool PollUntil(Func<bool> condition, TimeSpan timeout)
        {
            return PollUntil(condition, timeout, null, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        ///     Waits until a condition occurs or until a timeout
        ///     Implemented as polling the condition at 1s frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <param name="progress">A progress callback, will be called with values of 0..1</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static bool PollUntil(Func<bool> condition, TimeSpan timeout, Action<double> progress)
        {
            return PollUntil(condition, timeout, progress, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        ///     Waits until a condition occurs or until a timeout
        ///     Implemented as polling the condition at some frequency
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeout">The maximal period of time to wait for</param>
        /// <param name="progress">A progress callback, will be called with values of 0..1</param>
        /// <param name="queryFrequency">The polling frequency, defaults to 1 second</param>
        /// <returns>True if the condition was achieved, false if timeout occurred</returns>
        public static bool PollUntil(Func<bool> condition, TimeSpan timeout, Action<double> progress,
            TimeSpan queryFrequency)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var sw = Stopwatch.StartNew();
            do
            {
                if (condition())
                {
                    progress?.Invoke(1.0); // Done

                    return true;
                }

                Thread.Sleep(queryFrequency);
                progress?.Invoke(Math.Min(sw.ElapsedMilliseconds / timeout.TotalMilliseconds, 1));
            } while (sw.Elapsed < timeout);

            return false;
        }

        #endregion PollUntil
    }
}