using System;

namespace StatLight.Core.Common
{
#if !SILVERLIGHT
    [Serializable]
#endif
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