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
            return value switch
            {
                __VSTASKRUNCONTEXT.VSTC_UITHREAD_SEND => "UI Thread - RPC",
                __VSTASKRUNCONTEXT.VSTC_BACKGROUNDTHREAD => "Background Thread",
                __VSTASKRUNCONTEXT.VSTC_UITHREAD_BACKGROUND_PRIORITY => "UI Thread - Background",
                __VSTASKRUNCONTEXT.VSTC_UITHREAD_IDLE_PRIORITY => "UI Thread - Idle",
                __VSTASKRUNCONTEXT.VSTC_UITHREAD_NORMAL_PRIORITY => "UI Thread - Normal",
                __VSTASKRUNCONTEXT.VSTC_BACKGROUNDTHREAD_LOW_IO_PRIORITY => "Background, low I/O",
                _ => value.ToString(),
            };
        }
    }
}
