
namespace StatLight.Core.Reporting.Providers.TeamCity
{
    using System;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Reporting;
    using StatLight.Core.Events;

    public class TeamCityTestResultHandler : ITestingReportEvents
    {
        private readonly ICommandWriter _messageWriter;
        private readonly string _assemblyName;

        public TeamCityTestResultHandler(ICommandWriter messageWriter, string assemblyName)
        {
            _messageWriter = messageWriter;
            _assemblyName = assemblyName;
        }

        public void PublishStart()
        {
            _messageWriter.Write(
                CommandFactory.TestSuiteStarted(_assemblyName));
        }

        public void PublishStop()
        {
            _messageWriter.Write(
                CommandFactory.TestSuiteFinished(_assemblyName));
        }

        private void WrapTestWithStartAndEnd(Command command, string name, long durationMilliseconds)
        {
            WrapTestWithStartAndEnd(() => _messageWriter.Write(command), name, durationMilliseconds);
        }

        private void WrapTestWithStartAndEnd(Action action, string name, long durationMilliseconds)
        {
            _messageWriter.Write(CommandFactory.TestStarted(name));
            action();
            _messageWriter.Write(CommandFactory.TestFinished(name, durationMilliseconds));
        }

        public void Handle(TraceClientEvent message)
        {
            _messageWriter.Write(message.Message);
        }

        public void Handle(DialogAssertionServerEvent message)
        {
            string writeMessage = message.Message;
            WriteServerEventFailure("DialogAssertionServerEvent", writeMessage);
        }

        private void WriteServerEventFailure(string name, string writeMessage)
        {
            const int durationMilliseconds = 0;

            WrapTestWithStartAndEnd(() => _messageWriter.Write(
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
            WriteServerEventFailure("BrowserHostCommunicationTimeoutServerEvent", writeMessage);
        }

        public void Handle(TestCaseResult message)
        {
            var name = message.FullMethodName();
            var durationMilliseconds = message.TimeToComplete.Milliseconds;

            switch (message.ResultType)
            {
                case ResultType.Ignored:
                    WrapTestWithStartAndEnd(CommandFactory.TestIgnored(message.MethodName, string.Empty), message.MethodName, 0);
                    break;
                case ResultType.Passed:

                    WrapTestWithStartAndEnd(() =>
                    {

                    }, name, durationMilliseconds);
                    break;
                case ResultType.Failed:
                    WrapTestWithStartAndEnd(() => _messageWriter.Write(
                        CommandFactory.TestFailed(
                            name,
                            message.ExceptionInfo.FullMessage,
                            message.ExceptionInfo.FullMessage)),
                        name,
                        durationMilliseconds);
                    break;
                case ResultType.SystemGeneratedFailure:
                    WrapTestWithStartAndEnd(() => _messageWriter.Write(
                        CommandFactory.TestFailed(
                            name,
                            "StatLight generated test failure",
                            message.OtherInfo)),
                        name,
                        durationMilliseconds);
                    break;

                default:
                    "Unknown TestCaseResult (to StatLight) - {0}".FormatWith(message.ResultType)
                        .WrapConsoleMessageWithColor(ConsoleColor.Red, true);
                    break;
            }

        }
    }
}
