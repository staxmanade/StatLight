
using NUnit.Framework;
using StatLight.Core.WebServer;
using System;

namespace StatLight.Core.Tests.WebServer
{
	namespace WebServerLocationTests
	{
	    [TestFixture]
		public class WebServerLocationTests : FixtureBase
		{
			WebServerLocation webServerLocation;

			protected override void Before_all_tests()
			{
				base.Before_all_tests();
                webServerLocation = new WebServerLocation(TestLogger);
			}

			[Test]
			public void the_base_uri_should_start_with_http_localhost()
			{
				webServerLocation.BaseUrl.ToString().ShouldStartWith("http://localhost");
			}

			[Test]
			public void when_the_8887_port_is_in_use_it_should_find_another_open_port()
			{
				System.Net.Sockets.TcpClient portHog = null;
				try
				{
					portHog = new System.Net.Sockets.TcpClient(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8887));
				}
				catch (Exception) { }

				var otherLocation = new WebServerLocation(TestLogger);


				otherLocation.BaseUrl.Port.ShouldNotEqual(8887);
				if (portHog != null)
					portHog.Close();
			}

			[Test]
			public void should_be_able_to_get_the_test_page_url()
			{
				webServerLocation.TestPageUrl.ToString()
					.ShouldEqual("http://localhost:" + webServerLocation.BaseUrl.Port + "/" + StatLightServiceRestApi.GetHtmlTestPage);
			}


		}
	}
}