using StatLight.Core.Configuration;

namespace StatLight.Core.Tests.Runners
{
    using Moq;
    using NUnit.Framework;
    using StatLight.Core.Common;
    using StatLight.Core.Runners;
    using StatLight.Core.WebServer;

    [TestFixture]
    public class StatLightFactoryTests : using_a_random_temp_file_for_testing
    {
        private StatLightConfiguration _statLightConfiguration;

        protected override void Because()
        {
            base.Because();
            var clientTestRunConfiguration = base.CreateTestDefaultClinetTestRunConfiguraiton();
            _statLightConfiguration = new StatLightConfiguration(clientTestRunConfiguration,
                                                                 MockServerTestRunConfiguration);
        }

        [Test]
        public void should_be_able_to_get_a_StatLight_ContinuousConsoleRunner_runner()
        {
            IRunner runner = (new StatLightRunnerFactory(TestLogger, EventAggregatorFactory.Create(TestLogger))).CreateContinuousTestRunner(_statLightConfiguration);
            runner.ShouldBeOfType(typeof(ContinuousConsoleRunner));
        }

        [Test]
        public void should_be_able_to_create_the_StatLight_TeamCity_runner()
        {
            IRunner runner = (new StatLightRunnerFactory(TestLogger, EventAggregatorFactory.Create(TestLogger))).CreateTeamCityRunner(_statLightConfiguration);
            runner.ShouldBeOfType(typeof(TeamCityRunner));
        }
    }
}
