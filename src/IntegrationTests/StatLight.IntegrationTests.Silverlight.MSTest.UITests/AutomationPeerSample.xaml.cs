using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight.MSTest.UITests
{
    // This sample copied from
    // http://msmvps.com/blogs/theproblemsolver/archive/2009/01/26/unit-testing-in-silverlight-part-4-the-ui.aspx

    public partial class AutomationPeerSample : UserControl
    {
        public AutomationPeerSample()
        {
            InitializeComponent();
            Photos = new List<string>();
        }

        public IList<string> Photos { get; set; }

        private void cmdAdd_Click(object sender, RoutedEventArgs e)
        {
            string url = txtPhotoUrl.Text;
            Photos.Add(url);
        }

        private void txtPhotoUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            string url = txtPhotoUrl.Text ?? "";
            var uri = new Uri(url);

            if (string.IsNullOrEmpty(url))
                cmdAdd.IsEnabled = false;
            else if (string.IsNullOrEmpty(uri.Scheme))
                cmdAdd.IsEnabled = false;
            else if (uri.Scheme != "http")
                cmdAdd.IsEnabled = false;
            else
                cmdAdd.IsEnabled = true;
        }
    }

    [TestClass]
    public class AutomationPeerSampleTests : SilverlightTest
    {
        private AutomationPeerSample _automationPeerSample;

        [TestMethod]
        [Asynchronous]
        [Timeout(15000)]
        public void Test_we_can_set_a_value_and_click_a_button()
        {
            _automationPeerSample = new AutomationPeerSample();
            TestPanel.Children.Add(_automationPeerSample);

            var textBoxPeer = new TextBoxAutomationPeer(_automationPeerSample.txtPhotoUrl);
            var valueProvider = (IValueProvider)textBoxPeer;
            var buttonPeer = new ButtonAutomationPeer(_automationPeerSample.cmdAdd);
            var buttonInvoker = (IInvokeProvider)buttonPeer;

            EnqueueCallback(() => valueProvider.SetValue("http://farm4.static.flickr.com/3085/3092376392_dc1aaf9eb6.jpg"));
            EnqueueConditional(buttonPeer.IsEnabled);
            EnqueueCallback(buttonInvoker.Invoke);

            EnqueueTestComplete();
        }
    }
}

