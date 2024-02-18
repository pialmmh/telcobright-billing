using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using LibraryExtensions;

namespace TelcobrightMediation
{
    public class MefNerCalculationRuleComposer
    {
        [ImportMany("NerCalculationRule", typeof(INerCalculationRule))]
        public IEnumerable<INerCalculationRule> NerRules { get; set; }

        public void Compose()
        {
            UpwordPathFinder<DirectoryInfo> extFinder = new UpwordPathFinder<DirectoryInfo>("Extensions");
            string extPath = extFinder.FindAndGetFullPath();
            var catalog = new DirectoryCatalog(extPath);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}