
namespace StatLight.Core.Tests.Mocks
{
	using System;
	using System.IO;
	using System.Text;
	using StatLight.Core.Reporting.Messages;
	using StatLight.Core.Serialization;

	public class MessageFactory
	{
		public static Stream CreateResultStream(TestOutcome testOutcome)
		{
			return CreateResult(testOutcome).Serialize().ToStream();
		}

		public static MobilScenarioResult CreateResult(TestOutcome testOutcome)
		{
			return CreateResult(testOutcome, 0);
		}

		public static MobilScenarioResult CreateResult(TestOutcome testOutcome, int messageOrder)
		{
			if (testOutcome == TestOutcome.Passed)
				return CreatePassingResult(messageOrder);
			if (testOutcome == TestOutcome.Failed)
				return CreateFailingResult(messageOrder);

			throw new NotImplementedException();
		}

		private static MobilScenarioResult CreateFailingResult(string testClassName, string testName, Exception exception, int messageOrder)
		{
			var result = StubMobilScenarioResult(TestOutcome.Failed, testClassName, testName, messageOrder);
			result.ExceptionMessage = exception.ToString();
			return result;
		}

		private static MobilScenarioResult CreatePassingResult(string testClassName, string testName, int messageOrder)
		{
			var result = StubMobilScenarioResult(TestOutcome.Passed, testClassName, testName, messageOrder);
			return result;
		}

		private static MobilScenarioResult StubMobilScenarioResult(TestOutcome testOutcome, string testClassName, string testName, int messageOrder)
		{
			return new MobilScenarioResult
			{
				Started = DateTime.Now,
				Finished = DateTime.Now.AddSeconds(1),
				Result = testOutcome,
				TestClassName = testClassName,
				TestName = testName,
				MessageOrder = messageOrder,
			};
		}

		private static MobilScenarioResult CreateFailingResult(int messageOrder)
		{
			var ex = new Exception(RandomString(80, false));
			return CreateFailingResult(RandomString(10, false), RandomString(20, false), ex, messageOrder);
		}

		private static MobilScenarioResult CreatePassingResult(int messageOrder)
		{
			return CreatePassingResult(RandomString(10, false), RandomString(20, false), messageOrder);
		}

		private static string RandomString(int size, bool lowerCase)
		{
			var builder = new StringBuilder();
			var random = new Random();
		    for (int i = 0; i < size; i++)
			{
			    char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
			    builder.Append(ch);
			}
		    if (lowerCase)
				return builder.ToString().ToLower();
			return builder.ToString();
		}

		public static Stream CreateOtherMessageTypeStream(LogMessageType logMessageType)
		{
			return CreateOtherMessageType(logMessageType).Serialize().ToStream();
		}

		public static Stream CreateOtherMessageTypeStream(LogMessageType logMessageType, int messageOrder)
		{
			return CreateOtherMessageType(logMessageType, messageOrder).Serialize().ToStream();
		}

		public static MobilOtherMessageType CreateOtherMessageType(LogMessageType logMessageType)
		{
			return CreateOtherMessageType(logMessageType, 0);
		}

		public static MobilOtherMessageType CreateTestIgnoreMessage(string testName)
		{
			var message = CreateOtherMessageType(LogMessageType.TestExecution);
			message.Message = "Ignoring \"" + testName + "\"";
			return message;
		}


		public static MobilOtherMessageType CreateOtherMessageType(LogMessageType logMessageType, int messageOrder)
		{
			return new MobilOtherMessageType
			{
				Message = RandomString(30, false),
				MessageType = logMessageType,
				MessageOrder = messageOrder,
			};
		}

	}
}
