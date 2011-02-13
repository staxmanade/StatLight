namespace StatLight.Core.WebServer.XapHost
{
    public interface IXapHostFileLoader
    {
        byte[] LoadXapHost();
        string Path { get; }
    }
}