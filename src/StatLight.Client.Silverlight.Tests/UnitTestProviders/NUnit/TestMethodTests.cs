
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Xunit;
using System.Reflection;
using NUnit.Framework;
using StatLight.Client.Silverlight.UnitTestProviders.Xunit;


namespace NUnit.Framework
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class TestAttribute : Attribute
	{
		private string description;

		/// <summary>
		/// Descriptive text for this test
		/// </summary>
		public string Description
		{
			get { return description; }
			set { description = value; }
		}
	}
}

namespace StatLight.Client.Silverlight.Tests.UnitTestProviders.NUnit
{
	public class MockNUnitTestClass
	{
		[Test]
		public void TestShouldPass()
		{ }

		[Test(Description="Description")]
		public void TestShouldBeIgnored()
		{ }

		public static MethodInfo GetPassingTest()
		{
			return new MockNUnitTestClass().GetType().GetMethod("TestShouldPass");
		}

		public static MethodInfo GetIgnoreTest()
		{
			return new MockNUnitTestClass().GetType().GetMethod("TestShouldBeIgnored");
		}
	}

	[TestClass]
	public class when_validating_a_standard_test_fact_attribute_method : FixtureBase
	{
		ITestMethod testMethod;
		MethodInfo methodInfo;

		protected override void Before_each_test()
		{
			base.Before_each_test();

			methodInfo = MockNUnitTestClass.GetPassingTest();
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

		protected override void Before_each_test()
		{
			base.Before_each_test();

			methodInfo = MockNUnitTestClass.GetIgnoreTest();
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
