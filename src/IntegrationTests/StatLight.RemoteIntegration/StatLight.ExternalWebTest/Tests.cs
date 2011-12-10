using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Browser;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Model.Messaging;
using StatLight.ExternalWebTest.ServiceReference1;

namespace StatLight.ExternalWebTest
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

        [TestMethod]
        [Asynchronous]
        public void Should_try_to_GET_back_to_home_base_for_a_resource_that_does_not_exist()
        {
            var address = new Uri(Application.Current.Host.Source, "/SomeRandomRequest?a=1&b=2");
            var webClient = new WebClient();
            webClient.DownloadStringCompleted += (sender, e) =>
                                                     {
                                                         AssertErrorIs404(e.Error);
                                                         EnqueueTestComplete();
                                                     };
            EnqueueCallback(() => webClient.DownloadStringAsync(address));
        }

        private static void AssertErrorIs404(Exception error)
        {
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Message.Contains("The remote server returned an error: NotFound"),
                          "could not find expected message [The remote server returned an error: NotFound] contained in [" +
                          error.Message + "]");

        }


        [TestMethod]
        [Asynchronous]
        public void Should_try_to_POST_back_to_home_base_for_a_resource_that_does_not_exist()
        {
            var address = new Uri(Application.Current.Host.Source, "/SomeRandomRequest");

            var httpWebRequestHelper = new HttpWebRequestHelper(address, "POST", "data=asdf&ffff=aaaa");
            httpWebRequestHelper.ResponseComplete += e =>
                                                         {
                                                             AssertErrorIs404(e.Error);
                                                             EnqueueTestComplete();
                                                         };

            httpWebRequestHelper.Execute();
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