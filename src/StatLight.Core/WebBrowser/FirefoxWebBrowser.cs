using System;
using System.IO;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    internal class FirefoxWebBrowser : OutOfProcessWebBrowserBase
    {
        public FirefoxWebBrowser(ILogger logger, Uri uri, bool forceBrowserStart, bool isStartingMultipleInstances)
            : base(logger, uri, forceBrowserStart, isStartingMultipleInstances)
        {
        }

        protected override string ProcessName
        {
            get { return "firefox"; }
        }

        protected override string ExePath
        {
            get { return FireFoxPath; }
        }

        protected override string GetCommandLineArguments(Uri uri)
        {
            return "{0} {1}".FormatWith("-new-window", base.GetCommandLineArguments(uri));
        }

        public static string FireFoxPath
        {
            get
            {
                // These build tasks should be fun in 32-bit.
                string pf = X86ProgramFilesFolder();

                string firefox =
                    Path.Combine(
                    Path.Combine(
                    pf,
                    "Mozilla Firefox"),
                    "firefox.exe");

                return firefox;
            }
        }
    }
}