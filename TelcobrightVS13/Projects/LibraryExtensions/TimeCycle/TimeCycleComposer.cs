using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using TelcobrightMediation.Accounting;

namespace LibraryExtensions.TimeCycle
{
    public class TimeCycleComposer
    {
        [ImportMany("TimeCycle", typeof(ITimeCycle))]
        public IEnumerable<ITimeCycle> TimeCycles{ get; set; }
        public void Compose()
        {
            var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}