using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Radian.Core.IO
{
    public interface IContentItemLoader
    {
        IEnumerable<ContentItem> LoadAll(string directoryPath);
        IEnumerable<ContentItem<TData>> LoadAll<TData>(string directoryPath)
            where TData : class;
    }

    public class ContentItemLoader : IContentItemLoader
    {
        private readonly IFileLocator _fileLocator;

        public ContentItemLoader(IFileLocator fileLocator)
        {
            _fileLocator = fileLocator;
        }

        public IEnumerable<ContentItem> LoadAll(string directoryPath)
        {
            return _fileLocator.FindFiles(directoryPath, "*").Select(x => x.GetContentItem());
        }

        public IEnumerable<ContentItem<TData>> LoadAll<TData>(string directoryPath)
            where TData : class
        {
            return _fileLocator.FindFiles(directoryPath, "*").Select(x => x.GetContentItem<TData>());
        }
    }
}
