using System;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.Configuration
{
    public class ServerTestRunConfiguration
    {
        public ServerTestRunConfiguration(Func<byte[]> xapHost, long dialogSmackDownElapseMilliseconds, string xapToTest, XapHostType xapHostType, string queryString, bool forceBrowserStart, bool showTestingBrowserHost, bool isPhoneRun)
        {
            if (xapHost == null) throw new ArgumentNullException("xapHost");
            if (xapToTest == null) throw new ArgumentNullException("xapToTest");

            HostXap = xapHost;
            DialogSmackDownElapseMilliseconds = dialogSmackDownElapseMilliseconds;
            XapToTestPath = xapToTest;
            XapHostType = xapHostType;
            QueryString = queryString;
            ForceBrowserStart = forceBrowserStart;
            ShowTestingBrowserHost = showTestingBrowserHost;
            IsPhoneRun = isPhoneRun;
        }

        public long DialogSmackDownElapseMilliseconds { get; private set; }
        public Func<byte[]> HostXap { get; private set; }
        public string XapToTestPath { get; private set; }
        public XapHostType XapHostType { get; private set; }

        public string QueryString { get; private set; }

        public bool ForceBrowserStart { get; private set; }

        public bool ShowTestingBrowserHost { get; private set; }

        public bool IsPhoneRun { get; private set; }
    }
}
