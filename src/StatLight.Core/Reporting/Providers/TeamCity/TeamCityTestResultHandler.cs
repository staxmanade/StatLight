
using StatLight.Core.Events;

namespace StatLight.Core.Reporting.Providers.TeamCity
{
    using System;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Reporting.Messages;

    public class TeamCityTestResultHandler : ITestingReportEvents
    {
        private readonly ICommandWriter messageWriter;
        private readonly string assemblyName;

        public TeamCityTestResultHandler(ICommandWriter messageWriter, string assemblyName)
        {
            this.messageWriter = messageWriter;
            this.assemblyName = assemblyName;
        }

        public void PublishStart()
        {
            messageWriter.Write(
                CommandFactory.TestSuiteStarted(assemblyName));
        }

        public void PublishStop()
        {
            messageWriter.Write(
                CommandFactory.TestSuiteFinished(assemblyName));
        }

        private void WrapTestWithStartAndEnd(Command command, string name, long durationMilliseconds)
        {
            WrapTestWithStartAndEnd(() => messageWriter.Write(command), name, durationMilliseconds);
        }

        private void WrapTestWithStartAndEnd(Action action, string name, long durationMilliseconds)
        {
            messageWriter.Write(CommandFactory.TestStarted(name));
            action();
            messageWriter.Write(CommandFactory.TestFinished(name, durationMilliseconds));
        }
        public void Handle(TestExecutionMethodPassedClientEvent message)
        {
            var name = message.ClassName + "." + message.MethodName;
            var durationMilliseconds = message.TimeToComplete.Milliseconds;

            WrapTestWithStartAndEnd(() =>
            {
                
            }, name, durationMilliseconds);
        }

        public void Handle(TestExecutionMethodFailedClientEvent message)
        {
            var name = message.ClassName + "." + message.MethodName;
            var durationMilliseconds = message.TimeToComplete.Milliseconds;

            WrapTestWithStartAndEnd(() => messageWriter.Write(
                CommandFactory.TestFailed(
                    name,
                    message.ExceptionInfo.FullMessage,
                    message.ExceptionInfo.FullMessage)), 
                name,
                durationMilliseconds);
        }

        public void Handle(TestExecutionMethodIgnoredClientEvent message)
        {
            WrapTestWithStartAndEnd(CommandFactory.TestIgnored(message.Message, string.Empty), message.Message, 0);
        }

        public void Handle(TraceClientEvent message)
        {
            Console.WriteLine(message.Message);
        }

        public void Handle(DialogAssertionServerEvent message)
        {
            string writeMessage = message.ExceptionMessage;
            WriteServerEventFailure(writeMessage);
        }

        private void WriteServerEventFailure(string writeMessage)
        {
            const string name = "DialogAssertion";
            const int durationMilliseconds = 0;

            WrapTestWithStartAndEnd(() => messageWriter.Write(
                CommandFactory.TestFailed(
                    name,
                    writeMessage,
                    writeMessage)),
                                    name,
                                    durationMilliseconds);
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
            string writeMessage = message.Message;
            WriteServerEventFailure(writeMessage);
        }
    }
}
