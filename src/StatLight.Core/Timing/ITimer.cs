
namespace StatLight.Core.Timing
{
	using System;

	public interface ITimer
	{
		bool Enabled { get; set; }
		void Start();
		void Stop();
		event EventHandler<TimerWrapperElapsedEventArgs> Elapsed;
	}
}
