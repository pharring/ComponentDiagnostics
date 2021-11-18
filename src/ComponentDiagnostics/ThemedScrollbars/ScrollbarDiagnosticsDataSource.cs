using System;
using System.Collections.ObjectModel;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class ScrollbarDiagnosticsDataSource
    {
        public void Refresh()
        {
            _topLevelWindows.Clear();
            NativeMethods.EnumThreadWindows (NativeMethods.GetCurrentThreadId(), EnumProc, IntPtr.Zero);
        }

        bool EnumProc(IntPtr hwnd, IntPtr lParam)
        {
            if (NativeWindowInfo.ShouldIncludeInView(hwnd))
                _topLevelWindows.Add(new NativeWindowInfo(hwnd));

            return true;
        }

        readonly ObservableCollection<NativeWindowInfo> _topLevelWindows = new ObservableCollection<NativeWindowInfo>();
        ReadOnlyObservableCollection<NativeWindowInfo> _topLevelWindowsRO;

        public ReadOnlyObservableCollection<NativeWindowInfo> TopLevelWindows 
        { 
            get 
            {
                if (_topLevelWindowsRO == null)
                    _topLevelWindowsRO = new ReadOnlyObservableCollection<NativeWindowInfo>(_topLevelWindows);

                return _topLevelWindowsRO; 
            } 
        }
    }
}
