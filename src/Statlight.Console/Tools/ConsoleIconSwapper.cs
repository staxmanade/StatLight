
using System;
using System.Drawing;

namespace StatLight.Console.Tools
{
	internal class ConsoleIconSwapper : IDisposable
	{
	    readonly IntPtr _consoleWindowHwnd;
        private bool _disposed;
        
		public ConsoleIconSwapper()
		{
            _consoleWindowHwnd = NativeMethods.GetConsoleWindow();
		}

        ~ConsoleIconSwapper()
        {
            Dispose(false);
        }


		public void ShowConsoleIcon(Icon icon)
		{
            NativeMethods.SendMessage(_consoleWindowHwnd, NativeMethods.WmSeticon, NativeMethods.IconSmall, icon.Handle);
		}

		public void ChangeIconback()
		{
            NativeMethods.SendMessage(_consoleWindowHwnd, NativeMethods.WmSeticon, NativeMethods.IconSmall, (IntPtr)0);
		}

		public void Dispose()
		{
            Dispose(true);
		}


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Dispose of resources held by this instance.
                ChangeIconback();
                _disposed = true;

                // Suppress finalization of this disposed instance.
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

	}
}