using Radian.Core.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Core.Querying
{
    public class QueryConfiguration
    {
        public string ContentPath { get; set; }
        public ContentItemIndex ContentItemIndex { get; set; }
        public int SearchLimit { get; set; }

        public QueryConfiguration()
        {
            SearchLimit = 100;
        }
    }
}
