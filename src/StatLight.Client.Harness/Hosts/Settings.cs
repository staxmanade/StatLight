using System;
using System.Linq;
using System.Xml.Linq;

namespace StatLight.Client.Harness.Hosts
{
    public class Settings
    {
        static Settings()
        {
            var appManifestXml = XDocument.Load("StatLight.Settings.xml").Root;
            Func<string, XElement> getElement = settingName => appManifestXml.Elements(settingName).First();
            Func<string, string> getValue = settingName => getElement(settingName).Value;

            Port = int.Parse(getValue("Port"));
        }

        public static int Port { get; private set; }
    }
}