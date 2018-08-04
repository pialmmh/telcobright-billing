using ReportGenerator.reports.invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DXWebReportViewer.Helper
{
    public static class InvoiceHelper
    {
        public static Invoice GetInvoice()
        {
            /* 
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
            */

            return new Invoice()
            {
                Partner = GetPartner(),
                Type = "ANS",
                BillingFrom = new DateTime(2018, 4, 9),
                BillingTo = new DateTime(2018, 4, 15),
                InvoiceDate = new DateTime(2018, 4, 16),
                DueDate = new DateTime(2018, 4, 22),
                InvoiceNo = "BTEL/INV/INT/Spectron-18/04-121",
                Currency = "BDT",
                TimeZone = "UTC+6",
                ConversionRate = 83.5m,
                ConversionRateDate = new DateTime(2018, 5, 1),
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

            /*
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
            */

            
            return new List<InvoiceItem>
            {
                new InvoiceItem()
                {
                    Reference = "Int'l Outbound Calls",
                    UoM = "Calls",
                    Quantity = 35964,
                    TotalMinutes = 50899.75,
                    XAmount = 402300.50m,
                    YAmount = 213702.15m,
                    Amount = 241991.90m
                }
            };
            

            /*
            return new List<InvoiceItem>
            {
                new InvoiceItem()
                {
                    Reference = "International Outbound Calls",
                    Date = new DateTime(2018, 4, 9),
                    Description = "BDCOMONLINE LTD. (IPTSP)",
                    UoM = "Minutes",
                    Quantity = 121.50,
                    XAmount = 758.50m,
                    YAmount = 322.57m,
                    Revenue = 387.96m,
                    Amount = 387.96m
                },
                new InvoiceItem()
                {
                    Reference = "International Outbound Calls",
                    Date = new DateTime(2018, 4, 9),
                    Description = "UNKNOWN",
                    UoM = "Minutes",
                    Quantity = 317.00,
                    XAmount = 2329.50m,
                    YAmount = 1218.06m,
                    Revenue = 1384.78m,
                    Amount = 1384.78m
                }

            };
            */
        }
    }
}