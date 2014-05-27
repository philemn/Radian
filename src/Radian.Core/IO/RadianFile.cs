using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Core.IO
{
    public class RadianFile
    {
        private const string CONTENT_DIRECTORY = "content";
        private const string LAYOUT_DIRECTORY = "layouts";
        private const string PAGE_DIRECTORY = "pages";
        private const string QUERY_DIRECTORY = "queries";
        private const string VIEW_DIRECTORY = "views";

        private static readonly Dictionary<RadianFileType, string> TypeToDirectory = new Dictionary<RadianFileType, string>
        {
            { RadianFileType.Content, CONTENT_DIRECTORY },
            { RadianFileType.Layout, LAYOUT_DIRECTORY },
            { RadianFileType.Page, PAGE_DIRECTORY },
            { RadianFileType.Query, QUERY_DIRECTORY },
            { RadianFileType.View, VIEW_DIRECTORY},
        };

        private static readonly Dictionary<string, RadianFileType> DirectoryToType = TypeToDirectory.ToDictionary(x => x.Value, x => x.Key);

        private readonly JsonSerializer _serializer;

        public RadianFile(string rootPath, string path)
        {
            if (rootPath == null)
            {
                throw new ArgumentNullException("rootPath");
            }

            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            _serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            _serializer.Converters.Add(new ExpandoObjectConverter());

            RootPath = rootPath;
            Path = path.Replace('\\', '/');
            FullPath = new FileInfo(System.IO.Path.Combine(rootPath, path)).FullName;

            if (TypeToDirectory.Values
                .Select(x => x + "/")
                .Any(Path.StartsWith))
            {
                var typeDirectoryNameLength = Path.IndexOf('/');
                TypePath = Path.Substring(typeDirectoryNameLength + 1);
                Type = DirectoryToType[Path.Substring(0, typeDirectoryNameLength)];
            }
        }

        public string RootPath { get; private set; }

        public string Path { get; private set; }

        public string TypePath { get; private set; }

        public string FullPath { get; private set; }

        public RadianFileType Type { get; private set; }

        public bool Exists
        {
            get
            {
                return System.IO.File.Exists(FullPath);
            }
        }

        public static RadianFile Create(string rootPath, string fullPath)
        {
            var path = GetPath(rootPath, fullPath).TrimStart('\\').Replace('\\', '/');
            return new RadianFile(rootPath, path);
        }

        public static RadianFile Create(string rootPath, RadianFileType type, string typePath)
        {
            string typeDirectory;
            var path = TypeToDirectory.TryGetValue(type, out typeDirectory)
                ? string.Concat(typeDirectory, "/", typePath)
                : typePath;

            return new RadianFile(rootPath, path);
        }

        public FileStream GetFileStream()
        {
            return File.OpenRead(FullPath);
        }

        public string GetText()
        {
            return File.ReadAllText(FullPath);
        }

        public ContentItem GetContentItem()
        {
            ContentItem item = GetContentItem<ExpandoObject>();
            item.Data.__content = item.Content;
            return item;
        }

        public ContentItem<TData> GetContentItem<TData>()
            where TData : class
        {
            var info = new FileInfo(FullPath);
            var contentItem = new ContentItem<TData>();
            
            using (var fileStream = GetFileStream())
            using (var streamReader = new ContentItemStreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                contentItem.Path = Path;
                contentItem.CreatedOn = info.CreationTimeUtc;
                contentItem.ModifiedOn = info.LastWriteTimeUtc;
                contentItem.Data = _serializer.Deserialize<TData>(jsonReader);
                fileStream.Position = streamReader.TemplateStartPosition;
                contentItem.Content = new StreamReader(streamReader.BaseStream).ReadToEnd();
            }

            return contentItem;
        }

        public static string GetPath(string rootPath, string fullPath)
        {
            if (rootPath == null)
            {
                throw new ArgumentNullException("rootPath");
            }
            if (fullPath == null)
            {
                throw new ArgumentNullException("fullPath");
            }
            if (!fullPath.StartsWith(rootPath))
            {
                throw new InvalidOperationException("Full Path does not correspond to the specified Site Path");
            }

            return fullPath.Substring(rootPath.Length);
        }
    }
}
