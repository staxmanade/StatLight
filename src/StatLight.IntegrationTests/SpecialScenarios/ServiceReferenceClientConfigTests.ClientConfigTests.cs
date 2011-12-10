using System.Linq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Tests;

namespace StatLight.IntegrationTests.SpecialScenarios
{
    public class ServiceReferenceClientConfigTests : SpecialScenariosBase
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get
            {
                if (_clientTestRunConfiguration == null)
                {
                    _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(
                        new []
                        {
                            "StatLight.IntegrationTests.Silverlight.When_trying_to_get_at_the_ServiceReferenceClientConfig.Should_be_able_to_read_info_from_the_ServiceReferenceClientConfig",
                        });
                }
                return _clientTestRunConfiguration;
            }
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