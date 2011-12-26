using NUnit.Framework;
using StatLight.Core.Events;
using StatLight.Core.Reporting;

namespace StatLight.Core.Tests.Reporting
{
    public class DialogAssertionMatchmakerTests
    {
        [Test]
        public void Should_detect_assertion()
        {
            var testExecutionMethodBeginClientEvent = new TestExecutionMethodBeginClientEvent
                                                          {
                                                              NamespaceName = "a",
                                                              ClassName = "b",
                                                              MethodName = "c",
                                                          };
            var dialogAssertionServerEvent = new DialogAssertionServerEvent(DialogType.Assert)
                                                 {
                                                     Message = "at b.c",
                                                 };
            var dialogAssertionMatchmaker = new DialogAssertionMatchmaker();
            var testExecutionMethod = new TestExecutionMethodPassedClientEvent
                                          {
                                              NamespaceName = "a",
                                              ClassName = "b",
                                              MethodName = "c",
                                          };
            bool match = false;


            dialogAssertionMatchmaker.HandleMethodBeginClientEvent(testExecutionMethodBeginClientEvent);
            dialogAssertionMatchmaker.AddAssertionHandler(dialogAssertionServerEvent, item =>
            {
                match = true;
            });


            dialogAssertionMatchmaker.WasEventAlreadyClosed(testExecutionMethod).ShouldBeTrue();
            match.ShouldBeTrue();
        }


        [Test]
        public void If_assettion_happens_before_begin_test_event_arrives_should_match()
        {
            var testExecutionMethodBeginClientEvent = new TestExecutionMethodBeginClientEvent
            {
                NamespaceName = "a",
                ClassName = "b",
                MethodName = "c",
            };
            var dialogAssertionServerEvent = new DialogAssertionServerEvent(DialogType.Assert)
            {
                Message = "at b.c",
            };
            var dialogAssertionMatchmaker = new DialogAssertionMatchmaker();
            var testExecutionMethod = new TestExecutionMethodPassedClientEvent
                                          {
                                              NamespaceName = "a",
                                              ClassName = "b",
                                              MethodName = "c",
                                          };
            bool match = false;


            dialogAssertionMatchmaker.AddAssertionHandler(dialogAssertionServerEvent, item =>
            {
                match = true;
            });
            dialogAssertionMatchmaker.HandleMethodBeginClientEvent(testExecutionMethodBeginClientEvent);


            dialogAssertionMatchmaker.WasEventAlreadyClosed(testExecutionMethod).ShouldBeTrue();
            match.ShouldBeTrue();
        }
    }
}