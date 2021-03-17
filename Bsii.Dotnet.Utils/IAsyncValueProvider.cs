using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils
{
    public interface IAsyncValueProvider<T>
    {
        Task<T> GetNextAsync();
    }
}
