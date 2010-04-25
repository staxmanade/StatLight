namespace StatLight.Core.WebServer.XapInspection
{
    public interface IXapFile
    {
        string FileName { get; }
        byte[] File { get; }
    }
}