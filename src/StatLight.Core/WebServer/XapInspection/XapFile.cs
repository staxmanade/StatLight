namespace StatLight.Core.WebServer.XapInspection
{
    public class XapFile : IXapFile
    {
        private readonly byte[] _file;
        private readonly string _fileName;

        public XapFile(string fileName, byte[] file)
        {
            _file = file;
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public byte[] File
        {
            get { return _file; }
        }
    }
}