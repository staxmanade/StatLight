using StatLight.Core.Reporting.Messages;

namespace StatLight.Core.Reporting
{
	public interface ITestResultHandler
	{
		void HandleMessage(MobilScenarioResult result);
		void HandleMessage(MobilOtherMessageType result);
	}
}
