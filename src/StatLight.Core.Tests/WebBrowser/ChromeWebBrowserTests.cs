using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.WebBrowser;

namespace StatLight.Core.Tests.WebBrowser
{
    [TestFixture]
    [Explicit]
    public class ChromeWebBrowserTests : FixtureBase
    {
        readonly Uri _blankUri = new Uri("about:blank");

        private bool _isChromeInstalled;
        private ChromeWebBrowser _browser;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            _browser = GetChrome(true, false);

            if (_browser.IsBrowserInstalled)
            {
                _isChromeInstalled = true;

                ExecuteIfChromeInstalled(_browser.KillAnyRunningBrowserInstances, false);
            }
            _browser.GetAnyRunningBrowserProcesses().Any().ShouldBeFalse("Chrome appears to already be running...?? why?");
        }

        protected override void After_all_tests()
        {
            base.After_all_tests();

            Action z = () =>
            {
                foreach (var process in _browser.GetAnyRunningBrowserProcesses())
                {
                    try
                    {
                        process.Kill();
                    }
#pragma warning disable 168
                    catch (Exception ex)
#pragma warning restore 168
                    {
                    }
                }
            };
            ExecuteIfChromeInstalled(_browser.KillAnyRunningBrowserInstances, false);
        }

        private ChromeWebBrowser GetChrome(bool forceBrowserStart, bool isStartingMultipleInstances)
        {
            return new ChromeWebBrowser(TestLogger, _blankUri, forceBrowserStart, isStartingMultipleInstances);
        }

        [Test]
        public void Should_be_able_to_start_and_stop_a_Chrome_process()
        {
            ExecuteIfChromeInstalled(() =>
            {
                var browser = GetChrome(false, false);
                browser.Start();
                WaitForAMoment();
                AssertChromeIsRunning();
            }, true);
        }

        [Test]
        public void Should_fail_when_not_explicitly_allowed_to_kill_Chrome()
        {
            ExecuteIfChromeInstalled(_browser.KillAnyRunningBrowserInstances, false);

            ExecuteIfChromeInstalled(() =>
            {
                var browser = GetChrome(false, false);
                browser.Start();
                WaitForAMoment();

                // if Chrome is already started - we should not start a new instance (unless we choose to force kill it)
                typeof(StatLightException).ShouldBeThrownBy(browser.Start);
                AssertChromeIsRunning();
            }, true);
        }


        [Test]
        public void Should_kill_and_start_a_new_instance_of_Chrome_if_user_chooses_ForceBrowserStart()
        {
            ExecuteIfChromeInstalled(() =>
            {
                var browser = GetChrome(true, false);
                WaitForAMoment();
                browser.Start();
                WaitForAMoment();
                WaitForAMoment();

                // if Chrome is already started - we should not start a new instance (unless we choose to force kill it)
                browser.Start();
                WaitForAMoment();
                AssertChromeIsRunning();
            }, true);
        }


        [Test]
        public void Should_be_able_to_start_multiple_instances_of_a_browser_when_force_is_off()
        {
            ExecuteIfChromeInstalled(() =>
            {
                var b1 = GetChrome(false, true);
                var b2 = GetChrome(false, true);

                b1.Start();
                b2.Start();

                WaitForAMoment();
                AssertChromeIsRunning();

                b1.Stop();
                b2.Stop();
            }, true);
        }


        [Test]
        public void Should_be_able_to_start_multiple_instances_of_a_browser_when_force_is_on()
        {
            ExecuteIfChromeInstalled(() =>
            {
                var b1 = GetChrome(true, true);
                var b2 = GetChrome(true, true);

                b1.Start();
                b2.Start();

                WaitForAMoment();
                AssertChromeIsRunning();

                b1.Stop();
                b2.Stop();
            }, true);
        }

        private void AssertChromeIsRunning()
        {
            _browser.GetAnyRunningBrowserProcesses().Any().ShouldBeTrue("Number of Chrome processes");
        }

        private void ExecuteIfChromeInstalled(Action action, bool shouldAssertIgnore)
        {
            if (_isChromeInstalled)
                action();
            else
            {
                if (shouldAssertIgnore)
                    Assert.Ignore("Chrome does not appeard to be installed (in the following path {0})".FormatWith(ChromeWebBrowser.ChromePath));
            }
        }

        private static void WaitForAMoment()
        {
            Thread.Sleep(3000);
        }
    }
}