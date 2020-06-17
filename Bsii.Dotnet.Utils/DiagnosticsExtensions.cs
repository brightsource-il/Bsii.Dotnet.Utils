using System;
using System.Diagnostics;

namespace Bsii.Dotnet.Utils
{
    public static class DiagnosticsExtensions
    {
        public static IDisposable Use(this Activity activity)
        {
            activity.Start();
            return new DisposableAction(activity.Stop);
        }
    }
}