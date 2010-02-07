using System;
using System.Text;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using StatLight.Client.Model.Events;
using StatLight.Core.Reporting.Messages;
using StatLight.Core.Serialization;
using LogMessageType = StatLight.Core.Reporting.Messages.LogMessageType;

namespace StatLight.Client.Harness
{
    internal sealed class ServerHandlingLogProvider : LogProvider
    {
        protected override void ProcessRemainder(LogMessage message)
        {
            var serializedString = message.Serialize();
            StatLightPostbackManager.PostMessage(serializedString);

            string traceMessage = TraceLogMessage(message).Serialize();
            StatLightPostbackManager.PostMessage(traceMessage);

        }

        private TraceEvent TraceLogMessage(LogMessage message)
        {
            string msg = "";
            msg += "MessageType={0}".FormatWith(message.MessageType);
            msg += Environment.NewLine;
            msg += "Message={0}".FormatWith(message.Message);
            msg += Environment.NewLine;
            msg += "Decorators:";
            msg += Environment.NewLine;
            msg += GetDecorators(message.Decorators);
            msg += Environment.NewLine;
            return new TraceEvent() { Message = msg };
        }

        private string GetDecorators(DecoratorDictionary decorators)
        {
            var sb = new StringBuilder();
            foreach (var k in decorators)
            {
                sb.AppendFormat("KeyType={0}, ValueType={1}{2}", k.Key, k.Value, Environment.NewLine);
            }
            return sb.ToString();
        }
    }

    internal static class UnitTestResultProviderExtensions
    {
        public static string DecoratorDictionaryToString(this DecoratorDictionary d)
        {
            var sb = new StringBuilder();
            foreach (var k in d)
            {
                //if (k.Value.GetType().IsInstanceOfType(typeof(Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.TestMethod)))
                //{
                //    sb.AppendFormat("KeyType={0}, ValueType={1}{2}", k.Key, ((Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.TestMethod)k.Value).Name, Environment.NewLine);
                //}
                //if (k.Value.GetType().IsInstanceOfType(typeof(Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.TestClass)))
                //{
                //    sb.AppendFormat("KeyType={0}, ValueType={1}{2}", k.Key, ((Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.TestClass)k.Value).Type.FullName, Environment.NewLine);
                //}
                //else
                sb.AppendFormat("KeyType={0}, ValueType={1}{2}", k.Key, k.Value, Environment.NewLine);
            }
            return sb.ToString();
        }

        public static string Serialize(this LogMessage logMessage)
        {
            if (logMessage.MessageType == Microsoft.Silverlight.Testing.Harness.LogMessageType.TestResult &&
                logMessage.Decorators.ContainsKey(UnitTestLogDecorator.ScenarioResult))
            {
                var scenerioResult = logMessage.Decorators[UnitTestLogDecorator.ScenarioResult] as ScenarioResult;
                var messageObject = new MobilScenarioResult();
                messageObject.ExceptionMessage = scenerioResult.Exception != null ? scenerioResult.Exception.ToString() : string.Empty;
                messageObject.Finished = scenerioResult.Finished;
                messageObject.Result = (StatLight.Core.Reporting.Messages.TestOutcome)scenerioResult.Result;
                messageObject.Started = scenerioResult.Started;
                messageObject.TestClassName = scenerioResult.TestClass.Type.FullName;
                messageObject.TestName = scenerioResult.TestMethod.Name;
                return messageObject.Serialize();
            }
            else if (logMessage.MessageType == (Microsoft.Silverlight.Testing.Harness.LogMessageType)LogMessageType.Error)
            {
                var messageObject = new MobilOtherMessageType();
                messageObject.Message = logMessage.Message + " ---- " + logMessage.Decorators.DecoratorDictionaryToString();
                messageObject.MessageType = (LogMessageType)logMessage.MessageType;
                return messageObject.Serialize();
            }
            else
            {
                var messageObject = new MobilOtherMessageType();
                messageObject.Message = logMessage.Message;
                messageObject.MessageType = (LogMessageType)logMessage.MessageType;
                return messageObject.Serialize();
            }
        }
    }
}
