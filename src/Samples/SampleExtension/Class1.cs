using System.Windows.Forms;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;

namespace SampleExtension
{
    public class Class1 : ITestingReportEvents
    {
        public void Handle(TestCaseResult message)
        {
            if (message.ResultType == ResultType.Failed)
                DisplayMessage(message.ExceptionInfo.ToString());
        }

        public void Handle(TraceClientEvent message)
        {
            //DisplayMessage(message);
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
            //DisplayMessage(message);
        }

        public void Handle(FatalSilverlightExceptionServerEvent message)
        {
            //DisplayMessage(message);
        }

        public void DisplayMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
