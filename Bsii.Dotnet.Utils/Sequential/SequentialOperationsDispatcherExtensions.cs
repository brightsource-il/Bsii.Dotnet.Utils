using System;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils.Sequential
{
    public static class SequentialOperationsDispatcherExtensions
    {
        /// <summary>
        /// Executes an action, guarantees no other dispatches run in parallel with it
        /// Note - The dispatching is synchronous and doesn't perform the execution, the returned task is to represent the result
        /// Note - Nested dependent operations will cause deadlocks
        /// </summary>
        /// <returns>A task that represents the status of the action execution, can be used to get the results</returns>
        public static Task Dispatch(this ISequentialOperationsDispatcher dispatcher, Action exec)
        {
            return dispatcher.Dispatch(() =>
            {
                exec();
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Executes an action which generates a result value, guarantees no other dispatches run in parallel with it
        /// Note - The dispatching is synchronous and doesn't perform the execution, the returned task is to represent the result
        /// Note - Nested dependent operations will cause deadlocks
        /// </summary>
        /// <returns>A task that represents the status of the action execution, can be used to get the results</returns>
        public static Task<T> Dispatch<T>(this ISequentialOperationsDispatcher dispatcher, Func<T> exec)
        {
            return dispatcher.Dispatch(() =>
            {
                var res = exec();
                return Task.FromResult(res);
            });
        }

    }
}
