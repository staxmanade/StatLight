using UnitDriven;
namespace StatLight.Client.Tests.Harness.Hosts.UnitDrivenTestHost
{
    public class MethodTesterTests : FixtureBase
    {
        private MethodTester _methodTester;
        private MethodTesterMonitor _methodTesterMonitor;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            var classUnderTest = new ClassUnderTest();
			var testType = classUnderTest.GetType();
            var methodUnderTest = testType.GetMethod("PassingMethodUnderTest");

            _methodTester = new MethodTester(methodUnderTest, testType);
            _methodTesterMonitor = new MethodTesterMonitor(_methodTester);
            _methodTester.Initialize();
        }
    }

    public class MethodTesterMonitor
    {
        private readonly MethodTester _methodTester;

        public MethodTesterMonitor(MethodTester methodTester)
        {
            _methodTester = methodTester;
        }
    }

    public class ClassUnderTest
    {
        public void PassingMethodUnderTest()
        {
        }
    }
}