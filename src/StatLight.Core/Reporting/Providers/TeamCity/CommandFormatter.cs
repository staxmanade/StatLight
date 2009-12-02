namespace StatLight.Core.Reporting.Providers.TeamCity
{
	using System;
	using System.Collections.Generic;
	using System.Text;
    using StatLight.Core.Common;

	internal static class CommandFormatter
	{
		internal static void FormatException(ExceptionInfo exceptionInfo, StringBuilder builder)
		{
			if (exceptionInfo == null)
			{
				return;
			}

			if (builder.Length > 0)
			{
				builder.AppendLine("--------------------------------");
			}

			builder.AppendFormat("{0}: {1}", FormatValue(exceptionInfo.GetType().ToString()), FormatValue(exceptionInfo.Message));
			builder.AppendLine();

			if (!String.IsNullOrEmpty(exceptionInfo.Source))
			{
				builder.AppendFormat(" Source: {0}", FormatValue(exceptionInfo.Source));
				builder.AppendLine();
			}

			if (!String.IsNullOrEmpty(exceptionInfo.StackTrace))
			{
				builder.AppendFormat(" Stack: {0}", FormatValue(exceptionInfo.StackTrace));
				builder.AppendLine();
			}

			if (exceptionInfo.InnerException != null)
			{
				FormatException(exceptionInfo.InnerException, builder);
			}
		}

		internal static string FormatValue(string value)
		{
			StringBuilder builder = new StringBuilder(value);
			FormatValue(builder);

			return builder.ToString();
		}

		internal static object[] FormatValues(object[] values)
		{
			List<object> result = new List<object>();

			StringBuilder builder = new StringBuilder();

			foreach (object value in values)
			{
				builder.Length = 0;
				builder.Append(value);
				FormatValue(builder);

				result.Add(builder.ToString());
			}

			return result.ToArray();
		}

		internal static void FormatValue(StringBuilder builder)
		{
			Ensure.ArgumentIsNotNull(builder, "builder");

			builder.Replace("|", "||");
			builder.Replace("'", "|'");
			builder.Replace("\n", "|n");
			builder.Replace("\r", "|r");
			builder.Replace("]", "|]");
		}
	}

}
