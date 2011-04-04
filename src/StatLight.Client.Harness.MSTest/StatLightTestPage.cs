using System;
using System.Windows.Controls;
using Microsoft.Silverlight.Testing;
using StatLight.Client.Harness.Messaging;

namespace StatLight.Client.Harness.Hosts.MSTest
{
#if March2010 || April2010 || May2010 || Feb2011 || WINDOWS_PHONE
    public class StatLightTestPage : UserControl, ITestPage, IMobileTestPage
    {
        public static bool IsBrowserHostShown = false;
        public Panel TestPanel
        {
            get
            {
                Server.Trace("Looks like your trying to use the Silverlight Test Framework's TestPanel. To use this you will need to use the -b option in StatLight");
                return new TestPanelImplementation();
            }
        }

        /// <summary>
        /// Initializes the TestPage object.
        /// </summary>
        /// <param name="harness">The test harness instance.</param>
        public StatLightTestPage()
        {
            Loaded += (sender, e)=>
                          {
                              // Here's Jeff will make the TestPanelManager.Instance public
                              //TODO: TestPanelManager.Instance.TestPage = this;
                          };
        }

        private class TestPanelImplementation : Panel
        {
        }

        public bool NavigateBack()
        {
            return false;
        }
    }
#endif
}