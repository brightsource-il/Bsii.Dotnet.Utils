using System;
using System.Diagnostics;

namespace Bsii.Dotnet.Utils
{
    public static class DiagnosticsExtensions
    {
        /// <summary>
        /// Starts the activity and returns it as disposable.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static IDisposable Use(this Activity activity)
        {
            activity.Start();
            return new DisposableAction(activity.Stop);
        }

        /// <summary>
        /// Creates a child activity from the current activity
        /// <para>NOTE: the activity is not started automatically (this useful for later use in different async context)</para>
        /// </summary>
        public static Activity CreateChildActivity(this Activity activity, string operationName = default)
        {
            var res = new Activity(operationName ?? activity.OperationName);
            res.SetParentId(activity.Id);
            return res;
        }
    }
}