using System;
using System.Text;
using Microsoft.Silverlight.Testing.Harness;
using StatLight.Client.Harness.ClientEventMapping;
using StatLight.Client.Harness.Events;
using StatLight.Core.Reporting.Messages;
using LogMessageType = StatLight.Core.Reporting.Messages.LogMessageType;

namespace StatLight.Client.Harness
{
    internal sealed class ServerHandlingLogProvider : LogProvider
    {
        protected override void ProcessRemainder(LogMessage message)
        {
            var serializedString = message.Serialize();
            Server.PostMessage(serializedString);

            //string traceMessage = TraceLogMessage(message).Serialize();
            //Server.PostMessage(traceMessage);

            try
            {

                ClientEvent clientEvent;
                if (TryTranslateIntoClientEvent(message, out clientEvent))
                {
                    Server.PostMessage(clientEvent);
                }
            }
            catch (Exception ex)
            {
                var messageObject = new MobilOtherMessageType
                                        {
                                            Message = ex.ToString(),
                                            MessageType = LogMessageType.Error
                                        };
                Server.PostMessage(messageObject);
            }
        }

        private static bool TryTranslateIntoClientEvent(LogMessage message, out ClientEvent clientEvent)
        {
            var logMessageMapperDude = new LogMessageTranslator();

            return logMessageMapperDude.TryTranslate(message, out clientEvent);
        }

        private TraceClientEvent TraceLogMessage(LogMessage message)
        {
            const string newLine = "\n";

            string msg = "";
            msg += "MessageType={0}".FormatWith(message.MessageType);
            msg += newLine;
            msg += "Message={0}".FormatWith(message.Message);
            msg += newLine;
            msg += "Decorators:";
            msg += newLine;
            msg += GetDecorators(message.Decorators);
            msg += newLine;
            return new TraceClientEvent { Message = msg };
        }

        internal static string GetDecorators(DecoratorDictionary decorators)
        {
            var sb = new StringBuilder();
            foreach (var k in decorators)
            {
                sb.AppendFormat("KeyType(typeof,string)={0}, {1}, ValueType={2}, {3}{4}",
                    k.Key.GetType(), k.Key,
                    k.Value.GetType(), k.Value, Environment.NewLine);
            }
            return sb.ToString();
        }

    }
}
