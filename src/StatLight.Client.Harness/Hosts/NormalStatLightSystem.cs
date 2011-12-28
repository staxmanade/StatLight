using System;
using System.Net;
using System.Windows;
using StatLight.Core.Configuration;
using StatLight.Core.Events.Messaging;

namespace StatLight.Client.Harness.Hosts
{
    public class NormalStatLightSystem : StatLightSystemBase
    {
        internal NormalStatLightSystem(Action<UIElement> onReady)
        {

#if WINDOWS_PHONE

            Setup(onReady, "http://localhost:8888/");
            //TryFindHomeServer(8888, onReady);
#else
            var src = Application.Current.Host.Source;
            var urlx = src.Scheme + "://" + src.Host + ":" + src.Port + "/";
            Setup(onReady, urlx);
#endif
        }

        private void TryFindHomeServer(int port, Action<UIElement> onReady)
        {
            System.Diagnostics.Debugger.Break();
            //var url = "http://localhost:8888/crossdomain.xml";
            var url = "http://localhost:{0}/crossdomain.xml".FormatWith(port);
            var request = WebRequest.Create(url);

            request.BeginGetResponse(asyncResult =>
            {
                var response = (HttpWebResponse)request.EndGetResponse(asyncResult);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Setup(onReady, url);
                }
                else
                {
                    TryFindHomeServer(++port, onReady);
                }
            }, null);

        }

        private void Setup(Action<UIElement> onReady, string urlx)
        {
            SetPostbackUri(new Uri(urlx));

            OnReadySetupRootVisual(onReady);
        }

        private void OnReadySetupRootVisual(Action<UIElement> onReady)
        {
            if (onReady == null)
                throw new ArgumentNullException("onReady");

            OnReady = onReady;

            TestRunnerHost = LocateStatLightService<ITestRunnerHost>();
            GoGetTheTestRunConfiguration();
        }

        protected override void OnTestRunConfigurationDownloaded(ClientTestRunConfiguration clientTestRunConfiguration)
        {
            var loadedXapData = new ThisXapData(clientTestRunConfiguration.EntryPointAssembly, clientTestRunConfiguration.TestAssemblyFormalNames);
            Server.Debug("OnTestRunConfigurationDownloaded");
            TestRunnerHost.ConfigureWithLoadedXapData(loadedXapData);

            CompletedTestXapRequest = true;
            DisplayTestHarness();
        }
    }
}

