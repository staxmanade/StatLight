using System;
using NUnit.Framework;
using StatLight.Client.Harness.Events;
using StatLight.Core.Serialization;

namespace StatLight.Core.Tests.Events
{
    namespace ExceptionInfoTests
    {
        [TestFixture]
        public class When_serializing_and_deserializing_an_exceptionInfo : FixtureBase
        {
            Exception exception;
            private ExceptionInfo beforeExceptionInfo;
            private ExceptionInfo afterExceptionInfo;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                try
                {
                    throw new Exception("Hello");
                }
                catch (Exception ex2)
                {
                    exception = ex2;
                }

                beforeExceptionInfo = new ExceptionInfo(exception);

                afterExceptionInfo = beforeExceptionInfo.Serialize().Deserialize<ExceptionInfo>();

            }

            [Test]
            public void Should_have_FullMessage()
            {
                afterExceptionInfo.FullMessage.ShouldEqual(beforeExceptionInfo.FullMessage);
            }

            [Test]
            public void Should_have_Message()
            {
                afterExceptionInfo.Message.ShouldEqual(beforeExceptionInfo.Message);
            }

            [Test]
            public void Should_have_InnerException()
            {
                afterExceptionInfo.InnerException.ShouldEqual(beforeExceptionInfo.InnerException);
            }

            [Test]
            public void Should_have_StackTrace()
            {
                afterExceptionInfo.StackTrace.ShouldEqual(beforeExceptionInfo.StackTrace);
            }
        }
    }
}