using NUnit.Framework;

namespace StatLight.IntegrationTests
{
    public class FixtureBase
    {
        [TestFixtureSetUp]
        public void SetupContext()
        {
            Before_all_tests();
            Because();
        }

        [TestFixtureTearDown]
        public void TearDownContext()
        {
            After_all_tests();
        }

        protected virtual void Before_all_tests()
        {
        }

        protected virtual void Because()
        {
        }

        protected virtual void After_all_tests()
        {
        }
    }
}