using System;
using System.ComponentModel.Composition;
using StatLight.Core.Common;
using StatLight.Core.WebBrowser;

namespace StatLight.Core.Runners
{
    [InheritedExport]
    public interface IPhoneEmulator
    {
        IWebBrowser Create(ILogger logger, Func<byte[]> hostXap);
    }
}