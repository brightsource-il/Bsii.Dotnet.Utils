using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    /// <summary>
    /// Provides synchronization point for multiple awaiters on a single event source
    /// </summary>
    public class AsyncEventSource
    {
        private readonly AsyncValueSource<bool> _signaler = 
            new AsyncValueSource<bool>();

        public void Signal() => _signaler.Next(true);

        public Task WaitAsync() => _signaler.GetNextAsync();
    }
}
