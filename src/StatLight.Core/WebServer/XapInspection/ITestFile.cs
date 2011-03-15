namespace StatLight.Core.WebServer.XapInspection
{
    public interface ITestFile
    {
        string FileName { get; }
        byte[] File { get; }
    }
}