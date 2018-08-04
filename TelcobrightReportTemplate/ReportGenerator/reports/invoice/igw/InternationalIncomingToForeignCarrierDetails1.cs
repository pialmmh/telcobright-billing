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
    public partial class InternationalIncomingToForeignCarrierDetails1 : DevExpress.XtraReports.UI.XtraReport, IInvoiceReport
    {
        public InternationalIncomingToForeignCarrierDetails1(Invoice invoice)
        {
            InitializeComponent();
            generateReport(invoice);
        }

        public InvoiceReportType reportType { get { return InvoiceReportType.InternationalIncomingToForeignCarrierDetails1; } }

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
            xrLabelPartnerVatRegNo.Text = invoice.Partner.VatRegNo;
            xrLabelType.Text = string.Format("Type: {0}", invoice.Type);

            xrLabelBillingPeriod.Text = string.Format("from {0:dd-MMM-yyyy} to {1:dd-MMM-yyyy}", invoice.BillingFrom, invoice.BillingTo);
            xrLabelInvoiceDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.InvoiceDate);
            xrLabelInvoiceNo.Text = invoice.InvoiceNo;
            xrLabelCurrency.Text = invoice.Currency;
            xrLabelTimeZone.Text = invoice.TimeZone;
            #endregion

            #region Report Body
            xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellTermOperator.DataBindings.Add("Text", this.DataSource, "Description");
            xrTableCellUnitsCallsUoM.DataBindings.Add("Text", this.DataSource, "UoM");
            xrTableCellQuantity.DataBindings.Add("Text", this.DataSource, "Quantity", "{0:n2}");
            xrTableCellRate.DataBindings.Add("Text", this.DataSource, "Rate", "{0:n2}");
            xrTableCellRevenue.DataBindings.Add("Text", this.DataSource, "Revenue", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");

            xrTableCellRevenueTotal.DataBindings.Add("Text", this.DataSource, "Revenue", "{0:n2}");
            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            #endregion

        }
    }
}
