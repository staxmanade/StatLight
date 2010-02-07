
namespace StatLight.Console
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Collections.Generic;
	using StatLight.Core.UnitTestProviders;
	using StatLight.Core.WebServer.XapHost;

	public sealed class OptionException : Exception
	{
		public OptionException(string message)
			: base(message)
		{
		}
	}

	public class ArgOptions
	{
		private Mono.Options.OptionSet optionSet;

		private readonly string[] _args;

		public string XapPath { get; private set; }

		public string XmlReportOutputPath { get; private set; }

		public string TagFilters { get; private set; }

		public string LicenseKey { get; private set; }

		public bool ContinuousIntegrationMode { get; private set; }

		public bool ShowHelp { get; set; }

		//public bool IsValid { get; private set; }

		public bool ShowTestingBrowserHost { get; private set; }

		public bool OutputForTeamCity { get; private set; }

		public bool StartWebServerOnly { get; private set; }

		public UnitTestProviderType UnitTestProviderType { get; private set; }

		public MicrosoftTestingFrameworkVersion MicrosoftTestingFrameworkVersion { get; private set; }

		private ArgOptions()
		{
			optionSet = GetOptions();
		}

		public ArgOptions(string[] args)
		{
			LicenseKey = string.Empty;

			this.optionSet = GetOptions();

			_args = args;
			//IsValid = true;
			List<string> extra;

			try
			{
				extra = optionSet.Parse(_args);
			}
			catch (Mono.Options.OptionException e)
			{
				System.Console.Write("Error parsing arguments: ");
				System.Console.WriteLine(e.Message);
				System.Console.WriteLine("Try --help' for more information.");
			}
		}

		private Mono.Options.OptionSet GetOptions()
		{
			var msTestVersions = (from MicrosoftTestingFrameworkVersion jj in Enum.GetValues(typeof(MicrosoftTestingFrameworkVersion))
					 where jj != MicrosoftTestingFrameworkVersion.Default
					 select jj).ToDictionary(key => key.ToString().ToLower(), value => value);


			return new Mono.Options.OptionSet()
				.Add("x|XapPath", "Path to test xap file.", (v) =>
				{
					XapPath = v ?? string.Empty;
				}, Mono.Options.OptionValueType.Required)
				.Add("t|TagFilters", "The tag filter expression used to filter executed tests. (See Microsoft.Silverlight.Testing filter format for how to generate complicated filter expressions) Only available with MSTest.", v => TagFilters = v, Mono.Options.OptionValueType.Optional)
				.Add<string>("c|Continuous", "Runs a single test run, and then monitors the xap for build changes and re-runs the tests automatically.", v => ContinuousIntegrationMode = true)
				.Add<string>("b|ShowTestingBrowserHost", "Show the browser that is running the tests - necessary to run UI specific tests (hidden by default)", v => ShowTestingBrowserHost = true)
				.Add("o|OverrideTestProvider", "Allows you to override the default test provider of MSTest. Pass in one of the following [{0}]".FormatWith(string.Join(" | ", Enum.GetNames(typeof(UnitTestProviderType)))), v =>
					{
						v = v ?? string.Empty;
						if (string.Equals(v, "xunit", StringComparison.InvariantCultureIgnoreCase))
							this.UnitTestProviderType = UnitTestProviderType.XUnit;
						else if (string.Equals(v, "nunit", StringComparison.InvariantCultureIgnoreCase))
							this.UnitTestProviderType = UnitTestProviderType.NUnit;
						else if (string.Equals(v, "unitdriven", StringComparison.InvariantCultureIgnoreCase))
							this.UnitTestProviderType = UnitTestProviderType.UnitDriven;
						else if (string.Equals(v, "mstest", StringComparison.InvariantCultureIgnoreCase))
							this.UnitTestProviderType = UnitTestProviderType.MSTest;
						else if (string.Equals(v, string.Empty, StringComparison.InvariantCultureIgnoreCase))
							this.UnitTestProviderType = UnitTestProviderType.MSTest;
						else
						{
							throw new Exception("Could not find an OverrideTestProvider defined as [{0}]. Please specify one of the following [{1}]".FormatWith(v, string.Join(" | ", Enum.GetNames(typeof(UnitTestProviderType)))));
						}
					})
				.Add("v|Version", "Specify a specific Microsoft.Silverlight.Testing build version. Pass in one of the following [{0}]".FormatWith(string.Join(" | ", Enum.GetNames(typeof(MicrosoftTestingFrameworkVersion)))), v =>
					{
						v = v ?? string.Empty;

						if (v == string.Empty || string.Equals(v, "Default", StringComparison.CurrentCultureIgnoreCase))
							this.MicrosoftTestingFrameworkVersion = MicrosoftTestingFrameworkVersion.Default;
						else
						{
							var loweredV = v.ToLower();
							if (msTestVersions.ContainsKey(loweredV))
								this.MicrosoftTestingFrameworkVersion = msTestVersions[loweredV];
							else
								throw new Exception("Could not find a version defined as [{0}]. Please specify one of the following [{1}]".FormatWith(v, string.Join(" | ", Enum.GetNames(typeof(MicrosoftTestingFrameworkVersion)))));
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
				.Add<string>("teamcity", "Changes the console output to generate the teamcity message spec.", v => OutputForTeamCity = true)
				.Add<string>("webserveronly", "Starts up the StatLight web server without any browser. Useful when needing to attach Visual Studio Debugger to the browser and debug a test.", v => StartWebServerOnly = true)
				.Add<string>("?|help", "displays the help message", v => ShowHelp = true)
				;
		}

		public static void ShowHelpMessage(TextWriter @out)
		{
            @out.WriteLine("Usage: statlight -x=\"PathTo/UnitTests.xap\" [OPTIONS]");
			@out.WriteLine("");
			@out.WriteLine("More documentation: http://www.StatLight.net");
			@out.WriteLine("");
			@out.WriteLine("Options:");

			new ArgOptions().optionSet.WriteOptionDescriptions(@out);

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
}
