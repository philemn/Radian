using Radian.Core.Indexing;
using Radian.Core.IO;
using Radian.Core.Querying;
using Radian.Web.Templating;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Web
{
    public static class RadianConfiguration
    {
        private static object SyncLock = new object();

        static RadianConfiguration()
        {
            Initialize();
        }

        public static string SitePath { get; private set; }
        public static IFileLocator FileLocator { get; set; }
        public static QueryConfiguration QueryConfiguration { get; set; }
        public static RadianFormatConfiguration Formatting { get; set; }
        public static ITemplateEngine TemplateEngine { get; set; }

        public static void Reset()
        {
            lock (SyncLock)
            {
                Initialize();
            }
        }
        
        private static void Initialize()
        {
            var sitePath = ConfigurationManager.AppSettings["RadianSitePath"];

            if (sitePath.StartsWith("~\\"))
            {
                sitePath = AppDomain.CurrentDomain.BaseDirectory + sitePath.Substring(2);
            }

            SitePath = sitePath;
            FileLocator = new FileLocator(SitePath);
            QueryConfiguration = new QueryConfiguration
            {
                ContentItemIndex = new ContentItemIndex(FileLocator, "Content"),
                ContentPath = Path.Combine(SitePath, "Content"),
                SearchLimit = 100
            };
            Formatting = new RadianFormatConfiguration
            {
                DateFormatDefault = "yyyy-MM-dd"
            };
            TemplateEngine = new TemplateEngine();
        }
    }

    public class RadianFormatConfiguration
    {
        public string DateFormatDefault { get; set; }
    }
}
