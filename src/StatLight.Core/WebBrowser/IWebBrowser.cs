
using System;
namespace StatLight.Core.WebBrowser
{
    public interface IWebBrowser : IDisposable
    {
        void Start();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")]
        void Stop();
        int? ProcessId { get; }
    }
}
