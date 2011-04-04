using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.SmartDevice.Connectivity;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    public class WindowsPhoneWebBrowser : IWebBrowser
    {
        private readonly ILogger _logger;
        private readonly Func<byte[]> _xapHost;
        private RemoteApplication _remoteApplication;
        private readonly Device _wp7Device;
        private readonly Guid _phoneGuid = new Guid("6a158125-6083-43ec-9313-c4cc46a89bc4");
        private readonly Guid _appGuid = new Guid("74c3dd9a-fde3-4059-ae52-ef27fd85762f");
        private string _tempFileName;

        public WindowsPhoneWebBrowser(ILogger logger, Func<byte[]> xapHost)
        {
            _logger = logger;
            _xapHost = xapHost;

            var dsmgrObj = new DatastoreManager(1033);
            Platform wp7Sdk = dsmgrObj.GetPlatforms().First();

            //TODO: hook up to real wp7 phone and see if we can get StatLight to run on that?
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


            _tempFileName = Path.GetTempFileName();
            File.WriteAllBytes(_tempFileName, _xapHost());
            _logger.Debug("Loading into emulator: " + _tempFileName);
            Thread.Sleep(2000);

            _remoteApplication = _wp7Device.InstallApplication(
                _appGuid,
                _phoneGuid,
                "WindowsPhoneApplication1",
                null,
                _tempFileName);

            _logger.Debug("Sample XAP installed to Windows Phone 7 Emulator...");

            // Launch Application 
            _logger.Debug("Launching sample app on Windows Phone 7 Emulator...");
            _remoteApplication.Launch();
        }

        private void Uninstall()
        {
            if (_wp7Device.IsConnected())
                if (_wp7Device.IsApplicationInstalled(_appGuid))
                {
                    _logger.Debug("Uninstalling sample XAP to Windows Phone 7 Emulator/Device...");

                    if (_remoteApplication != null)
                    {
                        _remoteApplication = _wp7Device.GetApplication(_appGuid);
                        _remoteApplication.Uninstall();
                    }

                    _logger.Debug("Sample XAP Uninstalled from Windows Phone 7 Emulator/Device...");
                }
        }

        public void Stop()
        {
            if (File.Exists(_tempFileName))
                File.Delete(_tempFileName);

            if (_remoteApplication != null)
                _remoteApplication.TerminateRunningInstances();

            Uninstall();

            _wp7Device.Disconnect();
            _remoteApplication = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int? ProcessId
        {
            get { return null; }
        }
    }
}
