using System;
using System.Collections.Generic;
using Xunit;
namespace XunitLight.Silverlight.Sample
{
	public class OtherExamples
	{
		[Fact]
		public void ThisTestShouldFail()
		{
			Assert.True(false);
		}

		[Fact]
		public void ThisTestShouldPass()
		{
			Assert.True(true);
		}

		public class NestedSampleTestClass
		{
			[Fact]
			public void this_is_a_method_in_a_nested_test_class()
			{
				Assert.True(true);
			}
		}

		[Fact(Skip = "This test should be ignored")]
		public void this_test_should_be_ignored()
		{
			Assert.False(true, "this should not be called");
		}

	}
}