namespace StatLight.Core.Reporting.Providers.TeamCity
{
	using System;

	internal class ExceptionInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public string Type { get; set; }
		public string Message { get; set; }
		public string Source { get; set; }
		public string StackTrace { get; set; }
		public ExceptionInfo InnerException { get; set; }

		public static ExceptionInfo FromException(Exception exception)
		{
			if (exception == null)
			{
				return null;
			}

			ExceptionInfo result = new ExceptionInfo
			{
				Type = exception.GetType().FullName,
				Message = exception.Message,
				Source = exception.Source,
				StackTrace = exception.StackTrace
			};

			if (exception.InnerException != null)
			{
				result.InnerException = FromException(exception.InnerException);
			}

			return result;
		}
	}

}
