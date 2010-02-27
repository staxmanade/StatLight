using System;

namespace StatLight.Core.Common
{
    [Serializable]
    public class StatLightException : Exception
    {
        public StatLightException()
        {
        }

        public StatLightException(string message) : base(message)
        {
        }

        public StatLightException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}