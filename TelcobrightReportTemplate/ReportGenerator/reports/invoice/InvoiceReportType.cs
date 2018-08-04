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
        DomesticToANSDetails2,
        [Description("ICX International Outgoing To ANS")]
        InternationalOutgoingToANS,
        [Description("ICX International Outgoing To ANS Details 1")]
        InternationalOutgoingToANSDetails1,
        [Description("ICX International Outgoing To ANS Details 2")]
        InternationalOutgoingToANSDetails2,
        [Description("ICX International To IOS")]
        InternationalToIOS,
        [Description("ICX International To IOS Details 1")]
        InternationalToIOSDetails1,
        [Description("ICX International To IOS Details 2")]
        InternationalToIOSDetails2,
        [Description("ICX LTFS To IPTSP")]
        LTFSToIPTSP,
        [Description("ICX LTFS To IPTSP Details 1")]
        LTFSToIPTSPDetails1,
        [Description("ICX LTFS To IPTSP Details 2")]
        LTFSToIPTSPDetails2
    }
}
