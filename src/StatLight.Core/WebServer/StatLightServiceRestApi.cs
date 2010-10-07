
namespace StatLight.Core.WebServer
{
    using System;

    public static class StatLightServiceRestApi
    {
        public const string CrossDomain = "CrossDomain.xml";
        public const string ClientAccessPolicy = "clientaccesspolicy.xml";
        public const string GetXapToTest = "GetXapToTest.xap";
        public const string PostMessage = "PostMessage";
        public const string GetHtmlTestPage = "GetHtmlTestPage";
        public const string GetTestRunConfiguration = "GetTestRunConfiguration";
        public const string GetTestPageHostXap = "StatLight.Client.Harness.xap";

        public const string StatLightResultPostbackUrl = "StatLightResultPostbackUrl";


#if SILVERLIGHT

        internal static Uri PostbackUriBase;
        public static Uri ToFullUri(this string value)
        {
            var fullUri = new Uri(PostbackUriBase + value);
            return fullUri;
        }
#endif

        public static Uri ToUri(this string value)
        {
            return new Uri(value);
        }

    }
}
