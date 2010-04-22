using System.Linq;
using NUnit.Framework;
using StatLight.Core.Tests;
using StatLight.Core.WebServer;
using StatLight.Core.UnitTestProviders;
using System.Collections.Generic;

namespace StatLight.IntegrationTests
{
    public class ServiceReferenceClientConfigTests : IntegrationFixtureBase
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return _clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            _clientTestRunConfiguration = new ClientTestRunConfiguration
            {
                TagFilter = string.Empty,
                UnitTestProviderType = UnitTestProviderType.MSTest,
                MethodsToTest = new List<string>()
                       {
                            "StatLight.IntegrationTests.Silverlight.When_trying_to_get_at_the_ServiceReferenceClientConfig.Should_be_able_to_read_info_from_the_ServiceReferenceClientConfig",
                       }
            };
        }
        [Test]
        public void Should_be_able_to_detect_and_use_the_ServiceReferenceClientConfig_file()
        {
            TestReport
                .TestResults
                .Where(w => w.MethodName != null)
                .Where(w => w.MethodName.Contains("ServiceReferenceClientConfig"))
                .SingleOrDefault()
                .ExceptionInfo
                .ShouldBeNull();
        }

    }
}