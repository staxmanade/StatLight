using System;
using System.Linq;
using Microsoft.SmartDevice.Connectivity;
using StatLight.Core.Common;
using StatLight.Core.WebBrowser;

namespace StatLight.Core.Phone
{
    public class WindowsPhoneBrowserHost : IWebBrowser
    {
        private readonly ILogger _logger;
        private readonly string _testXapPath;
        RemoteApplication _remoteApplication;

        public WindowsPhoneBrowserHost(ILogger logger, string testXapPath)
        {
            _logger = logger;
            _testXapPath = testXapPath;
        }

        public void Dispose()
        {
        }

        public void Start()
        {
            var appGuid = new Guid("6a158125-6083-43ec-9313-c4cc46a89bc4");
            var phoneGuid = new Guid("74c3dd9a-fde3-4059-ae52-ef27fd85762f");

            // Get CoreCon WP7 SDK
            var dsmgrObj = new DatastoreManager(1033);
            Platform WP7SDK = dsmgrObj.GetPlatforms().First();
            bool useEmulator = true;
            Device WP7Device = null;
            if (useEmulator)
                WP7Device = WP7SDK.GetDevices().Single(d => d.Name == "Windows Phone 7 Emulator");
            else
                WP7Device = WP7SDK.GetDevices().Single(d => d.Name == "Windows Phone 7 Device");
            _logger.Debug("Connecting to Windows Phone 7 Emulator/Device...");
            WP7Device.Connect();
            _logger.Debug("Windows Phone 7 Emulator/Device Connected...");

            if (WP7Device.IsApplicationInstalled(phoneGuid))
            {
                _logger.Debug("Uninstalling sample XAP to Windows Phone 7 Emulator/Device...");

                _remoteApplication = WP7Device.GetApplication(phoneGuid);
                _remoteApplication.Uninstall();

                _logger.Debug("Sample XAP Uninstalled from Windows Phone 7 Emulator/Device...");
            }


            //JJ:
            _remoteApplication = WP7Device.InstallApplication(
                appGuid,
                phoneGuid,
                "WindowsPhoneApplication1",
                @"C:\Code\temp\WindowsPhoneAutomation\WindowsPhoneApplication1\Bin\Debug\ApplicationIcon.png",
                _testXapPath);

            _logger.Debug("Sample XAP installed to Windows Phone 7 Emulator...");

            // Launch Application 
            _logger.Debug("Launching sample app on Windows Phone 7 Emulator...");
            _remoteApplication.Launch();
        }

        public void Stop()
        {
        }

        public int? ProcessId
        {
            get { return null; }
        }
    }
}
