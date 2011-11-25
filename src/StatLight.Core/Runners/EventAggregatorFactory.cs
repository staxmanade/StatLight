using System;
using System.Collections.Generic;
using StatLight.Core.Events;
using StatLight.Client.Harness.Events;
using StatLight.Core.Common;

namespace StatLight.Core.Runners
{
    public static class EventAggregatorFactory
    {
        public static EventAggregator Create(ILogger logger)
        {
            var msgsToIgnoreTraceOn = new List<Type>
            {
                typeof (InitializationOfUnitTestHarnessClientEvent),
                typeof (TestExecutionClassCompletedClientEvent),
                typeof (TestExecutionClassBeginClientEvent),
                typeof (SignalTestCompleteClientEvent),
            };

            var config = new EventAggregator.Config
            {
                HoldReferences = true,
                OnMessageNotPublishedBecauseZeroListeners = msgNotHandled =>
                {
                    var msgType = msgNotHandled.GetType();

                    if (msgsToIgnoreTraceOn.Contains(msgType))
                        return;

                    logger.Debug("No event listener objects were defined to listen to message of type({0})".FormatWith(msgType.FullName));
                }
            };

            return new EventAggregator(config);
        }
    }
}