using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace QuartzTelcobright.MefComposer
{
    [Export("MefComposer", typeof(IMefComposer))]
    public class MefProcessComposer:IMefComposer
    {
        public string Type => "ITelcobrightProcess";
        [ImportMany("TelcobrightProcess", typeof(ITelcobrightProcess))]
        private IEnumerable<ITelcobrightProcess> TelcobrightProcesses { get; set; }
        private Dictionary<string,object> ComposedObjects { get; set; }
        

        public Dictionary<string, object> Compose(CompositionContainer container)
        {
            container.ComposeParts(this);
            this.ComposedObjects=new Dictionary<string, object>();
            //return TelcobrightProcesses.ToDictionary(c => c.ProcessId.ToString());
            foreach (ITelcobrightProcess  process in this.TelcobrightProcesses)
            {
                object o = (object) process;
                this.ComposedObjects.Add(process.ProcessId.ToString(),o);
            }
            return this.ComposedObjects;
        }

    }
}
