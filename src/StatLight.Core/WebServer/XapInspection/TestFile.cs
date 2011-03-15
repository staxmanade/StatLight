namespace StatLight.Core.WebServer.XapInspection
{
    public class TestFile : ITestFile
    {
        private readonly byte[] _file;
        private readonly string _fileName;

        public TestFile(string fullPath)
            : this(System.IO.Path.GetFileName(fullPath), System.IO.File.ReadAllBytes(fullPath))
        {

        }

        public TestFile(string fileName, byte[] file)
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