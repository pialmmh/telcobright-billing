using DevExpress.XtraReports.UI;
using DXWebReportViewer.Helper;
using ReportGenerator.reports.invoice;
using ReportGenerator.reports.invoice.igw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DXWebReportViewer
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Invoice invoice = InvoiceHelper.GetInvoice();
            IInvoiceReport InternationalIncomingToForeignCarrier = new InternationalOutgoingToIOSDetails2(invoice);
            InternationalIncomingToForeignCarrier.saveToPdf("E:\\Files\\telcobright\\demo.pdf");
            string reportTitle = InternationalIncomingToForeignCarrier.getReportTitle();
            ASPxDocumentViewer1.Report = (XtraReport)InternationalIncomingToForeignCarrier;
        }
    }
}