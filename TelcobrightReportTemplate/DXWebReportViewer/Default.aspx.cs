using DevExpress.XtraReports.UI;
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
            InvoiceDataForReport invoiceDataForReport = new InvoiceDataForReport();
            XtraReport InternationalIncomingToForeignCarrier = new InternationalIncomingToForeignCarrier(invoiceDataForReport);
            ASPxDocumentViewer1.Report = (XtraReport)InternationalIncomingToForeignCarrier;
        }
    }
}