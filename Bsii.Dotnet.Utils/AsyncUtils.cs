using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Bsii.Dotnet.Utils
{
    public static class AsyncUtils
    {
        public static async Task<List<TOut>> ParallelTransform<TIn, TOut>
        (
            IEnumerable<TIn> data,
            Func<TIn, Task<TOut>> transform,
            int maxDegreeOfParallelism,
            string description,
            Action<Exception> exceptionLogger = null
        )
        {
            async Task<TOut> WrappedTransform(TIn i)
            {
                try
                {
                    return await transform(i);
                }
                catch (Exception ex)
                {
                    exceptionLogger?.Invoke(ex);
                    return default(TOut);
                }
            }

            var tb = new TransformBlock<TIn, TOut>(
                (Func<TIn, Task<TOut>>) WrappedTransform,
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism
                });

            int nPosted = 0;
            foreach (var d in data)
            {
                if (tb.Post(d))
                {
                    nPosted++;
                }
            }
            tb.Complete();

            var res = new List<TOut>();
            while (nPosted-- > 0)
            {
                var r = await tb.ReceiveAsync();
                if (r != null)
                {
                    res.Add(r);
                }

            }

            await tb.Completion;
            return res;
        }
        
    }
}
