
using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.Tests.Runners
{
    using Moq;
    using NUnit.Framework;
    using StatLight.Core.Common;
    using StatLight.Core.Runners;
    using StatLight.Core.WebServer;
    using StatLight.Core.Tests.WebServer.XapMonitorTests;

    [TestFixture]
    public class StatLightFactoryTests : using_a_random_temp_file_for_testing
    {
        [Test]
        public void should_be_able_to_get_a_StatLight_ContinuousConsoleRunner_runner()
        {
            IRunner runner = StatLightRunnerFactory.CreateContinuousTestRunner(new Mock<ILogger>().Object, base.PathToTempXapFile, TestRunConfiguration.CreateDefault(), false, MockServerTestRunConfiguration);
            runner.ShouldBeOfType(typeof(ContinuousConsoleRunner));
        }

        [Test]
        public void should_be_able_to_create_the_StatLight_TeamCity_runner()
        {
            IRunner runner = StatLightRunnerFactory.CreateTeamCityRunner(base.PathToTempXapFile, TestRunConfiguration.CreateDefault(), MockServerTestRunConfiguration);
            runner.ShouldBeOfType(typeof(TeamCityRunner));
        }
    }
}
