using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Model.DTO;
using StatLight.Client.Model.Events;
using StatLight.Core.Serialization;

namespace StatLight.Client.Silverlight.Tests.Messaging
{
	[TestClass]
	public class When_creating_an_ExceptionInfo : FixtureBase
	{
		private ExceptionInfo _exceptionInfo;
		private Exception _exception;

		protected override void Before_all_tests()
		{
			base.Before_all_tests();

			try
			{
				Exception innerEx;
				try
				{
					throw new Exception("Inner ex!");
				}
				catch (Exception i)
				{
					innerEx = i;
				}

				throw new Exception("Test message!", innerEx);
			}
			catch (Exception e)
			{
				_exception = e;
				_exceptionInfo = new ExceptionInfo(e);
			}
		}

		[TestMethod]
		public void Should_be_able_to_create_the_ExceptionInfo()
		{
			_exceptionInfo.ShouldNotBeNull();
		}

		[TestMethod]
		public void Should_set_the_Message_correctly()
		{
			_exceptionInfo.Message.ShouldBeEqualTo(_exception.Message);
		}

		[TestMethod]
		public void Should_set_the_StackTrace()
		{
			_exceptionInfo.StackTrace.ShouldBeEqualTo(_exception.StackTrace);
		}

		[TestMethod]
		public void Should_set_the_inner_exception()
		{
			_exceptionInfo.InnerException.Message.ShouldBeEqualTo(_exception.InnerException.Message);
		}

		[TestMethod]
		public void Should_set_the_FullMessage()
		{
			_exceptionInfo.FullMessage.ShouldBeEqualTo(_exception.ToString());
		}

		[TestMethod]
		public void Should_override_the_toString_and_return_FullMessage()
		{
			_exceptionInfo.ToString().ShouldBeEqualTo(_exceptionInfo.FullMessage);
		}
	}

	[TestClass]
	public class When_serializing_event_messages : FixtureBase
	{
		private static Exception getTestException()
		{
			try
			{
				throw new Exception("test message");
			}
			catch (Exception e)
			{
				return e;
			}
		}

		private static ExceptionInfo GetExceptionInfo()
		{
			return new ExceptionInfo(getTestException());
		}

		private static ExecutedTestInfo GetExecutedTestInfo()
		{
			return new ExecutedTestInfo
					{
						Class = "class",
						Method = "method",
						Namespace = "namespace",
					};
		}

		[TestMethod]
		public void Should_be_able_to_serialize_the_TestPassedEvent()
		{
			var testPassedEvent = new TestPassedEvent
									{
										EndTime = DateTime.Now,
										ExecutedTestInfo = GetExecutedTestInfo(),
									};

			testPassedEvent
				.Serialize()
				.ShouldNotBeNull();
		}

		[TestMethod]
		public void Should_be_able_to_serialize_the_TestFailedEvent()
		{
			var testFailedEvent = new TestFailedEvent
									{
										EndTime = DateTime.Now,
										ExceptionInfo = GetExceptionInfo(),
										ExecutedTestInfo = GetExecutedTestInfo(),
									};

			testFailedEvent
				.Serialize()
				.ShouldNotBeNull();
		}

		[TestMethod]
		public void Should_be_able_to_serialize_the_TestResultBeginEvent()
		{
			var testResultBeginEvent = new TestBeginEvent
										{
											StartTime = DateTime.Now,
											ExecutedTestInfo = GetExecutedTestInfo(),
										};

			testResultBeginEvent
				.Serialize()
				.ShouldNotBeNull();
		}


		[TestMethod]
		public void Should_be_able_to_serialize_the_ExecutedTestInfo()
		{
			GetExecutedTestInfo()
				.Serialize()
				.ShouldNotBeNull();
		}


		[TestMethod]
		public void Should_be_able_to_serialize_the_ExceptionInfo()
		{
			GetExceptionInfo()
				.Serialize()
				.ShouldNotBeNull();
		}

	}

}