using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Radian.Web.Templating
{
    public class ViewTemplate : RazorEngine.Templating.TemplateBase<dynamic>
    {
        public ViewTemplate()
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
}
