using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Phone.MSTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                SystemTray.IsVisible = false;

                var testPage = UnitTestSystem.CreateTestPage() as IMobileTestPage;
                BackKeyPress += (x, xe) => xe.Cancel = testPage.NavigateBack();
                ((PhoneApplicationFrame) Application.Current.RootVisual).Content = testPage;

            };

        }
    }


    [TestClass]
    public class BasicTests : SilverlightTest
    {
        [TestMethod]
        public void AlwaysPass()
        {
            Assert.IsTrue(true, "method intended to always pass");
        }

        [TestMethod]
        [Description("This test always fails intentionally")]
        public void AlwaysFail()
        {
            Assert.IsFalse(true, "method intended to always fail");
        }
    }
}