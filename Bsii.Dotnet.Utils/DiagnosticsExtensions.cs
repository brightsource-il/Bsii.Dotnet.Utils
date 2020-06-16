using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Bsii.Dotnet.Utils
{
    public static class DiagnosticsExtensions
    {
        public static IDisposable useActivity(this Activity activity)
        {
            activity.Start();
            return new DisposableAction(activity.Stop);
        }
    }
}