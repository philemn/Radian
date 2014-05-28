using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Web
{
    public class Page
    {
        public string Url { get; set; }
        public string Layout { get; set; }
        public string Title { get; set; }
        public IDictionary<string, IList<Placeholder>> Placeholders { get; set; }

        public string RouteUrl
        {
            get
            {
                return Url.Substring(1);
            }
        }

        public IDictionary<string, object> Tokens { get; set; }
    }
}
