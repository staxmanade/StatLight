using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.Xunit;
using StatLight.Client.Tests;

namespace StatLight.Client.Harness.UnitTestProviders.Xunit
{
    [TestClass]
    public class XUnitTestProviderTests : FixtureBase
    {
        IUnitTestProvider provider;
        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            provider = new XUnitTestProvider();
        }

        [TestMethod]
        public void provider_should_support_MethodCanIgnore()
        {
            provider
                .HasCapability(UnitTestProviderCapabilities.MethodCanIgnore)
                .ShouldBeTrue();
        }

        [TestMethod]
        public void provider_should_support_MethodCanHaveTimeout()
        {
            provider
                .HasCapability(UnitTestProviderCapabilities.MethodCanHaveTimeout)
                .ShouldBeTrue();
        }
    }
}


