
using System;

namespace StatLight.Core.Common.Abstractions.Timing
{
    public interface ITimer
	{
		bool Enabled { get; set; }
		void Start();
		void Stop();
		event EventHandler<TimerWrapperElapsedEventArgs> Elapsed;
	}
}
