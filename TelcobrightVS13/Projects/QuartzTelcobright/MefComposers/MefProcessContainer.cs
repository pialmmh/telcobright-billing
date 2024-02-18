using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuartzTelcobright.MefComposer;
using TelcobrightInfra;

namespace QuartzTelcobright.MefComposers
{
    public class MefProcessContainer
    {
        public Dictionary<string, AbstractTelcobrightProcess> Processes { get; set; }
        public MefProcessContainer(MefCollectiveAssemblyComposer mefCollectiveAssemblyComposer)
        {
            this.Processes = new Dictionary<string, AbstractTelcobrightProcess>();
            foreach (KeyValuePair<string, object> keyValuePair in mefCollectiveAssemblyComposer
                .ComposedMefDictionaryBytype["AbstractTelcobrightProcess"])
            {
                var process = (AbstractTelcobrightProcess) keyValuePair.Value;
                this.Processes.Add(keyValuePair.Key, process);
            }
        }
    }
}
