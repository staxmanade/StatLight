
namespace StatLight.Console
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using StatLight.Core.Common;
    using StatLight.Core.UnitTestProviders;
    using StatLight.Core.WebServer.XapHost;

    public class ArgOptions
    {
        private readonly Mono.Options.OptionSet _optionSet;

        private readonly string[] _args;

        public string XapPath { get; private set; }

        public string XmlReportOutputPath { get; private set; }

        public string TagFilters { get; private set; }

        public bool ContinuousIntegrationMode { get; private set; }

        public bool ShowHelp { get; set; }

        //public bool IsValid { get; private set; }

        public bool ShowTestingBrowserHost { get; private set; }

        public bool OutputForTeamCity { get; private set; }

        public bool StartWebServerOnly { get; private set; }
        public List<string> MethodsToTest { get; private set; }
        public UnitTestProviderType UnitTestProviderType { get; private set; }

        public MicrosoftTestingFrameworkVersion? MicrosoftTestingFrameworkVersion { get; private set; }

        public int NumberOfBrowserHosts { get; private set; }

        private ArgOptions()
            : this(new string[] { })
        {
        }

        public ArgOptions(string[] args)
        {
            MethodsToTest = new List<string>();
            _optionSet = GetOptions();
            NumberOfBrowserHosts = 1;
            _args = args;
            //IsValid = true;
            List<string> extra;

            try
            {
                extra = _optionSet.Parse(_args);
            }
            catch (Mono.Options.OptionException e)
            {
                System.Console.Write("Error parsing arguments: ");
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine("Try --help' for more information.");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private Mono.Options.OptionSet GetOptions()
        {
            var msTestVersions = (from MicrosoftTestingFrameworkVersion version in Enum.GetValues(typeof(MicrosoftTestingFrameworkVersion))
                                  select version).ToDictionary(key => key.ToString().ToLower(), value => value);

            return new Mono.Options.OptionSet()
                .Add("x|XapPath", "Path to test xap file.", (v) =>
                {
                    XapPath = v ?? string.Empty;

                    if (!File.Exists(XapPath))
                    {
                        throw new FileNotFoundException("Could not find the file [{0}]".FormatWith(XapPath));
                    }
                }, Mono.Options.OptionValueType.Required)
                .Add("t|TagFilters", "The tag filter expression used to filter executed tests. (See Microsoft.Silverlight.Testing filter format for how to generate complicated filter expressions) Only available with MSTest.", v => TagFilters = v, Mono.Options.OptionValueType.Optional)
                .Add<string>("c|Continuous", "Runs a single test run, and then monitors the xap for build changes and re-runs the tests automatically.", v => ContinuousIntegrationMode = true)
                .Add<string>("b|ShowTestingBrowserHost", "Show the browser that is running the tests - necessary to run UI specific tests (hidden by default)", v => ShowTestingBrowserHost = true)
                .Add("MethodsToTest", "Semicolon seperated list of full method names to execute. EX: --methodsToTest=\"RootNamespace.ChildNamespace.ClassName.MethodUnderTest;RootNamespace.ChildNamespace.ClassName.Method2UnderTest;\"", v =>
                    {
                        v = v ?? string.Empty;
                        MethodsToTest = v.Split(';').Where(w => !string.IsNullOrEmpty(w)).ToList();
                    })
                .Add("o|OverrideTestProvider", "Allows you to override the default test provider of MSTest. Pass in one of the following [{0}]".FormatWith(typeof(UnitTestProviderType).FormatEnumString()), v =>
                    {
                        v = v ?? string.Empty;
                        UnitTestProviderType result;

                        if (v.Is("xunit"))
                            result = UnitTestProviderType.XUnit;
                        else if (v.Is("nunit"))
                            result = UnitTestProviderType.NUnit;
                        else if (v.Is("unitdriven"))
                            result = UnitTestProviderType.UnitDriven;
                        else if (v.Is("mstest"))
                            result = UnitTestProviderType.MSTest;
                        else if (v.Is(string.Empty))
                            result = UnitTestProviderType.MSTest;
                        else
                        {
                            throw new StatLightException("Could not find an OverrideTestProvider defined as [{0}]. Please specify one of the following [{1}]".FormatWith(v, typeof(UnitTestProviderType).FormatEnumString()));
                        }
                        this.UnitTestProviderType = result;
                    })
                .Add("v|Version", "Specify a specific Microsoft.Silverlight.Testing build version. Pass in one of the following [{0}]".FormatWith(typeof(MicrosoftTestingFrameworkVersion).FormatEnumString()), v =>
                    {
                        v = v ?? string.Empty;

                        if (string.IsNullOrEmpty(v))
                            this.MicrosoftTestingFrameworkVersion = null;
                        else
                        {
                            var loweredV = v.ToLower();
                            if (msTestVersions.ContainsKey(loweredV))
                                this.MicrosoftTestingFrameworkVersion = msTestVersions[loweredV];
                            else
                                throw new StatLightException("Could not find a version defined as [{0}]. Please specify one of the following [{1}]".FormatWith(v, typeof(MicrosoftTestingFrameworkVersion).FormatEnumString()));
                        }
                    })
                .Add("r|ReportOutputFile", "File path to write xml report.", v =>
                    {
                        v = v ?? string.Empty;
                        if (Directory.Exists(Path.GetDirectoryName(v)))
                            this.XmlReportOutputPath = v;
                        else
                            throw new DirectoryNotFoundException("Could not find directory in [{0}]".FormatWith(v));
                    })
                .Add("NumberOfBrowserHosts", "Default is 1. Allows you to specify the number of browser windows to spread work across.", v =>
                    {
                        int value;
                        v = v ?? string.Empty;
                        if (int.TryParse(v, out value))
                        {
                            NumberOfBrowserHosts = value;
                        }
                        else
                        {
                            throw new StatLightException("Could not parse parameter [{0}] for numberofbrowsers into an integer.".FormatWith(v));
                        }
                    })
                .Add<string>("teamcity", "Changes the console output to generate the teamcity message spec.", v => OutputForTeamCity = true)
                .Add<string>("webserveronly", "Starts up the StatLight web server without any browser. Useful when needing to attach Visual Studio Debugger to the browser and debug a test.", v => StartWebServerOnly = true)
                .Add<string>("?|help", "displays the help message", v => ShowHelp = true)
                ;
        }

        public static void ShowHelpMessage(TextWriter @out)
        {
            if (@out == null) throw new ArgumentNullException("out");

            @out.WriteLine("Usage: statlight -x=\"PathTo/UnitTests.xap\" [OPTIONS]");
            @out.WriteLine("");
            @out.WriteLine("More documentation: http://www.StatLight.net");
            @out.WriteLine("");
            @out.WriteLine("Options:");

            new ArgOptions()._optionSet.WriteOptionDescriptions(@out);

            @out.WriteLine("");
            @out.WriteLine("Examples:");
            @out.WriteLine("");
            @out.WriteLine(" > statlight -x=\"Tests.xap\"                 .. Run all tests in the xap.");
            @out.WriteLine(" > statlight -x=\"Tests.xap\" -b              .. Display the browser window");
            @out.WriteLine("                                                 while running the tests.");
            @out.WriteLine(" > statlight -x=\"Tests.xap\" -t=Integration  .. Only run the tests that were ");
            @out.WriteLine("                                                 tagged with \"Integration\".");
            @out.WriteLine(" > statlight -x=\"Tests.xap\" -c              .. One time run + continuously ");
            @out.WriteLine("                                                 monitor and re-run tests");
            @out.WriteLine("                                                 after build.");
            @out.WriteLine(" > statlight -x=\"Tests.xap\" --teamcity      .. One time run + communication");
            @out.WriteLine("                                                 needed for running C.I. under ");
            @out.WriteLine("                                                 TeamCity.");
            @out.WriteLine(" > statlight -x=\"Tests.xap\" -o=xunit        .. Run using the xunit provider");
            @out.WriteLine(" > statlight -x=\"Tests.xap\" --webserveronly .. This will start up the statlight ");
            @out.WriteLine("                                                 webserver. Print a url to the ");
            @out.WriteLine("                                                 console and remain running. You");
            @out.WriteLine("                                                 can then take the url and paste");
            @out.WriteLine("                                                 it into any browser to run the ");
            @out.WriteLine("                                                 tests.[Great for needing to ");
            @out.WriteLine("                                                 attaching a debugger] ");
        }

        public static void ShowHelpMessage(TextWriter @out, ArgOptions options)
        {
            ShowHelpMessage(@out);

            /* 
             * DEBUG - maybe some work and could add value (for when a user inputs bad data - display to them what statlight "sees")
             */

            //@out.WriteLine("---------- options ----------");
            //@out.WriteLine("--- Continuous             - {0}".FormatWith(options.ContinuousIntegrationMode));
            //@out.WriteLine("--- XapPath                - '{0}'".FormatWith(options.XapPath));
            //@out.WriteLine("--- TagFilters             - '{0}'".FormatWith(options.TagFilters));
            //@out.WriteLine("--- ShowTestingBrowserHost - {0}".FormatWith(options.ShowTestingBrowserHost));
            //@out.WriteLine("--- TestProvider           - {0}".FormatWith(options.UnitTestProviderType));
            //@out.WriteLine("--- ReportOutputFile       - '{0}'".FormatWith(options.XmlReportOutputPath));
            //@out.WriteLine("--- teamcity               - {0}".FormatWith(options.OutputForTeamCity));
            //@out.WriteLine("--- webserveronly          - {0}".FormatWith(options.StartWebServerOnly));
        }

    }

    static class HelperExtension
    {
        public static bool Is(this string actualValue, string expectedValue)
        {
            if (actualValue == null && expectedValue == null)
                return true;
            if (actualValue == null)
                return false;

            if (actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static string FormatEnumString(this Type enumType)
        {
            if(!enumType.IsEnum)
                throw new ArgumentException("Must be an enum Type={0}".FormatWith(enumType.FullName));

            return string.Join(" | ", Enum.GetNames(enumType));
        }
    }
}