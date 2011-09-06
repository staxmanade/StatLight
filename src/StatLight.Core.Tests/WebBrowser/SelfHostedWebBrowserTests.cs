using System;
using System.Threading;
using NUnit.Framework;

namespace StatLight.Core.Tests.WebBrowser
{
    public class SelfHostedWebBrowserTests : FixtureBase
    {
        [Test]
        [Explicit]
        public void Should_be_able_to_create_start_and_stop_the_SelfHostedWebBrowser()
        {
            var webBrowser = new Core.WebBrowser.SelfHostedWebBrowser(TestLogger, new Uri("http://localsomewhere"), true, null);
            webBrowser.Start();

            const int count = 2;
            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(1000);
            }

            webBrowser.Stop();
        }
    }
}
