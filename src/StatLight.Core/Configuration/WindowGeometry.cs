using System.Runtime.Serialization;
namespace StatLight.Core.Configuration
{
    [DataContract]
    public class WindowGeometry
    {
        [DataMember]
        public BrowserWindowState WindowState { get; set; }

        [DataMember]
        public Size WindowSize { get; set; }
    }

    public enum BrowserWindowState
    {
        Minimized,
        Maximized,
        Normal
    }

    [DataContract]
    public class Size
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }
    }
}
