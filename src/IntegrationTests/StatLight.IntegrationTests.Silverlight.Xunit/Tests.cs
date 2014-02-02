namespace StatLight.IntegrationTests.Silverlight.Xunit
{
    using System;
    using global::Xunit;

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

        [Fact]
        public void ShouldBeSL5()
        {
            Assert.Equal('5', System.Windows.Deployment.Current.RuntimeVersion[0]);
        }

        [Fact(Skip = "Skip it")]
        public void this_should_be_an_Ignored_test()
        {
            throw new Exception("This test should have been ignored.");
        }

    }
}