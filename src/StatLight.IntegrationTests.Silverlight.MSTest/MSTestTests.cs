
using System.Reflection;
using System.Windows;
using Microsoft.Silverlight.Testing;

namespace StatLight.IntegrationTests.Silverlight
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class MSTestTests : SilverlightTest
	{
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
            //Assert.IsTrue(false);
		}

		[TestMethod]
		[Ignore]
		public void this_should_be_an_Ignored_test()
		{
			throw new Exception("This test should have been ignored.");
		}



        [TestMethod]
        [Asynchronous]
        public void should_be_able_to_EncueueCallback_with_asyncronous_test()
        {
            var eventClass = new SomeEventClass();
            bool eventRaised = false;
            eventClass.FiredEvent += (sender, e) => { eventRaised = true; };

            WaitFor(eventClass, "FiredEvent");

            EnqueueCallback(() => Assert.IsTrue(eventRaised));

            eventClass.FireTheEvent();

            EnqueueTestComplete();
        }

		protected void WaitFor<T>(T objectToWaitForItsEvent, string eventName)
		{
			EventInfo eventInfo = objectToWaitForItsEvent.GetType().GetEvent(eventName);

			bool eventRaised = false;

			if (typeof(RoutedEventHandler).IsAssignableFrom(eventInfo.EventHandlerType))
				eventInfo.AddEventHandler(objectToWaitForItsEvent, (RoutedEventHandler)delegate { eventRaised = true; });
			else if (typeof(EventHandler).IsAssignableFrom(eventInfo.EventHandlerType))
				eventInfo.AddEventHandler(objectToWaitForItsEvent, (EventHandler)delegate { eventRaised = true; });

			EnqueueConditional(() => eventRaised);
		}

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
