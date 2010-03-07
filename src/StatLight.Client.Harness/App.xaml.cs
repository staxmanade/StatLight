using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.Harness.Service;
using Microsoft.Silverlight.Testing.UI;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.UI;
using StatLight.Client.Harness.UnitTestProviders.NUnit;
using StatLight.Client.Harness.UnitTestProviders.UnitDriven;
using StatLight.Client.Harness.UnitTestProviders.Xunit;
using StatLight.Core.Serialization;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;
using StatLight.Core.Reporting.Messages;
using LogMessageType = StatLight.Core.Reporting.Messages.LogMessageType;
using StatLight.Client.Harness.UnitTestProviders.MSTest;
using StatLight.Client.Harness.Events;
using System.Diagnostics;

namespace StatLight.Client.Harness
{
    public partial class App : Application
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;
        private bool _testRunConfigurationDownloadComplete;
        private bool _completedTestXapRequest;
        private readonly UnitTestSettings _settings = new UnitTestSettings();

        public App()
        {
            Startup += Application_Startup;
            Exit += Application_Exit;
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Server.Debug("Application_Startup");
            GoGetTheTestRunConfiguration();
            GoGetTheXapUnderTest();
        }

        private void GoGetTheTestRunConfiguration()
        {
            var client = new WebClient
                             {
                                 AllowReadStreamBuffering = true
                             };
            client.OpenReadCompleted += (sender, e) =>
            {
                _clientTestRunConfiguration = e.Result.Deserialize<ClientTestRunConfiguration>();
                ClientTestRunConfiguration.CurrentClientTestRunConfiguration = _clientTestRunConfiguration;
                _testRunConfigurationDownloadComplete = true;
                _settings.TagExpression = _clientTestRunConfiguration.TagFilter;
                _settings.LogProviders.Add(new WebpageHeaderLogProvider("StatLight filters[{0}]".FormatWith(_clientTestRunConfiguration.TagFilter)));

                DisplayTestHarness();
            };
            client.OpenReadAsync(StatLightServiceRestApi.GetTestRunConfiguration.ToFullUri());
        }

        private static void SetupUnitTestProvider(UnitTestProviderType unitTestProviderType)
        {
            Microsoft.Silverlight.Testing.UnitTesting.Metadata.UnitTestProviders.Providers.Clear();
            if (unitTestProviderType == UnitTestProviderType.XUnit)
            {
                UnitTestSystem.RegisterUnitTestProvider(new XUnitTestProvider());
            }
            else if (unitTestProviderType == UnitTestProviderType.NUnit)
            {
                UnitTestSystem.RegisterUnitTestProvider(new NUnitTestProvider());
            }
            else if (unitTestProviderType == UnitTestProviderType.UnitDriven)
            {
                UnitTestSystem.RegisterUnitTestProvider(new UnitDrivenTestProvider());
            }
            else
            {
                UnitTestSystem.RegisterUnitTestProvider(new VsttProvider());
            }
        }

        private void Application_Exit(object sender, EventArgs e)
        {
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                LogException(e.ExceptionObject);
            }
            catch (Exception)
            {
            }
            finally
            {
                e.Handled = true;
            }
        }

        #region most of the custom StatLight app code
        private void GoGetTheXapUnderTest()
        {
            Server.Debug("GoGetTheXapUnderTest");
            var client = new WebClient
             {
                 AllowReadStreamBuffering = true
             };
            client.OpenReadCompleted += OnXapToTestDownloaded;
            client.OpenReadAsync(StatLightServiceRestApi.GetXapToTest.ToFullUri());
        }

        private void OnXapToTestDownloaded(object sender, OpenReadCompletedEventArgs e)
        {
            Server.Debug("OnXapToTestDownloaded");
            if (e.Error == null)
            {
                var loadedXapData = new LoadedXapData(e.Result);

                // The below setup pretty much the standard setup used in the Microsoft testing framework
                _settings.TestHarness = new UnitTestHarness();
                _settings.TestService = new SilverlightTestService();
                _settings.LogProviders.Add(new UnitTestWebpageLog());
                _settings.LogProviders.Add(new TextFailuresLogProvider());

                // Below is the custom stuff...
                _settings.LogProviders.Add(new ServerHandlingLogProvider());
                foreach (var assembly in loadedXapData.TestAssemblies)
                {
                    _settings.TestAssemblies.Add(assembly);
                }
                _settings.TestHarness.TestHarnessCompleted += CurrentHarness_TestHarnessCompleted;

                _completedTestXapRequest = true;
                DisplayTestHarness();
            }
            else
                LogException(e.Error);
        }

        private void DisplayTestHarness()
        {
            if (_testRunConfigurationDownloadComplete && _completedTestXapRequest)
            {
                SetupUnitTestProvider(_clientTestRunConfiguration.UnitTestProviderType);

                RootVisual = UnitTestSystem.CreateTestPage(_settings);
            }
        }

        private static void CurrentHarness_TestHarnessCompleted(object sender, TestHarnessCompletedEventArgs e)
        {
            Server.SignalTestComplete(e.State);
        }

        private class LoadedXapData
        {
            private readonly Dictionary<string, Assembly> _testAssemblies = new Dictionary<string, Assembly>();
            public IEnumerable<Assembly> TestAssemblies
            {
                get
                {
                    return _testAssemblies.Select(s => s.Value);
                }
            }

            public LoadedXapData(Stream xapStream)
            {
                if (xapStream == null)
                    throw new ArgumentNullException("xapStream");

                var streamResourceInfo = GetResourceStream(
                    new StreamResourceInfo(xapStream, null),
                    new Uri("AppManifest.xaml", UriKind.Relative));

                if (streamResourceInfo == null)
                    throw new Exception("streamResourceInfo is null");

                string appManifestString = new StreamReader(streamResourceInfo.Stream).ReadToEnd();
                if (appManifestString == null)
                    throw new Exception("appManifestString is null");

                XDocument document = XDocument.Parse(appManifestString);
                XElement root = document.Root;
                if (root != null)
                {
                    string entryPoint = root.Attribute("EntryPointAssembly").Value;
                    var partsElement = root.FirstNode as XElement;

                    if (partsElement != null)
                    {
                        var parts = partsElement.Elements()
                            .Select(p => p.Attribute("Source").Value).ToList();
                        Server.Debug("Parts Count = {0}".FormatWith(parts.Count));
                        foreach (var part in parts)
                        {
                            var assemblyPart = new AssemblyPart { Source = part };

                            StreamResourceInfo assemblyStream = GetResourceStream(
                                new StreamResourceInfo(xapStream, "application/binary"),
                                new Uri(assemblyPart.Source, UriKind.Relative));

                            if (assemblyStream == null)
                                throw new Exception(string.Format("Assembly resource missing for [{0}]. (file not found in xap)", assemblyPart.Source));

                            Assembly ass = assemblyPart.Load(assemblyStream.Stream);
                            if (ass != null)
                            {
                                if (!ass.FullName.StartsWith("Microsoft.Silverlight.Testing,"))
                                {
                                    Server.Debug(ass.FullName);
                                    if (!_testAssemblies.ContainsKey(ass.FullName))
                                    {
                                        _testAssemblies.Add(ass.FullName, ass);
                                    }
                                }

                                //if (ass.FullName.Contains("Other"))
                                //    _testAssemblies.Add(ass.FullName, ass);
                                if (part == entryPoint + ".dll")
                                {
                                    //_testAssemblies.Add(ass.FullName, ass);
                                }
                            }
                        }
                    }
                    else
                        throw new InvalidOperationException("The application manifest did not contain any assembly part xml nodes.");

                    if (_testAssemblies.Count == 0)
                        throw new InvalidOperationException("Could not find the entry poing assembly [{0}].".FormatWith(entryPoint));
                }
                else
                    throw new InvalidOperationException("The AppManifest's document root was null.");
            }
        }
        #endregion

        private static void LogException(Exception ex)
        {
            try
            {
                var msg = ex.ToString();
#if DEBUG
                MessageBox.Show(msg);
#endif

                var messageObject = new UnhandledExceptionClientEvent
                {
                    Exception = ex,
                };
                Server.PostMessage(messageObject);
                Server.SignalTestComplete();
            }
            catch (Exception)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#endif
            }
        }
    }
}
