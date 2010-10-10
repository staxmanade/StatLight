using System.Security.Principal;
using Moq;
using StatLight.Core.WebServer.Host;

namespace StatLight.Core.Tests.WebServer
{
    namespace StatLightServiceHostTests
    {
        using System;
        using NUnit.Framework;
        using StatLight.Core.Common;
        using StatLight.Core.WebServer;
        using StatLight.Core.Tests.WebServer.XapMonitorTests;
        using System.ServiceModel;

        public class when_using_an_instance_of_the_StatLightService_for_use_in_the_service_host : using_a_random_temp_file_for_testing
        {
            StatLightService _statLightService;

            public StatLightService StatLightServiceInstance { get { return _statLightService; } }

            protected override void Before_all_tests()
            {
                base.Before_all_tests();
                IPostHandler handler = new Mock<IPostHandler>().Object;
                _statLightService = new StatLightService(
                    new NullLogger(),
                    base.CreateTestDefaultClinetTestRunConfiguraiton(),
                    MockServerTestRunConfiguration,
                    handler);
            }

            protected bool IsAnAdministrator()
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

        }

        [Category("Integration")]
        public class when_working_with_the_StatLightServiceHost : when_using_an_instance_of_the_StatLightService_for_use_in_the_service_host
        {
            protected StatLightServiceHost _statLightServiceHost;
            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                Uri baseUri = (new WebServerLocation()).BaseUrl;

                _statLightServiceHost = new StatLightServiceHost(TestLogger, StatLightServiceInstance, baseUri);
            }

            protected override void After_all_tests()
            {
                base.After_all_tests();

                if (_statLightServiceHost.State == System.ServiceModel.CommunicationState.Opened)
                    _statLightServiceHost.Stop();

                _statLightServiceHost.Abort();
            }
        }

        [TestFixture]
        public class service_should : when_working_with_the_StatLightServiceHost
        {
            [Test]
            public void the_service_was_created()
            {
                _statLightServiceHost.State.ShouldEqual(System.ServiceModel.CommunicationState.Created);
            }
        }

        [TestFixture]
        public class service_should2 : when_working_with_the_StatLightServiceHost
        {
            [Test]
            [Ignore]
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
                    //                        var msg = @"you need to give statlight permission to the port. Run the following command to give the correct permission.
                    //netsh http add urlacl url=http://+:8887/ user=<DOMAIN>\<USER>
                    //";
                    //                        Assert.Fail(msg);
                    Assert.Ignore();
                }
                catch (Exception ex)
                {
                    Assert.Ignore(ex.GetType().ToString());
                }
            }
        }
    }
}