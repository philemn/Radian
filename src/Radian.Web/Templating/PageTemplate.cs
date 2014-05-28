using Radian.Core;
using Radian.Core.IO;
using Radian.Core.Querying;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Radian.Web.Templating
{
    public class PageTemplate : RazorEngine.Templating.TemplateBase<Page>
    {
        public Page Page { get { return Model as Page; } }

        public RawString Placeholder(string placeHolderName)
        {
            var pageViews = Page.Placeholders[placeHolderName];
            var sb = new StringBuilder();
            var templateEngine = RadianConfiguration.TemplateEngine;

            foreach (var pageView in pageViews)
            {
                IContentQuery contentQuery;
                var contentItems = QueryCache.Queries.TryGetValue(pageView.Data, out contentQuery)
                    ? contentQuery.Query(Page.Tokens)
                    : new[] { new RadianFile(RadianConfiguration.SitePath, pageView.Data).GetContentItem() };

                foreach (var contentItem in contentItems)
                {
                    sb.AppendLine(templateEngine.RenderView(pageView.View, contentItem.Data));
                }
            }

            return new RawString(sb.ToString());
        }

        public IEnumerable<ContentItem> Query(string query)
        {
            IContentQuery contentQuery;

            if (!QueryCache.Queries.TryGetValue(query, out contentQuery))
            {
                throw new ArgumentOutOfRangeException("query", string.Format("Unable to find query '{0}'", query));
            }

            return contentQuery.Query(Page.Tokens);
        }

        public string UrlEncode(string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        private IEnumerable<ContentItem> GetQueryContent(IContentQuery query)
        {
            var tokens = new Dictionary<string, object>();
            return query.Query(tokens);
        }
    }
}
