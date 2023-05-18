using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediationModel;
namespace PortalApp.ReportHelper
{

    public class BtrcReportRow
    {
        public string partnerName { get; set; }
        public Decimal minutes { get; set; }
    }
    public class InternationalReportRow
    {
        public string partnerName { get; set; }
        public Decimal inNoOfCalls { get; set; }
        public Decimal incomingMinutes { get; set; }
        public Decimal outNoOfCalls { get; set; }
        public Decimal outgoingMinutes { get; set; }

    }

}