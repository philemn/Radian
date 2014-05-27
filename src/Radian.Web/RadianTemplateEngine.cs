using Radian.Controllers;
using Radian.Core;
using Radian.Core.IO;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Radian
{
    public interface IRadianTemplateEngine
    {
        string RenderView(string path, ExpandoObject model);
        string RenderPage(string path, Page model);
    }

    public class RadianTemplateEngine : IRadianTemplateEngine
    {
        private static ITemplateService _viewTemplateService;
        private static ITemplateService _pageTemplateService;
        private static Dictionary<string, string> _viewCacheNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _layoutCacheNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static RadianTemplateEngine()
        {
            var sitePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Site");
            var fileLocator = new FileLocator(sitePath);
            var pageLoader = new ContentItemLoader(fileLocator);
            var viewLoader = new ViewLoader(fileLocator);
            var layouts = pageLoader.LoadAll<Page>(Path.Combine(sitePath, "Layouts"));
            var pages = pageLoader.LoadAll<Page>(Path.Combine(sitePath, "Pages"));
            var views = viewLoader.LoadAll(sitePath);
            var pageTemplateServiceConfig = new TemplateServiceConfiguration { BaseTemplateType = typeof(RadianPageTemplate) };
            var viewTemplateServiceConfig = new TemplateServiceConfiguration { BaseTemplateType = typeof(RadianViewTemplate) };

            _pageTemplateService = new TemplateService(pageTemplateServiceConfig);
            _viewTemplateService = new TemplateService(viewTemplateServiceConfig);

            foreach (var layout in layouts)
            {
                _layoutCacheNames.Add(layout.Path, layout.Path);
                _pageTemplateService.Compile(layout.Content, typeof(ExpandoObject), layout.Path);
            }

            foreach (var page in pages)
            {
                if (!string.IsNullOrEmpty(page.Data.Layout))
                {
                    string layoutName;
                    if (!_layoutCacheNames.TryGetValue(page.Data.Layout, out layoutName))
                    {
                        throw new InvalidDataException(string.Format("Unable to locate layout '{0}'", page.Data.Layout));
                    }

                    page.Content = string.Concat(@"@{Layout=""", layoutName, @""";}", page.Content);
                }

                _pageTemplateService.Compile(page.Content, typeof(ExpandoObject), page.Path);
            }

            foreach (var view in views)
            {
                _viewCacheNames.Add(view.Path, view.Path);
                _viewTemplateService.Compile(view.Template, typeof(ExpandoObject), view.Path);
            }
        }
        public string RenderView(string path, ExpandoObject model)
        {
            string cacheName;

            if (!_viewCacheNames.TryGetValue(path, out cacheName))
            {
                throw new ArgumentOutOfRangeException("path", "The specified view path does not exists in the view cache");
            }

            return _viewTemplateService.Run(cacheName, model, null);
        }

        public string RenderPage(string path, Page model)
        {
            if (!_pageTemplateService.HasTemplate(path))
            {
                throw new ArgumentOutOfRangeException("path", "The specified page path does not exists in the page cache");
            }

            return _pageTemplateService.Run(path, model, null);
        }
    }
}