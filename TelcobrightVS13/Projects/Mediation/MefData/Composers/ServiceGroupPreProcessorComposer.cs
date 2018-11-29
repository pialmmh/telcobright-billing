using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace TelcobrightMediation
{
    public class ServiceGroupPreProcessorComposer
    {
        [ImportMany("ServiceGroupPreProcessor", typeof(IServiceGroupPreProcessor))]
        public IEnumerable<IServiceGroupPreProcessor> ServiceGroupPreProcessors { get; set; }

        public void Compose()
        {
            var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
        public void ComposeFromPath(string catalogPath)
        {
            var catalog = new DirectoryCatalog(catalogPath);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}