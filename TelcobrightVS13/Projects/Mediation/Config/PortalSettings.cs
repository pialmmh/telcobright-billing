using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinSCP;
using TelcobrightFileOperations;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class PortalSettings
    { 
        public string PortalLocalAccountNameAdministrator { get; set; }
        public string PortalLocalAccountPassword { get; set; }
        public string HomePageUrl { get; set; }
        public string HomePageUrlForIcx { get; set; }
        public string AlternateDisplayName { get; set; }
        public List<InternetSite> PortalSites { get; set; }
        public PortalSettings(string sectionName)
        {
            this.PortalSites = new List<InternetSite>();
        }
        public PortalPageSettings PageSettings { get; set; }
        public Dictionary<string, object> DicConfigObjects { get; set; }
        public PortalSettings()
        {
            this.DicConfigObjects = new Dictionary<string, object>();
        }
        public Dictionary<string,int> RouteTypeEnums { get; set; }= new Dictionary<string, int>();
    }
}
