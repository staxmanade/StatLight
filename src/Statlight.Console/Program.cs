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
    using System.Collections.Generic;

    class Program
    {
        const int ExitFailed = 1;
        const int ExitSucceeded = 0;

        static void Main(string[] args)
        {
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
                    List<string> methodsToTest = options.MethodsToTest;
                    string xmlReportOutputPath = options.XmlReportOutputPath;
                    MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion = options.MicrosoftTestingFrameworkVersion;
                    string tagFilters = options.TagFilters;
                    UnitTestProviderType unitTestProviderType = options.UnitTestProviderType;
                    //Debugger.Break();
                    if (options.ShowHelp)
                    {
                        ArgOptions.ShowHelpMessage(System.Console.Out, options);
                        return;
                    }
                    logger.Debug("unitTestProviderType = {0}".FormatWith(unitTestProviderType));

                    var statLightRunnerFactory = new StatLightRunnerFactory();

                    StatLightConfiguration statLightConfiguration =
                        StatLightConfiguration.GetStatLightConfiguration(
                            logger, 
                            unitTestProviderType, 
                            xapPath, microsoftTestingFrameworkVersion, 
                            methodsToTest, 
                            tagFilters);

                    var testReport = 
                        RunTestAndGetTestReport(
                            logger, 
                            continuousIntegrationMode,
                            showTestingBrowserHost, 
                            useTeamCity, 
                            startWebServerOnly, 
                            statLightConfiguration,
                            statLightRunnerFactory);

                    if (!string.IsNullOrEmpty(xmlReportOutputPath))
                    {
                        var xmlReport = new XmlReport(testReport, xapPath);
                        xmlReport.WriteXmlReport(xmlReportOutputPath);

                        "*********************************"
                            .WrapConsoleMessageWithColor(ConsoleColor.White, true);

                        "Wrote XML report to:{0}{1}"
                            .FormatWith(Environment.NewLine, new FileInfo(xmlReportOutputPath).FullName)
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

        private static TestReport RunTestAndGetTestReport(ILogger logger, bool continuousIntegrationMode,
            bool showTestingBrowserHost, bool useTeamCity, bool startWebServerOnly,
            StatLightConfiguration statLightConfiguration, StatLightRunnerFactory statLightRunnerFactory)
        {
            IRunner runner;

            if (useTeamCity)
            {
                logger.LogChatterLevel = LogChatterLevels.None;
                runner = statLightRunnerFactory.CreateTeamCityRunner(statLightConfiguration);
            }
            else if (startWebServerOnly)
            {
                runner = statLightRunnerFactory.CreateWebServerOnlyRunner(logger, statLightConfiguration);
            }
            else if (continuousIntegrationMode)
            {
                runner = statLightRunnerFactory.CreateContinuousTestRunner(logger, statLightConfiguration, showTestingBrowserHost);
            }
            else
            {
                runner = statLightRunnerFactory.CreateOnetimeConsoleRunner(logger, statLightConfiguration, showTestingBrowserHost);
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
