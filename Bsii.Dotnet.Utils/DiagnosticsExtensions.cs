using System;
using System.Diagnostics;

namespace Bsii.Dotnet.Utils
{
    public static class DiagnosticsExtensions
    {
        /// <summary>
        /// Starts the activity and returns it as disposable that stops it in case of an error.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static IDisposable Use(this Activity activity)
        {
            activity.Start();
            return new DisposableAction(activity.Stop);
        }
    }
}