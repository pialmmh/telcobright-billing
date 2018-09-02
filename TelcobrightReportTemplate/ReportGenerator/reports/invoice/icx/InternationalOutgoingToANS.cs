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
    public partial class InternationalOutgoingToANS : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => this.GetType().Name;

        public InternationalOutgoingToANS()
        {
            InitializeComponent();
        }

        public void SaveToPdf(string fileName)
        {
            this.ExportToPdf(fileName);
        }

        public void GenerateInvoice(object data)
        {
            /*
            this.DataSource = invoice.InvoiceItems;

            #region Page Header
            xrLabelVatRegNo.Text = "VAT Reg. No. 19061116647";
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
            xrTableCellUnitsCalls.DataBindings.Add("Text", this.DataSource, "UoM");
            xrTableCellUnitsCallsUoM.DataBindings.Add("Text", this.DataSource, "Quantity", "{0:n0}");
            xrTableCellTotalMinutes.DataBindings.Add("Text", this.DataSource, "TotalMinutes", "{0:n2}");
            xrTableCellXAmountBDT.DataBindings.Add("Text", this.DataSource, "XAmount", "{0:n2}");
            xrTableCellYAmountBDT.DataBindings.Add("Text", this.DataSource, "YAmount", "{0:n2}");
            xrTableCellXYAmountBDT.DataBindings.Add("Text", this.DataSource, "XYAmount", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");

            decimal subTotalAmount = invoice.InvoiceItems.Sum(x => x.Amount);

            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellInvoiceTotal.Text = string.Format("{0:n2}", subTotalAmount);
            xrTableCellAmountDueforPayment.Text = string.Format("{0:n2}", subTotalAmount);
            #endregion

            #region Report Footer
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            xrLabelAmountInwords.Text = textInfo.ToTitleCase(CurrencyHelper.NumberToTakaWords(Convert.ToDouble(subTotalAmount)));
            xrLabelConversionRate.Text = string.Format("As per Sonali Bank Rate (1USD = BDT {0:n2}) as on {1:dd-MMM-yyyy}", invoice.ConversionRate, invoice.ConversionRateDate);
            #endregion
            */
        }
    }
}
