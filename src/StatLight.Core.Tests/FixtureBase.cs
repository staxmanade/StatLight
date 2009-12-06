using System;
using Moq;
using NUnit.Framework;
using StatLight.Core.Common;
using Microsoft.Practices.Composite.Events;
using System.IO;
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
			TestEventAggregator = new EventAggregator();

			Before_each_test();
			Because();
		}

		protected virtual void Because()
		{
			
		}

		[TestFixtureTearDown]
		public void TearDownContext()
		{
			After_each_test();
		}

		protected virtual void Before_each_test()
		{
		}

		protected virtual void After_each_test()
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

		protected override void Before_each_test()
		{
			base.Before_each_test();

			_pathToTempXapFile = Path.GetTempFileName();

			using (var writer = File.CreateText(_pathToTempXapFile))
			{
				writer.Close();
			}

			var mockXapHostFileLoaderFactory = new Mock<XapHostFileLoaderFactory>(TestLogger);
			mockXapHostFileLoaderFactory
				.Setup(s => s.LoadXapHostFor(It.IsAny<MicrosoftTestingFrameworkVersion>()))
				.Returns(new byte[] { 0, 1, 1, 2, 3, 1, });
			_mockServerTestRunConfiguration = new ServerTestRunConfiguration(mockXapHostFileLoaderFactory.Object, MicrosoftTestingFrameworkVersion.Default);

			Assert.IsTrue(File.Exists(_pathToTempXapFile));
		}


		protected override void After_each_test()
		{
			base.After_each_test();
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