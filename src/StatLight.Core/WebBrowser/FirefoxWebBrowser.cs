using System;
using StatLight.Core.Common;
using StatLight.Core.Monitoring;

namespace StatLight.Core.WebBrowser
{
    internal class FirefoxWebBrowser
    {
        
    }

    internal enum WebBrowserType
    {
        SelfHostedWebBrowser
    }

    internal class WebBrowserFactory
    {
        private readonly ILogger _logger;

        public WebBrowserFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IWebBrowser Create(WebBrowserType browserType, Uri pageToHost, bool browserVisible, IDialogMonitorRunner dialogMonitorRunner)
        {
            switch(browserType)
            {
                case WebBrowserType.SelfHostedWebBrowser:
                    return new SelfHostedWebBrowser(_logger, pageToHost, browserVisible, dialogMonitorRunner);
            }

            throw new NotImplementedException();
        }
    }

}