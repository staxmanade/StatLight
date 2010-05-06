using System.Collections.Generic;
using StatLight.Core.Configuration;
using StatLight.Core.UnitTestProviders;

namespace StatLight.IntegrationTests
{
    public class IntegrationTestClientTestRunConfiguration : ClientTestRunConfiguration
    {
        public IntegrationTestClientTestRunConfiguration()
            : base(UnitTestProviderType.MSTest, new List<string>(), string.Empty, 1)
        { }

        public IntegrationTestClientTestRunConfiguration(List<string> methodsToTest)
            : base(UnitTestProviderType.MSTest, methodsToTest, string.Empty, 1)
        { }

        public IntegrationTestClientTestRunConfiguration(UnitTestProviderType unitTestProviderType)
            : base(unitTestProviderType, new List<string>(), string.Empty, 1)
        { }

        public IntegrationTestClientTestRunConfiguration(UnitTestProviderType unitTestProviderType, List<string> methodsToTest)
            : base(unitTestProviderType, methodsToTest, string.Empty, 1)
        { }
    }
}