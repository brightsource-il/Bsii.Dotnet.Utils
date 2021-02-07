using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Bsii.Dotnet.Utils
{
    public static class AsyncUtils
    {
        /// <summary>
        /// Transforms the enumerable object in parallel
        /// </summary>
        /// <typeparam name="TIn">The input type</typeparam>
        /// <typeparam name="TOut">The output type</typeparam>
        /// <param name="data">The input vector</param>
        /// <param name="transform">Transform function</param>
        /// <param name="exceptionFallbackTransform">Fallback transform function in case of exception, if not specified - using default of <see cref="TOut"></see></param>
        /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism, if not specified, using <see cref="Environment.ProcessorCount"/></param>
        /// <returns></returns>
        public static async Task<List<TOut>> ParallelTransform<TIn, TOut>
        (
            this IEnumerable<TIn> data,
            Func<TIn, Task<TOut>> transform,
            Func<Exception, TIn, Task<TOut>> exceptionFallbackTransform = null,
            int? maxDegreeOfParallelism = null)
        {
            async Task<TOut> WrappedTransform(TIn i)
            {
                try
                {
                    return await transform(i);
                }
                catch (Exception ex)
                {
                    if (exceptionFallbackTransform != null)
                    {
                        return await exceptionFallbackTransform(ex, i);
                    }

                    return default(TOut);
                }
            }

            var tb = new TransformBlock<TIn, TOut>(
                (Func<TIn, Task<TOut>>) WrappedTransform,
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount
                });

            var nPosted = 0;
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

        /// <summary>
        ///     Awaits on 2 tasks in parallel and returns the results
        /// </summary>
        public static async Task<Tuple<T1, T2>> ResolveAll<T1, T2>(Task<T1> t1, Task<T2> t2)
        {
            await Task.WhenAll(t1, t2);
            return new Tuple<T1, T2>(t1.Result, t2.Result);
        }

        /// <summary>
        ///     Awaits on 3 tasks in parallel and returns the results
        /// </summary>
        public static async Task<Tuple<T1, T2, T3>> ResolveAll<T1, T2, T3>(Task<T1> t1, Task<T2> t2, Task<T3> t3)
        {
            await Task.WhenAll(t1, t2, t3);
            return new Tuple<T1, T2, T3>(t1.Result, t2.Result, t3.Result);
        }

        /// <summary>
        ///     Awaits on 4 tasks in parallel and returns the results
        /// </summary>
        public static async Task<Tuple<T1, T2, T3, T4>> ResolveAll<T1, T2, T3, T4>(Task<T1> t1, Task<T2> t2,
            Task<T3> t3, Task<T4> t4)
        {
            await Task.WhenAll(t1, t2, t3, t4);
            return new Tuple<T1, T2, T3, T4>(t1.Result, t2.Result, t3.Result, t4.Result);
        }

        /// <summary>
        ///     Awaits on 5 tasks in parallel and returns the results
        /// </summary>
        public static async Task<Tuple<T1, T2, T3, T4, T5>> ResolveAll<T1, T2, T3, T4, T5>(Task<T1> t1, Task<T2> t2,
            Task<T3> t3, Task<T4> t4, Task<T5> t5)
        {
            await Task.WhenAll(t1, t2, t3, t4, t5);
            return new Tuple<T1, T2, T3, T4, T5>(t1.Result, t2.Result, t3.Result, t4.Result, t5.Result);
        }

        /// <summary>
        ///     Awaits on 6 tasks in parallel and returns the results
        /// </summary>
        public static async Task<Tuple<T1, T2, T3, T4, T5, T6>> ResolveAll<T1, T2, T3, T4, T5, T6>(Task<T1> t1,
            Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6)
        {
            await Task.WhenAll(t1, t2, t3, t4, t5, t6);
            return new Tuple<T1, T2, T3, T4, T5, T6>(t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result);
        }

        /// <summary>
        ///     Awaits on 7 tasks in parallel and returns the results
        /// </summary>
        public static async Task<Tuple<T1, T2, T3, T4, T5, T6, T7>> ResolveAll<T1, T2, T3, T4, T5, T6, T7>(Task<T1> t1,
            Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6, Task<T7> t7)
        {
            await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(t1.Result, t2.Result, t3.Result, t4.Result, t5.Result,
                t6.Result, t7.Result);
        }

        /// <summary>
        ///     Awaits on 8 tasks in parallel and returns the results
        /// </summary>
        public static async Task<Tuple<T1, T2, T3, T4, T5, T6, T7, T8>> ResolveAll<T1, T2, T3, T4, T5, T6, T7, T8>(
            Task<T1> t1,
            Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6, Task<T7> t7, Task<T8> t8)
        {
            await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(t1.Result, t2.Result, t3.Result, t4.Result, t5.Result,
                t6.Result, t7.Result, t8.Result);
        }

        /// <summary>
        /// A shorthand for task.ConfigureAwait(false).GetAwaiter().GetResult()
        /// </summary>
        public static void WaitContextless(this Task task) => 
            task.ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// A shorthand for task.ConfigureAwait(false).GetAwaiter().GetResult()
        /// </summary>
        public static T GetResultContextless<T>(this Task<T> task) => 
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        
        /// <summary>
        /// Throws a <see cref="TimeoutException"/> if the task didn't complete within the given <paramref name="time"/>
        /// </summary>
        public static async Task TimeoutAfter(this Task task, TimeSpan time)
        {
            if (task == await Task.WhenAny(task, Task.Delay(time)))
            {
                await task;
            }
            else
            {
                throw new TimeoutException();
            }
        }
    }
}