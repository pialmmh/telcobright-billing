using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QuartzTelcobright;
using TelcobrightMediation.Config;


namespace InstallConfig
{
    class MefConfigImportComposer
    {
        [ImportMany(typeof(AbstractConfigGenerator))]
        public IEnumerable<AbstractConfigGenerator> OperatorWiseMainConfigGenerators { get; private set; }
        public MefConfigImportComposer()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ConfigGeneratorMain).Assembly));
            CompositionContainer container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
        public IEnumerable<AbstractConfigGenerator> Compose()
        {
            return this.OperatorWiseMainConfigGenerators.ToList();
        }
    }

}
