using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;

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
}