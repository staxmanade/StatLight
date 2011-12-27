using Moq;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Tests;
using StatLight.Core.WebBrowser;

namespace StatLight.WindowsPhoneEmulator.Tests
{
    public class WindowsPhoneEmulatorDialogMonitorTests : FixtureBase
    {
        [Test]
        [Explicit("Must startup the windows phone emulator with the gpu dialog first.")]
        public void Invoke_Modal_dialog_clicking()
        {
            string messageFromSlapdown = null;
            ILogger testLogger = new Mock<ILogger>().Object;
            var windowsPhoneEmulatorDialogMonitor = new WindowsPhoneEmulatorDialogMonitor(testLogger);
            windowsPhoneEmulatorDialogMonitor.ExecuteDialogSlapDown(msg =>
            {
                messageFromSlapdown = msg;
            });

            messageFromSlapdown
                .ShouldNotBeNull()
                .ShouldNotBeEmpty();
        }
    }
}