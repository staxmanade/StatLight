using System.IO;

namespace StatLight.Core.WebServer
{
    public interface IPostHandler
    {
        void ResetTestRunStatistics();
        bool TryHandle(Stream messageStream, out string unknownPostData);
    }
}