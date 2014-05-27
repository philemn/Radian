using Radian.Core;
using Radian.Core.Indexing;
using Radian.Core.IO;
using Radian.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Radian
{
    public class QueryConfig
    {
        public static void CacheQueries(Dictionary<string, IContentQuery> queries)
        {
            var sitePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Site");
            var contentPath = Path.Combine(sitePath, "Content");
            var locator = new FileLocator(sitePath);
            var loader = new ContentItemLoader(locator);
            var queryBuilder = new QueryBuilder();
            var index = new ContentItemIndex(locator, "Content");
            var configuration = new QueryConfiguration { ContentPath = contentPath, ContentItemIndex = index };
            var files = loader.LoadAll<QueryMetadata>("Queries");

            foreach (var file in files)
            {
                var key = file.Path.Replace('\\', '/');
                queries.Add(key, queryBuilder.Build(file.Data.Map, configuration));
            }
        }
    }
}