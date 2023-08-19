using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace LibraryExtensions
{
    public class AutoCreateJobComposer
    {
        [ImportMany("AutoCreateJob", typeof(IAutoCreateJob))]
        public IEnumerable<IAutoCreateJob> AutoCreateJobs { get; set; }
        public void Compose()
        {
            var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}