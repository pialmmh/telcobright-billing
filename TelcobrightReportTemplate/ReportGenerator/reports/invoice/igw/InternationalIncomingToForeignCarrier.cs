using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Linq;
using ReportGenerator.Helper;
using System.Globalization;

namespace ReportGenerator.reports.invoice.igw
{
    public partial class InternationalIncomingToForeignCarrier : DevExpress.XtraReports.UI.XtraReport, IInvoiceReport
    {
        public InternationalIncomingToForeignCarrier(Invoice invoice)
        {
            InitializeComponent();
            generateReport(invoice);
        }

        public InvoiceReportType reportType { get { return InvoiceReportType.InternationalIncomingToForeignCarrier; } }

        public void saveToPdf(string fileName)
        {
            this.ExportToPdf(fileName);
        }

        private void generateReport(Invoice invoice)
        {
            this.DataSource = invoice.InvoiceItems;

            #region Page Header
            xrLabelVatRegNo.Text = "VAT Reg. No. 18141080328";
            xrLabelPartnerName.Text = invoice.Partner.PartnerName;
            xrLabelPartnerAddress.Text = invoice.Partner.PartnerAddress;
            xrLabelPartnerVatRegNo.Text = invoice.Partner.VatRegNo;
            xrLabelType.Text = string.Format("Type: {0}", invoice.Type);

            xrLabelBillingPeriod.Text = string.Format("from {0:dd-MMM-yyyy} to {1:dd-MMM-yyyy}", invoice.BillingFrom, invoice.BillingTo);
            xrLabelInvoiceDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.InvoiceDate);
            xrLabelInvoiceDueDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.DueDate);
            xrLabelInvoiceNo.Text = invoice.InvoiceNo;
            xrLabelCurrency.Text = invoice.Currency;
            xrLabelTimeZone.Text = invoice.TimeZone;
            #endregion

            #region Report Body
            xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellDescription.DataBindings.Add("Text", this.DataSource, "Description");
            xrTableCellUnitsCalls.DataBindings.Add("Text", this.DataSource, "Quantity", "{0:n0}");
            xrTableCellUnitsCallsUoM.DataBindings.Add("Text", this.DataSource, "UoM");
            xrTableCellTotalMinutes.DataBindings.Add("Text", this.DataSource, "TotalMinutes", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");

            decimal subTotalAmount = invoice.InvoiceItems.Sum(x => x.Amount);

            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellInvoiceTotal.Text = string.Format("{0:n2}", subTotalAmount);
            xrTableCellAmountDueforPayment.Text = string.Format("{0:n2}", subTotalAmount);

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            xrLabelAmountInwords.Text = textInfo.ToTitleCase(CurrencyHelper.NumberToWords(Convert.ToDouble(subTotalAmount)));
            #endregion

            #region Report Footer
            xrLabelPaymentAdvice.Text = "Account Name: Bangla Tel Ltd.\r\n" +
                "A / C No. 13251120032737\r\n" +
                "Bank Name: Prime Bank Limited, Banani Branch\r\n" +
                "Bank Address: House # 62, Block # E, Kemal Ataturk Avenue,\r\n" +
                "Banani, Dhaka 1213, Bangladesh\r\n" +
                "SWIFT code: PRBLBDDH020";

            xrLabelAddress.Text = "Red Crescent Borak Tower, Level-M, 37/3/A, Eskaton Garden Road, Dhaka-1000, Bagnladesh\r\n" +
                "PABX: +88028332924, 9334781, 9334782, Fax: +8802833275, Email: info @banglatel.com.bd\r\n" +
                "Website : www.banglatel.com.bd";
            #endregion
        }
    }
}
