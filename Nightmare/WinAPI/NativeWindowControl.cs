using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Nightmare.WinAPI
{
    public static class NativeWindowControl
    {
        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int x;
            public int y;
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        struct WINDOWPLACEMENT
        {
            public int Length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [DllImport("user32.dll")]
        extern static bool GetWindowPlacement(int hWnd, ref WINDOWPLACEMENT lpwndpl);

        public static Rect GetWindowPlacement(Window window)
        {
            var helper = new WindowInteropHelper(window);
            WINDOWPLACEMENT wpl = new WINDOWPLACEMENT();
            wpl.Length = Marshal.SizeOf(wpl);
            GetWindowPlacement((int)helper.Handle, ref wpl);
            return new Rect(wpl.rcNormalPosition.left, wpl.rcNormalPosition.top,
                wpl.rcNormalPosition.right - wpl.rcNormalPosition.left,
                wpl.rcNormalPosition.bottom - wpl.rcNormalPosition.top);
        }
    }
}
