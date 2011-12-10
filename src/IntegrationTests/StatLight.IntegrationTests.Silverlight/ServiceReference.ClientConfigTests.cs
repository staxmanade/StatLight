using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight
{
    [TestClass]
    public class When_trying_to_get_at_the_ServiceReferenceClientConfig
    {
        [TestMethod]
        public void Should_be_able_to_read_info_from_the_ServiceReferenceClientConfig()
        {
            var streamInfo = Application.GetResourceStream(new Uri("ServiceReferences.ClientConfig", UriKind.Relative));
            Assert.IsNotNull(streamInfo, "Should have been able to pull out the ServiceReferences.ClientConfig streamInfo from the applications xap.");
            var config = XDocument.Load(streamInfo.Stream);

            var endpoints = from endpoint in config.Descendants("endpoint")
                            select new { Address = endpoint.Attribute("address").Value };

            Assert.AreEqual(1, endpoints.Count());
        }

    }
}
