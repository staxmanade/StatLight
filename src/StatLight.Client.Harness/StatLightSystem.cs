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
    public class StatLightSystem
    {
        private bool _testRunConfigurationDownloadComplete;
        private bool _completedTestXapRequest;
        private ITestRunnerHost _testRunnerHost;
        private Action<UIElement> _onReady;

        private readonly Uri _postbackUriBase;

        private readonly bool _isRemotePostRun;
        private readonly Assembly _assemblyToTest;

        internal StatLightSystem(Action<UIElement> onReady)
        {
            var src = Application.Current.Host.Source;
            var urlx = src.Scheme + "://" + src.Host + ":" + src.Port + "/";

            _postbackUriBase = new Uri(urlx);
            SetPostbackUri(_postbackUriBase);

            OnReadySetupRootVisual(onReady);
        }

        public StatLightSystem(Assembly assemblyToTest, Action<UIElement> onReady)
        {
            _assemblyToTest = assemblyToTest;

            IQueryStringReader queryStringReader = new QueryStringReader();
            string postbackUrl = queryStringReader.GetValueOrDefault(StatLightServiceRestApi.StatLightResultPostbackUrl, default(string));
            if (postbackUrl != default(string))
            {
                Console.WriteLine("postbackUrl={0}".FormatWith(postbackUrl));
                _isRemotePostRun = true;
                _postbackUriBase = new Uri(postbackUrl);
            }
            _isRemotePostRun = true;
            _postbackUriBase = null;
            SetPostbackUri(_postbackUriBase);

            OnReadySetupRootVisual(onReady);
        }

        private static void SetPostbackUri(Uri postbackUriBase)
        {
            StatLightServiceRestApi.PostbackUriBase = postbackUriBase;
        }

        private void OnReadySetupRootVisual(Action<UIElement> onReady)
        {
            if (onReady == null)
                throw new ArgumentNullException("onReady");

            _onReady = onReady;

            if (_isRemotePostRun)
            {
                _remotelyHostedTestRunnerHost = LocateService<IRemotelyHostedTestRunnerHost>();

                if (_remotelyHostedTestRunnerHost != null)
                {
                    var assemblies = new List<Assembly>
                    {
                        Assembly.GetCallingAssembly()
                    };

                    var rootVisual = _remotelyHostedTestRunnerHost.StartRun(assemblies);
                    _onReady(rootVisual);
                }
                else
                {
                    throw new StatLightException("Could not locate the StatLight IRemotelyHostedTestRunnerHost");
                }
            }
            else
            {
                _testRunnerHost = LocateService<ITestRunnerHost>();
                GoGetTheTestRunConfiguration();
            }
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
                _testRunConfigurationDownloadComplete = true;

                _testRunnerHost.ConfigureWithClientTestRunConfiguration(clientTestRunConfiguration);
                Server.Debug("XapToTestUrl: {0}".FormatWith(clientTestRunConfiguration.XapToTestUrl));
                if (_isRemotePostRun)
                {

                    ILoadedXapData loadedXapData = new CurrentXapData(_assemblyToTest);
                    _testRunnerHost.ConfigureWithLoadedXapData(loadedXapData);
                    _completedTestXapRequest = true;
                    DisplayTestHarness();
                }
                else
                {
                    GoGetTheXapUnderTest(clientTestRunConfiguration.XapToTestUrl.ToUri());
                }
            };
            client.OpenReadAsync(StatLightServiceRestApi.GetTestRunConfiguration.ToFullUri());
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

                    _testRunnerHost.ConfigureWithLoadedXapData(loadedXapData);

                    _completedTestXapRequest = true;
                    DisplayTestHarness();
                }
                else
                    Server.LogException(e.Error);
            };
            client.OpenReadAsync(xapToTestUri);
        }

        private void DisplayTestHarness()
        {
            if (_testRunConfigurationDownloadComplete && _completedTestXapRequest)
            {
                var rootVisual = _testRunnerHost.StartRun();
                _onReady(rootVisual);
            }
        }

        private static T LocateService<T>() where T : class
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


        private static CompositionContainer _container;
        private IRemotelyHostedTestRunnerHost _remotelyHostedTestRunnerHost;

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
    }
}