
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Xunit;
using System.Reflection;
using StatLight.Client.Silverlight.UnitTestProviders.Xunit;


namespace Xunit
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class FactAttribute : Attribute
	{
		public FactAttribute()
		{ }

		public string Name { get; protected set; }
		public string Skip { get; set; }
		public int Timeout { get; set; }
	}
}

namespace StatLight.Client.Silverlight.Tests.UnitTestProviders.Xunit
{
	public class MockXunitTestClass
	{
		[Fact]
		public void TestShouldPass()
		{ }

		[Fact(Skip = "TestSkip", Timeout = 1)]
		public void TestShouldBeIgnored()
		{ }

		public static MethodInfo GetPassingTest()
		{
			return new MockXunitTestClass().GetType().GetMethod("TestShouldPass");
		}

		public static MethodInfo GetIgnoreTest()
		{
			return new MockXunitTestClass().GetType().GetMethod("TestShouldBeIgnored");
		}
	}

	[TestClass]
	public class when_validating_a_standard_test_fact_attribute_method : FixtureBase
	{
		ITestMethod testMethod;
		MethodInfo methodInfo;

		protected override void Before_all_tests()
		{
			base.Before_all_tests();

			methodInfo = MockXunitTestClass.GetPassingTest();
			testMethod = new TestMethod(methodInfo);
		}

		[TestMethod]
		public void test_method_name_should_be_correct()
		{
			testMethod.Name.ShouldBeEqualTo(methodInfo.Name);
		}

		[TestMethod]
		public void the_timeout_value_should_be_correct()
		{
			testMethod.Timeout.ShouldBeEqualTo(null);
		}

	}

	[TestClass]
	public class when_validating_an_method_with_a_skip_fact_attribute : FixtureBase
	{
		ITestMethod testMethod;
		MethodInfo methodInfo;

		protected override void Before_all_tests()
		{
			base.Before_all_tests();

			methodInfo = MockXunitTestClass.GetIgnoreTest();
			testMethod = new TestMethod(methodInfo);
		}

		[TestMethod]
		public void test_method_should_be_ignored()
		{
			testMethod.Ignore.ShouldBeTrue();
		}

		[TestMethod]
		public void the_timeout_value_should_be_correct()
		{
			testMethod.Timeout.ShouldBeEqualTo(1);
		}
	}
}
