using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Linq;
using ReportGenerator.Helper;
using System.Globalization;
using TelcobrightMediation;
using System.Collections.Generic;
using MediationModel;
using Newtonsoft.Json;
using LibraryExtensions;
using System.ComponentModel.Composition;

namespace ReportGenerator.Reports.InvoiceReports.ICX
{
    [Export("InvoiceTemplate", typeof(IInvoiceTemplate))]
    public partial class InternationalOutgoingToANSDetails2 : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => this.GetType().Name;

        public InternationalOutgoingToANSDetails2()
        {
            InitializeComponent();
        }

        public void SaveToPdf(string fileName)
        {
            this.ExportToPdf(fileName);
        }

        public void GenerateInvoice(object data)
        {
            invoice invoice = (invoice)data;
            List<InvoiceSectionDataRowForVoiceCall> invoiceBasicDatas = this.GetReportData(invoice);
            invoice_item invoiceItem = invoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
            this.DataSource = invoiceBasicDatas;

            #region Page Header
            xrLabelVatRegNo.Text = "VAT Reg. No. 18141080328";
            xrLabelPartnerName.Text = invoiceMap["companyName"];
            xrLabelPartnerVatRegNo.Text = invoiceMap["vatRegNo"];
            xrLabelType.Text = string.Format("Type: {0}", invoiceMap["customerType"]);

            xrLabelBillingPeriod.Text = invoiceMap["billingPeriod"];
            xrLabelInvoiceDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.INVOICE_DATE);
            xrLabelInvoiceNo.Text = invoice.REFERENCE_NUMBER;
            xrLabelCurrency.Text = invoiceMap["uom"];
            xrLabelTimeZone.Text = invoiceMap["timeZone"];
            #endregion

            #region Report Body
            xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellDate.DataBindings.Add("Text", this.DataSource, "Date", "{0:dd-MMM-yyyy}");
            xrTableCellTotalCalls.DataBindings.Add("Text", this.DataSource, "TotalCalls", "{0:n0}");
            xrTableCellTotalMinutes.DataBindings.Add("Text", this.DataSource, "TotalMinutes", "{0:n2}");
            xrTableCellXAMOUNT.DataBindings.Add("Text", this.DataSource, "XAmount", "{0:n2}");
            xrTableCellYAMOUNT.DataBindings.Add("Text", this.DataSource, "YAmount", "{0:n2}");
            xrTableCellXYAMOUNT.DataBindings.Add("Text", this.DataSource, "XYAmount", "{0:n2}");
            xrTableCellRevenue.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");

            xrTableCellRevenueTotal.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            #endregion
        }

        private List<InvoiceSectionDataRowForVoiceCall> GetReportData(invoice invoice)
        {
            invoice_item invoiceItem = invoice.invoice_item.Single();
            InvoiceSectionDataRetriever<InvoiceSectionDataRowForVoiceCall> sectionDataRetriever =
                new InvoiceSectionDataRetriever<InvoiceSectionDataRowForVoiceCall>();
            List<InvoiceSectionDataRowForVoiceCall> sectionData =
                sectionDataRetriever.GetSectionData(invoice, sectionNumber: 3);
            foreach (InvoiceSectionDataRowForVoiceCall item in sectionData)
            {
                item.Reference = invoiceItem.PRODUCT_ID;
            }
            return sectionData;
        }

    }
}
