using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Tests;

namespace StatLight.Client.Harness.UnitTestProviders.NUnit
{
    [TestClass]
    [Ignore]
    public class NUnitTestProviderTests : FixtureBase
    {
        IUnitTestProvider provider;
        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            provider = new NUnitTestProvider();
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


