using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Bsii.Dotnet.Utils.Sequential
{
    /// <summary>
    /// An implementation of sequential dispatcher based on data flow buffer block
    /// </summary>
    public sealed class BufferBlockSequentialDispatcher : ISequentialOperationsDispatcher
    {
        #region Internal Structures
        private interface IDispatchedOperation
        {
            Task ExecuteAsync();

            void SetException(Exception ex);
        }

        /// <summary>
        /// Generic variant
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class DispatchedOperation<T> : IDispatchedOperation
        {
            private readonly Func<Task<T>> _exec;
            private readonly Activity _capturedActivity;
            private readonly TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            public Task<T> OperationRepresentation => _tcs.Task;


            public DispatchedOperation(Func<Task<T>> exec)
            {
                _exec = exec;
                _capturedActivity = Activity.Current?.CreateChildActivity();
            }

            public async Task ExecuteAsync()
            {
                using (_capturedActivity?.Use())
                {
                    try
                    {
                        var res = await _exec();
                        _tcs.SetResult(res);
                    }
                    catch (Exception e)
                    {
                        _tcs.SetException(e);
                    }
                }
            }

            public void SetException(Exception ex)
            {
                _tcs.SetException(ex);
            }
        }

        /// <summary>
        /// Non generic variant
        /// </summary>
        private sealed class DispatchedOperation : IDispatchedOperation
        {
            private readonly Func<Task> _exec;
            private readonly TaskCompletionSource<object> _tcs =
                new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly Activity _capturedActivity;

            public Task OperationRepresentation => _tcs.Task;


            public DispatchedOperation(Func<Task> exec)
            {
                _exec = exec;
                _capturedActivity = Activity.Current?.CreateChildActivity();
            }

            public async Task ExecuteAsync()
            {
                using (_capturedActivity?.Use())
                {
                    try
                    {
                        await _exec();
                        _tcs.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        _tcs.SetException(e);
                    }
                }
            }

            public void SetException(Exception ex)
            {
                _tcs.SetException(ex);
            }
        }
        #endregion

        private readonly BufferBlock<IDispatchedOperation> _operations = new BufferBlock<IDispatchedOperation>();
        private readonly bool _discardAllButLatest;
        private bool _isStarted;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="discardAllButLatest">if true, only the latest dispatched operation will be invoked, others will be failed with an exception</param>
        public BufferBlockSequentialDispatcher(bool discardAllButLatest = false)
        {
            _discardAllButLatest = discardAllButLatest;
        }

        public Task<T> Dispatch<T>(Func<Task<T>> exec)
        {
            var op = new DispatchedOperation<T>(exec);
            if (!_operations.Post(op))
            {
                throw new InvalidOperationException("Failed posting operation to buffer block, it's probably completed");
            }
            return op.OperationRepresentation;
        }

        public Task Dispatch(Func<Task> exec)
        {
            var op = new DispatchedOperation(exec);
            if (!_operations.Post(op))
            {
                throw new InvalidOperationException("Failed posting operation to buffer block, it's probably completed");
            }
            return op.OperationRepresentation;
        }

        private async Task DispatchOperations()
        {
            while (await _operations.OutputAvailableAsync())
            {
                if (_operations.TryReceiveAll(out var operations)) //Else nothing to do, shouldn't happen at all
                {
                    if (_discardAllButLatest && operations.Count > 1) //Have multiple operation dispatched, user requested only the latest one to be executed
                    {
                        for (int i = 0; i < operations.Count - 1; ++i)
                        {
                            var op = operations[i];
                            op.SetException(new OperationCanceledException($"The operation is cancelled due to {operations.Count - i} operation dispatched to the buffer block after it"));
                        }
                        var lastOp = operations.Last();
                        await lastOp.ExecuteAsync(); //This is guaranteed not to throw
                    }
                    else
                    {
                        foreach (var op in operations)
                        {
                            await op.ExecuteAsync(); //This is guaranteed not to throw
                        }
                    }
                }
            }
        }

        public void Start()
        {
            if (_isStarted)
            {
                throw new InvalidOperationException("The dispatcher was already started");
            }
            _isStarted = true;
            Task.Run(DispatchOperations);
        }

        public Task StopAsync()
        {
            if (!_isStarted)
            {
                throw new InvalidOperationException("The dispatcher was not started and stop was attempted");
            }
            _isStarted = false;
            _operations.Complete();
            return _operations.Completion;
        }

        public void Dispose()
        {
            if (_isStarted)
            {
                StopAsync().WaitContextless();
            }
        }
    }
}
