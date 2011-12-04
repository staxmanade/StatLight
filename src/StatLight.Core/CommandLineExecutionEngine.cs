using System.Diagnostics;
using System.Linq;
using StatLight.Core.Reporting.Providers.Console;

namespace StatLight.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using StatLight.Core.Events;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events;
    using StatLight.Core.Properties;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers;
    using StatLight.Core.Reporting.Providers.MSTestTRX;
    using StatLight.Core.Runners;

    public class CommandLineExecutionEngine
    {
        private readonly ILogger _logger;
        private readonly Func<IRunner, TestReport> _runnerFunc;
        private readonly IStatLightRunnerFactory _statLightRunnerFactory;
        private readonly IEventPublisher _eventPublisher;

        public CommandLineExecutionEngine(ILogger logger, IStatLightRunnerFactory runnerFactory, IEventPublisher eventPublisher)
            : this(logger, runner => runner.Run(), runnerFactory, eventPublisher)
        {
        }

        internal CommandLineExecutionEngine(ILogger logger, Func<IRunner, TestReport> runnerFunc, IStatLightRunnerFactory statLightRunnerFactory, IEventPublisher eventPublisher)
        {
            _logger = logger;
            _statLightRunnerFactory = statLightRunnerFactory;
            _eventPublisher = eventPublisher;
            _runnerFunc = runnerFunc;
        }

        public RunnerType GetRunnerType(bool continuousIntegrationMode, bool useTeamCity, bool startWebServerOnly, bool useRemoteTestPage)
        {
            RunnerType runnerType = DetermineRunnerType(continuousIntegrationMode, useTeamCity, startWebServerOnly, useRemoteTestPage);

            _logger.Debug("RunnerType = {0}".FormatWith(runnerType));

            return runnerType;
        }

        public TestReportCollection Run(IEnumerable<StatLightConfiguration> configurations, RunnerType runnerType, string xmlReportOutputPath, ReportOutputFileType reportOutputFileType)
        {
            TestReportCollection testReports = GetTestReports(configurations, runnerType);

            WriteXmlReport(testReports, xmlReportOutputPath, reportOutputFileType);

            return testReports;
        }

        private TestReportCollection GetTestReports(IEnumerable<StatLightConfiguration> statLightConfigurations, RunnerType runnerType)
        {
            var testReports = new TestReportCollection();

            if (runnerType == RunnerType.ContinuousTest)
            {
                IRunner continuousTestRunner = _statLightRunnerFactory.CreateContinuousTestRunner(statLightConfigurations);
                continuousTestRunner.Run();
            }
            else
            {

                Stopwatch totalTime = Stopwatch.StartNew();

                foreach(var statLightConfiguration in statLightConfigurations)
                {
                    using (IRunner runner = GetRunner(
                        _logger,
                        runnerType,
                        statLightConfiguration,
                        _statLightRunnerFactory))
                    {
                        _logger.Debug("IRunner typeof({0})".FormatWith(runner.GetType().Name));
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        TestReport testReport = _runnerFunc(runner);
                        stopwatch.Stop();
                        testReports.Add(testReport);
                        _eventPublisher.SendMessage(new TestReportGeneratedServerEvent(testReport, stopwatch.Elapsed, statLightConfigurations.Count() > 1));
                    }
                }

                totalTime.Stop();

                _eventPublisher.SendMessage(new TestReportCollectionGeneratedServerEvent(testReports, totalTime.Elapsed));
            }

            return testReports;
        }

        private static void WriteXmlReport(TestReportCollection testReports, string xmlReportOutputPath, ReportOutputFileType reportOutputFileType)
        {
            if (!string.IsNullOrEmpty(xmlReportOutputPath))
            {
                IXmlReport xmlReport;
                switch (reportOutputFileType)
                {
                    case ReportOutputFileType.MSGenericTest:
                        xmlReport = new Core.Reporting.Providers.TFS.TFS2010.MSGenericTestXmlReport(testReports);
                        break;
                    case ReportOutputFileType.StatLight:
                        xmlReport = new Core.Reporting.Providers.Xml.XmlReport(testReports);
                        break;
                    case ReportOutputFileType.NUnit:
                        xmlReport = new Core.Reporting.Providers.NUnit.NUnitXmlReport(testReports);
                        break;
                    case ReportOutputFileType.TRX:
                        xmlReport = new TRXReport(testReports);
                        break;
                    default:
                        throw new StatLightException("Unknown ReportOutputFileType chosen Name=[{0}], Value=[{1}]".FormatWith(reportOutputFileType.ToString(), (int)reportOutputFileType));
                }

                xmlReport.WriteXmlReport(xmlReportOutputPath);

                "*********************************"
                    .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);

                "Wrote XML report to:{0}{1}"
                    .FormatWith(Environment.NewLine, new FileInfo(xmlReportOutputPath).FullName)
                    .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorWarning, true);

                "*********************************"
                    .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);
            }
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
                    throw new NotSupportedException();

                case RunnerType.WebServerOnly:
                    return statLightRunnerFactory.CreateWebServerOnlyRunner(statLightConfiguration);

                case RunnerType.RemoteRun:
                    return statLightRunnerFactory.CreateRemotelyHostedRunner(statLightConfiguration);

                default:
                    return statLightRunnerFactory.CreateOnetimeConsoleRunner(statLightConfiguration);
            }
        }

        public static RunnerType DetermineRunnerType(bool continuousIntegrationMode, bool useTeamCity, bool startWebServerOnly, bool isRemoteRun)
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