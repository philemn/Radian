using Newtonsoft.Json;
using Radian.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Configuration;
using System.Text;
using System.Web.Hosting;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Collections;
using RazorEngine.Text;
using Radian.Core.IO;
using Radian.Query;

namespace Radian.Controllers
{
    public class PageController : Controller
    {
        private readonly IRadianTemplateEngine _templateEngine;

        public PageController() : this(new RadianTemplateEngine())
        {
        }

        public PageController(IRadianTemplateEngine templateEngine)
        {
            _templateEngine = templateEngine;
        }

        public ActionResult Render(ContentItem<Page> page)
        {
            page.Data.Tokens = ControllerContext.RouteData.Values;

            foreach (var token in Request.QueryString.AllKeys)
            {
                page.Data.Tokens[token] = Request.QueryString[token];
            }

            var content = _templateEngine.RenderPage(page.Path, page.Data);
            return Content(content);
        }
    }

    public class RadianPageTemplate : RazorEngine.Templating.TemplateBase<Page>
    {
        private readonly string _sitePath;
        private readonly ContentItemLoader _loader;
        private readonly QueryBuilder _queryBuilder;
        private readonly QueryConfiguration _queryConfiguration;

        public Page Page { get { return Model as Page; } }

        public RadianPageTemplate()
        {
            _sitePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Site");
            _loader = new ContentItemLoader(new FileLocator(_sitePath));
            _queryBuilder = new QueryBuilder();
            _queryConfiguration = new QueryConfiguration { ContentPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Site\\Content") };
        }
        
        public RawString Placeholder(string placeHolderName)
        {
            var pageViews = Page.Placeholders[placeHolderName];
            var sb = new StringBuilder();
            var templateEngine = new RadianTemplateEngine();

            foreach (var pageView in pageViews)
            {
                IContentQuery contentQuery;
                var contentItems = QueryCache.Queries.TryGetValue(pageView.Data, out contentQuery)
                    ? contentQuery.Query(Page.Tokens)
                    : new[] { new RadianFile(_sitePath, pageView.Data).GetContentItem() };
                
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

    public class RadianViewTemplate : RazorEngine.Templating.TemplateBase<dynamic>
    {
        public RadianViewTemplate()
        {
            Format = new FormatHelper();
        }
        public dynamic Content { get { return Model; } }

        public FormatHelper Format { get; private set; }

        public string UrlEncode(string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        public RawString RenderContent()
        {
            return new RawString(Model.__content);
        }
    }

    public class FormatHelper
    {
        public string Date(object date)
        {
            return Date(date, "yyyy-MM-dd");
        }

        public string Date(object date, string format)
        {
            return Convert.ToDateTime(date).ToString(format);
        }
    }
}
