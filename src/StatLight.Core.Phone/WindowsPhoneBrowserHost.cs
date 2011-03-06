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
        private RemoteApplication _remoteApplication;

        private readonly Device _wp7Device;

        private readonly Guid _phoneGuid = new Guid("6a158125-6083-43ec-9313-c4cc46a89bc4");
        private readonly Guid _appGuid = new Guid("74c3dd9a-fde3-4059-ae52-ef27fd85762f");

        public WindowsPhoneBrowserHost(ILogger logger, string testXapPath)
        {
            _logger = logger;
            _testXapPath = testXapPath;

            var dsmgrObj = new DatastoreManager(1033);
            Platform wp7Sdk = dsmgrObj.GetPlatforms().First();

            bool useEmulator = true;
            if (useEmulator)
                _wp7Device = wp7Sdk.GetDevices().Single(d => d.Name == "Windows Phone 7 Emulator");
            else
                _wp7Device = wp7Sdk.GetDevices().Single(d => d.Name == "Windows Phone 7 Device");

        }

        public void Start()
        {
            // Get CoreCon WP7 SDK
            _logger.Debug("Connecting to Windows Phone 7 Emulator/Device...");
            _wp7Device.Connect();
            _logger.Debug("Windows Phone 7 Emulator/Device Connected...");

            Uninstall();

            
            _remoteApplication = _wp7Device.InstallApplication(
                _appGuid,
                _phoneGuid,
                "WindowsPhoneApplication1",
                null,
                _testXapPath);

            _logger.Debug("Sample XAP installed to Windows Phone 7 Emulator...");

            // Launch Application 
            _logger.Debug("Launching sample app on Windows Phone 7 Emulator...");
            _remoteApplication.Launch();
        }

        private void Uninstall()
        {
            if (_wp7Device.IsApplicationInstalled(_appGuid))
            {
                _logger.Debug("Uninstalling sample XAP to Windows Phone 7 Emulator/Device...");

                _remoteApplication = _wp7Device.GetApplication(_appGuid);
                _remoteApplication.Uninstall();

                _logger.Debug("Sample XAP Uninstalled from Windows Phone 7 Emulator/Device...");
            }
        }

        public void Stop()
        {
            _remoteApplication.TerminateRunningInstances();

            Uninstall();

            _wp7Device.Disconnect();
            _remoteApplication = null;
        }


        public void Dispose()
        {
            Stop();
        }

        public int? ProcessId
        {
            get { return null; }
        }
    }
}
