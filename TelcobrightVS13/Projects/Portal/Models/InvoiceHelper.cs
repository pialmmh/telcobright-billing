using MediationModel;
using MySql.Data.MySqlClient;
using ReportGenerator.reports.invoice;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace PortalApp.Models
{
    public class InvoiceHelper
    {
        private invoice invoice;

        public InvoiceHelper(invoice invoice)
        {
            this.invoice = invoice;
        }

        public Invoice GetInvoice()
        {

            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(GetQuery(), connection);
                cmd.Connection = connection;
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataSet dataset = new DataSet();
                da.Fill(dataset);

                DataTable dt = dataset.Tables[0];
                bool hasRows = dataset.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);
                if (hasRows)
                {
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
                        InvoiceItems = GetInvoiceItems(dt)
                    };
                }
                else return null;
            }
        }

        private Partner GetPartner()
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                int idPartner = Convert.ToInt32(this.invoice.PARTY_ID);
                partner partner = context.partners.First(x => x.idPartner == idPartner);
                return new Partner()
                {
                    PartnerName = partner.PartnerName,
                    PartnerAddress = partner.Address1,
                    VatRegNo = "-"
                };
            }
        }

        private List<InvoiceItem> GetInvoiceItems(DataTable dt)
        {
            List<InvoiceItem> invoiceItems = new List<InvoiceItem>();
            foreach (DataRow item in dt.Rows)
            {
                InvoiceItem invoiceItem = new InvoiceItem() {
                    Reference = item["Reference"].ToString(),
                    Description = item["Description"].ToString(),
                    Quantity = Convert.ToDouble(item["Quantity"]),
                    UoM = item["UoM"].ToString(),
                    TotalMinutes = Convert.ToDouble(item["TotalMinutes"]),
                    Amount = Convert.ToDecimal(item["Amount"])
                };
                invoiceItems.Add(invoiceItem);
            }
            return invoiceItems;
        }

        private string GetQuery()
        {
            string constructedSQL = string.Empty;
            InvoiceReportType reportType = (InvoiceReportType)Enum.Parse(typeof(InvoiceReportType), this.invoice.INVOICE_TYPE_ID, true);
            switch (reportType)
            {
                case InvoiceReportType.InternationalToIOS:
                    constructedSQL = "select 0.26 as Amount, STR_TO_DATE('2018/07/01', '%Y/%m/%d') as `Date`, 'Call Carrying Charges' as Description, 23 as Quantity, 0.025 as Rate, " +
                        "'International Inbound Calls' as Reference, 0.16 as Revenue, 7.22 as TotalMinutes, 'Calls' as UoM, " +
                        "0.16 as XAmount, 0.06 as YAmount, 0.10 as XYAmount from users";
                    break;
                default:
                    break;
            }
            return constructedSQL;
        }
    }
}