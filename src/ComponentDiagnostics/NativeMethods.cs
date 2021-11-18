using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    static class NativeMethods
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumThreadWindows(int threadID, EnumWindowsProc lpfn, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hwnd, int nCmd);

        // constants for GetWindow
        public const int GW_FIRST    = 0;
        public const int GW_LAST     = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_OWNER    = 4;
        public const int GW_CHILD    = 5;

        [DllImport("user32.dll", CharSet=CharSet.Unicode)]
        public static extern IntPtr GetPropW(IntPtr hwnd, string propName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLengthW(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowTextW(IntPtr hwnd, [Out] StringBuilder textBuilder, int nMaxCount);

        public static string GetWindowText(IntPtr hwnd)
        {
            StringBuilder builder = new StringBuilder(GetWindowTextLengthW(hwnd) + 1);
            GetWindowTextW(hwnd, builder, builder.Capacity);
            return builder.ToString();
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetClassNameW(IntPtr hwnd, [Out] StringBuilder classBuilder, int nMaxCount);

        public static string GetClassName(IntPtr hwnd)
        {
            StringBuilder builder = new StringBuilder(MaxClassNameLength);
            GetClassNameW(hwnd, builder, builder.Capacity);
            return builder.ToString();
        }

        const int MaxClassNameLength = 256;
     
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int LoadString(IntPtr hInstance, uint uID, [Out] StringBuilder lpBuffer, int nBufferMax);
    }
}
