namespace StatLight.Core.WebServer.Host
{
    public class ResponseFile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] FileData { get; set; }
        public string ContentType { get; set; }
    }
}