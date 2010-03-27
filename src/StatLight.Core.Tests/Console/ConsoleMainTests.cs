using NUnit.Framework;
using System.Collections.Generic;
namespace StatLight.Core.Tests.Console
{
    [TestFixture]
    public class ConsoleMainTests
    {

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