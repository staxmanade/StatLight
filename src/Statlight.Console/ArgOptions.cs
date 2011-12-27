namespace StatLight.Console
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Mono.Options;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Reporting;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer.XapHost;

    public class ArgOptions
    {
        private readonly OptionSet _optionSet;

        private readonly string[] _args;

        private readonly IList<string> _xapPaths = new List<string>();
        public IList<string> XapPaths
        {
            get { return _xapPaths; }
        }

        public string TagFilters { get; private set; }

        public bool ContinuousIntegrationMode { get; private set; }

        public bool ShowHelp { get; private set; }

        public bool OutputForTeamCity { get; private set; }

        public bool StartWebServerOnly { get; private set; }
        public Collection<string> MethodsToTest { get; private set; }
        public UnitTestProviderType UnitTestProviderType { get; private set; }

        public MicrosoftTestingFrameworkVersion? MicrosoftTestingFrameworkVersion { get; private set; }
        private WebBrowserType _webBrowserType = WebBrowserType.SelfHosted;
        public WebBrowserType WebBrowserType
        {
            get { return _webBrowserType; }
        }

        public bool IsRequestingDebug { get; private set; }

        public int NumberOfBrowserHosts { get; private set; }

        public bool UseRemoteTestPage { get; private set; }

        private string _queryString;
        public string QueryString
        {
            get { return _queryString ?? String.Empty; }
            private set { _queryString = value; }
        }

        public bool ForceBrowserStart { get; private set; }

        public WindowGeometry WindowGeometry { get; private set; }

        #region XmlReport
        public string XmlReportOutputPath { get; private set; }
        private ReportOutputFileType _reportOutputFileType = ReportOutputFileType.StatLight;
        public ReportOutputFileType ReportOutputFileType
        {
            get { return _reportOutputFileType; }
            set { _reportOutputFileType = value; }
        }
        #endregion

        private readonly IList<string> _dlls = new List<string>();

        public IList<string> Dlls
        {
            get { return _dlls; }
        }

        private readonly IDictionary<string, string> _overriddenSettings = new Dictionary<string, string>();
        public IDictionary<string, string> OverriddenSettings
        {
            get { return _overriddenSettings; }
        }

        public bool UserPhoneEmulator { get; private set; }

        private ArgOptions()
            : this(new string[] { })
        {
        }

        public ArgOptions(string[] args)
        {
            MethodsToTest = new Collection<string>();
            _optionSet = GetOptions();
            NumberOfBrowserHosts = 1;
            _args = args;
            WindowGeometry = new WindowGeometry();
            List<string> extra;

            try
            {
                extra = _optionSet.Parse(_args);
            }
            catch (OptionException e)
            {
                System.Console.Write("Error parsing arguments: ");
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine("Try --help' for more information.");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private OptionSet GetOptions()
        {
            var msTestVersions = (from MicrosoftTestingFrameworkVersion version in Enum.GetValues(typeof(MicrosoftTestingFrameworkVersion))
                                  select version).ToDictionary(key => key.ToString().ToLower(), value => value);

            return new OptionSet()
                .Add("x|XapPath", "Path to test xap file. (Can specify multiple -x={path1} -x={path2})", v => _xapPaths.Add(v ?? string.Empty), OptionValueType.Required)
                .Add("d|Dll", "Assembly to test.", v =>
                    {
                        if (!string.IsNullOrEmpty(v))
                        {
                            _dlls.Add(v);
                        }
                    })
                .Add("t|TagFilters", "The tag filter expression used to filter executed tests. (See Microsoft.Silverlight.Testing filter format for how to generate complicated filter expressions) Only available with MSTest.", v => TagFilters = v, OptionValueType.Optional)
                .Add<string>("c|Continuous", "Runs a single test run, and then monitors the xap for build changes and re-runs the tests automatically.", v => ContinuousIntegrationMode = true)
                .Add("b|BrowserWindow", "Sets the display visibility and/or size of the StatLight browser window. Leave blank to keep browser window hidden. Specify this flag to have the browser window shown with the default height/width. Or using the following pattern [M|m|n][HEIGHTxWIDTH] you can specify the window state [M|Maximized|m|Minimized|N|Normal][{HEIGHT}x{WIDTH}] to be able to specify a specific size. EX: -b:N800x600 a normal window with a width of 800 and height of 600.",
                    v => WindowGeometry = ParseWindowGeometry(v))
                .Add("MethodsToTest", "Semicolon seperated list of full method names to execute. EX: --methodsToTest=\"RootNamespace.ChildNamespace.ClassName.MethodUnderTest;RootNamespace.ChildNamespace.ClassName.Method2UnderTest;\"", v =>
                    {
                        v = v ?? string.Empty;
                        MethodsToTest = v.Split(';').Where(w => !string.IsNullOrEmpty(w)).ToCollection();
                    })
                .Add("o|OverrideTestProvider", "Allows you to override the default test provider of MSTest. Pass in one of the following [{0}]".FormatWith(typeof(UnitTestProviderType).FormatEnumString()), v =>
                    {
                        v = v ?? string.Empty;
                        UnitTestProviderType? result = null;

                        foreach (var typeName in Enum.GetNames(typeof(UnitTestProviderType)))
                        {
                            if (v.Is(typeName))
                            {
                                result = (UnitTestProviderType)Enum.Parse(typeof(UnitTestProviderType), typeName);
                                break;
                            }
                        }

                        if (!result.HasValue)
                        {
                            throw new StatLightException("Could not find an OverrideTestProvider defined as [{0}]. Please specify one of the following [{1}]".FormatWith(v, typeof(UnitTestProviderType).FormatEnumString()));
                        }
                        UnitTestProviderType = result.Value;
                    })
                .Add("v|Version", "(NOTE: YOU SHOULD NOT HAVE TO DO THIS) - Give a specific Microsoft.Silverlight.Testing build version. Pass in one of the following [{0}]. One example this may come in useful is if you have an assembly in your xap named similar to what StatLight is using to automatically detect the version. You can use this to override the 'figured out type'.".FormatWith(typeof(MicrosoftTestingFrameworkVersion).FormatEnumString()), v =>
                    {
                        v = v ?? string.Empty;

                        if (string.IsNullOrEmpty(v))
                            MicrosoftTestingFrameworkVersion = null;
                        else
                        {
                            var loweredV = v.ToLower();
                            if (msTestVersions.ContainsKey(loweredV))
                                MicrosoftTestingFrameworkVersion = msTestVersions[loweredV];
                            else
                                throw new StatLightException("Could not find a version defined as [{0}]. Please specify one of the following [{1}]".FormatWith(v, typeof(MicrosoftTestingFrameworkVersion).FormatEnumString()));
                        }
                    })
                .Add("r|ReportOutputFile", "File path to write xml report.", v =>
                    {
                        v = v ?? string.Empty;
                        if (Directory.Exists(Path.GetDirectoryName(v)))
                            XmlReportOutputPath = v;
                        else
                            throw new DirectoryNotFoundException("Could not find directory in [{0}]".FormatWith(v));
                    })
                .Add("ReportOutputFileType", "Specify the type of report output when using the -r|--ReportOutputFile=[path]. Possible options [{0}]".FormatWith(typeof(ReportOutputFileType).FormatEnumString()), v =>
                    {
                        ReportOutputFileType = ParseEnum<ReportOutputFileType>(v);
                    })
                .Add<string>("UseRemoteTestPage", "You can specify a remotly hosted test page (that contains a StatLight remote runner) by specifying -x=http://localhost/pathToTestPage.html and the --UseRemoteTestPage flag to have StatLight spin up a browser to call the remote page.", v => UseRemoteTestPage = true)
                .Add("WebBrowserType", "If you have other browser installed, you can have StatLight use any of the following web browsers [{0}]".FormatWith(typeof(WebBrowserType).FormatEnumString()), v =>
                    {
                        _webBrowserType = ParseEnum<WebBrowserType>(v);
                    })
                .Add("ForceBrowserStart", "You may need use this option to give permission for StatLight to forcefully close external web browser processes before starting a test run.", v => ForceBrowserStart = true)
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
                .Add<string>("UserPhoneEmulator", "If you have the windows phone SDK installed. Attempt this run with the emulator.", v => UserPhoneEmulator = true)
                .Add("QueryString", "Specify some QueryString that will be appended to the browser test page request. This can be helpful to setup a remote web service and pass in the url, or a port used. You can then access the querystring within silverlight HtmlPage.Document.QueryString[..]", v => QueryString = v ?? String.Empty)
                .Add<string>("teamcity", "Changes the console output to generate the teamcity message spec.", v => OutputForTeamCity = true)
                .Add<string>("MSGenericTestFormat", "This option has been replaced. Use the --ReportOutputFileType:MSGenericTestFormat", v =>
                    {
                        throw new StatLightException("THe --MSGenericTestFormat flag has been removed. You should now be using --ReportOutputFileType:{0}".FormatWith(ReportOutputFileType.MSGenericTest));
                    })
                .Add<string>("webserveronly", "Starts up the StatLight web server without any browser. Useful when needing to attach Visual Studio Debugger to the browser and debug a test.", v => StartWebServerOnly = true)
                .Add<string>("debug", "Prints a verbose spattering of internal logging information. Useful when trying to understand possible issues or when reporting issues back to StatLight.CodePlex.com", v => IsRequestingDebug = true)
                .Add("OverrideSetting", "Specify settings to overried at the command line. [EX: --OverrideSetting:MaxWaitTimeAllowedBeforeCommunicationErrorSent=00:00:20", v =>
                    {
                        if (string.IsNullOrEmpty(v))
                            return;

                        var item = v.Split('=');
                        if (item.Length != 2)
                            throw new StatLightException("Invalid settings format specified");

                        OverriddenSettings.Add(item[0], item[1]);
                    })
                .Add<string>("?|help", "displays the help message", v => ShowHelp = true)
                ;
        }

        private static T ParseEnum<T>(string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (ArgumentException)
            {
                throw new StatLightException("Could not find an WebBrowserType defined as [{0}]. Please specify one of the following [{1}].".FormatWith(value, typeof(T).FormatEnumString()));
            }
        }

        internal static WindowGeometry ParseWindowGeometry(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new WindowGeometry { State = BrowserWindowState.Normal };

            input = input.Trim().Replace("'", string.Empty).Replace("\"", string.Empty);

            var geometry = new WindowGeometry
            {
                State = BrowserWindowState.Normal,
            };

            const string pattern = "(Maximized|Minimized|maximized|minimized|Normal|normal|M|m|N)?(([0-9]+)x([0-9]+))?";
            var matches = Regex.Match(input, pattern);
            if (matches.Groups.Count != 5)
            {
                throw new StatLightException("If specifying the geometry it must be 'WIDTHxHEIGHT'");
            }

            var stateFlag = matches.Groups[1].Value;
            switch (stateFlag)
            {
                case "":
                    break; // leave the default
                case "M":
                case "Maximized":
                case "maximized":
                    geometry.State = BrowserWindowState.Maximized;
                    break;
                case "m":
                case "Minimized":
                case "minimized":
                    geometry.State = BrowserWindowState.Minimized;
                    break;
                case "N":
                case "Normal":
                case "normal":
                    geometry.State = BrowserWindowState.Normal;
                    break;
                default:
                    throw new StatLightException("Unknown browser state flag [{0}]. Try specifying either [M|m|N] for [Maximized|minimized|Normal]".FormatWith(stateFlag));
            }

            var widthString = matches.Groups[3].Value;
            var heightString = matches.Groups[4].Value;
            if (!string.IsNullOrEmpty(widthString) || !string.IsNullOrEmpty(heightString))
            {
                int width;
                int height;
                if (!int.TryParse(widthString, out width) || !int.TryParse(heightString, out height) || width < 1 || height < 1)
                {
                    throw new Exception("Width and height in geometry must be positive integers. (We parsed width[{0}] and height[{1}])".FormatWith(widthString, heightString));
                }

                geometry.Size = new WindowSize(width, height);
            }

            return geometry;
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
            if (!enumType.IsEnum)
                throw new ArgumentException("Must be an enum Type={0}".FormatWith(enumType.FullName));

            return string.Join(" | ", Enum.GetNames(enumType));
        }
    }
}
