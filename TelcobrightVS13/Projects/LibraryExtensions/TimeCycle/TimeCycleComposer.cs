using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace LibraryExtensions.TimeCycle
{
    public class TimeCycleComposer
    {
        [ImportMany("TimeCycle", typeof(ITimeCycle))]
        public IEnumerable<ITimeCycle> TimeCycles{ get; set; }
        public void ComposeFromPath(string path)
        {
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}