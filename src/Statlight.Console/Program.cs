
namespace StatLight.Console
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using StatLight.Console.Tools;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events;
    using StatLight.Core.Properties;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.Runners;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer.XapHost;

    class Program
    {
        const int ExitFailed = 1;
        const int ExitSucceeded = 0;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.WriteLine(e.ExceptionObject);
            };

            PrintNameVersionAndCopyright();

            using (var consoleIconSwapper = new ConsoleIconSwapper())
            {
                consoleIconSwapper.ShowConsoleIcon(CoreResources.FavIcon);

                try
                {
                    var options = new ArgOptions(args);

                    if (options.ShowHelp)
                    {
                        ArgOptions.ShowHelpMessage(Console.Out, options);
                        return;
                    }

                    if (!options.XapPaths.Any() && !options.Dlls.Any())
                    {
                        throw new StatLightException("No xap or silverlight dll's specified.");
                    }

                    ILogger logger = GetLogger(options.IsRequestingDebug);

                    WindowGeometry windowGeometry = options.WindowGeometry;
                    bool useRemoteTestPage = options.UseRemoteTestPage;
                    Collection<string> methodsToTest = options.MethodsToTest;
                    MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion = options.MicrosoftTestingFrameworkVersion;
                    string tagFilters = options.TagFilters;
                    UnitTestProviderType unitTestProviderType = options.UnitTestProviderType;
                    int numberOfBrowserHosts = options.NumberOfBrowserHosts;
                    string queryString = options.QueryString;
                    WebBrowserType webBrowserType = options.WebBrowserType;
                    bool forceBrowserStart = options.ForceBrowserStart;

                    IEnumerable<string> xapPaths = options.XapPaths;
                    IEnumerable<string> testDlls = options.Dlls;

                    options.DumpValuesForDebug(logger);

                    var statLightConfigurationFactory = new StatLightConfigurationFactory(logger);
                    var configurations = new List<StatLightConfiguration>();

                    foreach (var xapPath in xapPaths)
                    {
                        logger.Debug("Starting configuration for: {0}".FormatWith(xapPath));
                        StatLightConfiguration statLightConfiguration = statLightConfigurationFactory
                            .GetStatLightConfigurationForXap(
                                unitTestProviderType,
                                xapPath,
                                microsoftTestingFrameworkVersion,
                                methodsToTest,
                                tagFilters,
                                numberOfBrowserHosts,
                                useRemoteTestPage,
                                queryString,
                                webBrowserType,
                                forceBrowserStart,
                                windowGeometry);

                        configurations.Add(statLightConfiguration);
                    }

                    foreach (var dllPath in testDlls)
                    {
                        logger.Debug("Starting configuration for: {0}".FormatWith(dllPath));
                        StatLightConfiguration statLightConfiguration = statLightConfigurationFactory
                            .GetStatLightConfigurationForDll(
                                unitTestProviderType,
                                dllPath,
                                microsoftTestingFrameworkVersion,
                                methodsToTest,
                                tagFilters,
                                numberOfBrowserHosts,
                                useRemoteTestPage,
                                queryString,
                                webBrowserType,
                                forceBrowserStart,
                                windowGeometry);

                        configurations.Add(statLightConfiguration);
                    }

                    var eventAggregator = EventAggregatorFactory.Create(logger);
                    var statLightRunnerFactory = new StatLightRunnerFactory(logger, eventAggregator);
                    var commandLineExecutionEngine = new CommandLineExecutionEngine(logger, statLightRunnerFactory, eventAggregator);

                    RunnerType runnerType = commandLineExecutionEngine.GetRunnerType(
                        options.ContinuousIntegrationMode,
                        options.OutputForTeamCity,
                        options.StartWebServerOnly,
                        options.UseRemoteTestPage);

                    DateTime startOfRunTime = DateTime.Now;
                    TestReportCollection testReports = commandLineExecutionEngine.Run(
                        configurations,
                        runnerType,
                        options.XmlReportOutputPath,
                        options.ReportOutputFileType);

                    if(!options.OutputForTeamCity)
                    {
                        ConsoleTestCompleteMessage.PrintFinalTestSummary(testReports, startOfRunTime);
                    }

                    if (testReports.FinalResult == RunCompletedState.Failure)
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

        private static ILogger GetLogger(bool isRequestingDebug)
        {
            ILogger logger;
            if (isRequestingDebug)
            {
                logger = new ConsoleLogger(LogChatterLevels.Full);
            }
            else
            {
#if DEBUG
                logger = new ConsoleLogger(LogChatterLevels.Full);
#else
                logger = new ConsoleLogger(LogChatterLevels.Error | LogChatterLevels.Warning | LogChatterLevels.Information);
#endif
            }
            return logger;
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
            Write("");
            Write("");
            ArgOptions.ShowHelpMessage(Console.Out);
            Write("");
            Write("************* " + beginMsg + " *************");
            errorMessage.WrapConsoleMessageWithColor(Settings.Default.ConsoleColorError, true);
            Write("*********************************");
        }

        private static void PrintNameVersionAndCopyright()
        {
            var version = Assembly
                .GetEntryAssembly()
                .GetName()
                .Version;

            Write("");
            Write("StatLight - Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);
            Write("Copyright (C) 2009-2011 Jason Jarrett");
            Write("All Rights Reserved.");
            Write("");
        }

        private static void Write(string msg, params object[] args)
        {
            Console.WriteLine(msg, args);
        }
    }
}
