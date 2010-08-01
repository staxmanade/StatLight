
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
        public const string GetTestPageHostXap = "StatLight.Client.Harness.xap";


#if SILVERLIGHT

        private static readonly Client.Model.Messaging.IQueryStringReader QueryStringReader = new Client.Model.Messaging.QueryStringReader();
        private static Uri _postbackUriBase;
        public static Uri ToFullUri(this string value)
        {
            if(_postbackUriBase == null)
            {
                string postbackUrl = QueryStringReader.GetValueOrDefault("StatLight_PostbackUrl", default(string));
                if(postbackUrl == default(string))
                {
                    var src = System.Windows.Application.Current.Host.Source;
                    var urlx = src.Scheme + "://" + src.Host + ":" + src.Port + "/";

                    _postbackUriBase = new Uri(urlx);
                }
                else
                {
                    _postbackUriBase = new Uri(postbackUrl);
                }
            }

            return new Uri(_postbackUriBase + value);
        }
#endif

        public static Uri ToUri(this string value)
        {
            return new Uri(value);
        }

    }
}
