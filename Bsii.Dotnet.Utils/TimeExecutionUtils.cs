using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public static class TimeExecutionUtils
    {

        /// <summary>
        /// A shorthand for measuring async function  execution time 
        /// <param name="action">The action to be executed</param>
        /// <returns> a tuple includes a TimeSpan represents the execution time and the result </returns>
        /// </summary>
        public static async Task<(T res, TimeSpan elapsed)> TimeExecutionAsync<T>(this Func<Task<T>> action)
        {
            var sw = Stopwatch.StartNew();
            var res = await action();
            return (res, sw.Elapsed);
        }

        /// <summary>
        /// A shorthand for measuring async function execution time 
        /// <param name="action">The action to be executed</param>
        /// <returns> a TimeSpan represents the execution time</returns>
        /// </summary>
        public static async Task<TimeSpan> TimeExecutionAsync(this Func<Task> action)
        {
            var sw = Stopwatch.StartNew();
            await action();
            return sw.Elapsed;
        }

        /// <summary>
        /// A shorthand for measuring function execution time 
        /// <param name="action">The action to be executed</param>
        /// <returns> a tuple includes a TimeSpan represents the execution time and the result </returns>
        /// </summary>
        public static TimeSpan TimeExecution(this Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            return sw.Elapsed;
        }

        /// <summary>
        /// A shorthand for measuring function execution time 
        /// <param name="action">The action to be executed</param>
        /// <returns> a tuple includes a TimeSpan represents the execution time and the result </returns>
        /// </summary>
        public static (T res, TimeSpan elapsed) TimeExecution<T>(this Func<T> action)
        {
            var sw = Stopwatch.StartNew();
            var res = action();
            return (res, sw.Elapsed);
        }

        /// <summary>
        /// A shorthand for measuring task execution time 
        /// <param name="action">The action to be executed</param>
        /// <returns> a tuple includes a TimeSpan represents the execution time and the result </returns>
        /// </summary>
        public static  Task<TimeSpan> TimeExecutionAsync(this Task action) => TimeExecutionAsync(() => action);

        /// <summary>
        /// A shorthand for measuring task execution time 
        /// <param name="action">The action to be executed</param>
        /// <returns> a tuple includes a TimeSpan represents the execution time and the result </returns>
        /// </summary>
        public static  Task<(T res, TimeSpan elapsed)> TimeExecutionAsync<T>(this Task<T> action) => TimeExecutionAsync(() => action);
    }
}