using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Shell.Interop;

using HWND = System.IntPtr;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class NativeWindowInfo
    {
        const string ThemeModePropName   = "VisualStudio.ScrollbarThemeMode";
        const string GlowWindowClassName = "VisualStudioGlowWindow";

        public NativeWindowInfo(HWND hwnd)
        {
            Handle    = hwnd;
            ClassName = NativeMethods.GetClassName(hwnd);
            Text      = NativeMethods.GetWindowText(hwnd);
            Mode      = (__VSNativeScrollbarThemeMode)(int) NativeMethods.GetPropW(hwnd, ThemeModePropName);

            Description = string.Format("Window {0:X8} \"{1}\" {2}", (int) Handle, Text, ClassName);

            HWND hwndChild = NativeMethods.GetWindow(hwnd, NativeMethods.GW_CHILD);
            while (hwndChild != IntPtr.Zero)
            {
                if (ShouldIncludeInView(hwnd))
                    _children.Add(new NativeWindowInfo(hwndChild));

                hwndChild = NativeMethods.GetWindow(hwndChild, NativeMethods.GW_HWNDNEXT);
            }
        }

        public static bool ShouldIncludeInView(HWND hwnd)
        {
            // invisible windows aren't interesting
            if (!NativeMethods.IsWindowVisible(hwnd))
                return false;

            // glow windows aren't interesting
            if (NativeMethods.GetClassName(hwnd) == GlowWindowClassName)
                return false;

            return true;
        }

        public HWND   Handle                        { get; private set; }
        public string Description                   { get; private set; }
        public string ClassName                     { get; private set; }
        public string Text                          { get; private set; }
        public __VSNativeScrollbarThemeMode Mode    { get; private set; }

        readonly ObservableCollection<NativeWindowInfo> _children = new ObservableCollection<NativeWindowInfo>();
        ReadOnlyObservableCollection<NativeWindowInfo> _childrenRO;

        public ReadOnlyObservableCollection<NativeWindowInfo> Children
        {
            get
            {
                if (_childrenRO == null)
                    _childrenRO = new ReadOnlyObservableCollection<NativeWindowInfo>(_children);

                return _childrenRO;
            }
        }
    }
}
