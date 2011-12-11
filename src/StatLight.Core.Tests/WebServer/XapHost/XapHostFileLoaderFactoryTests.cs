using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.WebServer.XapHost;
using System;

namespace StatLight.Core.Tests.WebServer.XapHost
{
    [TestFixture]
    public class XapHostFileLoaderFactoryTests : FixtureBase
    {
        private XapHostFileLoaderFactory _xapHostFileLoaderFactory;

        private const XapHostType DefaultXapHostType = XapHostType.MSTest2010May;

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
        public void Should_return_the_default_MSTest_xap_host_for_XUnitLight_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.XUnitLight, null).ShouldEqual(DefaultXapHostType);
        }

        // Depricated: [Test]
        // Depricated: public void Should_return_the_default_MSTest_xap_host_for_MSTest_MSTest2008December_test_provider()
        // Depricated: {
        // Depricated:     _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.MSTest2008December)
        // Depricated:         .ShouldEqual(XapHostType.MSTest2008December);
        // Depricated: }
        // Depricated: 
        // Depricated: [Test]
        // Depricated: public void Should_return_the_default_MSTest_xap_host_for_MSTest_MSTest2009March_test_provider()
        // Depricated: {
        // Depricated:     _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.MSTest2009March)
        // Depricated:         .ShouldEqual(XapHostType.MSTest2009March);
        // Depricated: }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_MSTest_MSTest2009July_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.MSTest2009July)
                .ShouldEqual(XapHostType.MSTest2009July);
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_MSTest_MSTest2009October_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.MSTest2009October)
                .ShouldEqual(XapHostType.MSTest2009October);
        }

        [Test]
        public void Should_return_the_default_MSTest_xap_host_for_MSTest_MSTest2009November_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.MSTest, MicrosoftTestingFrameworkVersion.MSTest2009November)
                .ShouldEqual(XapHostType.MSTest2009November);
        }

        [Test]
        public void Should_return_the_default_UnitDriven_xap_host_for_December2009_test_provider()
        {
            _xapHostFileLoaderFactory.MapToXapHostType(UnitTestProviderType.UnitDriven, null)
                .ShouldEqual(XapHostType.UnitDriven2009December);
        }

    }
}