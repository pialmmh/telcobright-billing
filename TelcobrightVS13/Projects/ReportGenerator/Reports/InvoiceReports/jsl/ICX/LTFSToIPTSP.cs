﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using MediationModel;
using Newtonsoft.Json;
using ReportGenerator.Helper;
using TelcobrightMediation.Helper;

namespace TelcobrightMediation.Reports.InvoiceReports.jsl.ICX
{
    [Export("InvoiceTemplate", typeof(IInvoiceTemplate))]
    public partial class LTFSToIPTSP : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => TemplateNameHelper.GetTemplateName(GetType());

        public LTFSToIPTSP()
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
            List<InvoiceSectionDataRowForA2ZVoice> invoiceBasicDatas = this.GetReportData(invoice);
            invoice_item invoiceItem = invoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
            this.DataSource = invoiceBasicDatas;

            #region Page Header
            xrLabelVatRegNo.Text = "BIN: 001285404-0208";
            xrLabelPartnerName.Text = invoiceMap["companyName"];
            xrLabelPartnerAddress.Text = invoiceMap["billingAddress"];
            xrLabelPartnerVatRegNo.Text = "BIN: " + invoiceMap["vatRegNo"];
            xrLabelType.Text = string.Format("Type: {0}", invoiceMap["customerType"]);

            DateTime startDate = DateTime.ParseExact(invoiceMap["billingStartDate"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(invoiceMap["billingEndDate"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            xrLabelBillingPeriod.Text = $@"from {startDate.ToString("dd-MMM-yyyy")} to {endDate.ToString("dd-MMM-yyyy")}";
            xrLabelInvoiceDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.INVOICE_DATE);
            xrLabelInvoiceDueDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.DUE_DATE);
            xrLabelInvoiceNo.Text = invoice.REFERENCE_NUMBER;
            xrLabelCurrency.Text = invoiceMap["uom"];
            xrLabelTimeZone.Text = invoiceMap["timeZone"];
            #endregion

            #region Report Body
            xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellDescription.DataBindings.Add("Text", this.DataSource, "Description");
            xrTableCellTotalCalls.DataBindings.Add("Text", this.DataSource, "TotalCalls", "{0:n0}");
            xrTableCellTotalMinutes.DataBindings.Add("Text", this.DataSource, "TotalMinutes", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");

            decimal subTotalAmount = invoiceBasicDatas.Sum(x => x.Amount);
            decimal invTotalAmount = invoiceBasicDatas.Sum(x => x.GrandTotalAmount);

            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellVAT.Text = string.Format("{0:n2}", invTotalAmount - subTotalAmount);
            xrTableCellInvoiceTotal.Text = string.Format("{0:n2}", invTotalAmount);
            xrTableCellAmountDueforPayment.Text = string.Format("{0:n2}", invTotalAmount);

            #endregion

            #region Report Footer
            if (invoiceMap.ContainsKey("paymentAdvice") && invoiceMap["paymentAdvice"] != null)
            {
                xrLabelPaymentAdvice.Text = invoiceMap["paymentAdvice"].ToString();
            }
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            xrLabelAmountInwords.Text = textInfo.ToTitleCase(CurrencyHelper.NumberToTakaWords(Convert.ToDouble(invTotalAmount)));
            #endregion
        }

        private List<InvoiceSectionDataRowForA2ZVoice> GetReportData(invoice invoice)
        {
            invoice_item invoiceItem = invoice.invoice_item.Single();
            InvoiceSectionDataRetriever<InvoiceSectionDataRowForA2ZVoice> sectionDataRetriever =
                new InvoiceSectionDataRetriever<InvoiceSectionDataRowForA2ZVoice>();
            List<InvoiceSectionDataRowForA2ZVoice> sectionData =
                sectionDataRetriever.GetSectionData(invoice, sectionNumber: 1);
            foreach (InvoiceSectionDataRowForA2ZVoice item in sectionData)
            {
                item.Reference = invoiceItem.PRODUCT_ID;
                item.Description = "Call Carrying Charges";
            }
            return sectionData;
        }

    }
}
