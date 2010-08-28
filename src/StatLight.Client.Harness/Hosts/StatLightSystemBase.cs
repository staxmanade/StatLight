using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Net;
using System.Reflection;
using System.Windows;
using StatLight.Client.Harness.Messaging;
using StatLight.Core.Configuration;
using StatLight.Core.Serialization;
using StatLight.Core.WebServer;

namespace StatLight.Client.Harness.Hosts
{
    public abstract class StatLightSystemBase
    {
        private static CompositionContainer _container;
        protected bool TestRunConfigurationDownloadComplete;
        protected ITestRunnerHost TestRunnerHost;
        protected bool CompletedTestXapRequest;
        protected Action<UIElement> OnReady;

        private static CompositionContainer Container
        {
            get
            {
                if (_container == null)
                {

                    try
                    {
                        _container = new CompositionContainer(new DeploymentCatalog());
                    }
                    catch (ReflectionTypeLoadException rfex)
                    {
                        ReflectionInfoHelper.HandleReflectionTypeLoadException(rfex);
                    }
                    catch (CompositionException compositionException)
                    {
                        Server.Trace(compositionException.ToString());
                        foreach (var err in compositionException.Errors)
                            Server.Trace(err.ToString());
                    }
                }
                return _container;
            }
        }

        protected abstract void OnTestRunConfigurationDownloaded(ClientTestRunConfiguration clientTestRunConfiguration);

        protected static T LocateService<T>() where T : class
        {
            T service = null;
            try
            {
                service = Container.GetExportedValue<T>();
            }
            catch (ReflectionTypeLoadException rfex)
            {
                ReflectionInfoHelper.HandleReflectionTypeLoadException(rfex);
            }
            catch (CompositionException compositionException)
            {
                Server.Trace(compositionException.ToString());
                foreach (var err in compositionException.Errors)
                    Server.Trace(err.ToString());
            }

            if (service == null)
                Server.Trace("Could not locate service {0}.".FormatWith(typeof(T).FullName));

            return service;
        }

        public void GoGetTheTestRunConfiguration()
        {
            var client = new WebClient
                             {
                                 AllowReadStreamBuffering = true
                             };

            client.OpenReadCompleted += (sender, e) =>
                                            {
                                                var clientTestRunConfiguration = e.Result.Deserialize<ClientTestRunConfiguration>();
                                                ClientTestRunConfiguration.CurrentClientTestRunConfiguration = clientTestRunConfiguration;
                                                TestRunConfigurationDownloadComplete = true;
                                                TestRunnerHost.ConfigureWithClientTestRunConfiguration(clientTestRunConfiguration);
                                                Server.Debug("XapToTestUrl: {0}".FormatWith(clientTestRunConfiguration.XapToTestUrl));
                                                OnTestRunConfigurationDownloaded(clientTestRunConfiguration);
                                            };
            client.OpenReadAsync(StatLightServiceRestApi.GetTestRunConfiguration.ToFullUri());
        }

        protected void DisplayTestHarness()
        {
            if (TestRunConfigurationDownloadComplete && CompletedTestXapRequest)
            {
                var rootVisual = TestRunnerHost.StartRun();
                OnReady(rootVisual);
            }
        }

        protected static void SetPostbackUri(Uri postbackUriBase)
        {
            StatLightServiceRestApi.PostbackUriBase = postbackUriBase;
        }

    }
}