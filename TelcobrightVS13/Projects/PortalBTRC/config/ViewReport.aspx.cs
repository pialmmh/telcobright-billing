using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TelcobrightMediation;

namespace PortalApp.config
{
    public partial class ViewReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var refNo = Request.QueryString["refNo"];
            if (refNo != null)
            {
                IInvoiceTemplate template = (IInvoiceTemplate)this.Session[refNo];
                ASPxDocumentViewer1.Report = (XtraReport)template;
            }
        }
    }
}