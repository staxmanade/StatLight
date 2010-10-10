
using System;

namespace StatLight.Core.Common.Abstractions.Timing
{
    public class TimerWrapper : ITimer, IDisposable
	{
		private System.Timers.Timer _baseTimer;

		public event EventHandler<TimerWrapperElapsedEventArgs> Elapsed;

		public TimerWrapper(double millisecondInterval)
		{
			_baseTimer = new System.Timers.Timer(millisecondInterval);
			_baseTimer.Elapsed += _baseTimer_Elapsed;
		}

		void _baseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			var handler = Elapsed;
			if (handler != null)
				handler(this, new TimerWrapperElapsedEventArgs(e.SignalTime));
		}

		public void Start()
		{
			_baseTimer.Start();
		}

		public void Stop()
		{
			_baseTimer.Stop();
		}

		public bool Enabled
		{
			get { return _baseTimer.Enabled; }
			set { _baseTimer.Enabled = value; }
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_baseTimer != null)
					_baseTimer.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
