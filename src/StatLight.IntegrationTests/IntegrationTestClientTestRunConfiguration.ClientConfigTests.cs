using System.Collections.Generic;
using StatLight.Core.Configuration;
using StatLight.Core.UnitTestProviders;

namespace StatLight.IntegrationTests
{
    public class IntegrationTestClientTestRunConfiguration : ClientTestRunConfiguration
    {
        public IntegrationTestClientTestRunConfiguration()
            : base(UnitTestProviderType.MSTest, new List<string>(), string.Empty)
        { }

        public IntegrationTestClientTestRunConfiguration(List<string> methodsToTest)
            : base(UnitTestProviderType.MSTest, methodsToTest, string.Empty)
        { }

        public IntegrationTestClientTestRunConfiguration(UnitTestProviderType unitTestProviderType)
            : base(unitTestProviderType, new List<string>(), string.Empty)
        { }

        public IntegrationTestClientTestRunConfiguration(UnitTestProviderType unitTestProviderType, List<string> methodsToTest)
            : base(unitTestProviderType, methodsToTest, string.Empty)
        { }
    }
}