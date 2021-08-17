using System;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public interface IAsyncCachingEventProvider
    {
        ValueTask WaitAsync(TimeSpan maxAge, TimeSpan? timeOut = null);
    }
}