using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Radian.Core;
using Radian.Core.IO;
using System.Dynamic;
using System.ComponentModel;
using Radian.Core.Indexing;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis.Standard;
using Version = Lucene.Net.Util.Version;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Radian.Core.Querying
{
    public interface IQueryBuilder
    {
        IContentQuery Build(QueryMetadata query, QueryConfiguration configuration);
    }

    public class QueryBuilder : IQueryBuilder
    {
        private const string ContentQueryTemplate = @"
namespace Radian.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Radian.Core;
    using Radian.Core.Querying;

    public class CompiledContentQuery : ContentQuery
    {
        public CompiledContentQuery(QueryConfiguration configuration) : base(configuration)
        {
        }

        public override IEnumerable<ContentItem> Query(IDictionary<string, object> tokens)
        {
            var query = from item in content() select item;
            return query;
        }
    }
}";
        private static readonly SyntaxTree QuerySyntaxTree = CSharpSyntaxTree.ParseText(ContentQueryTemplate);

        public IContentQuery Build(QueryMetadata query, QueryConfiguration configuration)
        {
            var root = QuerySyntaxTree.GetRoot();
            var replacement = SyntaxFactory.ParseExpression(query.Map) as QueryExpressionSyntax;
            var existing = root.DescendantNodes().OfType<QueryExpressionSyntax>().First();

            if (replacement == null)
            {
                throw new ArgumentException("The supplied map value is not valid LINQ query expression syntax", "query");
            }

            var syntaxTree = root.ReplaceNode(existing, replacement).SyntaxTree;
            
            if (query.Limit.HasValue)
            {
                var returnStatement = syntaxTree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().First();
                var newReturnStatement = SyntaxFactory.ReturnStatement(
                    SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("query"),
                            SyntaxFactory.IdentifierName("Take")),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[] {
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        SyntaxFactory.Literal(query.Limit.Value)))
                            }))),
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                
                syntaxTree = syntaxTree.GetRoot().ReplaceNode(returnStatement, newReturnStatement).SyntaxTree;
            }

            var references = new List<MetadataReference>()
            {
                new MetadataFileReference(typeof (object).Assembly.Location),
                new MetadataFileReference(typeof (Enumerable).Assembly.Location),
                new MetadataFileReference(typeof (Radian.Core.ContentItem).Assembly.Location),
                new MetadataFileReference(typeof (IContentQuery).Assembly.Location),
                new MetadataFileReference(typeof (Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location)
            };

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var compilation = CSharpCompilation.Create("Radian.Query", new[] { syntaxTree }, references, compilationOptions);

            var diagnostics = compilation.GetDiagnostics().ToList();
            LogDiagnostics(diagnostics);

            var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);

            LogDiagnostics(emitResult.Diagnostics);

            var assembly = Assembly.Load(stream.GetBuffer());
            return (IContentQuery)Activator.CreateInstance(assembly.GetTypes().First(x => typeof(IContentQuery).IsAssignableFrom(x)), configuration);
        }

        private void LogDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            var list = diagnostics.ToList();
            if (!list.Any()) return;

            Console.WriteLine();
            Console.WriteLine("The following errors were encountered while parsing the query ...");
            Console.WriteLine();

            foreach (var d in list)
            {
                var lineSpan = d.Location.GetLineSpan();
                var startLine = lineSpan.StartLinePosition.Line;
                Console.WriteLine("Line {0}: {1}", startLine, d.GetMessage());
            }

            Console.WriteLine();

            if (diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error))
            {
                throw new Exception("An error was encountered while building query");
            }
        }
    }

    public interface IContentQuery
    {
        IEnumerable<ContentItem> Query(IDictionary<string, object> queryTokens);
    }

    public abstract class ContentQuery : IContentQuery
    {
        private readonly IContentItemLoader _loader;
        protected QueryConfiguration Configuration { get; private set; }

        public ContentQuery(QueryConfiguration configuration)
        {
            _loader = new ContentItemLoader(new FileLocator(configuration.ContentPath));
            Configuration = configuration;
        }

        public abstract IEnumerable<ContentItem> Query(IDictionary<string, object> queryTokens);

        protected IEnumerable<ContentItem> Content(string path = "")
        {
            var searchPath = Path.Combine(Configuration.ContentPath, path);
            return _loader.LoadAll(searchPath);
        }

        protected IEnumerable<ContentItem> content(string path = "")
        {
            return Content(path);
        }

        protected IEnumerable<ContentItem> Search(string query, string path = null)
        {
            return Search(query, path, Configuration.SearchLimit);
        }

        protected IEnumerable<ContentItem> search(string query, string path = null)
        {
            return Search(query, path, Configuration.SearchLimit);
        }

        protected IEnumerable<ContentItem> Search(string query, int limit)
        {
            return Search(query, null, limit);
        }

        protected IEnumerable<ContentItem> search(string query, int limit)
        {
            return Search(query, null, limit);
        }

        protected IEnumerable<ContentItem> Search(string query, string path, int limit)
        {
            var searchDirectory = Path.Combine(Configuration.ContentPath, path);
            return Configuration.ContentItemIndex.Search(query, limit, searchDirectory);
        }

        protected IEnumerable<ContentItem> search(string query, string path, int limit)
        {
            return Search(query, path, limit);
        }

        protected ContentItem Computed<T>(T data)
            where T : class
        {
            return new ContentItem { Data = ConvertToExpando(data) };
        }

        protected ContentItem computed<T>(T data)
            where T : class
        {
            return Computed(data);
        }

        private static ExpandoObject ConvertToExpando<T>(T obj)
        {
            var expando = new ExpandoObject();
            IDictionary<string, object> dict = expando;

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(typeof(T)))
            {
                dict.Add(property.Name, property.GetValue(obj));
            }

            return expando;
        }
    }
}
