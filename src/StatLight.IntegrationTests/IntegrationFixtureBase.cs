using StatLight.IntegrationTests.ProviderTests;

namespace StatLight.IntegrationTests
{
	using System.IO;
	using StatLight.Core.Common;
	using StatLight.Core.Runners;
	using StatLight.Core.WebServer;
	using StatLight.Core.WebServer.XapHost;

	public abstract class IntegrationFixtureBase : FixtureBase
	{
		private string _pathToIntegrationTestXap;
		private readonly ILogger _testLogger;

		protected IntegrationFixtureBase()
		{
			_testLogger = new ConsoleLogger(LogChatterLevels.Full);
			_pathToIntegrationTestXap = TestXapFileLocations.SilverlightIntegrationTests;
		}

		public string PathToIntegrationTestXap
		{
			get { return _pathToIntegrationTestXap; }
			set
			{
				if (File.Exists(value))
					_pathToIntegrationTestXap = value;
				else
					throw new FileNotFoundException("test xap file not found...[{0}]".FormatWith(value));
			}
		}

		protected IRunner Runner { get; private set; }
		protected abstract ClientTestRunConfiguration ClientTestRunConfiguration { get; }

		protected override void Before_all_tests()
		{
			var serverTestRunConfiguration = new ServerTestRunConfiguration(new XapHostFileLoaderFactory(_testLogger), MicrosoftTestingFrameworkVersion.Default);
			Runner = StatLightRunnerFactory.CreateOnetimeConsoleRunner(_testLogger, _pathToIntegrationTestXap, ClientTestRunConfiguration, serverTestRunConfiguration, true);
		}

	}
}