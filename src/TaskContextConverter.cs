using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class TaskContextConverter : ValueConverter<VsTaskRunContext, string>
    {
        protected override string Convert(VsTaskRunContext value, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value)
            {
                case VsTaskRunContext.UIThreadSend:
                    return "UI Thread - RPC";
                case VsTaskRunContext.BackgroundThread:
                    return "Background Thread";
                case VsTaskRunContext.UIThreadBackgroundPriority:
                    return "UI Thread - Background";
                case VsTaskRunContext.UIThreadIdlePriority:
                    return "UI Thread - Idle";
                default:
                    return value.ToString();
            }
        }
    }
}
