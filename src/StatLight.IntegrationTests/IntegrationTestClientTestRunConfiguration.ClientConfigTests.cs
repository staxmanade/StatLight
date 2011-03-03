using System.Collections.Generic;
using StatLight.Core.Configuration;

namespace StatLight.IntegrationTests
{
    public class IntegrationTestClientTestRunConfiguration : ClientTestRunConfiguration
    {
        public IntegrationTestClientTestRunConfiguration()
            : base(UnitTestProviderType.MSTest, new List<string>(), string.Empty, 1, StatLight.Core.WebBrowser.WebBrowserType.SelfHosted, false, string.Empty, new List<string>())
        { }

        public IntegrationTestClientTestRunConfiguration(IEnumerable<string> methodsToTest)
            : base(UnitTestProviderType.MSTest, methodsToTest, string.Empty, 1, StatLight.Core.WebBrowser.WebBrowserType.SelfHosted, false, string.Empty, new List<string>())
        { }

        public IntegrationTestClientTestRunConfiguration(UnitTestProviderType unitTestProviderType)
            : base(unitTestProviderType, new List<string>(), string.Empty, 1, StatLight.Core.WebBrowser.WebBrowserType.SelfHosted, false, string.Empty, new List<string>())
        { }

        public IntegrationTestClientTestRunConfiguration(UnitTestProviderType unitTestProviderType, IEnumerable<string> methodsToTest)
            : base(unitTestProviderType, methodsToTest, string.Empty, 1, StatLight.Core.WebBrowser.WebBrowserType.SelfHosted, false, string.Empty, new List<string>())
        { }
    }
}
