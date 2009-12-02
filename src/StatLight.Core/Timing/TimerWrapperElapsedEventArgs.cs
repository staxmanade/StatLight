namespace StatLight.Core.Timing
{
	using System;

	public class TimerWrapperElapsedEventArgs : EventArgs
	{
		public TimerWrapperElapsedEventArgs(DateTime signalTime)
		{
			this.SignalTime = signalTime;
		}

		public DateTime SignalTime { get; set; }
	}
}
