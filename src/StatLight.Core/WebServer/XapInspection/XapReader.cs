using StatLight.Core.UnitTestProviders;

namespace StatLight.Core.WebServer.XapInspection
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Xml.Linq;
	using Ionic.Zip;

	public class XapReader
	{
		public XapReadItems GetTestAssembly(string archiveFileName)
		{
			var xapItems = new XapReadItems();

			using (var archive = ZipFile.Read(archiveFileName))
			{
				xapItems.AppManifest = LoadAppManifest(archive);

				if(xapItems.AppManifest != null)
				{
					string testAssemblyName = GetTestAssemblyNameFromAppManifest(xapItems.AppManifest);

					xapItems.TestAssembly = LoadTestAssembly(archive, testAssemblyName);
				}

				xapItems.UnitTestProvider = DetermineUnitTestProviderType(archive);
			}
			return xapItems;
		}

		private UnitTestProviderType DetermineUnitTestProviderType(ZipFile archive)
		{
			Func<string, string, bool> fileNameCompare =
				(fileA, fileB) =>
					string.Equals(fileA, fileB, StringComparison.CurrentCultureIgnoreCase);

			bool hasMSTest = false;

			foreach (ZipEntry zipEntry in archive)
			{
				if (fileNameCompare(zipEntry.FileName, "XUnitLight.Silverlight.dll"))
					return UnitTestProviderType.XUnit;

				if (zipEntry.FileName.ToLower().Contains("unitdriven"))
					return UnitTestProviderType.UnitDriven;

				if (zipEntry.FileName.ToLower().Contains("nunit"))
					return UnitTestProviderType.NUnit;

				if (fileNameCompare(zipEntry.FileName, "Microsoft.Silverlight.Testing.dll"))
					hasMSTest = true;
			}

			if (hasMSTest)
				return UnitTestProviderType.MSTest;

			return UnitTestProviderType.Undefined;
		}

		private static Assembly LoadTestAssembly(ZipFile zip1, string testAssemblyName)
		{
			var fileData = ReadFileIntoBytes(zip1, testAssemblyName);
			if (fileData != null)
				return Assembly.Load(fileData);
			return null;
		}

		private static string GetTestAssemblyNameFromAppManifest(string appManifest)
		{
			var root = XElement.Parse(appManifest);

			var entryPointAssemblyNode = root.Attribute("EntryPointAssembly");

			if (entryPointAssemblyNode == null)
				throw new Exception("Cannot find the EntryPointAssembly attribute in the AppManifest.xaml");

			return entryPointAssemblyNode.Value + ".dll";
		}

		private static string LoadAppManifest(ZipFile zip1)
		{
			var fileData = ReadFileIntoBytes(zip1, "AppManifest.xaml");
			if(fileData != null)
				return Encoding.UTF8.GetString(fileData).Substring(1);

			return null;
		}

		private static byte[] ReadFileIntoBytes(ZipFile zipFile, string fileName)
		{
			var file = zipFile[fileName];
			if (file == null)
				return null;

			using (var stream = new MemoryStream())
			{
				file.Extract(stream);
				return stream.ToArray();
			}
		}
	}
}