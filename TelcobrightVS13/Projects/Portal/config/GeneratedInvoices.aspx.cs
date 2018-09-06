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
        private static int DefaultTimeZoneId = 3251;
        private static int GmtOffset = 21600;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Tbc = PageUtil.GetTelcobrightConfig();
                using (PartnerEntities context = new PartnerEntities())
                {

                    List<partner> allPartners = context.partners.OrderBy(i => i.PartnerName).ToList();
                    this.Session["sesAllPartners"] = allPartners;
                    List<account> allAccounts = context.accounts.ToList();

                    List<timezone> tz = context.timezones.Include("zone.country")
                        .OrderBy(o => o.zone.country.country_name).ToList();
                    this.Session["sesAllTimeZones"] = tz;

                    List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                    List<invoice> generatedInvoices = context.invoices.Where(x => x.PAID_DATE == null).OrderByDescending(x => x.INVOICE_DATE).ToList();
                    this.Session["generatedInvoices"] = generatedInvoices;
                    gvInvoice.DataSource = generatedInvoices;
                    gvInvoice.DataBind();

                }
            }
        }

        protected void gvInvoice_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int INVOICE_ID = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "INVOICE_ID"));
                List<invoice> generatedInvoices = (List<invoice>)this.Session["generatedInvoices"];
                invoice invoice = generatedInvoices.First(x => x.INVOICE_ID == INVOICE_ID);
                InvoiceSectionDataRetriever<InvoiceSectionDataRowForVoiceCall> sectionDataRetriever =
                    new InvoiceSectionDataRetriever<InvoiceSectionDataRowForVoiceCall>();
                List<InvoiceSectionDataRowForVoiceCall> sectionData =
                    sectionDataRetriever.GetSectionData(invoice, sectionNumber: 1);
                sectionData.Count();
                // Invoice type
                /*
                DropDownList ddlistInvoiceType = (DropDownList)e.Row.FindControl("ddlistInvoiceType");
                foreach (var item in Enum.GetValues(typeof(InvoiceReportType)))
                {
                    ddlistInvoiceType.Items.Add(item.ToString());
                }
                string invoiceType = DataBinder.Eval(e.Row.DataItem, "INVOICE_TYPE_ID").ToString();
                ddlistInvoiceType.SelectedValue = invoiceType;

                // Partner
                List<partner> allPartners = (List<partner>)this.Session["sesAllPartners"];
                DropDownList ddlistPartner = (DropDownList)e.Row.FindControl("ddlistPartner");
                ddlistPartner.DataSource = allPartners;
                ddlistPartner.DataBind();
                int idPartner = int.Parse(DataBinder.Eval(e.Row.DataItem, "PARTY_ID").ToString());
                ddlistPartner.SelectedValue = idPartner.ToString();

                // Invoice Date
                TextBox txtInvoiceDate = (TextBox)e.Row.FindControl("txtInvoiceDate");
                DateTime invoiceDate = DateTime.Parse(DataBinder.Eval(e.Row.DataItem, "INVOICE_DATE").ToString());
                txtInvoiceDate.Text = invoiceDate.ToString("yyyy-MM-dd HH:mm:ss");

                // Due Date
                TextBox txtDueDate = (TextBox)e.Row.FindControl("txtDueDate");
                DateTime dueDate = DateTime.Parse(DataBinder.Eval(e.Row.DataItem, "DUE_DATE").ToString());
                txtDueDate.Text = dueDate.ToString("yyyy-MM-dd HH:mm:ss");
                */
            }
        }

        protected void lbSaveAsPdf_Click(object sender, EventArgs e)
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                LinkButton lbSaveAsPdf = sender as LinkButton;
                GridViewRow gvrow = (GridViewRow)lbSaveAsPdf.NamingContainer;
                int invoiceId = Convert.ToInt32(gvInvoice.DataKeys[gvrow.RowIndex].Value);
                invoice invoice = context.invoices.First(x => x.INVOICE_ID == invoiceId);

                InvoiceHelper invoiceHelper = new InvoiceHelper(invoice);
                /*
                Invoice preparedInvoice = invoiceHelper.GetInvoice();
                if (preparedInvoice != null)
                {
                    Type reportType = Type.GetType("ReportGenerator.reports.invoice.igw." + invoice.INVOICE_TYPE_ID);
                    if (reportType == null)
                    {
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            reportType = asm.GetType("ReportGenerator.reports.invoice.igw." + invoice.INVOICE_TYPE_ID);
                            if (reportType != null)
                                break;
                        }
                    }

                    String refNo = Guid.NewGuid().ToString();
                    IInvoiceReport invoiceReport = (IInvoiceReport)Activator.CreateInstance(reportType, preparedInvoice);
                    string pathtofile = string.Format("{0}\\{1}.pdf", Server.MapPath("/InvoicePdfs"), refNo);
                    invoiceReport.saveToPdf(pathtofile);

                    // show pdf
                    Response.Redirect("~/config/ViewPDF.aspx?refNo=" + HttpUtility.UrlEncode(refNo), false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else throw new Exception("Invalid invoice data");
                */
            }
        }
    }
}