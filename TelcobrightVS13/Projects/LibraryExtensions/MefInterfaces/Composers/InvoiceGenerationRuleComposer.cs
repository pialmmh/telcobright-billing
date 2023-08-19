using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace LibraryExtensions
{
    public class InvoiceGenerationRuleComposer
    {
        [ImportMany("InvoiceGenerationRule", typeof(IInvoiceGenerationRule))]
        public IEnumerable<IInvoiceGenerationRule> InvoiceGenerationRules { get; set; }

        public void Compose()
        {
            var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
        public void ComposeFromPath(string path)
        {
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}