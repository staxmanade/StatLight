using System.IO;

namespace StatLight.Core.WebServer.Host
{
    public interface IPostHandler
    {
        void Handle(Stream messageStream);
        void ResetTestRunStatistics();
        void TryWaitingForMessagesToCompletePosting();
    }
}