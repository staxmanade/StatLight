namespace StatLight.Console
{
	using System;
	using System.Diagnostics;
	using System.Reflection;
	using System.ServiceModel;
	using StatLight.Console.Tools;
	using StatLight.Core.Common;
	using StatLight.Core.Reporting;
	using StatLight.Core.Reporting.Providers.Console;
	using StatLight.Core.Reporting.Providers.Xml;
	using StatLight.Core.Runners;
	using StatLight.Core.WebServer;
	using StatLight.Core.WebServer.XapHost;

	class Program
	{
		const int ExitFailed = 1;
		const int ExitSucceeded = 0;

		static void Main(string[] args)
		{
			Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
			Trace.AutoFlush = true;
			Trace.Indent();

			PrintNameVersionAndCopyright();

#if DEBUG
			ILogger logger = new ConsoleLogger(LogChatterLevels.Full);
#else
			ILogger logger = new ConsoleLogger(LogChatterLevels.Error | LogChatterLevels.Warning | LogChatterLevels.Information);
#endif

			ArgOptions options;

			using (var consoleIconSwapper = new ConsoleIconSwapper())
			{
				consoleIconSwapper.ShowConsoleIcon(CoreResources.FavIcon);

				try
				{
					options = new ArgOptions(args);

					string xapPath = options.XapPath;
					bool continuousIntegrationMode = options.ContinuousIntegrationMode;
					bool showTestingBrowserHost = options.ShowTestingBrowserHost;
					bool useTeamCity = options.OutputForTeamCity;
					bool startWebServerOnly = options.StartWebServerOnly;
					MicrosoftTestingFrameworkVersion microsoftTestingFrameworkVersion =
						options.MicrosoftTestingFrameworkVersion;

					if (string.IsNullOrEmpty(xapPath))
						options.ShowHelp = true;

					if (options.ShowHelp)
					{
						ArgOptions.ShowHelpMessage(System.Console.Out);
						return;
					}

					var config = TestRunConfiguration.CreateDefault();
					config.TagFilter = options.TagFilters;
					config.UnitTestProviderType = options.UnitTestProviderType;

					var testReport = RunTestAndGetTestReport(logger, xapPath, continuousIntegrationMode, showTestingBrowserHost, useTeamCity, startWebServerOnly, config, microsoftTestingFrameworkVersion);

					ConsoleTestCompleteMessage.WriteOutCompletionStatement(testReport);

					if (!string.IsNullOrEmpty(options.XmlReportOutputPath))
					{
						WriteErrorToConsole("Writing XML report to: {0}".FormatWith(options.XmlReportOutputPath));
						var xmlReport = new XmlReport(testReport, xapPath);
						xmlReport.WriteXmlReport(options.XmlReportOutputPath);
					}

					if (testReport.FinalResult == RunCompletedState.Failure)
						Environment.ExitCode = ExitFailed;
					else
						Environment.ExitCode = ExitSucceeded;

				}
				catch (AddressAccessDeniedException addressAccessDeniedException)
				{
					Environment.ExitCode = ExitFailed;
					var helpMessage = @"
Cannot run StatLight. The current account does not have the correct privilages.

Exception:
{0}

Try: (the following two steps that should allow StatLight to start a web server on the requested port)
     1. Run cmd.exe as Administrator.
     2. Enter the following command in the command prompt.
          netsh http add urlacl url=http://+:8887/ user=DOMAIN\user
".FormatWith(addressAccessDeniedException.Message);

					WriteErrorToConsole(helpMessage);
				}
				catch (Exception argParseException)
				{
					Environment.ExitCode = ExitFailed;
					WriteErrorToConsole(argParseException.Message);
				}
			}
		}

		private static void WriteErrorToConsole(string errorMessage)
		{
			System.Console.WriteLine("");
			System.Console.WriteLine("");
			ArgOptions.ShowHelpMessage(System.Console.Out);
			System.Console.WriteLine("");
			System.Console.WriteLine("************* Error *************");
			errorMessage.WrapConsoleMessageWithColor(ConsoleColor.Red, true);
			System.Console.WriteLine("*********************************");
		}

		private static TestReport RunTestAndGetTestReport(ILogger logger, string xapPath, bool continuousIntegrationMode, bool showTestingBrowserHost, bool useTeamCity, bool startWebServerOnly, TestRunConfiguration config, MicrosoftTestingFrameworkVersion microsoftTestingFrameworkVersion)
		{
			IRunner runner;

			var serverTestRunConfiguration = new ServerTestRunConfiguration(new XapHostFileLoaderFactory(logger), microsoftTestingFrameworkVersion);

			if (useTeamCity)
			{
				logger.LogChatterLevel = LogChatterLevels.None;
				runner = StatLightRunnerFactory.CreateTeamCityRunner(xapPath, config, serverTestRunConfiguration);
			}
			else if (startWebServerOnly)
			{
				runner = StatLightRunnerFactory.CreateWebServerOnlyRunner(logger, xapPath, config, serverTestRunConfiguration);
			}
			else if (continuousIntegrationMode)
			{
				runner = StatLightRunnerFactory.CreateContinuousTestRunner(logger, xapPath, config, showTestingBrowserHost, serverTestRunConfiguration);
			}
			else
			{
				runner = StatLightRunnerFactory.CreateOnetimeConsoleRunner(logger, xapPath, config, serverTestRunConfiguration, showTestingBrowserHost);
			}

			return runner.Run();
		}


		private static void PrintNameVersionAndCopyright()
		{
			var version = Assembly
				.GetEntryAssembly()
				.GetName()
				.Version;

			Console.WriteLine("");
			Console.WriteLine("StatLight - Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);
			Console.WriteLine("Copyright (C) 2009 Jason Jarrett");
			Console.WriteLine("All Rights Reserved.");
			Console.WriteLine("");
		}

		private static void ShowHelp(Mono.Options.OptionSet p)
		{
			System.Console.WriteLine("Usage: TODO [OPTIONS]");
			System.Console.WriteLine();
			System.Console.WriteLine("Options:");
			p.WriteOptionDescriptions(System.Console.Out);
		}
	}
}
