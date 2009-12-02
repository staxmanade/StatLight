using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapHost
{
	public class DefaultXapHostFileLoader : DiskXapHostFileLoader
	{
		public DefaultXapHostFileLoader(ILogger logger)
			: base(logger, StatLightClientXapNames.Default)
		{
		}
	}
}