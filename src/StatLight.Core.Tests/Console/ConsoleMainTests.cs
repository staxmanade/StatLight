using System;
using NUnit.Framework;
using System.Collections.Generic;
using StatLight.Core.Common;

namespace StatLight.Core.Tests.Console
{
    [TestFixture]
    public class ConsoleMainTests
    {
        [Test]
        public void When_a_resolutionException_has_no_inner_should_return_the_resolutionException()
        {
            var tinyIoCContainer = new TinyIoC.TinyIoCContainer();
            tinyIoCContainer.Register<IFoo, Foo>();

            MethodThatThrows m = () => tinyIoCContainer.Resolve<IFoo>().ShouldNotBeNull();

            Exception exception = StatLight.Console.Program.ResolveNonTinyIocException(m.GetException());

            exception.ShouldNotBeNull();
            exception.ShouldBeOfType(typeof(TinyIoC.TinyIoCResolutionException));
        }

        [Test]
        public void When_a_resolutionException_has_an_inner_should_return_the_innerException()
        {
            var tinyIoCContainer = new TinyIoC.TinyIoCContainer();
            tinyIoCContainer.Register<IFoo, Foo2>();

            MethodThatThrows m = () => tinyIoCContainer.Resolve<IFoo>().ShouldNotBeNull();

            Exception exception = StatLight.Console.Program.ResolveNonTinyIocException(m.GetException());

            exception.ShouldNotBeNull()
                .ShouldBeOfType(typeof(StatLightException));
        }

        interface IFoo
        {
        }

        public class Foo : IFoo
        {
            public Foo(int foo)
            {
            }
        }

        public class Foo2 : IFoo
        {
            public Foo2()
            {
                throw new StatLightException("FOO");
            }
        }
    }


    [TestFixture]
    public class InitializationArgumentsValidatorTests
    {

    }

    [TestFixture]
    public class TextInformationalMessageWriterTests : FixtureBase
    {




        private TextInformationalMessageWriter _textInformationalMessageWriter;
        private TestMessageWriter _testMessageWriter;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            _testMessageWriter = new TestMessageWriter();

            _textInformationalMessageWriter = new TextInformationalMessageWriter(_testMessageWriter);
        }
    }

    internal class TextInformationalMessageWriter
    {
        private readonly ITextWriter _textWriter;

        public TextInformationalMessageWriter(ITextWriter textWriter)
        {
            _textWriter = textWriter;
        }
    }


    public interface ITextWriter
    {
        void Write(string message);
    }

    public class TestMessageWriter : ITextWriter
    {
        private readonly List<string> messages = new List<string>();
        public List<string> Messages { get { return messages; } }

        public void Write(string message)
        {
            Write(message);
        }
    }
}