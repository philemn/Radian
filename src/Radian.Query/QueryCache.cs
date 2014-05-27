using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Query
{
    public static class QueryCache
    {
        static QueryCache()
        {
            Queries = new Dictionary<string, IContentQuery>(StringComparer.OrdinalIgnoreCase);
        }

        public static Dictionary<string, IContentQuery> Queries { get; set; }
    }
}
