using System.Collections.ObjectModel;
using System.Diagnostics;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.Runners;
using StatLight.Core.WebBrowser;
using StatLight.Core.WebServer;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.IntegrationTests
{
    public class WindowsPhoneWebBrowserTests : FixtureBase
    {
        [Test]
        [Explicit]
        public void Should_work()
        {
            ILogger logger = new TraceLogger();

            var xapUnderTest = @"C:\Code\StatLight\src\StatLight.IntegrationTests.Phone.MSTest\Bin\Debug\StatLight.IntegrationTests.Phone.MSTest.xap";
            var y = new XapHostFileLoaderFactory(logger, @"C:\code\statlight\src\build\bin\debug\");
            var statLightConfigurationFactory = new StatLightConfigurationFactory(logger, y);
            var statLightConfigurationForXap = statLightConfigurationFactory.GetStatLightConfigurationForXap(
                UnitTestProviderType.MSTestPhone,
                xapUnderTest,
                MicrosoftTestingFrameworkVersion.May2010,
                new Collection<string>(),
                string.Empty,
                1,
                false,
                string.Empty,
                WebBrowserType.Phone,
                true, true);
            var statLightRunnerFactory = new StatLightRunnerFactory(logger);
            var webServerLocation = new WebServerLocation(logger, 8887);
            var webServer = statLightRunnerFactory.CreateWebServer(logger, statLightConfigurationForXap, webServerLocation);
            webServer.Start();

            var windowsPhoneBrowserHost = new WindowsPhoneWebBrowser(logger, statLightConfigurationForXap.Server.HostXap);

            //statLightConfigurationForXap.Client.TestAssemblyFormalNames.Each(x => Trace.WriteLine(x));


            windowsPhoneBrowserHost.Start();

            System.Threading.Thread.Sleep(100000);
            //windowsPhoneBrowserHost.Stop();
        }
    }
}