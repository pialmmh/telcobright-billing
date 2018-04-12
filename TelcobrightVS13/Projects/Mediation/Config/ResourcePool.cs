using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class ResourcePool
    {
        public Dictionary<string, WinSCP.Session> WinscpSessionPool { get; set; }
        public ResourcePool()
        {
            this.WinscpSessionPool = new Dictionary<string, WinSCP.Session>();
        }
    }
}
