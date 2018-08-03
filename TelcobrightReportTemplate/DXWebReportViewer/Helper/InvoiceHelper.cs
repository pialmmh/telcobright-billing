using ReportGenerator.reports.invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DXWebReportViewer.Helper
{
    public static class InvoiceHelper
    {
        public static Invoice IGWInternational_Incoming_To_Foreign_Carrier()
        {
            return new Invoice()
            {
                Partner = GetPartner(),
                Type = "INT",
                BillingFrom = new DateTime(2018, 4, 9),
                BillingTo = new DateTime(2018, 4, 15),
                InvoiceDate = new DateTime(2018, 4, 16),
                DueDate = new DateTime(2018, 4, 22),
                InvoiceNo = "BTEL/INV/INT/Spectron-18/04-121",
                Currency = "USD",
                TimeZone = "UTC+6",
                InvoiceItems = GetInvoiceItems()
            };
        }

        private static Partner GetPartner()
        {
            return new Partner()
            {
                PartnerName = "Spactron Ltd",
                PartnerAddress = "4th Floor, Serhal Building, Sami El Solh Av,\n" +
                    "Badaro Sector, Beirut, Lebanon",
                VatRegNo = "-"
            };
        }

        private static List<InvoiceItem> GetInvoiceItems()
        {
            /*
            List<InvoiceItem> invoiceItems = new List<InvoiceItem>();
            invoiceItems.Add(new InvoiceItem()
            {
                Reference = "Int'l Inbound Calls",
                Description = "Call Carrying Charges",
                Quantity = 23,
                UoM = "Calls",
                TotalMinutes = 7.22,
                Amount = 0.18m
            });
            return invoiceItems;
            */

            /*
            List<InvoiceItem> invoiceItems = new List<InvoiceItem>();
            invoiceItems.Add(new InvoiceItem()
            {
                Reference = "International Inbound Calls",
                Description = "Robi Axiata Limited",
                UoM = "Minutes",
                Quantity = 0.13,
                Revenue = 0.00m,
                Amount = 0.00m
            });
            invoiceItems.Add(new InvoiceItem()
            {
                Reference = "International Inbound Calls",
                Description = "BANGLALINK DIGITAL COMMUNICATIONS LTD.",
                UoM = "Minutes",
                Quantity = 0.13,
                Revenue = 0.00m,
                Amount = 0.00m
            });
            invoiceItems.Add(new InvoiceItem()
            {
                Reference = "International Inbound Calls",
                Description = "GrameenPhone Limited",
                UoM = "Minutes",
                Quantity = 6.67,
                Revenue = 0.17m,
                Amount = 0.17m
            });
            return invoiceItems;
            */

            return new List<InvoiceItem>
            {
                new InvoiceItem()
                {
                    Reference = "International Inbound Calls",
                    Date = new DateTime(2018, 4, 9),
                    UoM = "Minutes",
                    Quantity = 6.55,
                    Rate = 0.025m,
                    Revenue = 0.16m,
                    Amount = 0.16m
                },
                new InvoiceItem()
                {
                    Reference = "International Inbound Calls",
                    Date = new DateTime(2018, 4, 11),
                    UoM = "Minutes",
                    Quantity = 0.67,
                    Rate = 0.025001m,
                    Revenue = 0.02m,
                    Amount = 0.02m
                }
            };
        }
    }
}