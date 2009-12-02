using StatLight.Core.Reporting.Messages;

namespace StatLight.Core.WebServer
{
	using System;

	public class TestResultEventArgs : EventArgs
	{
		private MobilScenarioResult _mobilScenarioResult;
		public MobilScenarioResult MobilScenarioResult { get { return _mobilScenarioResult; } }

		public TestResultEventArgs(MobilScenarioResult mobilScenarioResult)
		{
			_mobilScenarioResult = mobilScenarioResult;
		}

		private MobilOtherMessageType _mobilOtherMessageType;
		public MobilOtherMessageType MobilOtherMessageType { get { return _mobilOtherMessageType; } }

		public TestResultEventArgs(MobilOtherMessageType mobilOtherMessageType)
		{
			_mobilOtherMessageType = mobilOtherMessageType;
		}

	}
}
