using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StatLight.Core.Common;
using StatLight.Core.Reporting;
using StatLight.Core.WebBrowser;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.Configuration
{
    public class InputOptions
    {
        public InputOptions()
        {
            // Setup some "sane" defaults
            WindowGeometry = new WindowGeometry();
            UseRemoteTestPage = false;
            MethodsToTest = new List<string>();
            MicrosoftTestingFrameworkVersion = null;
            TagFilters = String.Empty;
            UnitTestProviderType = UnitTestProviderType.Undefined;
            NumberOfBrowserHosts = 1;
            QueryString = String.Empty;
            WebBrowserType = WebBrowserType.SelfHosted;
            ForceBrowserStart = true;
            XapPaths = new List<string>();
            DllPaths = new List<string>();
            ReportOutputPath = String.Empty;
            ReportOutputFileType = ReportOutputFileType.StatLight;
            ContinuousIntegrationMode = false;
            OutputForTeamCity = false;
            StartWebServerOnly = false;
            IsRequestingDebug = false;
            SettingsOverride = new Dictionary<string, string>();
            IsPhoneRun = false;
        }

        public WindowGeometry WindowGeometry { get; private set; }
        public bool UseRemoteTestPage { get; private set; }
        public IEnumerable<string> MethodsToTest { get; private set; }
        public MicrosoftTestingFrameworkVersion? MicrosoftTestingFrameworkVersion { get; private set; }
        public string TagFilters { get; private set; }
        public UnitTestProviderType UnitTestProviderType { get; private set; }
        public int NumberOfBrowserHosts { get; private set; }
        public string QueryString { get; private set; }
        public WebBrowserType WebBrowserType { get; private set; }
        public bool ForceBrowserStart { get; protected set; }
        public IEnumerable<string> XapPaths { get; private set; }
        public IEnumerable<string> DllPaths { get; private set; }
        public string ReportOutputPath { get; private set; }
        public ReportOutputFileType ReportOutputFileType { get; private set; }
        public bool ContinuousIntegrationMode { get; private set; }
        public bool OutputForTeamCity { get; private set; }
        public bool StartWebServerOnly { get; private set; }
        public bool IsRequestingDebug { get; set; }
        public IDictionary<string, string> SettingsOverride { get; private set; }
        public bool IsPhoneRun { get; set; }

        public InputOptions DumpValuesForDebug(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            var properties = GetType().GetProperties().OrderBy(o => o.Name);
            const string stringFormat = "{0,-35}: {1}";

            logger.Debug("****************** Input options as configured ******************");
            foreach (var propertyInfo in properties)
            {
                var propertyValue = propertyInfo.GetValue(this, new object[0]);
                var enumerablePropertyValue = propertyValue as IEnumerable<string>;
                if (enumerablePropertyValue != null)
                {
                    logger.Debug(stringFormat.FormatWith(propertyInfo.Name, "IEnumerable<string>"));
                    logger.Debug("{0,-35}  {{".FormatWith(""));
                    foreach (var itemValue in enumerablePropertyValue)
                    {
                        logger.Debug("{0,-35}    '{1}'".FormatWith("", itemValue));
                    }
                    logger.Debug("{0,-35}  }}".FormatWith(""));
                }
                else
                {
                    logger.Debug(stringFormat.FormatWith(propertyInfo.Name, propertyValue));
                }
            }
            logger.Debug("*****************************************************************");
            return this;
        }

        private static void AssertFileExists(IEnumerable<string> xapPaths)
        {
            foreach (var xapPath in xapPaths)
            {
                if (!File.Exists(xapPath))
                {
                    throw new FileNotFoundException("File could not be found. [{0}]".FormatWith(xapPath));
                }
            }
        }


        public InputOptions SetWindowGeometry(WindowGeometry windowGeometry)
        {
            if (windowGeometry == null) throw new ArgumentNullException("windowGeometry");
            if (windowGeometry.ShouldShowWindow && !Environment.UserInteractive)
                throw new StatLightException("You cannot use the -b option as your C.I. server's agent process is not running in desktop interactive mode.");

            WindowGeometry = windowGeometry;
            return this;
        }

        public InputOptions SetUseRemoteTestPage(bool useRemoteTestPage)
        {
            UseRemoteTestPage = useRemoteTestPage;
            return this;
        }

        public InputOptions SetMethodsToTest(IEnumerable<string> methodsToTest)
        {
            if (methodsToTest == null) throw new ArgumentNullException("methodsToTest");
            MethodsToTest = methodsToTest.ToList();
            return this;
        }

        public InputOptions SetMicrosoftTestingFrameworkVersion(MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion)
        {
            MicrosoftTestingFrameworkVersion = microsoftTestingFrameworkVersion;
            return this;
        }

        public InputOptions SetTagFilters(string tagFilters)
        {
            TagFilters = tagFilters;
            return this;
        }


        public InputOptions SetUnitTestProviderType(UnitTestProviderType unitTestProviderType)
        {
            UnitTestProviderType = unitTestProviderType;
            return this;
        }

        public InputOptions SetNumberOfBrowserHosts(int numberOfBrowserHosts)
        {
            if (numberOfBrowserHosts <= 0) throw new ArgumentException("value must be greater than 0", "numberOfBrowserHosts");

            NumberOfBrowserHosts = numberOfBrowserHosts;
            return this;
        }

        public InputOptions SetQueryString(string queryString)
        {
            if (queryString == null) throw new ArgumentNullException("queryString");
            QueryString = queryString;
            return this;
        }

        public InputOptions SetWebBrowserType(WebBrowserType webBrowserType)
        {
            WebBrowserType = webBrowserType;
            return this;
        }

        public InputOptions SetForceBrowserStart(bool forceBrowserStart)
        {
            ForceBrowserStart = forceBrowserStart;
            return this;
        }

        public InputOptions SetXapPaths(IEnumerable<string> xapPaths)
        {
            if (xapPaths == null) throw new ArgumentNullException("xapPaths");
            AssertFileExists(xapPaths);
            XapPaths = xapPaths;
            return this;
        }

        public InputOptions SetDllPaths(IEnumerable<string> dllPaths)
        {
            if (dllPaths == null) throw new ArgumentNullException("dllPaths");
            AssertFileExists(dllPaths);
            DllPaths = dllPaths;
            return this;
        }

        public InputOptions SetReportOutputPath(string reportOutputPath)
        {
            ReportOutputPath = reportOutputPath;
            return this;
        }

        public InputOptions SetReportOutputFileType(ReportOutputFileType reportOutputFileType)
        {
            ReportOutputFileType = reportOutputFileType;
            return this;
        }

        public InputOptions SetContinuousIntegrationMode(bool continuousIntegrationMode)
        {
            ContinuousIntegrationMode = continuousIntegrationMode;
            return this;
        }

        public InputOptions SetOutputForTeamCity(bool outputForTeamCity)
        {
            OutputForTeamCity = outputForTeamCity;
            return this;
        }

        public InputOptions SetStartWebServerOnly(bool startWebServerOnly)
        {
            StartWebServerOnly = startWebServerOnly;
            return this;
        }

        public InputOptions SetIsRequestingDebug(bool isRequestingDebug)
        {
            IsRequestingDebug = isRequestingDebug;
            return this;
        }

        public InputOptions SetSettingsOverride(IDictionary<string, string> settingsOverride)
        {
            if (settingsOverride == null) throw new ArgumentNullException("settingsOverride");
            SettingsOverride = settingsOverride;
            return this;
        }

        public InputOptions SetIsPhoneRun(bool isPhoneRun)
        {
            IsPhoneRun = isPhoneRun;
            return this;
        }
        
    }
}
