using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace TelcobrightMediation
{
    public class InvoiceTemplateComposer
    {
        [ImportMany("InvoiceTemplate", typeof(IInvoiceTemplate))]
        public IEnumerable<IInvoiceTemplate> InvoiceTemplates { get; set; }
        public void ComposeFromPath(string catalogPath)
        {
            var catalog = new DirectoryCatalog(catalogPath);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}