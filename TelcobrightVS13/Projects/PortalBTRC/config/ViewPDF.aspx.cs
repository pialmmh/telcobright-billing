using PortalApp.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PortalApp.config
{
    public partial class ViewPDF : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var refNo = Request.QueryString["refNo"];
            if (refNo != null)
            {
                string pathtofile = string.Format("{0}\\{1}.pdf", Server.MapPath("/InvoicePdfs"), refNo);
                PdfHandler pdfHandler = new PdfHandler(pathtofile);
                pdfHandler.ProcessRequest(this.Context);
            }
        }
    }
}