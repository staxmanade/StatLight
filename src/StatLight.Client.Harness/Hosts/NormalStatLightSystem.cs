using System;
using System.Net;
using System.Windows;
using StatLight.Core.Configuration;
using StatLight.Core.Events.Messaging;
using StatLight.Core.WebServer;

namespace StatLight.Core.Events.Hosts
{
    public class NormalStatLightSystem : StatLightSystemBase
    {
        private readonly Uri _postbackUriBase;

        internal NormalStatLightSystem(Action<UIElement> onReady)
        {
            
#if WINDOWS_PHONE
            
            var urlx = "http://localhost:8887/";
#else
            var src = Application.Current.Host.Source;
            var urlx = src.Scheme + "://" + src.Host + ":" + src.Port + "/";
#endif
            _postbackUriBase = new Uri(urlx);
            SetPostbackUri(_postbackUriBase);

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
