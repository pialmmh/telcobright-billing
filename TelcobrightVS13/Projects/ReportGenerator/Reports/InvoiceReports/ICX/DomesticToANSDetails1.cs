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

namespace ReportGenerator.Reports.InvoiceReports.ICX
{
    public partial class DomesticToANSDetails1 : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => this.GetType().Name;

        public DomesticToANSDetails1()
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
            List<VoiceCallInvoiceSectionData> invoiceBasicDatas = this.GetReportData(invoice);
            this.DataSource = invoiceBasicDatas;

            /*
            #region Page Header
            xrLabelVatRegNo.Text = "VAT Reg. No. 19061116647";
            xrLabelPartnerName.Text = invoice.Partner.PartnerName;
            xrLabelPartnerVatRegNo.Text = invoice.Partner.VatRegNo;
            xrLabelType.Text = string.Format("Type: {0}", invoice.Type);

            xrLabelBillingPeriod.Text = string.Format("from {0:dd-MMM-yyyy} to {1:dd-MMM-yyyy}", invoice.BillingFrom, invoice.BillingTo);
            xrLabelInvoiceDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.InvoiceDate);
            xrLabelInvoiceNo.Text = invoice.InvoiceNo;
            xrLabelCurrency.Text = invoice.Currency;
            xrLabelTimeZone.Text = invoice.TimeZone;
            #endregion
            */

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

        private List<VoiceCallInvoiceSectionData> GetReportData(invoice invoice)
        {
            List<VoiceCallInvoiceSectionData> invoiceBasicDatas = new List<VoiceCallInvoiceSectionData>();
            invoice_item invoice_item = invoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(invoice_item.JSON_DETAIL);
            Dictionary<string, InvoiceSection> invoiceSections = invoiceMap.Where(kv => kv.Key.StartsWith("Section-"))
                .Select(kv => JsonConvert.DeserializeObject<InvoiceSection>(kv.Value))
                .ToDictionary(s => s.TemplateName);

            var section = invoiceSections["Section-2"];
            JsonCompressor<VoiceCallInvoiceSectionData> jsonCompressor = new JsonCompressor<VoiceCallInvoiceSectionData>();
            VoiceCallInvoiceSectionData invoiceDataBasic = jsonCompressor.DeSerializeToObject(section.SerializedData);

            return invoiceBasicDatas;
        }

    }
}
