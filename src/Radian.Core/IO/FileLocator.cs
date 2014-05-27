using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Core.IO
{
    public interface IFileLocator
    {
        string RootPath { get; }
        IEnumerable<RadianFile> FindFiles(string directory);
        IEnumerable<RadianFile> FindFiles(string directory, string searchPattern);
    }

    public class FileLocator : IFileLocator
    {
        private readonly string _rootPath;

        public FileLocator(string rootPath)
        {
            _rootPath = rootPath;
        }

        public string RootPath { get { return _rootPath; } }

        public IEnumerable<RadianFile> FindFiles(string directory)
        {
            return FindFiles(directory, "*");
        }

        public IEnumerable<RadianFile> FindFiles(string directory, string searchPattern)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(_rootPath, directory), searchPattern))
            {
                yield return RadianFile.Create(_rootPath, file);
            }

            foreach (var result in Directory.GetDirectories(Path.Combine(_rootPath, directory)).SelectMany(x => FindFiles(x, searchPattern)))
            {
                yield return result;
            }
        }
    }
}
