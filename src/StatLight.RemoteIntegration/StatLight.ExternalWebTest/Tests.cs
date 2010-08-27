using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Browser;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.ExternalWebTest.ServiceReference1;

namespace StatLight.RemoteIntegration
{
    [TestClass]
    public class Tests : SilverlightTest
    {
        [TestMethod]
        [Asynchronous]
        public void Should_be_able_to_get_remote_service_data()
        {
            const int expectedValue42 = 42;

            Uri uri = GetSiteBaseUri();
            Console.WriteLine(uri.ToString());

            var a = new EndpointAddress(uri);
            var b = new CustomBinding(new BinaryMessageEncodingBindingElement(), new HttpTransportBindingElement());
            var service1Client = new Service1Client(b, a);

            service1Client.DoWorkCompleted += (sender, e) =>
            {
                if (e.Error != null)
                    Assert.IsNull(e.Error,
                                  string.Format(
                                      "Encountered an error on service call {0}{1}",
                                      Environment.NewLine, e.Error.ToString()));

                int actualValueFromService = e.Result;

                Assert.AreEqual(expectedValue42, actualValueFromService);

                EnqueueTestComplete();
            };

            EnqueueCallback(service1Client.DoWorkAsync);
        }

        public static Uri GetSiteBaseUri()
        {
            const string keyName = "RemoteCallbackServiceUrl";
            var dict = HtmlPage.Document.QueryString;
            if (dict.ContainsKey(keyName))
            {
                string websiteRootUrl = dict[keyName];
                return new Uri(websiteRootUrl);
            }

            var xapFileLocation = Application.Current.Host.Source;
            if (xapFileLocation != null)
            {
                // use the "../" to go back up one directory from the ClientBin
                var websiteRootUrl = new Uri(xapFileLocation, @"../");
                return new Uri(websiteRootUrl, "/Service1.svc");
            }

            throw new NotSupportedException("Could figure out how to generate a url to call back to home base...");
        }
    }
}