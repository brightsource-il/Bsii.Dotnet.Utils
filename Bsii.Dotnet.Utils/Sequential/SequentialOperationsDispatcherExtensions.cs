using System;
using System.Threading.Tasks;

namespace Bsii.Dotnet.Utils.Sequential
{
    public static class SequentialOperationsDispatcherExtensions
    {
        public static Task Dispatch(this ISequentialOperationsDispatcher dispatcher, Action exec)
        {
            return dispatcher.Dispatch(() =>
            {
                exec();
                return Task.CompletedTask;
            });
        }

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
