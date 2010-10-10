using System;
using System.IO;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    internal class FirefoxWebBrowser : OutOfProcessWebBrowserBase
    {
        public FirefoxWebBrowser(ILogger logger, Uri uri)
            : base(logger, uri)
        {
        }

        protected override string ExePath
        {
            get
            {
                // These build tasks should be fun in 32-bit.
                string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                if (!Directory.Exists(pf))
                {
                    throw new InvalidOperationException("Could not locate the application directory.");
                }

                string firefox =
                    Path.Combine(
                    Path.Combine(
                    pf,
                    "Mozilla Firefox"),
                    "firefox.exe");

                if (!File.Exists(firefox))
                {
                    throw new FileNotFoundException("The Firefox web browser application could not be located on this system.", firefox);
                }
                return firefox;
            }
        }
    }
}