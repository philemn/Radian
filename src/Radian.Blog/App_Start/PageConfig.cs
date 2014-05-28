using Radian.Core;
using Radian.Core.IO;
using Radian.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace Radian
{
    public class PageConfig
    {
        public static void RegisterPages(RouteCollection routes)
        {
            var sitePath = RadianConfiguration.SitePath;
            var loader = new ContentItemLoader(new FileLocator(sitePath));

            foreach (var page in loader.LoadAll<Page>("Pages"))
            {
                routes.MapRoute(
                    name: "Page: " + page.Data.Url,
                    url: page.Data.RouteUrl,
                    defaults: new { controller = "Page", action = "Render", page = page });
            }

            routes.MapRoute(
                name: "Page",
                url: "{*pagePath}",
                defaults: new { controller = "Page", action = "Render" });
        }
    }
}