using System;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    internal class WebBrowserFactory
    {
        private readonly ILogger _logger;

        public WebBrowserFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IWebBrowser Create(WebBrowserType browserType, Uri pageToHost, bool browserVisible, bool forceBrowserStart, bool isStartingMultipleInstances, Func<byte[]> xapHost)
        {
            switch (browserType)
            {
                case WebBrowserType.SelfHosted:
                    return new SelfHostedWebBrowser(_logger, pageToHost, browserVisible);
                case WebBrowserType.Firefox:
                    return new FirefoxWebBrowser(_logger, pageToHost, forceBrowserStart, isStartingMultipleInstances);
                case WebBrowserType.Chrome:
                    return new ChromeWebBrowser(_logger, pageToHost, forceBrowserStart, isStartingMultipleInstances);
                case WebBrowserType.Phone:
                    return new WindowsPhoneWebBrowser(_logger, xapHost);
            }

            throw new NotImplementedException();
        }
    }
}