using System;
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
        }

        /// <summary>
        /// Generic variant
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class DispatchedOperation<T> : IDispatchedOperation
        {
            private readonly Func<Task<T>> _exec;
            private readonly TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            public Task<T> OperationRepresentation => _tcs.Task;


            public DispatchedOperation(Func<Task<T>> exec)
            {
                _exec = exec;
            }

            public async Task ExecuteAsync()
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

        /// <summary>
        /// Non generic variant
        /// </summary>
        private sealed class DispatchedOperation : IDispatchedOperation
        {
            private readonly Func<Task> _exec;
            private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            public Task OperationRepresentation => _tcs.Task;


            public DispatchedOperation(Func<Task> exec)
            {
                _exec = exec;
            }

            public async Task ExecuteAsync()
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
        #endregion

        private readonly BufferBlock<IDispatchedOperation> _operations = new BufferBlock<IDispatchedOperation>();
        private bool _isStarted = false;

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
                var op = await _operations.ReceiveAsync();
                await op.ExecuteAsync(); //This is guaranteed not to throw
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
