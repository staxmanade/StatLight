using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;
using StatLight.Core.Reporting;

namespace StatLight.Core.Tests.Reporting
{
    namespace DialogAssertionMessageMatchMakerTests
    {
        public class DialogAssertionMessageMatchMakerTestBase : FixtureBase
        {
            private DialogAssertionMessageMatchMaker _dialogAssertionMessageMatchMaker;
            protected DialogAssertionMessageMatchMaker DialogAssertionMessageMatchMaker { get { return _dialogAssertionMessageMatchMaker; } }

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                _dialogAssertionMessageMatchMaker = new DialogAssertionMessageMatchMaker();
            }

        }

        [TestFixture]
        public class when_a_dialog_assertion_event_sequence_is__BeginMethod_DialogAssertion_MethodResult : DialogAssertionMessageMatchMakerTestBase
        {
            private TestCaseResult _testCaseResult;
            private TestExecutionMethodBeginClientEvent _beginEvent;
            private bool _matchMade;
            protected override void Because()
            {
                base.Because();

                DialogAssertionMessageMatchMaker.Handle(new TestExecutionMethodBeginClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m" });
                DialogAssertionMessageMatchMaker.Handle(new DialogAssertionServerEvent { }, (e) => _matchMade = true);
            }

            [Test]
            public void Should_detect_new_event_as_handled()
            {
                TestExecutionMethod message = new TestExecutionMethodPassedClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m" };
                DialogAssertionMessageMatchMaker.WasEventAlreadyClosed(message).ShouldBeTrue();
            }

            [Test]
            public void Should_have_executed_dialog_assertion_matchMade_delegate()
            {
                _matchMade.ShouldBeTrue();
            }
        }


        [TestFixture]
        public class when_a_dialog_assertion_event_sequence_is__DialogAssertion_BeginMethod_MethodResult : DialogAssertionMessageMatchMakerTestBase
        {
            private TestCaseResult _testCaseResult;
            private TestExecutionMethodBeginClientEvent _beginEvent;
            private bool _matchMade;

            protected override void Because()
            {
                base.Because();

                DialogAssertionMessageMatchMaker.Handle(new DialogAssertionServerEvent { }, (e) => _matchMade = true);
                DialogAssertionMessageMatchMaker.Handle(new TestExecutionMethodBeginClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m" });
            }

            [Test]
            public void Should_detect_new_event_as_handled()
            {
                TestExecutionMethod message = new TestExecutionMethodPassedClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m" };
                DialogAssertionMessageMatchMaker.WasEventAlreadyClosed(message).ShouldBeTrue();
            }

            [Test]
            public void Should_have_executed_dialog_assertion_matchMade_delegate()
            {
                _matchMade.ShouldBeTrue();
            }
        }

    }
}