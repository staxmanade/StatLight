using StatLight.Core.Monitoring;

namespace StatLight.Core.Tests.WebBrowser
{
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using Moq;
	using NUnit.Framework;
	using StatLight.Core.Timing;
	using StatLight.Core.WebServer;
	using StatLight.Core.Events;

	//[TestFixture]
	//[Ignore("TODO")]
	//public class when_the_DebugAssertMonitor_is_monitoring_for_the_assertion_window : FixtureBase
	//{
	////    private DialogMonitorRunner debugAssertMonitor;
	////    private Mock<IStatLightService> mockStatLightService;
	////    private Mock<ITimer> mockDialogPingingTimer;
	////    private DateTime currentTime;

	////    bool timerEnabled = false;

	////    protected override void Before_all_tests()
	////    {
	////        base.Before_all_tests();

	////        currentTime = new DateTime(2009, 1, 1);
	////        mockStatLightService = new Mock<IStatLightService>();
	////        mockDialogPingingTimer = new Mock<ITimer>();

	////        mockDialogPingingTimer.SetupProperty(s => s.Enabled, timerEnabled);

	////        debugAssertMonitor = new DialogMonitorRunner(TestLogger, base.TestEventAggregator, mockDialogPingingTimer.Object);
	////    }

	////    [Test]
	////    public void when_the_DialogPingingTimer_should_have_been_enabled()
	////    {
	////        mockDialogPingingTimer.Object.Enabled.ShouldBeTrue();
	////    }

	////    [Test]
	////    [Explicit("Doesn't work in TeamCity Continuous Integration.")]
	////    public void when_the_DialogPingingTimer_fires_it_should_kick_off_a_slapdown()
	////    {
	////        bool slapDownStarted = false;
	////        base.TestEventAggregator
	////            .GetEvent<DialogAssertionEvent>()
	////            .Subscribe((result) => { slapDownStarted = true; });

	////        Thread modialDialogThread = new Thread(() =>
	////            {
	////                MessageBox.Show("", "Assertion Failed", MessageBoxButtons.OKCancel);
	////            }
	////        );
	////        modialDialogThread.Start();

	////        Thread.Sleep(200);
	////        mockDialogPingingTimer.Raise((t) => t.Elapsed += null, new TimerWrapperElapsedEventArgs(currentTime));

	////        slapDownStarted.ShouldBeTrue();
	////    }

	////    [Test]
	////    public void when_dialog_timer_fired_and_no_dialog_found_the_slapDown_event_should_not_be_raised()
	////    {
	////        bool slapDownStarted = false;
	////        base.TestEventAggregator
	////            .GetEvent<DialogAssertionEvent>()
	////            .Subscribe((resutl) => { slapDownStarted = true; });

	////        mockDialogPingingTimer.Raise((t) => t.Elapsed += null, new TimerWrapperElapsedEventArgs(currentTime));

	////        slapDownStarted.ShouldBeFalse();
	////    }
	//}
}
