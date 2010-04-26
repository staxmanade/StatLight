using System;
using System.Runtime.InteropServices;

namespace StatLight.Console.Tools
{
    internal static class NativeMethods
    {
        public const int WmSeticon = 0x80;	// Api constant
        public const int IconSmall = 0;	// Api constant


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2"), DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int message, int wParam, IntPtr lParam);

        [DllImport("kernel32")]
        public static extern IntPtr GetConsoleWindow();
    }
}