using NUnit.Framework;
using StatLight.Core.Configuration;

namespace StatLight.Core.Tests.Configuration
{
    public class WindowGeometryTests
    {
        [Test]
        public void Should_determine_minimized_correctly()
        {
            var windowGeometry = new WindowGeometry
            {
                State = BrowserWindowState.Minimized
            };


            windowGeometry.ShouldShowWindow.ShouldBeFalse();
        }
    }
}