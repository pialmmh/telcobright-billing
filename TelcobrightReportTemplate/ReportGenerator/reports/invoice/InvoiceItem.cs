using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator.reports.invoice
{
    public class InvoiceItem
    {
        public string Reference { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public string UoM { get; set; }
        public decimal XAmount { get; set; }
        public decimal YAmount { get; set; }
        public double TotalMinutes { get; set; }
        public decimal? Rate { get; set; }
        public decimal Revenue { get; set; }
        public decimal Amount { get; set; }
        public decimal XYAmount { get; set; }
    }
}
