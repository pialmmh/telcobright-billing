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
    public partial class DomesticToANSDetails1 : DevExpress.XtraReports.UI.XtraReport, IInvoiceTemplate
    {
        public string TemplateName => TemplateNameHelper.GetTemplateName(GetType());

        public DomesticToANSDetails1()
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
            if (IsDictionary(data))
            {
                var map = (Dictionary<string, object>)data;
                invoice = (invoice)map["invoice"];
                mergedInvoice = (invoice)map["mergedInvoice"];
            }
            else
            {
                invoice = (invoice)data;
            }
            List<InvoiceSectionDataRowForA2ZVoice> invoiceBasicDatas = this.GetReportData(invoice);
            invoice_item invoiceItem = invoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);

            List<InvoiceSectionDataRowForA2ZVoice> invoiceBasicDatasMerged = null;
            invoice_item invoiceItemMerged = null;
            Dictionary<string, string> invoiceMapMerged = null;
            if (mergedInvoice != null) {
                invoiceBasicDatasMerged = this.GetReportData(mergedInvoice);
                invoiceItemMerged = mergedInvoice.invoice_item.Single();
                invoiceMapMerged = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItemMerged.JSON_DETAIL);
            }

            this.DataSource = invoiceBasicDatas;
            #region Page Header
            //xrLabelVatRegNo.Text = "BIN: 001285404-0208";
            xrLabelPartnerName.Text = invoiceMap["companyName"];
            xrLabelPartnerVatRegNo.Text = "BIN: " + invoiceMap["vatRegNo"];
            xrLabelType.Text = string.Format("Type: {0}", invoiceMap["customerType"]);

            DateTime startDate = DateTime.ParseExact(invoiceMap["billingStartDate"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(invoiceMap["billingEndDate"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            xrLabelBillingPeriod.Text = $@"from {startDate.ToString("dd-MMM-yyyy")} to {endDate.ToString("dd-MMM-yyyy")}";
            xrLabelInvoiceDate.Text = string.Format("{0:dd-MMM-yyyy}", invoice.INVOICE_DATE);
            xrLabelInvoiceNo.Text = invoice.REFERENCE_NUMBER;
            xrLabelCurrency.Text = invoiceMap["uom"];
            xrLabelTimeZone.Text = invoiceMap["timeZone"];
            #endregion

            #region Report Body
            //xrTableCellReference.DataBindings.Add("Text", this.DataSource, "Reference");
            xrTableCellTermOperator.DataBindings.Add("Text", this.DataSource, "OutPartnerName");
            xrTableCellTotalCalls.DataBindings.Add("Text", this.DataSource, "TotalCalls", "{0:n0}");
            xrTableCellTotalMinutes.DataBindings.Add("Text", this.DataSource, "TotalMinutes", "{0:n2}");
            xrTableCellRate.DataBindings.Add("Text", this.DataSource, "Rate", "{0:##0.######}");
            xrTableCellRevenue.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellAmount.DataBindings.Add("Text", this.DataSource, "GrandTotalAmount", "{0:n2}");


            xrTableCellRevenueTotal.DataBindings.Add("Text", this.DataSource, "Amount", "{0:n2}");
            xrTableCellSubTotalAmount.DataBindings.Add("Text", this.DataSource, "GrandTotalAmount", "{0:n2}");

            if (invoiceBasicDatasMerged != null && invoiceBasicDatasMerged.Count == 3)
            {
                //xrTableCell23.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "Reference");
                xrTableCell24.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "InPartnerName");
                xrTableCell25.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "TotalCalls", "{0:n0}");
                xrTableCell26.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "TotalMinutes", "{0:n2}");
                xrTableCell27.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "Rate", "{0:##0.######}");
                xrTableCell28.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "Amount", "{0:n2}");
                xrTableCell29.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "GrandTotalAmount", "{0:n2}");

                //xrTableCell16.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "Reference");
                xrTableCell17.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "InPartnerName");
                xrTableCell18.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "TotalCalls", "{0:n0}");
                xrTableCell19.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "TotalMinutes", "{0:n2}");
                xrTableCell20.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "Rate", "{0:##0.######}");
                xrTableCell21.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "Amount", "{0:n2}");
                xrTableCell22.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "GrandTotalAmount", "{0:n2}");

                //xrTableCell30.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "Reference");
                xrTableCell31.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "InPartnerName");
                xrTableCell32.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "TotalCalls", "{0:n0}");
                xrTableCell33.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "TotalMinutes", "{0:n2}");
                xrTableCell34.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "Rate", "{0:##0.######}");
                xrTableCell35.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "Amount", "{0:n2}");
                xrTableCell36.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "GrandTotalAmount", "{0:n2}");

                SubBand2.Visible = false;
                SubBand3.Visible = false;
            }

            else if (invoiceBasicDatasMerged != null && invoiceBasicDatasMerged.Count == 4)
            {
                //xrTableCell23.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "Reference");
                xrTableCell24.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "InPartnerName");
                xrTableCell25.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "TotalCalls", "{0:n0}");
                xrTableCell26.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "TotalMinutes", "{0:n2}");
                xrTableCell27.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "Rate", "{0:##0.######}");
                xrTableCell28.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "Amount", "{0:n2}");
                xrTableCell29.DataBindings.Add("Text", invoiceBasicDatasMerged[0], "GrandTotalAmount", "{0:n2}");

                //xrTableCell16.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "Reference");
                xrTableCell17.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "InPartnerName");
                xrTableCell18.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "TotalCalls", "{0:n0}");
                xrTableCell19.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "TotalMinutes", "{0:n2}");
                xrTableCell20.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "Rate", "{0:##0.######}");
                xrTableCell21.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "Amount", "{0:n2}");
                xrTableCell22.DataBindings.Add("Text", invoiceBasicDatasMerged[1], "GrandTotalAmount", "{0:n2}");

                //xrTableCell30.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "Reference");
                xrTableCell31.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "InPartnerName");
                xrTableCell32.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "TotalCalls", "{0:n0}");
                xrTableCell33.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "TotalMinutes", "{0:n2}");
                xrTableCell34.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "Rate", "{0:##0.######}");
                xrTableCell35.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "Amount", "{0:n2}");
                xrTableCell36.DataBindings.Add("Text", invoiceBasicDatasMerged[2], "GrandTotalAmount", "{0:n2}");

                //xrTableCell37.DataBindings.Add("Text", invoiceBasicDatasMerged[3], "Reference");
                xrTableCell38.DataBindings.Add("Text", invoiceBasicDatasMerged[3], "InPartnerName");
                xrTableCell39.DataBindings.Add("Text", invoiceBasicDatasMerged[3], "TotalCalls", "{0:n0}");
                xrTableCell40.DataBindings.Add("Text", invoiceBasicDatasMerged[3], "TotalMinutes", "{0:n2}");
                xrTableCell41.DataBindings.Add("Text", invoiceBasicDatasMerged[3], "Rate", "{0:##0.######}");
                xrTableCell42.DataBindings.Add("Text", invoiceBasicDatasMerged[3], "Amount", "{0:n2}");
                xrTableCell43.DataBindings.Add("Text", invoiceBasicDatasMerged[3], "GrandTotalAmount", "{0:n2}");
                SubBand2.Visible = true;
                SubBand3.Visible = false;
            }

            else if(invoiceBasicDatasMerged != null && invoiceBasicDatasMerged.Count == 5)
            {
                //xrTableCell82.DataBindings.Add("Text", invoiceBasicDatasMerged[4], "Reference");
                xrTableCell83.DataBindings.Add("Text", invoiceBasicDatasMerged[4], "InPartnerName");
                xrTableCell84.DataBindings.Add("Text", invoiceBasicDatasMerged[4], "TotalCalls", "{0:n0}");
                xrTableCell85.DataBindings.Add("Text", invoiceBasicDatasMerged[4], "TotalMinutes", "{0:n2}");
                xrTableCell86.DataBindings.Add("Text", invoiceBasicDatasMerged[4], "Rate", "{0:##0.######}");
                xrTableCell87.DataBindings.Add("Text", invoiceBasicDatasMerged[4], "Amount", "{0:n2}");
                xrTableCell88.DataBindings.Add("Text", invoiceBasicDatasMerged[4], "GrandTotalAmount", "{0:n2}");
                SubBand2.Visible = true;
                SubBand3.Visible = true;
            }

            if (invoiceBasicDatasMerged != null)
            {
                decimal subTotalAmount = invoiceBasicDatasMerged.Sum(x => x.Amount);
                decimal invTotalAmount = invoiceBasicDatasMerged.Sum(x => x.GrandTotalAmount);

                xrTableCell48.Text = string.Format("{0:n2}", subTotalAmount);
                xrTableCell49.Text = string.Format("{0:n2}", invTotalAmount);
            }

            if (invoiceBasicDatasMerged == null)
            {
                SubBand1.Visible = false;
            }
            if (invoiceBasicDatasMerged == null)
            {
                SubBand2.Visible = false;
            }
            if (invoiceBasicDatasMerged == null)
            {
                SubBand3.Visible = false;
            }
            if (invoiceBasicDatasMerged == null)
            {
                SubBand4.Visible = false;
            }


            //xrTableCell48.DataBindings.Add("Text", invoiceBasicDatasMerged, "Amount", "{0:n2}");
            //xrTableCell49.DataBindings.Add("Text", invoiceBasicDatasMerged, "GrandTotalAmount", "{0:n2}");
            #endregion


        }

        private List<InvoiceSectionDataRowForA2ZVoice> GetReportData(invoice invoice)
        {
            invoice_item invoiceItem = invoice.invoice_item.Single();
            InvoiceSectionDataRetriever<InvoiceSectionDataRowForA2ZVoice> sectionDataRetriever =
                new InvoiceSectionDataRetriever<InvoiceSectionDataRowForA2ZVoice>();
            List<InvoiceSectionDataRowForA2ZVoice> sectionData =
                sectionDataRetriever.GetSectionData(invoice, sectionNumber: 2);
            foreach (InvoiceSectionDataRowForA2ZVoice item in sectionData)
            {
                item.Reference = invoiceItem.PRODUCT_ID;
            }
            return sectionData;
        }

    }
}
