using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit;
using StatLight.Client.Tests;

namespace StatLight.Client.Harness.UnitTestProviders.NUnit
{
    [TestClass]
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
    }
}


