using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;
using Radian.Core.IO;
using System.Collections.Concurrent;
using Lucene.Net.QueryParsers;

namespace Radian.Core.Indexing
{
    public class ContentItemIndexer : IDisposable
    {
        private const string ID_FIELD = "__id";
        private const string TYPE_FIELD = "__type";
        private readonly IndexWriter _indexWriter;
        private ConcurrentDictionary<string, RadianFile> _indexedFiles;
        private bool _disposed;

        public ContentItemIndexer()
        {
            var directory = new RAMDirectory();
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            _indexWriter = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            _indexedFiles = new ConcurrentDictionary<string, RadianFile>();
        }

        public IEnumerable<string> IndexedFields
        {
            get
            {
                using (var reader = _indexWriter.GetReader())
                {
                    return reader.GetFieldNames(IndexReader.FieldOption.INDEXED);
                }
            }
        }

        public IEnumerable<ContentItem> Search(string query, int limit, string searchDirectory)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                yield break;
            }

            var fields = IndexedFields.ToArray();
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var parser = new MultiFieldQueryParser(Version.LUCENE_30, fields, analyzer);

            using (var searcher = GetSearcher())
            {
                var luceneQuery = parser.Parse(query);

                var results = string.IsNullOrEmpty(searchDirectory)
                    ? searcher.Search(luceneQuery, limit)
                    : searcher.Search(luceneQuery, new PrefixFilter(new Term(ID_FIELD, searchDirectory)), limit);

                for (var i = 0; i < results.TotalHits; i++)
                {
                    var scoreDoc = results.ScoreDocs[i];
                    var doc = searcher.Doc(scoreDoc.Doc);
                    var path = doc.GetField(ID_FIELD);

                    RadianFile file;
                    if (_indexedFiles.TryGetValue(path.StringValue, out file))
                    {
                        yield return file.GetContentItem();
                    }
                }
            }
        }

        public IndexSearcher GetSearcher()
        {
            return new IndexSearcher(_indexWriter.GetReader());
        }

        public void Index(RadianFile file)
        {
            using (var searcher = GetSearcher())
            {
                var term = GetTerm(file);
                var document = BuildDocument(file);
                if (searcher.IndexReader.TermDocs(term).Next())
                {
                    _indexWriter.UpdateDocument(term, document);
                }
                else
                {
                    _indexWriter.AddDocument(document);
                    _indexedFiles.TryAdd(file.FullPath, file);
                }
            }
        }

        public void Index(IEnumerable<RadianFile> files)
        {
            foreach (var file in files)
            {
                Index(file);
            }
        }

        public void Delete(RadianFile file)
        {
            _indexWriter.DeleteDocuments(GetTerm(file));
            _indexedFiles.TryRemove(file.FullPath, out file);
        }

        private Document BuildDocument(RadianFile file)
        {
            var item = file.GetContentItem();
            var document = new Document();

            document.Add(new Field(ID_FIELD, file.FullPath, Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(TYPE_FIELD, file.Type.ToString(), Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));

            IDictionary<string, object> properties = item.Data;
            foreach (var property in properties.Where(x => x.Value.GetType() == typeof(string)))
            {
                document.Add(new Field(property.Key, property.Value as string, Field.Store.YES, Field.Index.ANALYZED));
            }

            return document;
        }

        private Term GetTerm(RadianFile file)
        {
            return new Term(ID_FIELD, file.FullPath);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _indexWriter.Dispose();
                _disposed = true;
            }
        }
    }
}
