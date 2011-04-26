using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using StatLight.Client.Model.Messaging;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.WebServer;

namespace StatLight.Client.Harness.Hosts
{
    public class RemoteRunStatLightSystem : StatLightSystemBase
    {
        private readonly Assembly _assemblyToTest;
        private IRemotelyHostedTestRunnerHost _remotelyHostedTestRunnerHost;

        public RemoteRunStatLightSystem(Assembly assemblyToTest, Action<UIElement> rootVisualActionOnReady)
        {
            _assemblyToTest = assemblyToTest;
            SetPostbackUri(null);
            IQueryStringReader queryStringReader = new QueryStringReader();
            string postbackUrl = queryStringReader.GetValueOrDefault(StatLightServiceRestApi.StatLightResultPostbackUrl, default(string));
            if (postbackUrl != default(string))
            {
                Console.WriteLine("postbackUrl={0}".FormatWith(postbackUrl));
                SetPostbackUri(new Uri(postbackUrl));
            }

            OnReadySetupRootVisual(rootVisualActionOnReady);
        }

        protected override void OnTestRunConfigurationDownloaded(ClientTestRunConfiguration clientTestRunConfiguration)
        {
            ILoadedXapData loadedXapData = new CurrentXapData(_assemblyToTest);
            TestRunnerHost.ConfigureWithLoadedXapData(loadedXapData);
            CompletedTestXapRequest = true;
            DisplayTestHarness();
        }


        private void OnReadySetupRootVisual(Action<UIElement> onReady)
        {
            if (onReady == null)
                throw new ArgumentNullException("onReady");

            OnReady = onReady;

            _remotelyHostedTestRunnerHost = LocateStatLightService<IRemotelyHostedTestRunnerHost>();

            if (_remotelyHostedTestRunnerHost != null)
            {
                var assemblies = new List<Assembly>
                                     {
                                         Assembly.GetCallingAssembly()
                                     };

                var rootVisual = _remotelyHostedTestRunnerHost.StartRun(assemblies);
                OnReady(rootVisual);
            }
            else
            {
                throw new StatLightException("Could not locate the StatLight IRemotelyHostedTestRunnerHost");
            }

        }
    }
}