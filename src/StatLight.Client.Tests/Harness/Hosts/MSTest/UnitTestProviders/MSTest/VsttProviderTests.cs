using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Core.Events.Hosts.MSTest.UnitTestProviders.MSTest;

namespace StatLight.Client.Tests.Harness.Hosts.MSTest.UnitTestProviders.MSTest
{
    [TestClass]
    public class VsttProviderTests
    {
        [TestMethod]
        public void Should_support_assembly_initialize()
        {
            var vsttProvider = new VsttProvider();
            vsttProvider.HasCapability(UnitTestProviderCapabilities.AssemblySupportsInitializeMethod).ShouldBeTrue();
        }

        [TestMethod]
        public void should_detect_assemblyInit_methods()
        {
            var unitTestFrameworkAssembly = new UnitTestFrameworkAssembly(null, null, this.GetType().Assembly);
            unitTestFrameworkAssembly.AssemblyInitializeMethod.ShouldNotBeNull();
        }


        [TestMethod]
        public void should_detect_assemblyCleanup_methods()
        {
            var unitTestFrameworkAssembly = new UnitTestFrameworkAssembly(null, null, this.GetType().Assembly);
            unitTestFrameworkAssembly.AssemblyCleanupMethod.ShouldNotBeNull();
        }

        [AssemblyInitialize]
        public void AssemblyInitialize()
        {
            
        }

        [AssemblyCleanup]
        public void AssemblyCleanup()
        {

        }

    }
}