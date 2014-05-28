using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Web.Templating
{
    public class FormatHelper
    {
        public string Date(object date)
        {
            return Date(date, RadianConfiguration.Formatting.DateFormatDefault);
        }

        public string Date(object date, string format)
        {
            return Convert.ToDateTime(date).ToString(format);
        }
    }
}
