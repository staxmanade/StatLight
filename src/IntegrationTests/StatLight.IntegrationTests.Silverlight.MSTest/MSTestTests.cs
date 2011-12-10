using System.Windows;
using Microsoft.Silverlight.Testing;

namespace StatLight.IntegrationTests.Silverlight
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MSTestTests : SilverlightTest
    {
        private bool _assemblyInitializeWasCalled;

        #region Passing Tests
        [TestClass]
        public class MSTestNestedClassTests
        {
            [TestMethod]
            public void this_should_be_a_passing_test()
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        [Description("Test description on failing test.")]
        [Owner("SomeOwnerString")]
        [TestProperty("tpName", "tpValue")]
        [TestProperty("tpName", "tpValue")]
        public void this_should_be_a_passing_test()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void this_should_also_be_a_passing_test()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        [Asynchronous]
        public void should_be_able_to_EncueueCallback_with_asyncronous_test()
        {
            var eventClass = new SomeEventClass();
            bool eventRaised = false;
            eventClass.FiredEvent += (sender, e) => { eventRaised = true; };

            EnqueueCallback(eventClass.FireTheEvent);
            EnqueueCallback(() => Assert.IsTrue(eventRaised));

            EnqueueTestComplete();
        }

        #endregion

        #region Failing Tests
        [TestMethod]
        [Description("Test description on failing test.")]
        [Owner("SomeOwnerString")]
        [WorkItem(123)]
        [TestProperty("tpName", "tpValue")]
        [TestProperty("tpName", "tpValue")]
        public void this_should_be_a_Failing_test()
        {
            Exception ex1;
            try
            {
                throw new Exception("exception1");
            }
            catch (Exception ex)
            {
                ex1 = ex;
            }
            Exception ex2;
            try
            {
                throw new Exception("exception2", ex1);
            }
            catch (Exception ex)
            {
                ex2 = ex;
            }
            throw ex2;
        }

        [TestMethod]
        [Asynchronous]
        [Timeout(100)]
        public void Should_fail_due_to_async_test_timeout()
        {
            EnqueueCallback(() => System.Threading.Thread.Sleep(1000));

            EnqueueTestComplete();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void this_should_fail_because_it_didnt_raise_an_expected_exception()
        {
        }

#if DEBUG
        [TestMethod]
        public void Should_fail_due_to_a_dialog_assertion()
        {
            System.Diagnostics.Debug.Assert(false, "Should_fail_due_to_a_dialog_assertion - message");
        }
#endif

        #endregion

        #region Ignored test
        [TestMethod]
        [Ignore]
        public void this_should_be_an_Ignored_test()
        {
            throw new Exception("This test should have been ignored.");
        }
        #endregion

#if !SILVERLIGHT3
        [TestMethod]
        public void ShouldBeSL4()
        {
            Assert.AreEqual('4', System.Windows.Deployment.Current.RuntimeVersion[0]);
        }
#else
        [TestMethod]
        public void ShouldBeSL3()
        {
            Assert.AreEqual('3', System.Windows.Deployment.Current.RuntimeVersion[0]);
        }
#endif

        [TestMethod]
        public void Should_fail_due_to_a_message_box_modal_dialog()
        {
            MessageBox.Show("Should_fail_due_to_a_message_box_modal_dialog - message");
        }

        [TestMethod]
        public void Should_have_access_to_the_MSTest_TestContext()
        {
            Assert.IsNotNull(TestContext);
        }

        [TestMethod]
        public void Should_have_executed_assembly_initialize()
        {
            Assert.IsTrue(_assemblyInitializeWasCalled);
        }

        [TestMethod]
        public void Should_be_able_to_write_to_the_TestContext()
        {
            TestContext.WriteLine("Test 0");
            TestContext.WriteLine("Test 1");
            TestContext.WriteLine("Test 2");
            TestContext.WriteLine("Test 3");
            TestContext.WriteLine("Test 4");
            TestContext.WriteLine("Test 5");
            TestContext.WriteLine("Test 6");
            TestContext.WriteLine("Test 7");
            TestContext.WriteLine("Test 8");
            TestContext.WriteLine("Test 9");
            TestContext.WriteLine("Test 10");
            TestContext.WriteLine("Test 11");
            TestContext.WriteLine("Test 12");
            TestContext.WriteLine("Test 13");
            TestContext.WriteLine("Test 14");
            TestContext.WriteLine("Test 15");
            TestContext.WriteLine("Test 16");
            TestContext.WriteLine("Test 17");
            TestContext.WriteLine("Test 18");
            TestContext.WriteLine("Test 19");
            TestContext.WriteLine("Test 20");
            TestContext.WriteLine("Test 21");
            TestContext.WriteLine("Test 22");
            TestContext.WriteLine("Test 23");
            TestContext.WriteLine("Test 24");
            TestContext.WriteLine("Test 25");
            TestContext.WriteLine("Test 26");
            TestContext.WriteLine("Test 27");
            TestContext.WriteLine("Test 28");
            TestContext.WriteLine("Test 29");
            TestContext.WriteLine("Test 30");
            TestContext.WriteLine("Test 31");
            TestContext.WriteLine("Test 32");
            TestContext.WriteLine("Test 33");
            TestContext.WriteLine("Test 34");
            TestContext.WriteLine("Test 35");
            TestContext.WriteLine("Test 36");
            TestContext.WriteLine("Test 37");
            TestContext.WriteLine("Test 38");
            TestContext.WriteLine("Test 39");
            TestContext.WriteLine("Test 40");
            TestContext.WriteLine("Test 41");
            TestContext.WriteLine("Test 42");
            TestContext.WriteLine("Test 43");
            TestContext.WriteLine("Test 44");
            TestContext.WriteLine("Test 45");
            TestContext.WriteLine("Test 46");
            TestContext.WriteLine("Test 47");
            TestContext.WriteLine("Test 48");
            TestContext.WriteLine("Test 49");
            TestContext.WriteLine("Test 50");
            TestContext.WriteLine("Test 51");
            TestContext.WriteLine("Test 52");
            TestContext.WriteLine("Test 53");
            TestContext.WriteLine("Test 54");
            TestContext.WriteLine("Test 55");
            TestContext.WriteLine("Test 56");
            TestContext.WriteLine("Test 57");
            TestContext.WriteLine("Test 58");
            TestContext.WriteLine("Test 59");
            TestContext.WriteLine("Test 60");
            TestContext.WriteLine("Test 61");
            TestContext.WriteLine("Test 62");
            TestContext.WriteLine("Test 63");
            TestContext.WriteLine("Test 64");
            TestContext.WriteLine("Test 65");
            TestContext.WriteLine("Test 66");
            TestContext.WriteLine("Test 67");
            TestContext.WriteLine("Test 68");
            TestContext.WriteLine("Test 69");
            TestContext.WriteLine("Test 70");
            TestContext.WriteLine("Test 71");
            TestContext.WriteLine("Test 72");
            TestContext.WriteLine("Test 73");
            TestContext.WriteLine("Test 74");
            TestContext.WriteLine("Test 75");
            TestContext.WriteLine("Test 76");
            TestContext.WriteLine("Test 77");
            TestContext.WriteLine("Test 78");
            TestContext.WriteLine("Test 79");
            TestContext.WriteLine("Test 80");
            TestContext.WriteLine("Test 81");
            TestContext.WriteLine("Test 82");
            TestContext.WriteLine("Test 83");
            TestContext.WriteLine("Test 84");
            TestContext.WriteLine("Test 85");
            TestContext.WriteLine("Test 86");
            TestContext.WriteLine("Test 87");
            TestContext.WriteLine("Test 88");
            TestContext.WriteLine("Test 89");
            TestContext.WriteLine("Test 90");
            TestContext.WriteLine("Test 91");
            TestContext.WriteLine("Test 92");
            TestContext.WriteLine("Test 93");
            TestContext.WriteLine("Test 94");
            TestContext.WriteLine("Test 95");
            TestContext.WriteLine("Test 96");
            TestContext.WriteLine("Test 97");
            TestContext.WriteLine("Test 98");
            TestContext.WriteLine("Test 99");
            TestContext.WriteLine("Test 100");
        }

        [AssemblyInitialize]
        public void AssemblyInitialize_Method()
        {
            _assemblyInitializeWasCalled = true;
        }

        public TestContext TestContext { get; set; }
    }

    public class SomeEventClass
    {
        public event EventHandler FiredEvent = delegate { };

        public void FireTheEvent()
        {
            FiredEvent(this, EventArgs.Empty);
        }
    }
}
