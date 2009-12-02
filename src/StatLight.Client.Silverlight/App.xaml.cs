using System;
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
using StatLight.Client.Silverlight.UnitTestProviders.NUnit;
using StatLight.Client.Silverlight.UnitTestProviders.UnitDriven;
using StatLight.Client.Silverlight.UnitTestProviders.Xunit;
using StatLight.Core.Serialization;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;
using StatLight.Core.Reporting.Messages;
using LogMessageType=StatLight.Core.Reporting.Messages.LogMessageType;
using StatLight.Client.Silverlight.UnitTestProviders.MSTest;

namespace StatLight.Client.Silverlight
{
	public partial class App : Application
	{
		private TestRunConfiguration _testRunConfiguration;
		private bool _testRunConfigurationDownloadComplete;
		private bool _completedTestXapRequest;
		private readonly UnitTestSettings _settings = new UnitTestSettings();

		public App()
		{
			this.Startup += this.Application_Startup;
			this.Exit += this.Application_Exit;
			this.UnhandledException += this.Application_UnhandledException;

			InitializeComponent();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
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
				_testRunConfiguration = e.Result.Deserialize<TestRunConfiguration>();
				TestRunConfiguration.CurrentTestRunConfiguration = _testRunConfiguration;
				_testRunConfigurationDownloadComplete = true;
				_settings.TagExpression = _testRunConfiguration.TagFilter;
				_settings.LogProviders.Add(new WebpageHeaderLogProvider("StatLight filters[{0}]".FormatWith(_testRunConfiguration.TagFilter)));

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
			var client = new WebClient
			 {
				 AllowReadStreamBuffering = true
			 };
			client.OpenReadCompleted += OnXapToTestDownloaded;
			client.OpenReadAsync(StatLightServiceRestApi.GetXapToTest.ToFullUri());
		}

		private void OnXapToTestDownloaded(object sender, OpenReadCompletedEventArgs e)
		{
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
				_settings.TestAssemblies.Add(loadedXapData.TestAssembly);
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
				SetupUnitTestProvider(_testRunConfiguration.UnitTestProviderType);

				this.RootVisual = UnitTestSystem.CreateTestPage(_settings);
			}
		}

		private static void CurrentHarness_TestHarnessCompleted(object sender, TestHarnessCompletedEventArgs e)
		{
			StatLightPostbackManager.SignalTestComplete();
		}

		private class LoadedXapData
		{
			public Assembly TestAssembly { get; private set; }

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
							.Select(p => p.Attribute("Source").Value);

						foreach (var part in parts)
						{
							AssemblyPart assemblyPart = new AssemblyPart();
							assemblyPart.Source = part;

							StreamResourceInfo assemblyStream = GetResourceStream(
								new StreamResourceInfo(xapStream, "application/binary"),
								new Uri(assemblyPart.Source, UriKind.Relative));

							if (assemblyStream == null)
								throw new Exception(string.Format("Assembly resource missing for [{0}]. (file not found in xap)", assemblyPart.Source));

							Assembly ass = assemblyPart.Load(assemblyStream.Stream);

							if (part == entryPoint + ".dll")
							{
								this.TestAssembly = ass;
							}
						}
					}
					else
						throw new InvalidOperationException("The application manifest did not contain any assembly part xml nodes.");

					if (this.TestAssembly == null)
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

				var messageObject = new MobilOtherMessageType
				{
					Message = msg,
					MessageType = LogMessageType.Error
				};
				var serializedString = messageObject.Serialize();
				StatLightPostbackManager.PostMessage(serializedString);
				StatLightPostbackManager.SignalTestComplete();
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
