using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using StatLight.Console;
using StatLight.Core.Configuration;
using StatLight.Core.Reporting;
using StatLight.Core.Runners;

namespace StatLight.Core.Tests.Console
{
    namespace StatLightExecutionEngine
    {

        [TestFixture]
        public class CommandLineExecutionEngineTests : FixtureBase
        {
            [Test]
            public void Should_run_the_OneTimeRunner()
            {
                ArgOptions argOptions = "-x=test.xap".ToArgOptions();
                var mockConfiugrationFactory = new Mock<IStatLightConfigurationFactory>();
                var mockStatLightRunnerFactory = new Mock<IStatLightRunnerFactory>();
                mockStatLightRunnerFactory
                    .Setup(s => s.CreateOnetimeConsoleRunner(TestLogger, It.IsAny<StatLightConfiguration>()))
                    .Returns(new Mock<IRunner>().Object);
                Func<IRunner, TestReport> f = runner => new TestReport(argOptions.XapPaths.First());
                var engine = new CommandLineExecutionEngine(TestLogger, argOptions, mockConfiugrationFactory.Object, f, mockStatLightRunnerFactory.Object);

                engine.Run();

                mockStatLightRunnerFactory.Verify(x => x.CreateOnetimeConsoleRunner(TestLogger, It.IsAny<StatLightConfiguration>()));
            }
        }
    }
}
