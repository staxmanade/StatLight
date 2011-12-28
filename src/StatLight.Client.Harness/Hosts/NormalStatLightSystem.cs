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
            if (onReady == null)
                throw new ArgumentNullException("onReady");

            string url = "http://localhost:{0}/".FormatWith(Settings.Port);

            SetPostbackUri(new Uri(url));

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

    public class Settings
    {
        static Settings()
        {
            var appManifestXml = XDocument.Load("StatLight.Settings.xml").Root;
            Func<string, XElement> getElement = settingName => appManifestXml.Elements(settingName).First();
            Func<string, string> getValue = settingName => getElement(settingName).Value;

            Port = int.Parse(getValue("Port"));
        }

        public static int Port { get; private set; }
    }
}
