using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormSSHTest
{
    public class Phone
    {
        public string name { get; set; }
        public string extension { get; set; }
        public string host { get; set; }
        public bool dynamic { get; set; }
        public string statusText { get; set; }
        
        public string GetStatus()
        {
            return dynamic == true ? (host.Contains("Unspecified")?"_":"Online")
                : statusText.Contains("(") ? "Online" : "_";
        }


    }
}
