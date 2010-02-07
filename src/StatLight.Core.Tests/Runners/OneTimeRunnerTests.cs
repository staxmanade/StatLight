using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StatLight.Core.Runners;
using StatLight.Core.WebServer;
using StatLight.Core.WebBrowser;
using StatLight.Core.Reporting;
using Moq;
using StatLight.Core.Common;

namespace StatLight.Core.Tests.Runners
{
	//[TestFixture]
	//public class when_Run_is_called_on_the_OneTimeRunner : FixtureBase
	//{
	//    Mock<IWebServer> mockWebServer = new Mock<IWebServer>();
	//    Mock<IStatLightService> mockStatLightService = new Mock<IStatLightService>();
	//    Mock<IBrowserFormHost> mockBrowserFormHost = new Mock<IBrowserFormHost>();
	//    Mock<ITestResultHandler> mockTestResultHandler = new Mock<ITestResultHandler>();

	//    OneTimeRunner runner;


	//    protected override void Before_all_tests()
	//    {
	//        base.Before_all_tests();

	//        runner = new OneTimeRunner(
	//            new NullLogger(),
	//            mockStatLightService.Object,
	//            mockWebServer.Object,
	//            mockBrowserFormHost.Object,
	//            mockTestResultHandler.Object);
	//    }

	//    //[Test]
	//    public void the_WebServer_should_have()
	//    {
	//    }
	//}
}
