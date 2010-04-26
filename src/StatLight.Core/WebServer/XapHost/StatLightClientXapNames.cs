namespace StatLight.Core.WebServer.XapHost
{
	public static class StatLightClientXapNames
	{
		public static readonly string ClientXapNameFormat = "StatLight.Client.For.{0}.xap";
		public static readonly string Default = ClientXapNameFormat.FormatWith("March2010");
	}
}