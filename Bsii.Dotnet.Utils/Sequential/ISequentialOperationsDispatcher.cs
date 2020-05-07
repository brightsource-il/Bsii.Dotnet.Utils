using System;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils.Sequential
{
    /// <summary>
    /// Guarantees sequential execution of operations dispatched to it
    /// Note - nested dependent operations will cause deadlocks
    /// Disposing this will ensure <see cref="StopAsync"/> is called
    /// </summary>
    public interface ISequentialOperationsDispatcher : IDisposable
    {

        /// <summary>
        /// Executes an async action which generates a result value, guarantees no other dispatches runs in parallel with it
        /// Note - The dispatching is synchronous and doesn't perform the execution, the returned task is to represent the result
        /// Note - Nested dependent operations will cause deadlocks
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exec"></param>
        /// <returns>A task that represents the status of the action execution, can be used to get the results</returns>
        Task<T> Dispatch<T>(Func<Task<T>> exec);

        /// <summary>
        /// Executes an async action, guarantees no other dispatches runs in parallel with it
        /// Note - The dispatching is synchronous and doesn't perform the execution, the returned task is to represent the result
        /// Note - Nested dependent operations will cause deadlocks
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exec"></param>
        /// <returns>A task that represents the status of the action execution</returns>
        Task Dispatch(Func<Task> exec);

        /// <summary>
        /// Starts processing dispatched operations, does this in background
        /// </summary>
        void Start();

        /// <summary>
        /// Stops processing of the commands, doesn't accept new commands afterwards
        /// </summary>
        /// <returns>the task that can be awaited for notification of operations completion</returns>
        Task StopAsync();

    }
}
