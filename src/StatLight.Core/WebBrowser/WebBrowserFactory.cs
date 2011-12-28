using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.Runners;
using StatLight.Core.WebServer;

namespace StatLight.Core.WebBrowser
{
    public class WebBrowserFactory
    {
        private readonly ILogger _logger;
        private readonly ICurrentStatLightConfiguration _currentStatLightConfiguration;
        private readonly WebServerLocation _webServerLocation;

        public WebBrowserFactory(ILogger logger, ICurrentStatLightConfiguration currentStatLightConfiguration, WebServerLocation webServerLocation)
        {
            _logger = logger;
            _currentStatLightConfiguration = currentStatLightConfiguration;
            _webServerLocation = webServerLocation;
        }

        public IWebBrowser Create(WebBrowserType browserType, Uri pageToHost, bool forceBrowserStart, bool isStartingMultipleInstances, WindowGeometry windowGeometry)
        {
            if (windowGeometry == null) throw new ArgumentNullException("windowGeometry");
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "testPageUrlWithQueryString")]
        public IEnumerable<IWebBrowser> CreateWebBrowsers()
        {
            var statLightConfiguration = _currentStatLightConfiguration.Current;
            WebBrowserType webBrowserType = statLightConfiguration.Client.WebBrowserType;
            string queryString = statLightConfiguration.Server.QueryString;
            bool forceBrowserStart = statLightConfiguration.Server.ForceBrowserStart;
            WindowGeometry windowGeometry = statLightConfiguration.Client.WindowGeometry;
            int numberOfBrowserHosts = statLightConfiguration.Client.NumberOfBrowserHosts;
            var testPageUrlWithQueryString = new Uri(_webServerLocation.TestPageUrl + "?" + queryString);

            Func<int, IWebBrowser> webBrowserFactoryHelper;

            if (statLightConfiguration.Server.IsPhoneRun)
            {
                webBrowserFactoryHelper = instanceId =>
                {
                    Func<byte[]> hostXap = statLightConfiguration.Server.HostXap;
                    return CreatePhone(hostXap);
                };
            }
            else
            {
                webBrowserFactoryHelper = instanceId => Create(webBrowserType, testPageUrlWithQueryString,
                                                               forceBrowserStart, numberOfBrowserHosts > 1,
                                                               windowGeometry);
            }

            _logger.Debug("testPageUrlWithQueryString = " + testPageUrlWithQueryString);

            List<IWebBrowser> webBrowsers = Enumerable
                .Range(1, numberOfBrowserHosts)
                .Select(browserI => webBrowserFactoryHelper(browserI))
                .ToList();
            return webBrowsers;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        public IWebBrowser CreatePhone(Func<byte[]> hostXap)
        {
            var fileName = AppDomain.CurrentDomain.BaseDirectory + @"\StatLight.WindowsPhoneEmulator.dll";
            var assembly = Assembly.LoadFrom(fileName);
            using(var assemblyCatalog = new AssemblyCatalog(assembly))
            using (var compositionContainer = new CompositionContainer(assemblyCatalog))
            {
                var phoneEmulator = compositionContainer.GetExport<IPhoneEmulator>();
                var phoneEmulatorWrapper = phoneEmulator.Value;
                return phoneEmulatorWrapper.Create(logger: _logger, hostXap: hostXap);
            }
        }

    }
}
