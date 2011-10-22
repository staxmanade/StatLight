using System.Runtime.Serialization;
namespace StatLight.Core.Configuration
{
    [DataContract]
    public class WindowGeometry
    {
        public WindowGeometry()
        {
            State = BrowserWindowState.Minimized;
            Size = new WindowSize(800, 600);
        }
        [DataMember]
        public BrowserWindowState State { get; set; }

        [DataMember]
        public WindowSize Size { get; set; }

        public bool ShouldShowWindow
        {
            get { return State != BrowserWindowState.Minimized; }
        }
    }

    public enum BrowserWindowState
    {
        Minimized,
        Maximized,
        Normal
    }

    [DataContract]
    public class WindowSize
    {
        public WindowSize(int width, int height)
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
