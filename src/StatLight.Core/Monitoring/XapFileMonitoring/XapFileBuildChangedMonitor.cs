
using System;
using System.IO;

namespace StatLight.Core.Monitoring.XapFileMonitoring
{
    public class XapFileBuildChangedMonitor : IXapFileBuildChangedMonitor, IDisposable
	{
		public event EventHandler<XapFileBuildChangedEventArgs> FileChanged = delegate {};

		FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();
		private FileInfo _file;

		public XapFileBuildChangedMonitor(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException(filePath);

			_file = new FileInfo(filePath);

			BeginFileWatching();
		}

		private void BeginFileWatching()
		{
			_fileSystemWatcher.Path = _file.Directory.FullName;

			// Only watch the single file.
			_fileSystemWatcher.Filter = _file.Name;

			_fileSystemWatcher.Changed += _watcher_TakeFileAction;
			_fileSystemWatcher.Created += _watcher_TakeFileAction;

			// Begin watching.
			_fileSystemWatcher.EnableRaisingEvents = true;
		}

		private DateTime startTime = DateTime.MinValue;
		private static TimeSpan diffTime = new TimeSpan(0,0,5);

		void _watcher_TakeFileAction(object sender, FileSystemEventArgs e)
		{
			if((DateTime.Now - startTime) > diffTime)
			{
				startTime = DateTime.Now;
				this.FileChanged(this, new XapFileBuildChangedEventArgs());
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_fileSystemWatcher.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
