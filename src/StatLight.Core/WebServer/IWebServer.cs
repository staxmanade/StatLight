
namespace StatLight.Core.WebServer
{
	public interface IWebServer
	{
		void Start();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")]
		void Stop();
	}
}
