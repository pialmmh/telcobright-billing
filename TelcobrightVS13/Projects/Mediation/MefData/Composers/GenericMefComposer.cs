using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace TelcobrightMediation
{
    public class InvoiceSectionGeneratorComposer
    {
        [ImportMany("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
        public IEnumerable<IInvoiceSectionGenerator> InvoiceSectionGenerators { get; set; }
        public void Compose()
        {
            var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}