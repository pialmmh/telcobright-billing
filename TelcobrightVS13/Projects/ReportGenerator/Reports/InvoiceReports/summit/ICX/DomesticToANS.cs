using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using MediationModel;
using Newtonsoft.Json;
using ReportGenerator.Helper;
using TelcobrightMediation.Helper;
using System.Collections;

namespace TelcobrightMediation.Reports.InvoiceReports.summit.ICX
{
    [Export("InvoiceTemplate", typeof(IInvoiceTemplate))]
    public partial class DomesticToANS : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => TemplateNameHelper.GetTemplateName(GetType());


        public DomesticToANS()
        {
            InitializeComponent();
        }
        
        public void SaveToPdf(string fileName)
        {
            this.ExportToPdf(fileName);
        }
        public bool IsDictionary(object o)
        {
            if (o == null) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
        public void GenerateInvoice(object data)
        {
            invoice invoice = null;
            invoice mergedInvoice = null;
            if (IsDictionary(data)) {
                var map = (Dictionary<string, object>)data;
                invoice = (invoice)map["invoice"];
                mergedInvoice = (invoice)map["mergedInvoice"];
            }
            else {
                invoice = (invoice)data;
            }
            List<InvoiceSectionDataRowForA2ZVoice> invoiceBasicDatas = this.GetReportData(invoice);
            List<InvoiceSectionDataRowForA2ZVoice> invoiceBasicDatasMerged = this.GetReportData(mergedInvoice);

            //invoice_item mergeInvoiceItem = mergedInvoice.invoice_item.Single();
            invoice_item invoiceItem = invoice.invoice_item.Single();
            invoice_item invoiceItemMerged = mergedInvoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
            Dictionary<string, string> invoiceMapMerged = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItemMerged.JSON_DETAIL);
            this.DataSource = invoiceBasicDatas;
            //this.DataSource = mergeInvoiceBasicDatas;

            #region Page Header
            //xrLabelVatRegNo.Text = "BIN: 001285404-0208";
            xrLabelPartnerName.Text = invoiceMap["companyName"];
            xrLabelPartnerAddress.Text = invoiceMap["billingAddress"];
            xrLabelPartnerVatRegNo.Text = "BIN: " + invoiceMap["vatRegNo"];
            xrLabelType.Text = string.Format("Type: {0}", invoiceMap["customerType"]);

            DateTime startDate = DateTime.ParseExact(invoiceMap["billingStartDate"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(invoiceMap["billingEndDate"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            xrLabelBillingPeriod.Text = $@"from {startDate.ToString("dd-MMM-yyyy")} to {endDate.ToString("dd-MMM-yyyy")}";
            xrLabelInvoiceDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.INVOICE_DATE);
            xrLabelInvoiceNo.Text = invoice.REFERENCE_NUMBER;
            //xrLabelInvoiceDueDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.DUE_DATE);
            xrLabelCurrency.Text = invoiceMap["uom"];
            xrLabelTimeZone.Text = invoiceMap["timeZone"];
            #endregion

            #region Report Body
            xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellDescription.DataBindings.Add("Text", this.DataSource, "Description");
            xrTableCellUnitsCalls.DataBindings.Add("Text", this.DataSource, "TotalCalls", "{0:n0}");
            xrTableCellTotalMinutes.DataBindings.Add("Text", this.DataSource, "TotalMinutes", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCell8.DataBindings.Add("Text", invoiceBasicDatasMerged, "Reference");
            xrTableCell15.DataBindings.Add("Text", invoiceBasicDatasMerged, "Description");
            xrTableCell16.DataBindings.Add("Text", invoiceBasicDatasMerged, "TotalCalls", "{0:n0}");
            xrTableCell17.DataBindings.Add("Text", invoiceBasicDatasMerged, "TotalMinutes", "{0:n2}");
            xrTableCell18.DataBindings.Add("Text", invoiceBasicDatasMerged, "Amount", "{0:n2}");

            decimal subTotalAmount = invoiceBasicDatas.Sum(x => x.Amount);
            decimal invTotalAmount = invoiceBasicDatas.Sum(x => x.GrandTotalAmount);
            decimal subTotalAmountMerged = invoiceBasicDatasMerged.Sum(x => x.Amount);
            decimal invTotalAmountMerged = invoiceBasicDatasMerged.Sum(x => x.GrandTotalAmount);
            decimal invTotalAmountBoth = invTotalAmount + invTotalAmountMerged;
            decimal invVat = invTotalAmount - subTotalAmount;
            decimal invMergedVat = invTotalAmountMerged - subTotalAmountMerged;
            decimal totalVat = invVat + invMergedVat;
            decimal subTotalAmountBoth = subTotalAmount + subTotalAmountMerged;
            //decimal mergedInvoiceAmount = mergedInvoice.originalAmount;

            //xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellSubTotalAmount.Text = string.Format("{0:n2}", subTotalAmountBoth);
            xrTableCellVAT.Text = string.Format("{0:n2}", totalVat);
            xrTableCellInvoiceTotal.Text = string.Format("{0:n2}", invTotalAmountBoth);
            xrTableCellAmountDueforPayment.Text = string.Format("{0:n2}", invTotalAmountBoth);

            #endregion

            #region Report Footer
            if (invoiceMap.ContainsKey("paymentAdvice") && invoiceMap["paymentAdvice"] != null)
            {
                xrLabelPaymentAdvice.Text = invoiceMap["paymentAdvice"].ToString();
            }
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            xrLabelAmountInwords.Text = textInfo.ToTitleCase(CurrencyHelper.NumberToTakaWords(Convert.ToDouble(invTotalAmountBoth)));
            #endregion
        }

        private List<InvoiceSectionDataRowForA2ZVoice> GetReportData(invoice invoice/*,invoice mergedInvoice*/)
        {
            invoice_item invoiceItem = invoice.invoice_item.Single();
            //invoice_item mergeInvoiceItem = mergedInvoice.invoice_item.Single();
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
