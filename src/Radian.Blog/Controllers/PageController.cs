using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Hosting;
using System.Dynamic;
using System.Collections;
using Radian.Core;
using Radian.Core.IO;
using Radian.Core.Querying;
using Radian.Web.Templating;
using Radian.Web;

namespace Radian.Blog.Controllers
{
    public class PageController : Controller
    {
        private readonly ITemplateEngine _templateEngine;

        public PageController()
        {
            _templateEngine = RadianConfiguration.TemplateEngine;
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
}
