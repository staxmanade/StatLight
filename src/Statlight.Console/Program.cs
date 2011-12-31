

namespace StatLight.Console
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using StatLight.Console.Tools;
    using StatLight.Core;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Properties;
    using StatLight.Core.Reporting;
    using TinyIoC;

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
                consoleIconSwapper.ShowConsoleIcon(Resources.FavIcon);

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

                    var inputOptions = new InputOptions()
                        .SetWindowGeometry(options.WindowGeometry)
                        .SetUseRemoteTestPage(options.UseRemoteTestPage)
                        .SetMethodsToTest(options.MethodsToTest)
                        .SetMicrosoftTestingFrameworkVersion(options.MicrosoftTestingFrameworkVersion)
                        .SetTagFilters(options.TagFilters)
                        .SetUnitTestProviderType(options.UnitTestProviderType)
                        .SetNumberOfBrowserHosts(options.NumberOfBrowserHosts)
                        .SetQueryString(options.QueryString)
                        .SetWebBrowserType(options.WebBrowserType)
                        .SetForceBrowserStart(options.ForceBrowserStart)
                        .SetXapPaths(options.XapPaths)
                        .SetDllPaths(options.Dlls)
                        .SetReportOutputPath(options.XmlReportOutputPath)
                        .SetReportOutputFileType(options.ReportOutputFileType)
                        .SetContinuousIntegrationMode(options.ContinuousIntegrationMode)
                        .SetOutputForTeamCity(options.OutputForTeamCity)
                        .SetStartWebServerOnly(options.StartWebServerOnly)
                        .SetIsRequestingDebug(options.IsRequestingDebug)
                        .SetSettingsOverride(options.OverriddenSettings)
                        .SetIsPhoneRun(options.UsePhoneEmulator)
                        ;

                    TestReportCollection testReports = null;

                    try
                    {
                        TinyIoCContainer ioc = BootStrapper.Initialize(inputOptions);

                        var commandLineExecutionEngine = ioc.Resolve<RunnerExecutionEngine>();

                        testReports = commandLineExecutionEngine.Run();
                    }
                    catch (TinyIoCResolutionException tinyIoCResolutionException)
                    {
                        if (options.IsRequestingDebug)
                        {
                            throw;
                        }

                        throw ResolveNonTinyIocException(tinyIoCResolutionException);
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

        internal static Exception ResolveNonTinyIocException(Exception ex)
        {
            if (ex.InnerException == null)
                return ex;

            if (ex is TinyIoCResolutionException || ex is TargetInvocationException)
                return ResolveNonTinyIocException(ex.InnerException);

            return ex;
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
