using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Radian.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Core.Indexing
{
    public class ContentItemIndex : IDisposable
    {
        private readonly string _rootPath;
        private readonly IFileLocator _locator;
        private readonly ContentItemIndexer _indexer;
        private readonly FileSystemWatcher _watcher;
        private bool _disposed;

        public ContentItemIndex(IFileLocator locator) : this(locator, path: "")
        {
        }

        public ContentItemIndex(IFileLocator locator, string path)
        {
            _rootPath = locator.RootPath;
            _locator = locator;
            _indexer = new ContentItemIndexer();
            _watcher = SetupWatcher();
            IndexContent(path);
        }

        public IEnumerable<string> IndexedFields
        {
            get { return _indexer.IndexedFields; }
        }

        private void IndexContent(string path)
        {
            var files = _locator.FindFiles(path);
            _indexer.Index(files);
        }

        public IEnumerable<ContentItem> Search(string query, int limit, string searchDirectory = null)
        {
            return _indexer.Search(query, limit, searchDirectory);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _indexer.Dispose();
                _watcher.Dispose();
                _disposed = true;
            }
        }

        private FileSystemWatcher SetupWatcher()
        {
            var watcher = new FileSystemWatcher
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;

            return _watcher;
        }

        void OnDeleted(object sender, FileSystemEventArgs e)
        {
            var file = RadianFile.Create(_rootPath, e.FullPath);
            _indexer.Delete(file);
        }

        void OnCreated(object sender, FileSystemEventArgs e)
        {
            var file = RadianFile.Create(_rootPath, e.FullPath);
            _indexer.Index(file);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var file = RadianFile.Create(_rootPath, e.FullPath);
            _indexer.Index(file);
        }
    }
}
