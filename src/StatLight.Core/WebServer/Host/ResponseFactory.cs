using System;
using StatLight.Core.Properties;

namespace StatLight.Core.WebServer.Host
{
	public class ResponseFactory
	{
		public string ClientAccessPolicy { get { return Resources.ClientAccessPolicy; } }
		public string CrossDomain { get { return Resources.CrossDomain; } }

		private static int _htmlPageInstanceId = 0;

		public ResponseFile Get(string file)
		{
			if (file.Equals(StatLightServiceRestApi.CrossDomain, StringComparison.OrdinalIgnoreCase))
				return new ResponseFile {File = Resources.CrossDomain, ContentType = "text/xml"};

			if (file.Equals(StatLightServiceRestApi.ClientAccessPolicy, StringComparison.OrdinalIgnoreCase))
				return new ResponseFile { File = Resources.ClientAccessPolicy, ContentType = "text/xml" };

			if (file.Equals(StatLightServiceRestApi.GetHtmlTestPage, StringComparison.OrdinalIgnoreCase))
			{
				_htmlPageInstanceId++;
				return GetTestHtmlPage(_htmlPageInstanceId);
			}

			throw new NotImplementedException();
		}

		public bool IsKnownFile(string localPath)
		{
			if (IsKnown(localPath, StatLightServiceRestApi.CrossDomain))
				return true;

			if (IsKnown(localPath, StatLightServiceRestApi.ClientAccessPolicy))
				return true;

			if (IsKnown(localPath, StatLightServiceRestApi.GetHtmlTestPage))
				return true;


			return false;
		}


		public static ResponseFile GetTestHtmlPage(int instanceId)
		{
			var page = Resources.TestPage.Replace("BB86D193-AD39-494A-AEB7-58F948BA5D93", instanceId.ToString());

			return new ResponseFile {File = page, ContentType = "text/html"};
		}

		private static bool IsKnown(string filea, string fileb)
		{
			return string.Equals(filea, fileb, StringComparison.OrdinalIgnoreCase);
		}
	}
}