
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
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.Xml;
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

            ArgOptions options;

            using (var consoleIconSwapper = new ConsoleIconSwapper())
            {
                consoleIconSwapper.ShowConsoleIcon(CoreResources.FavIcon);

                try
                {
                    options = new ArgOptions(args);

                    if (options.ShowHelp ||
                        !options.XapPaths.Any())
                    {
                        ArgOptions.ShowHelpMessage(Console.Out, options);
                        return;
                    }

                    ILogger logger = GetLogger(options.IsRequestingDebug);


                    var commandLineExecutionEngine = new CommandLineExecutionEngine(logger, options);
                    TestReportCollection testReports = commandLineExecutionEngine.Run();

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
            errorMessage.WrapConsoleMessageWithColor(ConsoleColor.Red, true);
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
            Write("Copyright (C) 2009 Jason Jarrett");
            Write("All Rights Reserved.");
            Write("");
        }

        private static void Write(string msg, params object[] args)
        {
            Console.WriteLine(msg, args);
        }
    }


    public class CommandLineExecutionEngine
    {
        private readonly ILogger _logger;
        private readonly ArgOptions _options;

        public CommandLineExecutionEngine(ILogger logger, ArgOptions args)
            : this(logger, args,
                new StatLightConfigurationFactory(logger),
                runner => runner.Run(),
                new StatLightRunnerFactory())
        {
        }

        internal CommandLineExecutionEngine(ILogger logger, ArgOptions args, IStatLightConfigurationFactory statLightConfigurationFactory, Func<IRunner, TestReport> runnerFunc, IStatLightRunnerFactory statLightRunnerFactory)
        {
            _logger = logger;
            _statLightRunnerFactory = statLightRunnerFactory;
            _options = args;
            _statLightConfigurationFactory = statLightConfigurationFactory;
            _runnerFunc = runnerFunc;
        }


        private readonly IStatLightConfigurationFactory _statLightConfigurationFactory;
        private readonly Func<IRunner, TestReport> _runnerFunc;
        private readonly IStatLightRunnerFactory _statLightRunnerFactory;

        public TestReportCollection Run()
        {

            IEnumerable<string> xapPaths = _options.XapPaths;
            IEnumerable<string> testDlls = _options.Dlls;

            bool continuousIntegrationMode = _options.ContinuousIntegrationMode;
            bool showTestingBrowserHost = _options.ShowTestingBrowserHost;
            bool useTeamCity = _options.OutputForTeamCity;
            bool tfsGenericReport = _options.TFSGenericReport;
            bool useRemoteTestPage = _options.UseRemoteTestPage;
            bool startWebServerOnly = _options.StartWebServerOnly;
            Collection<string> methodsToTest = _options.MethodsToTest;
            string xmlReportOutputPath = _options.XmlReportOutputPath;
            MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion = _options.MicrosoftTestingFrameworkVersion;
            string tagFilters = _options.TagFilters;
            UnitTestProviderType unitTestProviderType = _options.UnitTestProviderType;
            int numberOfBrowserHosts = _options.NumberOfBrowserHosts;
            string queryString = _options.QueryString;
            WebBrowserType webBrowserType = _options.WebBrowserType;
            bool forceBrowserStart = _options.ForceBrowserStart;

            _options.DumpValuesForDebug(_logger);

            RunnerType runnerType = DetermineRunnerType(continuousIntegrationMode, useTeamCity, startWebServerOnly, useRemoteTestPage);

            _logger.Debug("RunnerType = {0}".FormatWith(runnerType));

            var testReports = new TestReportCollection();

            foreach (var xapPath in xapPaths)
            {
                StatLightConfiguration statLightConfiguration = _statLightConfigurationFactory
                    .GetStatLightConfiguration(
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
                        showTestingBrowserHost);

                using (IRunner runner = GetRunner(
                        _logger,
                        runnerType,
                        statLightConfiguration,
                        _statLightRunnerFactory))
                {
                    _logger.Debug("IRunner typeof({0})".FormatWith(runner.GetType().Name));
                    TestReport testReport = _runnerFunc(runner);
                    testReports.Add(testReport);
                }
            }

            if (!string.IsNullOrEmpty(xmlReportOutputPath))
            {
                if (tfsGenericReport)
                {
                    var xmlReport = new Core.Reporting.Providers.TFS.TFS2010.XmlReport(testReports);
                    xmlReport.WriteXmlReport(xmlReportOutputPath);
                }
                else
                {
                    var xmlReport = new XmlReport(testReports);
                    xmlReport.WriteXmlReport(xmlReportOutputPath);
                }

                "*********************************"
                    .WrapConsoleMessageWithColor(ConsoleColor.White, true);

                "Wrote XML report to:{0}{1}"
                    .FormatWith(Environment.NewLine, new FileInfo(xmlReportOutputPath).FullName)
                    .WrapConsoleMessageWithColor(ConsoleColor.Yellow, true);

                "*********************************"
                    .WrapConsoleMessageWithColor(ConsoleColor.White, true);
            }

            return testReports;
        }


        private static IRunner GetRunner(ILogger logger, RunnerType runnerType,
            StatLightConfiguration statLightConfiguration, IStatLightRunnerFactory statLightRunnerFactory)
        {
            switch (runnerType)
            {
                case RunnerType.TeamCity:
                    logger.LogChatterLevel = LogChatterLevels.None;
                    return statLightRunnerFactory.CreateTeamCityRunner(statLightConfiguration);

                case RunnerType.ContinuousTest:
                    return statLightRunnerFactory.CreateContinuousTestRunner(logger, statLightConfiguration);

                case RunnerType.WebServerOnly:
                    return statLightRunnerFactory.CreateWebServerOnlyRunner(logger, statLightConfiguration);

                case RunnerType.RemoteRun:
                    return statLightRunnerFactory.CreateRemotelyHostedRunner(logger, statLightConfiguration);

                default:
                    return statLightRunnerFactory.CreateOnetimeConsoleRunner(logger, statLightConfiguration);
            }
        }

        private static RunnerType DetermineRunnerType(bool continuousIntegrationMode, bool useTeamCity, bool startWebServerOnly, bool isRemoteRun)
        {
            if (useTeamCity)
            {
                return RunnerType.TeamCity;
            }

            if (isRemoteRun)
            {
                return RunnerType.RemoteRun;
            }

            if (startWebServerOnly)
            {
                return RunnerType.WebServerOnly;
            }

            if (continuousIntegrationMode)
            {
                return RunnerType.ContinuousTest;
            }

            return RunnerType.OneTimeConsole;
        }

    }

}
