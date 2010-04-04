using System.IO;
using System.Threading;
using Moq;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.WebServer;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.Tests
{
	public class FixtureBase
	{
		public FixtureBase()
		{
			TestLogger = new NullLogger();
		}

		public ILogger TestLogger { get; set; }
		public IEventAggregator TestEventAggregator { get; set; }

		[TestFixtureSetUp]
		public void SetupContext()
		{
			TestEventAggregator = new EventAggregator(new SynchronizationContext());

			Before_all_tests();
			Because();
		}

		[TestFixtureTearDown]
		public void TearDownContext()
		{
			After_all_tests();
		}

		protected virtual void Before_all_tests()
		{
		}

        protected virtual void Because()
        {
        }

        protected virtual void After_all_tests()
		{
		}
	}


	public class using_a_random_temp_file_for_testing : FixtureBase
	{
		private string _pathToTempXapFile;
		private ServerTestRunConfiguration _mockServerTestRunConfiguration;

		protected string PathToTempXapFile
		{
			get { return _pathToTempXapFile; }
		}

		public ServerTestRunConfiguration MockServerTestRunConfiguration
		{
			get { return _mockServerTestRunConfiguration; }
		}

		protected override void Before_all_tests()
		{
			base.Before_all_tests();

			_pathToTempXapFile = Path.GetTempFileName();

			using (var writer = File.CreateText(_pathToTempXapFile))
			{
				writer.Close();
			}
			var mockXapHostFileLoaderFactory = new Mock<XapHostFileLoaderFactory>(TestLogger);
            mockXapHostFileLoaderFactory
				.Setup(s => s.LoadXapHostFor(It.IsAny<MicrosoftTestingFrameworkVersion>()))
				.Returns(new byte[] { 0, 1, 1, 2, 3, 1, });
            _mockServerTestRunConfiguration = new ServerTestRunConfiguration(mockXapHostFileLoaderFactory.Object, MicrosoftTestingFrameworkVersion.March2010);

			Assert.IsTrue(File.Exists(_pathToTempXapFile));
        }


		protected override void After_all_tests()
		{
			base.After_all_tests();
			if (File.Exists(_pathToTempXapFile))
				File.Delete(_pathToTempXapFile);
		}

		protected virtual void replace_test_file()
		{
			File.Delete(_pathToTempXapFile);

			using (var writer = File.CreateText(_pathToTempXapFile))
			{
				writer.Close();
			}
		}
	}

}