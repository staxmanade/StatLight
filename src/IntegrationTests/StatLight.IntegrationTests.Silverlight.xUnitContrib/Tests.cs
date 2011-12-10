using System;
using Xunit;

namespace StatLight.IntegrationTests.Silverlight.xUnitContrib
{
    public class XunitTests
    {
        public class XunitNestedClassTests
        {
            [Fact]
            public void this_should_be_a_passing_test()
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void this_should_be_a_passing_test()
        {
            Assert.True(true);
        }

        [Fact]
        public void this_should_also_be_a_passing_test()
        {
            Assert.True(true);
        }

        [Fact]
        public void this_should_be_a_Failing_test()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Skip it")]
        public void this_should_be_an_Ignored_test()
        {
            throw new Exception("This test should have been ignored.");
        }

    }
}