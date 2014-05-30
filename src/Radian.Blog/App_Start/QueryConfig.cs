using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Radian.Core;
using Radian.Core.Indexing;
using Radian.Core.IO;
using Radian.Core.Querying;

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
            var queryContentItems = loader.LoadAll<QueryMetadata>("Queries");

            foreach (var query in queryContentItems)
            {
                var key = query.Path.Replace('\\', '/');
                queries.Add(key, queryBuilder.Build(query.Data, configuration));
            }
        }
    }
}