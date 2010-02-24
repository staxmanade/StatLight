using NUnit.Framework;
using StatLight.Core.Reporting.Providers.Console;

namespace StatLight.Core.Tests.Reporting.Providers.Console
{
    [TestFixture]
    public class when_verifying_the_console_runner_can_accept_messages : PublishedEventsToHandleBase<ConsoleResultHandler>
    {
        ConsoleResultHandler handler;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            handler = new ConsoleResultHandler(TestLogger, TestEventAggregator);
        }

        protected override ConsoleResultHandler Handler
        {
            get { return handler; }
        }
    }
}
