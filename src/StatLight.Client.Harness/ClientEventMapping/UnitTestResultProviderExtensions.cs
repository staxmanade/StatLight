using System;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using StatLight.Core.Reporting.Messages;
using StatLight.Core.Serialization;
using LogMessageType = Microsoft.Silverlight.Testing.Harness.LogMessageType;

namespace StatLight.Client.Harness.ClientEventMapping
{
    internal static class UnitTestResultProviderExtensions
    {

        public static bool DecoratorMatches(this LogMessage logMessage, object key, Predicate<object> value)
        {
            if (logMessage.Decorators.ContainsKey(key))
            {
                if (value(logMessage.Decorators[key]))
                    return true;
            }
            return false;
        }

        public static bool Is(this LogMessage logMessage, object value)
        {
            var decorators = logMessage.Decorators;
            if (value is TestStage)
            {
                if (decorators.ContainsKey(LogDecorator.TestStage))
                    if ((TestStage)decorators[LogDecorator.TestStage] == (TestStage)value)
                    {
                        return true;
                    }
            }
            else if (value is TestGranularity)
            {
                if (decorators.ContainsKey(LogDecorator.TestGranularity))
                    if ((TestGranularity)decorators[LogDecorator.TestGranularity] == (TestGranularity)value)
                    {
                        return true;
                    }
            }
            return false;
        }

        public static string Serialize(this LogMessage logMessage)
        {
            if (logMessage.MessageType == LogMessageType.TestResult &&
                logMessage.Decorators.ContainsKey(UnitTestLogDecorator.ScenarioResult))
            {
                var scenerioResult = (ScenarioResult)logMessage.Decorators[UnitTestLogDecorator.ScenarioResult];
                var messageObject = new MobilScenarioResult
                                        {
                                            ExceptionMessage = scenerioResult.Exception != null
                                                                   ? scenerioResult.Exception.ToString()
                                                                   : string.Empty,
                                            Finished = scenerioResult.Finished,
                                            Result = (Core.Reporting.Messages.TestOutcome)scenerioResult.Result,
                                            Started = scenerioResult.Started,
                                            TestClassName = scenerioResult.TestClass.Type.FullName,
                                            TestName = scenerioResult.TestMethod.Name
                                        };
                return messageObject.Serialize();
            }

            if (logMessage.MessageType == LogMessageType.Error)
            {
                var messageObject = new MobilOtherMessageType
                                        {
                                            Message = logMessage.Message + " ---- " + ServerHandlingLogProvider.GetDecorators(logMessage.Decorators),
                                            MessageType = (Core.Reporting.Messages.LogMessageType)logMessage.MessageType
                                        };
                return messageObject.Serialize();
            }
            else
            {
                var messageObject = new MobilOtherMessageType
                                        {
                                            Message = logMessage.Message,
                                            MessageType = (Core.Reporting.Messages.LogMessageType)logMessage.MessageType
                                        };
                return messageObject.Serialize();
            }
        }
    }
}