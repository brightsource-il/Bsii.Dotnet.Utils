using System;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public interface IAsyncCachingValueProvider<T>
    {
        ValueTask<T> GetAsync(TimeSpan maxAge, TimeSpan? timeOut = null);
    }
}