using System.Collections.ObjectModel;
using StatLight.Core.WebBrowser;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.Configuration
{
    public interface IStatLightConfigurationFactory
    {
        StatLightConfiguration GetStatLightConfigurationForXap(UnitTestProviderType unitTestProviderType, string xapPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, bool showTestingBrowserHost, bool isPhoneRun);

        StatLightConfiguration GetStatLightConfigurationForDll(UnitTestProviderType unitTestProviderType, string dllPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, bool showTestingBrowserHost, bool isPhoneRun);
    }
}