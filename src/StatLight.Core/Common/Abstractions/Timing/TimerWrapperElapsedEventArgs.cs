using System;

namespace StatLight.Core.Common.Abstractions.Timing
{
    public class TimerWrapperElapsedEventArgs : EventArgs
	{
		public TimerWrapperElapsedEventArgs(DateTime signalTime)
		{
			this.SignalTime = signalTime;
		}

		public DateTime SignalTime { get; set; }
	}
}
