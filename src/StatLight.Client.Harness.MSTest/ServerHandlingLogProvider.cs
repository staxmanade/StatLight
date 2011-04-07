using System;
using System.Text;
using Microsoft.Silverlight.Testing.Harness;
using StatLight.Client.Harness.Events;
using StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation;
using StatLight.Client.Harness.Messaging;

namespace StatLight.Client.Harness.Hosts.MSTest
{
    internal sealed class ServerHandlingLogProvider : LogProvider
    {
        protected override void ProcessRemainder(LogMessage message)
        {
            //DEBUG
            //var msg = TraceLogMessage(message);
            //Console.WriteLine(msg.Message);

            try
            {
                ClientEvent clientEvent;
                if (TryTranslateIntoClientEvent(message, out clientEvent))
                {
                    if (clientEvent != null)
                        Server.PostMessage(clientEvent);
                }
                else
                {
                    var traceClientEvent = TraceLogMessage(message);
                    Server.PostMessage(traceClientEvent);
                }
            }
            catch (Exception ex)
            {
                Server.LogException(ex);
            }
        }

        private static bool TryTranslateIntoClientEvent(LogMessage message, out ClientEvent clientEvent)
        {
            var logMessageMapperDude = new LogMessageTranslator();

            return logMessageMapperDude.TryTranslate(message, out clientEvent);
        }

        public static TraceClientEvent TraceLogMessage(LogMessage message)
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


