namespace StatLight.Core.Tests.WebServer
{
	namespace StatLightServiceHostTests
	{
		using System;
		using NUnit.Framework;
		using StatLight.Core.Common;
		using StatLight.Core.WebServer;
		using Microsoft.Practices.Composite.Events;
		using StatLight.Core.Tests.WebServer.XapMonitorTests;
        using System.ServiceModel;

		public class when_using_an_instance_of_the_StatLightService_for_use_in_the_service_host : using_a_random_temp_file_for_testing
		{
			StatLightService _statLightService;

			public StatLightService StatLightServiceInstance { get { return _statLightService; } }

			protected override void Before_each_test()
			{
				base.Before_each_test();

                _statLightService = new StatLightService(
                    new NullLogger(), 
                    base.TestEventAggregator, 
                    base.PathToTempXapFile, 
                    TestRunConfiguration.CreateDefault(), 
                    MockServerTestRunConfiguration);
			}
		}

		[TestFixture]
		[Category("Integration")]
		public class when_working_with_the_StatLightServiceHost : when_using_an_instance_of_the_StatLightService_for_use_in_the_service_host
		{
			StatLightServiceHost _statLightServiceHost;
			protected override void Before_each_test()
			{
				base.Before_each_test();

				Uri baseUri = (new WebServerLocation()).BaseUrl;

				_statLightServiceHost = new StatLightServiceHost(TestLogger, StatLightServiceInstance, baseUri);
			}

			protected override void After_each_test()
			{
				base.After_each_test();

				if (_statLightServiceHost.State == System.ServiceModel.CommunicationState.Opened)
					_statLightServiceHost.Stop();

				_statLightServiceHost.Abort();
			}

			[Test]
			public void the_service_was_created()
			{
				_statLightServiceHost.State.ShouldEqual(System.ServiceModel.CommunicationState.Created);
			}

			[Test]
			public void the_service_should_be_able_to_open_and_close_successfully()
			{
                try
                {

                    _statLightServiceHost.Start();
                    _statLightServiceHost.State.ShouldEqual(System.ServiceModel.CommunicationState.Opened);

                    _statLightServiceHost.Stop();
                    _statLightServiceHost.State.ShouldEqual(System.ServiceModel.CommunicationState.Closed);
                }
                catch (AddressAccessDeniedException)
                {
                    var msg = @"you need to give statlight permission to the port. Run the following command to give the correct permission.
netsh http add urlacl url=http://+:8887/ user=<DOMAIN>\<USER>
";
                    Assert.Fail(msg);
                }
			}
		}
	}
}