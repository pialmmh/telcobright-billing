using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using LibraryExtensions;

namespace TelcobrightMediation
{
    public class PartnerRuleComposer
    {
        [ImportMany("Partner", typeof(IPartnerRule))]
        public IEnumerable<IPartnerRule> Partners { get; set; }

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