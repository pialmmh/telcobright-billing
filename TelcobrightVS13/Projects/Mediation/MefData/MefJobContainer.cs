using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class MefJobContainer
    {
        public MefJobComposer CmpJob = new MefJobComposer();
        public IDictionary<string, ITelcobrightJob> DicExtensions = new Dictionary<string, ITelcobrightJob>();
        public IDictionary<string, ITelcobrightJob> DicExtensionsIdJobWise = new Dictionary<string, ITelcobrightJob>();
    }
}