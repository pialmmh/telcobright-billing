using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using TelcobrightMediation.MefData.GenericAssignment;

namespace LibraryExtensions
{
    public class MefGenericAssignmentFactoryComposer
    {
        [ImportMany("IGenericAssignmentFactory", typeof(IGenericParameterAssignmentFactory))]
        public IEnumerable<IGenericParameterAssignmentFactory> GenericAssignmentFactories { get; set; }

        public void Compose(string path = "")
        {
            string assemblyPath = (path == "" ? @"..\..\bin\Extensions\" : path);
            var catalog = new DirectoryCatalog(assemblyPath);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

    }
}