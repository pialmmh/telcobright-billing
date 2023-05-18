using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Config
{
    public class SimpleCacheSettings
    {
        public Dictionary<string, string> SimpleCachedItemsToBePopulated { get; set; }//dic for fast lookup, 2nd string (val) has no use
        public SimpleCacheSettings()
        {
            this.SimpleCachedItemsToBePopulated = new Dictionary<string, string>();
        }
    }
}
