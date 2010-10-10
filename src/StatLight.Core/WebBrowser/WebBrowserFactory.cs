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

        public IWebBrowser Create(WebBrowserType browserType, Uri pageToHost, bool browserVisible)
        {
            switch (browserType)
            {
                case WebBrowserType.SelfHostedWebBrowser:
                    return new SelfHostedWebBrowser(_logger, pageToHost, browserVisible);
                case WebBrowserType.FireFox:
                    return new FirefoxWebBrowser(_logger, pageToHost);
            }

            throw new NotImplementedException();
        }
    }
}