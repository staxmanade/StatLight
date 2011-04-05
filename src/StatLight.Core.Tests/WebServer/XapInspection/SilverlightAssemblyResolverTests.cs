using NUnit.Framework;
using StatLight.Core.WebServer.XapInspection;

namespace StatLight.Core.Tests.WebServer.XapInspection
{
    public class SilverlightAssemblyResolverTests
    {
        [Test]
        [Explicit]
        public void Should_find_tools_folder()
        {
            SilverlightAssemblyResolver
                .GetSilverlightToolkitToolFolder()
                .ShouldEqual(@"C:\Program Files (x86)\Microsoft SDKs\Silverlight\v4.0\Toolkit\Apr10\Tools\");
        }
    }
}