using System;

namespace Bsii.Dotnet.Utils
{
    public class DisposableAction : IDisposable
    {
        private readonly Action _action;
        private bool _isDisposed;

        public DisposableAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _action();
            }
        }
    }
}
