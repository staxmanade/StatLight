
using StatLight.Core.Events;

namespace StatLight.Core.Tests.Mocks
{
    using System;
    using System.IO;
    using System.Text;
    using StatLight.Core.Reporting.Messages;
    using StatLight.Core.Serialization;

    public class MessageFactory
    {
        public static Stream Create<T>()
            where T : new()
        {
            var foo = new T();

            return foo.Serialize().ToStream();
        }

    }
}
