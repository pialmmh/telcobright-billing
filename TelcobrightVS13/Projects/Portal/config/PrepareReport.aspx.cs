using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;
using TelcobrightMediation;

namespace PortalApp.config
{
    public partial class PrepareReport : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }
        private static Dictionary<string, IInvoiceTemplate> invoiceTemplates { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var reportName = Request.QueryString["reportName"];
            var invoiceId = Request.QueryString["invoiceId"];
            if (reportName != null)
            {
                using (PartnerEntities context = new PartnerEntities())
                {
                    Tbc = PageUtil.GetTelcobrightConfig();
                    List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                    InvoiceTemplateComposer invoiceTemplateComposer = new InvoiceTemplateComposer();
                    DirectoryInfo dllDir = new DirectoryInfo(PageUtil.GetPortalBinPath()).Parent
                        .GetDirectories("Extensions")
                        .Single().GetDirectories("InvoiceTemplates").Single();
                    invoiceTemplateComposer.ComposeFromPath(dllDir.FullName);
                    invoiceTemplates = invoiceTemplateComposer.InvoiceTemplates.ToDictionary(c => c.TemplateName);

                    //String reportName = linkButton.CommandName;
                    int startPos = reportName.LastIndexOf("Template-", StringComparison.Ordinal) + "Template-".Length;
                    int length = reportName.Length - startPos;
                    reportName = $"{Tbc.DatabaseSetting.DatabaseName}#{reportName.Substring(startPos, length)}";

                    IInvoiceTemplate template = invoiceTemplates[reportName];

                    //GridViewRow gvrow = (GridViewRow)linkButton.NamingContainer;
                    //int INVOICE_ID = Convert.ToInt32(gvInvoice.DataKeys[gvrow.RowIndex].Value);
                    int INVOICE_ID = Convert.ToInt32(invoiceId);
                    invoice invoice = context.invoices.First(x => x.INVOICE_ID == INVOICE_ID);

                    String refNo = Guid.NewGuid().ToString();
                    template.GenerateInvoice(invoice);
                    this.Session[refNo] = template;
                    Response.Redirect("~/config/ViewReport.aspx?refNo=" + HttpUtility.UrlEncode(refNo), false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }
    }
}