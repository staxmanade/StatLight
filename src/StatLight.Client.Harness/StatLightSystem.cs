using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Net;
using System.Reflection;
using System.Windows;
using StatLight.Client.Harness.Hosts;
using StatLight.Client.Model.Messaging;
using StatLight.Core.Configuration;
using StatLight.Core.Serialization;
using StatLight.Core.WebServer;

namespace StatLight.Client.Harness
{
    public static class StatLightSystemFactory
    {
        public static void Run(Assembly assemblyToTest, Action<UIElement> rootVisualActionOnReady)
        {
            new RemoteRunStatLightSystem(assemblyToTest, rootVisualActionOnReady);
        }
    }

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

            _remotelyHostedTestRunnerHost = LocateService<IRemotelyHostedTestRunnerHost>();

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

    public class StatLightSystem : StatLightSystemBase
    {
        private readonly Uri _postbackUriBase;

        internal StatLightSystem(Action<UIElement> onReady)
        {
            var src = Application.Current.Host.Source;
            var urlx = src.Scheme + "://" + src.Host + ":" + src.Port + "/";

            _postbackUriBase = new Uri(urlx);
            SetPostbackUri(_postbackUriBase);

            OnReadySetupRootVisual(onReady);
        }

        private void OnReadySetupRootVisual(Action<UIElement> onReady)
        {
            if (onReady == null)
                throw new ArgumentNullException("onReady");

            OnReady = onReady;


            TestRunnerHost = LocateService<ITestRunnerHost>();
            GoGetTheTestRunConfiguration();
        }


        private void GoGetTheXapUnderTest(Uri xapToTestUri)
        {
            Server.Debug("GoGetTheXapUnderTest");
            var client = new WebClient
            {
                AllowReadStreamBuffering = true
            };
            client.OpenReadCompleted += (sender, e) =>
            {
                Server.Debug("OnXapToTestDownloaded");
                if (e.Error == null)
                {
                    var loadedXapData = new LoadedXapData(e.Result);

                    TestRunnerHost.ConfigureWithLoadedXapData(loadedXapData);

                    CompletedTestXapRequest = true;
                    DisplayTestHarness();
                }
                else
                    Server.LogException(e.Error);
            };
            client.OpenReadAsync(xapToTestUri);
        }


        protected override void OnTestRunConfigurationDownloaded(ClientTestRunConfiguration clientTestRunConfiguration)
        {
            GoGetTheXapUnderTest(clientTestRunConfiguration.XapToTestUrl.ToUri());
        }
    }
}