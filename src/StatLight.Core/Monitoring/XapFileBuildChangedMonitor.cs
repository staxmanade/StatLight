using System;
using System.Collections.Generic;
using System.IO;
using StatLight.Core.Events;

namespace StatLight.Core.Monitoring
{
    public class XapFileBuildChangedMonitor : IDisposable
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly Dictionary<string, SystemWatcherData> _fileSystemWatcher = new Dictionary<string, SystemWatcherData>();
        private readonly List<FileInfo> _files = new List<FileInfo>();
        private static readonly TimeSpan DiffTime = new TimeSpan(0, 0, 5);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public XapFileBuildChangedMonitor(IEventPublisher eventPublisher, IEnumerable<string> files)
        {
            if (eventPublisher == null)
                throw new ArgumentNullException("eventPublisher");

            if (files == null)
                throw new ArgumentNullException("files");

            _eventPublisher = eventPublisher;

            foreach (var file in files)
            {
                if (!File.Exists(file))
                    throw new FileNotFoundException(file);

                var fileInfo = new FileInfo(file);

                var fileSystemWatcher = new FileSystemWatcher(fileInfo.Directory.FullName)
                {
                    Filter = fileInfo.Name,
                    EnableRaisingEvents = true,
                };
                // Only watch the single file.

                fileSystemWatcher.Changed += _watcher_TakeFileAction;
                fileSystemWatcher.Created += _watcher_TakeFileAction;

                _fileSystemWatcher.Add(fileInfo.FullName, new SystemWatcherData { Watcher = fileSystemWatcher });

                _files.Add(fileInfo);
            }

        }

        void _watcher_TakeFileAction(object sender, FileSystemEventArgs e)
        {
            SystemWatcherData item = _fileSystemWatcher[e.FullPath];

            if ((DateTime.Now - item.StartTime) > DiffTime)
            {
                item.StartTime = DateTime.Now;
                _eventPublisher.SendMessage(new XapFileBuildChangedServerEvent(e.FullPath));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileSystemWatcher.Each(x => x.Value.Watcher.Dispose());
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private class SystemWatcherData
        {
            public SystemWatcherData()
            {
                StartTime = DateTime.MinValue;
            }
            public FileSystemWatcher Watcher { get; set; }
            public DateTime StartTime { get; set; }
        }
    }
}
