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

        /// <summary>
        /// Sends a signal to current awaiters
        /// </summary>
        public void Signal() => _signaler.SetNext(true);

        /// <summary>
        /// Waits for a future signal
        /// </summary>
        public Task WaitAsync() => _signaler.GetLatestWithGraceOrNextAsync();
    }
}
