using Microsoft.Phone.Controls;
using StatLight.Core.Events.Hosts;

namespace StatLight.Client.Harness.Phone
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Loaded += (sender, e) =>
            {
                var statLightSystem = new NormalStatLightSystem(newRootVisual => this.Content = newRootVisual);
            };
        }
    }
}