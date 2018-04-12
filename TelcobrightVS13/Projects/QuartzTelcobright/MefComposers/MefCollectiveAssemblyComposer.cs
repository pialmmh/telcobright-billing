using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using QuartzTelcobright.MefComposer;

namespace QuartzTelcobright.MefComposers
{
    public class MefCollectiveAssemblyComposer
    {
        private CompositionContainer Container { get; set; }
        [ImportMany("MefComposer", typeof(IMefComposer))]
        private IEnumerable<IMefComposer> AllComposers { get; set; }
        public Dictionary<string, Dictionary<string, object>> ComposedMefDictionaryBytype { get; private set; }
        public MefCollectiveAssemblyComposer(string assemblyCatalogLocation)
        {
            ComposeComposers();
            var catalog = new DirectoryCatalog(assemblyCatalogLocation);
            this.Container = new CompositionContainer(catalog);//directory catalog for all mef for the projects
            this.ComposedMefDictionaryBytype = new Dictionary<string, Dictionary<string, object>>();
            foreach (IMefComposer composer in this.AllComposers)
            {
                Dictionary<string, object> mefObjectsDictionary = composer.Compose(this.Container);
                this.ComposedMefDictionaryBytype.Add(composer.Type, mefObjectsDictionary);
            }
        }

        private void ComposeComposers()
        {
            var assemblyCatalog = new AggregateCatalog();
            //assemblyCatalog.Catalogs.Add(new AssemblyCatalog(typeof(MefCollectiveAssemblyComposer).Assembly));
            assemblyCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            CompositionContainer executingAssemblyContainer = new CompositionContainer(assemblyCatalog);
            executingAssemblyContainer.ComposeParts(this);
        }

    }
}
