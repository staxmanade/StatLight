using System.Reflection;
using StatLight.Core.UnitTestProviders;

namespace StatLight.Core.WebServer.XapInspection
{
	public class XapReadItems
	{
		public UnitTestProviderType UnitTestProvider { get; set; }
		public string AppManifest { get; set; }
		public Assembly TestAssembly { get; set; }
	}
}