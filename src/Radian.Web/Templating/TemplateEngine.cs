﻿using Radian.Core;
using Radian.Core.IO;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Radian.Web.Templating
{
    public interface ITemplateEngine
    {
        string RenderView(string path, ExpandoObject model);
        string RenderPage(string path, Page model);
    }

    public class TemplateEngine : ITemplateEngine
    {
        private ITemplateService _viewTemplateService;
        private ITemplateService _pageTemplateService;
        private ConcurrentDictionary<string, string> _viewCacheNames = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, string> _layoutCacheNames = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public TemplateEngine()
        {
            var locator = RadianConfiguration.FileLocator;
            var viewLoader = new ViewLoader(new FileLocator(RadianConfiguration.SitePath));
            var layouts = locator.FindFiles("Layouts").Select(x => x.GetContentItem<Page>());
            var pages = locator.FindFiles("Pages").Select(x => x.GetContentItem<Page>());
            var views = locator.FindFiles("Views").Select(x => new View { Path = x.Path, Template = x.GetText() });
            var pageTemplateServiceConfig = new TemplateServiceConfiguration { BaseTemplateType = typeof(PageTemplate) };
            var viewTemplateServiceConfig = new TemplateServiceConfiguration { BaseTemplateType = typeof(ViewTemplate) };

            _pageTemplateService = new TemplateService(pageTemplateServiceConfig);
            _viewTemplateService = new TemplateService(viewTemplateServiceConfig);

            Parallel.ForEach(layouts, layout =>
            {
                _layoutCacheNames.TryAdd(layout.Path, layout.Path);
                _pageTemplateService.Compile(layout.Content, typeof(ExpandoObject), layout.Path);
            });

            Parallel.ForEach(pages, page =>
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
            });

            Parallel.ForEach(views, view =>
            {
                _viewCacheNames.TryAdd(view.Path, view.Path);
                _viewTemplateService.Compile(view.Template, typeof(ExpandoObject), view.Path);
            });
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