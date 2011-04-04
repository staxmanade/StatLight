using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Phone;

namespace StatLight.IntegrationTests
{
    public class WindowsPhoneBrowserHostTests : FixtureBase
    {
        [Test]
        public void Should_work()
        {
            ILogger logger = new ConsoleLogger();

            //var testXapPath = @"C:\Code\StatLight\src\StatLight.IntegrationTests.Phone.MSTest\Bin\Debug\StatLight.IntegrationTests.Phone.MSTest.xap";
            var testXapPath = @"C:\Code\StatLight\src\StatLight.Client.Harness.Phone\Bin\Debug\StatLight.Client.Harness.Phone.xap";
            //var testXapPath = TestXapFileLocations.WinPhone;

            var windowsPhoneBrowserHost = new WindowsPhoneBrowserHost(logger, testXapPath);

            windowsPhoneBrowserHost.Start();

            //System.Threading.Thread.Sleep(10000);
            //
            //windowsPhoneBrowserHost.Stop();
        }
    }
}