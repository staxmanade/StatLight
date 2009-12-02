
namespace StatLight.Core.WebServer
{
	using System;

	public static class StatLightServiceRestApi
	{
		public const string GetXapToTest = "GetXapToTest.xap";
		public const string PostMessage = "PostMessage";
		public const string SignalTestComplete = "SignalTestComplete?TotalMessagesPostedCount={totalMessagesPostedCount}";
		public const string LogNonTestException = "LogNonTestException";
		public const string GetHtmlTestPage = "GetHtmlTestPage";
		public const string GetTestRunConfiguration = "GetTestRunConfiguration";
		public const string GetTestPageHostXap = "StatLight.Client.Silverlight.xap";


#if SILVERLIGHT
		public static Uri ToFullUri(this string value)
		{
			var src = System.Windows.Application.Current.Host.Source;
			var url = src.Scheme + "://" + src.Host + ":" + src.Port + "/";

			return new Uri(url + value);
		}
#endif

		public static Uri ToUri(this string value)
		{
			return new Uri(value);
		}

	}
}
