using System;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Tests;
using StatLight.Core.WebBrowser;

namespace StatLight.WindowsPhoneEmulator.Tests
{
    [TestFixture]
    public class Phone_emulator_dynamically_loading_from_disk_tests : FixtureBase
    {
        [Test]
        public void Should_be_able_to_resolve_the_phone_assembly()
        {
            IWebBrowser webBrowser = GetWebBrowser();
            webBrowser.ShouldNotBeNull();
        }

        [Test]
        public void Should_be_able_to_resolve_the_phone_assembly_multiple_of_times()
        {
            IWebBrowser webBrowser1 = GetWebBrowser();
            IWebBrowser webBrowser2 = GetWebBrowser();

            webBrowser1.ShouldNotBeTheSameAs(webBrowser2);
        }

        private static IWebBrowser GetWebBrowser()
        {
            var logger = new TraceLogger(LogChatterLevels.Full);
            var webBrowserFactory = new WebBrowserFactory(logger, null, null);
            Func<byte[]> hostXap = () => new byte[] { 0, 0, 0 };
            return webBrowserFactory.CreatePhone(hostXap);
        }
    }
}
