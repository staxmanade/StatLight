
namespace StatLight.Core.Tests.Reporting
{
	namespace TestResultAggregatorTests
	{

		using Moq;
		using NUnit.Framework;
		using StatLight.Core.Events;
		using StatLight.Core.Reporting;
		using StatLight.Core.Reporting.Messages;
		using StatLight.Core.Tests.Mocks;

		public class with_a_TestResultAggregator : FixtureBase
		{
			public TestResultAggregator Aggregator { get; private set; }

			public Mock<ITestResultHandler> TestResultHandler { get; private set; }

			protected override void Before_all_tests()
			{
				base.Before_all_tests();

				TestResultHandler = new Mock<ITestResultHandler>();

				Aggregator = new TestResultAggregator(TestResultHandler.Object, base.TestEventAggregator);
			}
		}

		[TestFixture]
		public class when_a_MobileScenerioResult_has_been_published : with_a_TestResultAggregator
		{
			protected override void Before_all_tests()
			{
				base.Before_all_tests();

				RaisePassingTest();
			}

			[Test]
			public void should_add_the_result_to_the_testReport()
			{
				Aggregator.CurrentReport.TotalPassed.ShouldEqual(1);
			}

			[Test]
			public void should_route_it_to_the_testResultHandler()
			{
				TestResultHandler.Verify(s => s.HandleMessage(It.IsAny<MobilScenarioResult>()));
			}

			private void RaisePassingTest()
			{
				base.TestEventAggregator
					.GetEvent<TestResultEvent>()
					.Publish(MessageFactory.CreateResult(TestOutcome.Passed));
			}
		}

		[TestFixture]
		public class when_a_MobilOtherMessageType_has_been_published : with_a_TestResultAggregator
		{
			protected override void Before_all_tests()
			{
				base.Before_all_tests();

				base.TestEventAggregator
					.GetEvent<TestHarnessOtherMessageEvent>()
					.Publish(MessageFactory.CreateOtherMessageType(LogMessageType.Error));
			}

			[Test]
			public void should_route_it_to_the_testResultHandler()
			{
				TestResultHandler.Verify(
					s => s.HandleMessage(It.Is<MobilOtherMessageType>(x => x.MessageType == LogMessageType.Error)));
			}
		}

		[TestFixture]
		public class when_a_BrowserHostCommunicationTimeout_has_been_published : with_a_TestResultAggregator
		{
			protected override void Before_all_tests()
			{
				base.Before_all_tests();

				base.TestEventAggregator
					.GetEvent<BrowserHostCommunicationTimeoutEvent>()
					.Publish();
			}

			[Test]
			public void Should_add_a_failure_message_to_the_TestResultHandler()
			{
				base.TestResultHandler
					.Verify(v => v.HandleMessage(
						It.Is<MobilOtherMessageType>(m => m.MessageType == LogMessageType.Error)));
			}

			[Test]
			public void Should_add_a_failure_message_to_the_current_TestReport()
			{
				Aggregator.CurrentReport.OtherMessages.Count.ShouldEqual(1);
			}
		}
	}
}
