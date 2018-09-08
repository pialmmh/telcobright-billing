using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Itenso.TimePeriod;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Config;
using Itenso.TimePeriod;
using PortalApp.Models;
using System.Reflection;
using System.IO;
using PortalApp.Handler;

namespace PortalApp.config
{
    public partial class GeneratedInvoices : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }
        private static Dictionary<string, IInvoiceTemplate> invoiceTemplates { get; set; }
        private static List<invoice> generatedInvoices { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Tbc = PageUtil.GetTelcobrightConfig();

                InvoiceTemplateComposer invoiceTemplateComposer = new InvoiceTemplateComposer();
                DirectoryInfo dllDir = new DirectoryInfo(PageUtil.GetPortalBinPath()).Parent.GetDirectories("Extensions")
                    .Single().GetDirectories("InvoiceTemplates").Single();
                invoiceTemplateComposer.ComposeFromPath(dllDir.FullName);
                invoiceTemplates = invoiceTemplateComposer.InvoiceTemplates.ToDictionary(c => c.TemplateName);

                using (PartnerEntities context = new PartnerEntities())
                {
                    generatedInvoices = context.invoices.Where(x => x.PAID_DATE == null).OrderByDescending(x => x.INVOICE_DATE).ToList();
                    gvInvoice.DataSource = generatedInvoices;
                    gvInvoice.DataBind();
                }
            }
            else
            {
                gvInvoice.DataSource = generatedInvoices;
                gvInvoice.DataBind();
            }
        }

        protected void gvInvoice_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int INVOICE_ID = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "INVOICE_ID"));
                invoice invoice = generatedInvoices.First(x => x.INVOICE_ID == INVOICE_ID);
                invoice_item invoiceItem = invoice.invoice_item.Single();
                Dictionary<string, string> invoiceMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
                List<KeyValuePair<string, string>> sectionData = invoiceMap.Where(kv => kv.Key.StartsWith("Section-")).ToList();

                // Invoice Date
                TextBox txtInvoiceDate = (TextBox)e.Row.FindControl("txtInvoiceDate");
                var dtInvoice = DataBinder.Eval(e.Row.DataItem, "INVOICE_DATE");
                if (dtInvoice != null)
                {
                    DateTime invoiceDate = DateTime.Parse(dtInvoice.ToString());
                    txtInvoiceDate.Text = ((DateTime)invoiceDate).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else txtInvoiceDate.Text = string.Empty;
                // Due Date
                TextBox txtDueDate = (TextBox)e.Row.FindControl("txtDueDate");
                var dtDue = DataBinder.Eval(e.Row.DataItem, "INVOICE_DATE");
                if (dtDue != null)
                {
                    DateTime dueDate = DateTime.Parse(dtDue.ToString());
                    txtDueDate.Text = ((DateTime)dueDate).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else txtDueDate.Text = string.Empty;

                int sectionNumber = 0;
                foreach (KeyValuePair<string, string> item in sectionData)
                {
                    sectionNumber += 1;
                    LinkButton lb = new LinkButton();
                    lb.ID = "btnViewSection_" + sectionNumber;
                    lb.Text = "Section " + sectionNumber;
                    lb.CommandName = item.Key;
                    lb.Click += ViewReportOnClick;
                    e.Row.Cells[5].Controls.Add(lb);

                    Label lbl = new Label();
                    lbl.Text = " ";
                    e.Row.Cells[5].Controls.Add(lbl);
                }
            }
        }

        private void ViewReportOnClick(object sender, EventArgs eventArgs)
        {
            LinkButton linkButton = (LinkButton)sender;
            String reportName = linkButton.CommandName;
            int startPos = reportName.LastIndexOf("Template-", StringComparison.Ordinal) + "Template-".Length;
            int length = reportName.Length - startPos;
            reportName = reportName.Substring(startPos, length);

            IInvoiceTemplate template = invoiceTemplates[reportName];

            GridViewRow gvrow = (GridViewRow)linkButton.NamingContainer;
            int INVOICE_ID = Convert.ToInt32(gvInvoice.DataKeys[gvrow.RowIndex].Value);
            invoice invoice = generatedInvoices.First(x => x.INVOICE_ID == INVOICE_ID);

            String refNo = Guid.NewGuid().ToString();
            template.GenerateInvoice(invoice);
            this.Session[refNo] = template;
            Response.Redirect("~/config/ViewReport.aspx?refNo=" + HttpUtility.UrlEncode(refNo), false);
            Context.ApplicationInstance.CompleteRequest();

            //string pathtofile = string.Format("{0}\\{1}.pdf", Server.MapPath("/InvoicePdfs"), refNo);
            //template.SaveToPdf(pathtofile);
            // show pdf
            //Response.Redirect("~/config/ViewPDF.aspx?refNo=" + HttpUtility.UrlEncode(refNo), false);
            //Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnOK_Click(object sender, EventArgs e)
        {

        }

        protected void LinkButtonEdit_Click(object sender, EventArgs e)
        {
            this.mpeInvoice.Show();
        }
    }
}