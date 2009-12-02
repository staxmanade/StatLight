
using System;

namespace StatLight.Core.Common
{
	[Flags]
	public enum LogChatterLevels
	{
		None = 2,
		Error = 4,
		Warning = 8,
		Information = 16,
		Debug = 32,
		Full = Error | Warning | Information | Debug,
	}
}
