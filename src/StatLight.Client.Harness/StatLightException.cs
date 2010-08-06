using System;

namespace StatLight.Client.Harness
{
    public class StatLightException : Exception
    {
        public StatLightException(string message)
            : base(message)
        {}
    }
}