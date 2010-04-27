using NUnit.Framework;
using StatLight.Core.WebServer.XapHost;
using StatLight.Core.UnitTestProviders;
using System;

namespace StatLight.Core.Tests.WebServer.XapHost
{
    [TestFixture]
    public class XapHostFileLoaderFactoryTests : FixtureBase
    {
        private XapHostFileLoaderFactory _xapHostFileLoaderFactory;

        private const XapHostType DefaultXapHostType = XapHostType.MSTestApril2010;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();
            _xapHostFileLoaderFactory = new XapHostFileLoaderFactory(base.TestLogger);
        }

        [Test]
        public void Should_fail_for_non_mapped_types()
        {
            typeof(NotSupportedException)
                .ShouldBeThrownBy(() =>
                    _xapHostFileLoaderFactory.MapToXapHostType(
                        (UnitTestProviderType)int.MaxValue,
                        (MicrosoftTestingFrameworkVersion)int.MaxValue));
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_NUnit_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.NUnit, null).ShouldEqual(DefaultXapHostType);
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_XUnit_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.XUnit, null).ShouldEqual(DefaultXapHostType);
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_MSTest_December2008_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.December2008)
                .ShouldEqual(XapHostType.MSTestDecember2008);
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_MSTest_March2009_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.March2009)
                .ShouldEqual(XapHostType.MSTestMarch2009);
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_MSTest_July2009_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.July2009)
                .ShouldEqual(XapHostType.MSTestJuly2009);
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_MSTest_October2009_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.October2009)
                .ShouldEqual(XapHostType.MSTestOctober2009);
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_MSTest_November2009_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.November2009)
                .ShouldEqual(XapHostType.MSTestNovember2009);
        }

        [Test]
        public void Should_return_the_default_UnitDriven_xap_host_for_December2009_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.UnitDriven, null)
                .ShouldEqual(XapHostType.UnitDrivenDecember2009);
        }

    }
}