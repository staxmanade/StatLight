using System.IO;

namespace StatLight.Core.WebServer
{
    public interface IPostHandler
    {
        void ResetTestRunStatistics();
        void TryWaitingForMessagesToCompletePosting();
        bool TryHandle(Stream messageStream, out string unknownPostData);
    }
}