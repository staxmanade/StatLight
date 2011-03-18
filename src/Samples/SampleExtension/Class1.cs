using System;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;

namespace SampleExtension
{
    public class Class1 : ITestingReportEvents
    {
        public void Handle(TestCaseResult message)
        {
            Console.WriteLine("Hello From Class1");
        }

        public void Handle(TraceClientEvent message)
        {
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
        }

        public void Handle(FatalSilverlightExceptionServerEvent message)
        {
        }
    }
}
