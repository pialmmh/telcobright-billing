using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using MediationModel;
using Newtonsoft.Json;
using ReportGenerator.Helper;
using TelcobrightMediation.Helper;

namespace TelcobrightMediation.Reports.InvoiceReports.summit.ICX
{
    [Export("InvoiceTemplate", typeof(IInvoiceTemplate))]
    public partial class InternationalOutgoingToANS : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => TemplateNameHelper.GetTemplateName(GetType());
        private static int currentInvoiceNumber = 2000;

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
            invoice invoice = (invoice)data;
            List<InvoiceSectionDataRowForA2ZVoice> invoiceBasicDatas = this.GetReportData(invoice);
            invoice_item invoiceItem = invoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
            this.DataSource = invoiceBasicDatas;

            string invoiceNumber = $"{currentInvoiceNumber}-{DateTime.Now.AddMonths(-1).ToString("MMM-yyyy")}";
            currentInvoiceNumber++;

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
            //xrLabelInvoiceDueDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.DUE_DATE);
            //xrLabelInvoiceNo.Text = invoice.REFERENCE_NUMBER;
            xrLabelInvoiceNo.Text = invoiceNumber;
            xrLabelCurrency.Text = invoiceMap["uom"];
            xrLabelTimeZone.Text = invoiceMap["timeZone"];
            #endregion

            #region Report Body
            xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellTotalCalls.DataBindings.Add("Text", this.DataSource, "TotalCalls", "{0:n0}");
            xrTableCellTotalMinutes.DataBindings.Add("Text", this.DataSource, "TotalMinutes", "{0:n2}");
            xrTableCellXAmountBDT.DataBindings.Add("Text", this.DataSource, "XAmount", "{0:n2}");
            xrTableCellYAmountBDT.DataBindings.Add("Text", this.DataSource, "YAmount", "{0:n2}");
            xrTableCellXYAmountBDT.DataBindings.Add("Text", this.DataSource, "XYAmount", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");

            decimal subTotalAmount = invoiceBasicDatas.Sum(x => x.Amount);
            decimal invTotalAmount = invoiceBasicDatas.Sum(x => x.GrandTotalAmount);

            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            if (startDate >= new DateTime(2019, 7, 1))
            {
                xrTableCellVAT.Text = string.Format("{0:n2}", invTotalAmount - subTotalAmount);
                xrTableCellInvoiceTotal.Text = string.Format("{0:n2}", invTotalAmount);
                xrTableCellAmountDueforPayment.Text = string.Format("{0:n2}", invTotalAmount);
            }
            else
            {
                invTotalAmount = subTotalAmount;
                xrTableCellVatText.Text = string.Empty;
                xrTableCellVAT.Text = string.Empty;
                xrTableCellInvoiceTotalText.Text = string.Empty;
                xrTableCellInvoiceTotal.Text = string.Empty;
                xrTableCellAmountDueforPayment.Text = string.Format("{0:n2}", invTotalAmount);
            }

            #endregion

            #region Report Footer
            if (invoiceMap.ContainsKey("paymentAdvice") && invoiceMap["paymentAdvice"] != null)
            {
                xrLabelPaymentAdvice.Text = invoiceMap["paymentAdvice"].ToString();
            }
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            xrLabelAmountInwords.Text = textInfo.ToTitleCase(CurrencyHelper.NumberToTakaWords(Convert.ToDouble(invTotalAmount)));
            xrLabelConversionRate.Text = string.Format("As per Sonali Bank Rate (1USD = BDT {0:n2})", invoiceMap["usdRate"]);
                //as on { 1:dd - MMM - yyyy}
            //invoiceMap["endDate"]
            #endregion

        }

        private List<InvoiceSectionDataRowForA2ZVoice> GetReportData(invoice invoice)
        {
            invoice_item invoiceItem = invoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
            decimal usdRate = Convert.ToDecimal(invoiceMap.Where(x => x.Key == "usdRate").Select(x => x.Value).Single());
            InvoiceSectionDataRetriever<InvoiceSectionDataRowForA2ZVoice> sectionDataRetriever =
                new InvoiceSectionDataRetriever<InvoiceSectionDataRowForA2ZVoice>();
            List<InvoiceSectionDataRowForA2ZVoice> sectionData =
                sectionDataRetriever.GetSectionData(invoice, sectionNumber: 1);
            foreach (InvoiceSectionDataRowForA2ZVoice item in sectionData)
            {
                item.Reference = invoiceItem.PRODUCT_ID;
                item.YAmount = item.YAmount * usdRate;
            }
            return sectionData;
        }
    }
}
