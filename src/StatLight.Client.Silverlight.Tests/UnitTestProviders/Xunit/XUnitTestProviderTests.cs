﻿
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Silverlight.UnitTestProviders.Xunit;

namespace StatLight.Client.Silverlight.Tests.UnitTestProviders.Xunit
{
	[TestClass]
	public class XUnitTestProviderTests : FixtureBase
	{
		IUnitTestProvider provider;
		protected override void Before_each_test()
		{
			base.Before_each_test();

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