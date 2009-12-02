using System;
using Microsoft.Silverlight.Testing.Harness;

namespace StatLight.Client.Silverlight.LogMessageHandling
{
	internal static class LogMessageHandlerFactory
	{
		public static ILogMessageHandler GetHandlerFor(LogMessage logMessage)
		{
			if (logMessage.MessageType == LogMessageType.TestResult)
				return new ScenarioResultLogMessageHandler(logMessage);

			throw new NotImplementedException();
		}

	}
}
