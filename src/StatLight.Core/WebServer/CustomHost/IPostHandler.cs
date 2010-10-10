using System.IO;

namespace StatLight.Core.WebServer.CustomHost
{
    public interface IPostHandler
    {
        void Handle(Stream messageStream);
        void ResetTestRunStatistics();
        void TryWaitingForMessagesToCompletePosting();
    }
}