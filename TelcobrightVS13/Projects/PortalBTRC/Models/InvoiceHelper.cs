using MediationModel;

namespace PortalApp.Models
{
    public class InvoiceHelper
    {
        private invoice invoice;

        public InvoiceHelper(invoice invoice)
        {
            this.invoice = invoice;
        }
        /*
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
            foreach (DataRow row in dt.Rows)
            {
                InvoiceItem invoiceItem = new InvoiceItem();
                if (row.Table.Columns.Contains("Amount")) invoiceItem.Amount = row["Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Amount"]);
                if (row.Table.Columns.Contains("Date")) invoiceItem.Date = row["Date"] == DBNull.Value ? DateTime.Now.Date : Convert.ToDateTime(row["Date"]);
                if (row.Table.Columns.Contains("Description")) invoiceItem.Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString();
                if (row.Table.Columns.Contains("Quantity")) invoiceItem.Quantity = row["Quantity"] == DBNull.Value ? 0 : Convert.ToDouble(row["Quantity"]);
                if (row.Table.Columns.Contains("Rate")) invoiceItem.Rate = row["Rate"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["Rate"]);
                if (row.Table.Columns.Contains("Reference")) invoiceItem.Reference = row["Reference"] == DBNull.Value ? null : row["Reference"].ToString();
                if (row.Table.Columns.Contains("Revenue")) invoiceItem.Revenue = row["Revenue"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Revenue"]);
                if (row.Table.Columns.Contains("TotalMinutes")) invoiceItem.TotalMinutes = row["TotalMinutes"] == DBNull.Value ? 0 : Convert.ToDouble(row["TotalMinutes"]);
                if (row.Table.Columns.Contains("UoM")) invoiceItem.UoM = row["UoM"] == DBNull.Value ? null : row["UoM"].ToString();
                if (row.Table.Columns.Contains("XAmount")) invoiceItem.XAmount = row["XAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["XAmount"]);
                if (row.Table.Columns.Contains("YAmount")) invoiceItem.YAmount = row["YAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["YAmount"]);
                if (row.Table.Columns.Contains("XYAmount")) invoiceItem.XYAmount = row["XYAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["XYAmount"]);
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
                        "null as Reference, 0.16 as Revenue, 7.22 as TotalMinutes, 'Calls' as UoM, " +
                        "0.16 as XAmount, 0.06 as YAmount, 0.10 as XYAmount from users";
                    break;
                default:
                    break;
            }
            return constructedSQL;
        }
        */
    }
}