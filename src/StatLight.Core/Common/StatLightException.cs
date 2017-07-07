using System;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

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

#if !SILVERLIGHT
		protected StatLightException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
	}
}