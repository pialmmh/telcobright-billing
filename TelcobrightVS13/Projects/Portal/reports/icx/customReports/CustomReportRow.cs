using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediationModel;
namespace PortalApp.ReportHelper
{
    public class customBlCallForwarding
    {
        public DateTime startTime { get; set; }
        public string SourceNetwork { get; set; }
        public string DestinationNetwork { get; set; }
        public Decimal CallerNumber_ANUM_ { get; set; }        
        public Decimal CalledNumber_BNum_ { get; set; }
        public int BilledDuration { get; set; }
        public Decimal RedirectNumber { get; set; }
        public string Remarks { get; set; }        
    }
    public class customBlCallRoaming
    {
        public DateTime startTime { get; set; }
        public string SourceNetwork { get; set; }
        public string DestinationNetwork { get; set; }
        public Decimal CallerNumber_ANUM_ { get; set; }
        public Decimal CalledNumber_BNum_ { get; set; }        
        public Decimal RedirectNumber { get; set; }
        public int BilledDuration { get; set; }
        public string Remarks { get; set; }
    }

}