using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using StatLight.Core.Monitoring;
using StatLight.Core.WebBrowser;

namespace StatLight.Core.Tests.WebBrowser
{
    public class BrowserFormHostTests : FixtureBase
    {
        [Test]
        [Explicit]
        public void Should_be_able_to_create_start_and_stop_the_BrowserFormHost()
        {
            var browserFormHost = new BrowserFormHost(TestLogger, new Uri("http://localsomewhere"), true, new Mock<IDialogMonitorRunner>().Object);
            browserFormHost.Start();

            const int count = 2;
            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(1000);
            }

            browserFormHost.Stop();
        }
    }
}
