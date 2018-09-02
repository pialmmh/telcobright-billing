using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace TelcobrightMediation
{
    public class RouteDisplayClassComposer
    {
        [ImportMany("DisplayClass", typeof(IDisplayClass))]
        public IEnumerable<IDisplayClass> DisplayClasses { get; set; }

        public void Compose(string path)
        {
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}