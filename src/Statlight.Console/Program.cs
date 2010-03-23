using System.IO;

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
    using StatLight.Core.UnitTestProviders;
    using StatLight.Core.WebServer.XapInspection;

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
                    {
                        options.ShowHelp = true;
                        Environment.ExitCode = ExitFailed;
                    }

                    if (!File.Exists(xapPath))
                    {
                        throw new FileNotFoundException("Could not find the file [{0}]".FormatWith(xapPath));
                    }

                    if (options.ShowHelp)
                    {
                        ArgOptions.ShowHelpMessage(System.Console.Out, options);
                        return;
                    }

                    var unitTestProviderType = options.UnitTestProviderType;

                    if (unitTestProviderType == UnitTestProviderType.Undefined)
                    {
                        unitTestProviderType = DetermineUnitTestProviderType(xapPath);
                    }

                    var config = ClientTestRunConfiguration.CreateDefault();
                    config.TagFilter = options.TagFilters;
                    config.UnitTestProviderType = unitTestProviderType;

                    DateTime startOfRun = DateTime.Now; 
                    var testReport = RunTestAndGetTestReport(logger, xapPath, continuousIntegrationMode, showTestingBrowserHost, useTeamCity, startWebServerOnly, config, microsoftTestingFrameworkVersion);

                    if (!string.IsNullOrEmpty(options.XmlReportOutputPath))
                    {
                        var xmlReport = new XmlReport(testReport, xapPath);
                        xmlReport.WriteXmlReport(options.XmlReportOutputPath);

                        "*********************************"
                            .WrapConsoleMessageWithColor(ConsoleColor.White, true);

                        "Wrote XML report to:{0}{1}"
                            .FormatWith(Environment.NewLine, new FileInfo(options.XmlReportOutputPath).FullName)
                            .WrapConsoleMessageWithColor(ConsoleColor.Yellow, true);

                        "*********************************"
                            .WrapConsoleMessageWithColor(ConsoleColor.White, true);
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

                    WriteErrorToConsole(helpMessage, "Error");
                }
                catch (OptionException optionException)
                {
                    HandleKnownError(optionException);
                }
                catch (FileNotFoundException fileNotFoundException)
                {
                    HandleKnownError(fileNotFoundException);
                }
                catch (StatLightException statLightException)
                {
                    HandleKnownError(statLightException);
                }
                    
                catch (Exception exception)
                {
                    HandleUnknownError(exception);
                }
            }
        }

        private static void HandleUnknownError(Exception exception)
        {
            Environment.ExitCode = ExitFailed;
            WriteErrorToConsole(exception.ToString(), "Unexpected Error");
        }

        private static void HandleKnownError(Exception optionException)
        {
            Environment.ExitCode = ExitFailed;
            WriteErrorToConsole(optionException.Message, "Error");
        }

        private static UnitTestProviderType DetermineUnitTestProviderType(string xapPath)
        {
            //TODO: Print message telling the user what the type is - and if they give it
            // we don't have to "reflect" on the xap to determine the test provider type.
            var xapReadItems = new XapReader().GetTestAssembly(xapPath);
            return xapReadItems.UnitTestProvider;
        }

        private static void WriteErrorToConsole(string errorMessage, string beginMsg)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("");
            ArgOptions.ShowHelpMessage(System.Console.Out);
            System.Console.WriteLine("");
            System.Console.WriteLine("************* " + beginMsg + " *************");
            errorMessage.WrapConsoleMessageWithColor(ConsoleColor.Red, true);
            System.Console.WriteLine("*********************************");
        }

        private static TestReport RunTestAndGetTestReport(ILogger logger, string xapPath, bool continuousIntegrationMode, bool showTestingBrowserHost, bool useTeamCity, bool startWebServerOnly, ClientTestRunConfiguration config, MicrosoftTestingFrameworkVersion microsoftTestingFrameworkVersion)
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
    }
}
