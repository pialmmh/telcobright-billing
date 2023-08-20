using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace LibraryExtensions
{
    public class DigitRuleComposer
    {
        [ImportMany("DigitRule", typeof(IDigitRule))]
        public IEnumerable<IDigitRule> DigitRules{ get; set; }

        public void Compose()
        {
            var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}