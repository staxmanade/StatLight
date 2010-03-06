
using StatLight.Core.Reporting;
using System;
namespace StatLight.Core.Runners
{
    public interface IRunner : IDisposable
	{
		TestReport Run();
	}
}
