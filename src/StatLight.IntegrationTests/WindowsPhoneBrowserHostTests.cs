using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.Phone;
using StatLight.Core.Runners;
using StatLight.Core.WebBrowser;
using StatLight.Core.WebServer;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.IntegrationTests
{
    public class TraceLogger : LoggerBase
    {
        public TraceLogger()
            : this(LogChatterLevels.Full)
        {
        }
        public TraceLogger(LogChatterLevels logChatterLevel)
            : base(logChatterLevel)
        {
        }

        public override void Information(string message)
        {
            Trace.WriteLine("Info: " + message);
        }

        public override void Warning(string message)
        {
            Trace.WriteLine("Warn: " + message);
        }

        public override void Error(string message)
        {
            Trace.WriteLine("Err: " + message);
        }

        public override void Debug(string message)
        {
            Debug(message, true);
        }

        public override void Debug(string message, bool writeNewLine)
        {
            if (writeNewLine)
                Trace.WriteLine("Debug: " + message + Environment.NewLine);
            else
                Trace.WriteLine("Debug: " + message);
        }
    }

    public class WindowsPhoneBrowserHostTests : FixtureBase
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

            //var testXapPath = @"C:\Code\StatLight\src\StatLight.Client.Harness.Phone\Bin\Debug\StatLight.Client.Harness.Phone.xap";
            //var testXapPath = TestXapFileLocations.WinPhone;

            var tempFileName = Path.GetTempFileName();
            File.WriteAllBytes(tempFileName, statLightConfigurationForXap.Server.HostXap());
            logger.Debug("temp file: " + tempFileName);

            statLightConfigurationForXap.Client.TestAssemblyFormalNames.Each(x => Trace.WriteLine(x));

            var windowsPhoneBrowserHost = new WindowsPhoneBrowserHost(logger, tempFileName);

            windowsPhoneBrowserHost.Start();

            System.Threading.Thread.Sleep(100000);
            //windowsPhoneBrowserHost.Stop();
        }
    }
}