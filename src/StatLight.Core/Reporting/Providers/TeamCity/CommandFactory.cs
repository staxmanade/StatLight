namespace StatLight.Core.Reporting.Providers.TeamCity
{
	using System.Globalization;

	internal static class CommandFactory
	{
		public static Command TestSuiteStarted(string assemblyName)
		{
			return new Command(CommandType.testSuiteStarted)
				.AddMessage("name", assemblyName);
		}

		public static Command TestSuiteFinished(string assemblyName)
		{
			return new Command(CommandType.testSuiteFinished)
				.AddMessage("name", assemblyName);
		}

		public static Command TestStarted(string assemblyName)
		{
			return new Command(CommandType.testStarted)
				.AddMessage("name", assemblyName)
				.AddMessage("captureStandardOutput", "true");
		}

		public static Command TestFinished(string assemblyName, long duration)
		{
			return new Command(CommandType.testFinished)
				.AddMessage("name", assemblyName)
				.AddMessage("duration", duration.ToString(CultureInfo.InvariantCulture));
		}

		public static Command TestIgnored(string assemblyName, string reason)
		{
			return new Command(CommandType.testIgnored)
				.AddMessage("name", assemblyName)
				.AddMessage("message", reason);
		}

		public static Command TestFailed(string assemblyName, string message, string details)
		{
			return new Command(CommandType.testFailed)
				.AddMessage("name", assemblyName)
				.AddMessage("message", message)
				.AddMessage("details", details);
		}

		public static Command TestFailed(string assemblyName, string message, string details, string type, string expected, string actual)
		{
			return new Command(CommandType.testFailed)
				.AddMessage("type", type)
				.AddMessage("name", assemblyName)
				.AddMessage("message", message)
				.AddMessage("details", details)
				.AddMessage("expected", expected)
				.AddMessage("actual", actual);
		}

		public static Command TestStdErr(string assemblyName, string @out)
		{
			return new Command(CommandType.testStdErr)
				.AddMessage("name", assemblyName)
				.AddMessage("out", @out);
		}

		public static Command TestStdOut(string assemblyName, string @out)
		{
			return new Command(CommandType.testStdOut)
				.AddMessage("name", assemblyName)
				.AddMessage("out", @out);
		}
	}
}
