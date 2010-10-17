using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.WebBrowser;

namespace StatLight.Core.Tests.WebBrowser
{
    [TestFixture]
    public class FirefoxWebBrowserTests : FixtureBase
    {
        readonly Uri _blankUri = new Uri("about:blank");

        private bool _isFirefoxInstalled = false;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            if (FirefoxWebBrowser.IsFirefoxInstalled())
            {
                _isFirefoxInstalled = true;

                ExecuteIfFirefoxInstalled(FirefoxWebBrowser.KillFirefox, false);
            }
        }

        protected override void After_all_tests()
        {
            base.After_all_tests();

            ExecuteIfFirefoxInstalled(FirefoxWebBrowser.KillFirefox, false);
        }

        private FirefoxWebBrowser GetFirefox(bool forceBrowserStart, bool isStartingMultipleInstances)
        {
            return new FirefoxWebBrowser(TestLogger, _blankUri, forceBrowserStart, isStartingMultipleInstances);
        }

        [Test]
        public void Should_be_able_to_start_and_stop_a_firefox_process()
        {
            ExecuteIfFirefoxInstalled(() =>
            {
                var browser = GetFirefox(false, false);
                browser.Start();
                Thread.Sleep(3000);
                AssertFirefoxIsRunning();
            }, true);
        }

        [Test]
        public void Should_fail_when_not_explicitly_allowed_to_kill_firefox()
        {
            ExecuteIfFirefoxInstalled(() =>
            {
                var browser = GetFirefox(false, false);
                browser.Start();
                Thread.Sleep(3000);

                // if firefox is already started - we should not start a new instance (unless we choose to force kill it)
                typeof(StatLightException).ShouldBeThrownBy(browser.Start);
                AssertFirefoxIsRunning();
            }, true);
        }


        [Test]
        public void Should_kill_and_start_a_new_instance_of_firefox_if_user_chooses_ForceBrowserStart()
        {
            ExecuteIfFirefoxInstalled(() =>
            {
                var browser = GetFirefox(true, false);
                browser.Start();
                Thread.Sleep(3000);

                // if firefox is already started - we should not start a new instance (unless we choose to force kill it)
                browser.Start();
                Thread.Sleep(5000);
                AssertFirefoxIsRunning();
            }, true);
        }


        [Test]
        public void Should_be_able_to_start_multiple_instances_of_a_browser_when_force_is_off()
        {
            ExecuteIfFirefoxInstalled(() =>
            {
                var b1 = GetFirefox(false, true);
                var b2 = GetFirefox(false, true);

                b1.Start();
                b2.Start();

                Thread.Sleep(3000);
                AssertFirefoxIsRunning();

                b1.Stop();
                b2.Stop();
            }, true);
        }


        [Test]
        public void Should_be_able_to_start_multiple_instances_of_a_browser_when_force_is_on()
        {
            ExecuteIfFirefoxInstalled(() =>
            {
                var b1 = GetFirefox(true, true);
                var b2 = GetFirefox(true, true);

                b1.Start();
                b2.Start();

                Thread.Sleep(3000);
                AssertFirefoxIsRunning();

                b1.Stop();
                b2.Stop();
            }, true);
        }

        private void AssertFirefoxIsRunning()
        {
            FirefoxWebBrowser.GetFirefoxProcesses().Any().ShouldBeTrue("Number of firefox processes");
        }

        private void ExecuteIfFirefoxInstalled(Action action, bool shouldAssertIgnore)
        {
            if (_isFirefoxInstalled)
                action();
            else
            {
                if(shouldAssertIgnore)
                    Assert.Ignore("Firefox does not appeard to be installed (in the following path {0})".FormatWith(FirefoxWebBrowser.FireFoxPath));
            }
        }

    }
}