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
    public class DomesticReportRow
    {
        public string partnerName { get; set; }
        public Decimal noOfCalls { get; set; }
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
    public class MonthlyReportRow
    {
        public int SLNo { get; set; }

        public DateTime date { get; set; }
        public Decimal IntIncomingNoOfCalls { get; set; }
        public Decimal IntIncomingNoOfMinutes { get; set; }
        public Decimal IntOutgoingNoOfCalls { get; set; }
        public Decimal IntOutgoingNoOfMinutes { get; set; }
        public Decimal domNoOfCalls { get; set; }
        public Decimal domesticMinutes { get; set; }
    }



    public class MonthlyReportRowForExcel
    {
        //public  int  SlNo=0;
        public int SLNo { get; set; }
        public string date { get; set; }
        public Decimal IntIncomingNoOfCalls { get; set; }
        public Decimal IntIncomingNoOfMinutes { get; set; }
        public Decimal IntOutgoingNoOfCalls { get; set; }
        public Decimal IntOutgoingNoOfMinutes { get; set; }
        public Decimal domNoOfCalls { get; set; }
        public Decimal domesticMinutes { get; set; }

        public MonthlyReportRowForExcel(MonthlyReportRow report)
        {
            //SlNo += 1;
            this.SLNo = report.SLNo;
            this.date = report.date.ToString("yyyy-MM-dd");
            this.IntIncomingNoOfCalls = Math.Round(report.IntIncomingNoOfCalls);
            this.IntIncomingNoOfMinutes = Math.Round(report.IntIncomingNoOfMinutes, 0);
            this.IntOutgoingNoOfCalls = Math.Round(report.IntOutgoingNoOfCalls);
            this.IntOutgoingNoOfMinutes = Math.Round(report.IntOutgoingNoOfMinutes);
            this.domNoOfCalls = Math.Round(report.domNoOfCalls);
            this.domesticMinutes = Math.Round(report.domesticMinutes);

        }

    }
}