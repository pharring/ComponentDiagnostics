using System;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    sealed class TaskContextConverter : ValueConverter<IConvertible, string>
    {
        protected override string Convert(IConvertible value, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert((__VSTASKRUNCONTEXT)value.ToUInt32(culture));
        }

        private static string Convert(__VSTASKRUNCONTEXT value)
        {
            switch (value)
            {
                case __VSTASKRUNCONTEXT.VSTC_UITHREAD_SEND:
                    return "UI Thread - RPC";
                case __VSTASKRUNCONTEXT.VSTC_BACKGROUNDTHREAD:
                    return "Background Thread";
                case __VSTASKRUNCONTEXT.VSTC_UITHREAD_BACKGROUND_PRIORITY:
                    return "UI Thread - Background";
                case __VSTASKRUNCONTEXT.VSTC_UITHREAD_IDLE_PRIORITY:
                    return "UI Thread - Idle";
                case __VSTASKRUNCONTEXT.VSTC_UITHREAD_NORMAL_PRIORITY:
                    return "UI Thread - Normal";
                case __VSTASKRUNCONTEXT.VSTC_BACKGROUNDTHREAD_LOW_IO_PRIORITY:
                    return "Background, low I/O";
                default:
                    return value.ToString();
            }
        }
    }
}
