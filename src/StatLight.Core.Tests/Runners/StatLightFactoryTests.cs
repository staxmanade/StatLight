
namespace StatLight.Core.Tests.Runners
{
    using NUnit.Framework;
    using StatLight.Core.Configuration;
    using StatLight.Core.Runners;
    using TinyIoC;

    [TestFixture]
    public class StatLightFactoryTests : using_a_random_temp_file_for_testing
    {
        private StatLightConfiguration _statLightConfiguration;
        private TinyIoCContainer container;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();
            container = BootStrapper.Initialize(new InputOptions(), TestLogger);
        }

        protected override void Because()
        {
            base.Because();
            var clientTestRunConfiguration = base.CreateTestDefaultClinetTestRunConfiguraiton();
            _statLightConfiguration = new StatLightConfiguration(clientTestRunConfiguration,
                                                                 MockServerTestRunConfiguration);
            var currentStatLightConfiguration = new CurrentStatLightConfiguration(new[] {_statLightConfiguration});
            container.Register<ICurrentStatLightConfiguration>(currentStatLightConfiguration);
        }

        [Test]
        public void should_be_able_to_get_a_StatLight_ContinuousConsoleRunner_runner()
        {
            (new StatLightRunnerFactory(TestLogger, container)).CreateContinuousTestRunner(new[] { _statLightConfiguration });
        }

        [Test]
        public void should_be_able_to_create_the_StatLight_TeamCity_runner()
        {
            IRunner runner = (new StatLightRunnerFactory(TestLogger, container)).CreateTeamCityRunner(_statLightConfiguration);
            runner.ShouldBeOfType(typeof(TeamCityRunner));
        }
    }
}
