using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator.reports.invoice
{
    public enum InvoiceReportType
    {
        [Description("IGW International Incoming To Foreign Carrier")]
        InternationalIncomingToForeignCarrier,
        [Description("IGW International Incoming To Foreign Carrier Details 1")]
        InternationalIncomingToForeignCarrierDetails1,
        [Description("IGW International Incoming To Foreign Carrier Details 2")]
        InternationalIncomingToForeignCarrierDetails2,
        [Description("IGW International Outgoing To IOS")]
        InternationalOutgoingToIOS,
        [Description("IGW International Outgoing To IOS Details 1")]
        InternationalOutgoingToIOSDetails1,
        [Description("IGW International Outgoing To IOS Details 2")]
        InternationalOutgoingToIOSDetails2,

        [Description("ICX Domestic To ANS")]
        DomesticToANS,
        [Description("ICX Domestic To ANS Details 1")]
        DomesticToANSDetails1,
        [Description("ICX Domestic To ANS Details 2")]
        DomesticToANSDetails2
    }
}
