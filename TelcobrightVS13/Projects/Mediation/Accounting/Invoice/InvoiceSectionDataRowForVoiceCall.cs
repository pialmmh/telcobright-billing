using System;

namespace TelcobrightMediation
{
    public class InvoiceSectionDataRowForVoiceCall
    {
        public string InPartnerName { get; set; }
        public string OutPartnerName { get; set; }
        public string Reference { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public string UoM { get; set; }
        public int TotalCalls { get; set; }
        public double TotalMinutes { get; set; }
        public decimal? Rate { get; set; }
        public decimal Revenue { get; set; }
        public decimal Amount { get; set; }
        public decimal XAmount { get; set; }
        public decimal YAmount { get; set; }
        public decimal XYAmount { get; set; }
    }
}
