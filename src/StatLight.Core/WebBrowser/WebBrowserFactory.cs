using System;
using StatLight.Core.Common;
using StatLight.Core.Configuration;

namespace StatLight.Core.WebBrowser
{
    internal class WebBrowserFactory
    {
        private readonly ILogger _logger;

        public WebBrowserFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IWebBrowser Create(WebBrowserType browserType, Uri pageToHost, bool forceBrowserStart, bool isStartingMultipleInstances, WindowGeometry windowGeometry)
        {
            switch (browserType)
            {
                case WebBrowserType.SelfHosted:
                    return new SelfHostedWebBrowser(_logger, pageToHost, windowGeometry.ShouldShowWindow, windowGeometry);
                case WebBrowserType.Firefox:
                    return new FirefoxWebBrowser(_logger, pageToHost, forceBrowserStart, isStartingMultipleInstances);
                case WebBrowserType.Chrome:
                    return new ChromeWebBrowser(_logger, pageToHost, forceBrowserStart, isStartingMultipleInstances);
            }

            throw new NotImplementedException();
        }
    }
}