using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Core
{
    public class ContentItem : ContentItem<dynamic>
    {
    }

    public class ContentItem<TData>
        where TData : class
    {
        public virtual string Path { get; set; }
        public virtual DateTime CreatedOn { get; set; }
        public virtual DateTime ModifiedOn { get; set; }
        public virtual TData Data { get; set; }
        public virtual string Content { get; set; }

        public static implicit operator ContentItem(ContentItem<TData> item)
        {
            return new ContentItem
            {
                Path = item.Path,
                CreatedOn = item.CreatedOn,
                ModifiedOn = item.ModifiedOn,
                Data = item.Data,
                Content = item.Content
            };
        }
    }
}
