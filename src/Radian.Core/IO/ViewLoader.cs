using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Core.IO
{
    public interface IViewLoader
    {
        IEnumerable<RadianView> LoadAll(string sitePath);
    }

    public class ViewLoader : IViewLoader
    {
        private readonly IFileLocator _locator;

        public ViewLoader(IFileLocator locator)
        {
            _locator = locator;
        }

        public IEnumerable<RadianView> LoadAll(string sitePath)
        {
            var viewPath = Path.Combine(sitePath, "Views");
            var files = _locator.FindFiles(viewPath);
            foreach (var file in files)
            {
                yield return new RadianView
                {
                    Path = file.Path,
                    Template = file.GetText()
                };
            }
        }
    }
}
