
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace StatLight.Console.Tools
{
	internal class ConsoleIconSwapper : IDisposable
	{
		IntPtr consoleWindowHwnd;

		private const int WM_SETICON = 0x80;	// Api constant
		private const int ICON_SMALL = 0;	// Api constant

		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hwnd, int message, int wParam, IntPtr lParam);

		[DllImport("kernel32")]
		private static extern IntPtr GetConsoleWindow();

		public ConsoleIconSwapper()
		{
			consoleWindowHwnd = GetConsoleWindow();
		}

		public void ShowConsoleIcon(Icon icon)
		{
			SendMessage(consoleWindowHwnd, WM_SETICON, ICON_SMALL, icon.Handle);
		}

		public void ChangeIconback()
		{
			SendMessage(consoleWindowHwnd, WM_SETICON, ICON_SMALL, (IntPtr)0);
		}

		public void Dispose()
		{
			ChangeIconback();
		}
	}
}