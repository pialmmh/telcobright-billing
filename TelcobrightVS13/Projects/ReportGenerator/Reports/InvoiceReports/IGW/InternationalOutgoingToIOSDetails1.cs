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

namespace ReportGenerator.Reports.InvoiceReports.IGW
{
    [Export("InvoiceTemplate", typeof(IInvoiceTemplate))]
    public partial class InternationalOutgoingToIOSDetails1 : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => this.GetType().Name;

        public InternationalOutgoingToIOSDetails1()
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
            this.DataSource = invoiceBasicDatas;

            /*
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
            */

            #region Report Body
            xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellOrigOperator.DataBindings.Add("Text", this.DataSource, "Description");
            xrTableCellUnitsCallsUoM.DataBindings.Add("Text", this.DataSource, "UoM");
            xrTableCellQuantity.DataBindings.Add("Text", this.DataSource, "Quantity", "{0:n2}");
            xrTableCellXAMOUNT.DataBindings.Add("Text", this.DataSource, "XAmount", "{0:n2}");
            xrTableCellYAMOUNT.DataBindings.Add("Text", this.DataSource, "YAmount", "{0:n2}");
            xrTableCellXYAMOUNT.DataBindings.Add("Text", this.DataSource, "XYAmount", "{0:n2}");
            xrTableCellRevenue.DataBindings.Add("Text", this.DataSource, "Revenue", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");

            xrTableCellRevenueTotal.DataBindings.Add("Text", this.DataSource, "Revenue", "{0:n2}");
            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            #endregion
        }
        private List<InvoiceSectionDataRowForVoiceCall> GetReportData(invoice invoice)
        {
            InvoiceSectionDataRetriever<InvoiceSectionDataRowForVoiceCall> sectionDataRetriever =
                new InvoiceSectionDataRetriever<InvoiceSectionDataRowForVoiceCall>();
            List<InvoiceSectionDataRowForVoiceCall> sectionData =
                sectionDataRetriever.GetSectionData(invoice, sectionNumber: 2);
            return sectionData;
        }

    }
}
