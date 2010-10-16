using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.RemoteIntegration.ServiceReference1;

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

            Service1Client service1Client = WcfHelper.GetNewService1Client();

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

    }


    static class WcfHelper
    {
        public static Uri GetSiteBaseUri()
        {
            var xapFileLocation = Application.Current.Host.Source;
            if (xapFileLocation != null)
            {
                var websiteRootUrl = new Uri(xapFileLocation, @"../");
                return new Uri(websiteRootUrl, "/Service1.svc");
            }
            throw new NotSupportedException("Could not get the Host.Source property");
        }

        public static Service1Client GetNewService1Client()
        {
            Uri uri = GetSiteBaseUri();
            Console.WriteLine("HELLO WORLD");
            Console.WriteLine(uri.ToString());
            Console.WriteLine("HELLO WORLD");
            
            var a = new EndpointAddress(uri);
            var b = new CustomBinding(new BinaryMessageEncodingBindingElement(), new HttpTransportBindingElement());
            return new Service1Client(b, a);
        }

    }
}