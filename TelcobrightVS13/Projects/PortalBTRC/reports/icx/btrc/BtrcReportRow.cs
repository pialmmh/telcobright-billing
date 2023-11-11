using System;

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

    public class MonthlyOutSummary
    {
        public DateTime Date { get; set; }
        public string OriginatingANS { get; set; }
        public string TerminatingCarrier { get; set; }
        public int ICRouteName { get; set; }
        public int OGRouteName { get; set; }
        public string TerminatingRegion { get; set; }
        public int TotalCalls { get; set; }
        public int TotalSuccessfulCalls { get; set; }
        public Decimal TotalDuration { get; set; }
        public int TotalPaidMinute { get; set; }
        public Decimal ACD { get; set; }
        public int ASR { get; set; }
        public int CER { get; set; }
        public Decimal MHT { get; set; }
        public int XRate { get; set; }
        public Decimal YRate { get; set; }
        public Decimal ConversionRate { get; set; }
        public Decimal XAmount { get; set; }
        public Decimal YAmount { get; set; }
        public Decimal ZAmount { get; set; }     
        public Decimal Portion15PercOfZ { get; set; }

    }
    public class CustomReportRow
    {
        public string StartTime { get; set; }
        public string sourceNetwork { get; set; }
        public string destinationNtwork { get; set; }
        public string callerNumberANUM { get; set; }
        public string callerNumberBNUM { get; set; }
        public int billedDuration { get; set; }
        public string redirectNumber { get; set; }
        public string remarks { get; set; }

    }
}