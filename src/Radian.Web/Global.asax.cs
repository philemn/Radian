using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Radian.Core.Querying;

namespace Radian
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            QueryConfig.CacheQueries(QueryCache.Queries);
            PageConfig.RegisterPages(RouteTable.Routes);
            RadianConfig.ConfigureAutoRestart();
        }

        protected void Application_End()
        {
            RadianConfig.Unload();
        }
    }
}