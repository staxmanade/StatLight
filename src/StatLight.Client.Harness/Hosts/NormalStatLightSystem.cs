using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using StatLight.Core.Configuration;
using StatLight.Core.Events.Messaging;

namespace StatLight.Client.Harness.Hosts
{
    public class NormalStatLightSystem : StatLightSystemBase
    {
        internal NormalStatLightSystem(Action<UIElement> onReady)
        {
            var appManifestXml = XDocument.Load("StatLight.Settings.xml");
            var portElement = appManifestXml.Root.Elements("Port").First();
            var port = int.Parse(portElement.Value);
            Setup(onReady, "http://localhost:{0}/".FormatWith(port));
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

