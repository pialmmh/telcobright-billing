using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using MediationModel;
using Newtonsoft.Json;
using ReportGenerator.Helper;
using TelcobrightMediation.Helper;

namespace TelcobrightMediation.Reports.InvoiceReports.rgrn.Transit
{
    [Export("InvoiceTemplate", typeof(IInvoiceTemplate))]
    public partial class InternationalIncomingToForeignCarrier : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => TemplateNameHelper.GetTemplateName(GetType());

        public InternationalIncomingToForeignCarrier()
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
            xrLabelPartnerName.Text = invoiceMap["companyName"];
            xrLabelCustomerId.Text = $@"Customer ID:{invoiceMap["partnerName"]}";
            xrLabelPartnerAddress.Text = invoiceMap["billingAddress"];
            //            xrLabelType.Text = string.Format("Type: {0}", invoiceMap["customerType"]);

            DateTime startDate = DateTime.ParseExact(invoiceMap["billingStartDate"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(invoiceMap["billingEndDate"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            xrLabelBillingPeriod.Text = $@"Period: {startDate:yyyy-MM-dd HH:mm:ss} to {endDate:yyyy-MM-dd HH:mm:ss}";
            xrLabelInvoiceDate.Text = $@"Invoice: {$"{invoice.INVOICE_DATE:yyyy-MM-dd}"}";
            xrLabelInvoiceNo.Text = $@"Invoice No: {invoice.REFERENCE_NUMBER}";
            //xrLabelTimeZone.Text = $@"Time Zone: {invoiceMap["timeZone"]}";
            xrLabelTimeZone.Text = $@"Time Zone: GMT+0:00";
            #endregion

            #region Report Body

            xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellTotalCalls.DataBindings.Add("Text", this.DataSource, "TotalCalls", "{0:n0}");
            xrTableCellStartDate.Text = $@"{startDate:yyyy-MM-dd}";
            xrTableCellEndDate.Text = $@"{endDate:yyyy-MM-dd}";
            xrTableCellTotalMinutes.DataBindings.Add("Text", this.DataSource, "TotalMinutes", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");

            decimal subTotalAmount = invoiceBasicDatas.Sum(x => x.Amount);

            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellInvoiceTotal.Text = $@"{subTotalAmount:n2}";

            #endregion

            #region Report Footer
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            xrLabelAmountInwords.Text = textInfo.ToTitleCase(CurrencyHelper.NumberToWords(Convert.ToDouble(subTotalAmount)));

            xrLabelPaymentAdvice.Text = $"Account Name : Royal Green Limited\r\n" +
                                        $"Account No : 04666485418\r\n" +
                                        $"ACH routing Number: 011000138\r\n" +
                                        $"Bank Name: Bank Of America\r\n" +
                                        $"Swift Code:  BOFAUS3N";

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
                item.Reference = "Voice Call";
                item.Description = "Call Carrying Charges";
            }
            return sectionData;
        }

    }
}
