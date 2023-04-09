using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuartzTelcobright.MefComposer;

namespace QuartzTelcobright.MefComposers
{
    public class MefProcessContainer
    {
        public Dictionary<string, ITelcobrightProcess> Processes { get; set; }

        public MefProcessContainer(MefCollectiveAssemblyComposer mefCollectiveAssemblyComposer)
        {
            this.Processes = new Dictionary<string, ITelcobrightProcess>();
            foreach (KeyValuePair<string, object> keyValuePair in mefCollectiveAssemblyComposer
                .ComposedMefDictionaryBytype["ITelcobrightProcess"])
            {
                this.Processes.Add(keyValuePair.Key, (ITelcobrightProcess) keyValuePair.Value);
            }
        }
    }
}
