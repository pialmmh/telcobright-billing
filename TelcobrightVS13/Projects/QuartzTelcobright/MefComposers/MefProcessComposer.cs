using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace QuartzTelcobright.MefComposer
{
    [Export("MefComposer", typeof(IMefComposer))]
    public class MefProcessComposer:IMefComposer
    {
        public string Type => "AbstractTelcobrightProcess";
        [ImportMany("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
        private IEnumerable<AbstractTelcobrightProcess> TelcobrightProcesses { get; set; }
        private Dictionary<string,object> ComposedObjects { get; set; }
        public Dictionary<string, object> Compose(CompositionContainer container)
        {
            container.ComposeParts(this);
            this.ComposedObjects=new Dictionary<string, object>();
            foreach (ITelcobrightProcess  process in this.TelcobrightProcesses)
            {
                object o = (object) process;
                this.ComposedObjects.Add(process.ProcessId.ToString(),o);
            }
            return this.ComposedObjects;
        }

    }
}
