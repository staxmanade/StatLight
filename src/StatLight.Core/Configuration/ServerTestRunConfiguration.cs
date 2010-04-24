using System;

namespace StatLight.Core.Configuration
{
    public class ServerTestRunConfiguration
    {
        public ServerTestRunConfiguration(byte[] xapHost, long dialogSmackDownElapseMilliseconds, string xapToTest)
        {
            if (xapHost == null) throw new ArgumentNullException("xapHost");
            if (xapToTest == null) throw new ArgumentNullException("xapToTest");

            HostXap = xapHost;
            DialogSmackDownElapseMilliseconds = dialogSmackDownElapseMilliseconds;
            XapToTestPath = xapToTest;
        }

        public long DialogSmackDownElapseMilliseconds { get; private set; }
        public byte[] HostXap { get; private set; }

        public string XapToTestPath { get; private set; }
    }
}
