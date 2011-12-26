using System.Windows.Controls;
using Microsoft.Silverlight.Testing;
using StatLight.Core.Events.Messaging;

namespace StatLight.Core.Events.Hosts.MSTest
{
#if MSTest2009July || MSTest2009October || MSTest2009November
#else
    public class StatLightTestPage : UserControl, ITestPage
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
    }
#endif
}