using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using LibraryExtensions;

namespace TelcobrightMediation
{
    public class AutomationContainer
    {
        [ImportMany("Automation", typeof(IAutomation))]
        public IEnumerable<IAutomation> LoadedAutomations { get; set; }

        public Dictionary<string, IAutomation> Automations = new Dictionary<string, IAutomation>();

        public void Compose()
        {
            UpwordPathFinder<DirectoryInfo> extFinder = new UpwordPathFinder<DirectoryInfo>("Extensions");
            string extPath= extFinder.FindAndGetFullPath();
            var catalog = new DirectoryCatalog(extPath);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
            this.Automations = this.LoadedAutomations.ToDictionary(a => a.RuleName);
        }

        public void ComposeFromPath(string path)
        {
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

    }
}