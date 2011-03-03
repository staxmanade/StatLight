using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Phone;
using StatLight.IntegrationTests.ProviderTests;

namespace StatLight.IntegrationTests
{
    public class WindowsPhoneBrowserHostTests : FixtureBase
    {
        [Test]
        public void Should_NAME()
        {
            ILogger logger = new ConsoleLogger();

            var testXapPath = TestXapFileLocations.WinPhone;

            var windowsPhoneBrowserHost = new WindowsPhoneBrowserHost(logger, testXapPath);

            windowsPhoneBrowserHost.Start();
        }
    }
}