namespace StatLight.Core.Configuration
{
    public class WindowGeometry
    {
        public BrowserWindowState WindowState { get; set; }
        public Size WindowSize { get; set; }
    }

    public enum BrowserWindowState
    {
        Minimized,
        Maximized,
        Normal
    }

    public class Size
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
