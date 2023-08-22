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
        private static List<invoice> generatedInvoices { get; set; }
        private static List<partner> allPartners { get; set; }
        private static List<account> allAccounts { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Tbc = PageUtil.GetTelcobrightConfig();
                List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;

                TextBoxYear.Text = DateTime.Now.Year.ToString();
                DropDownListMonth.SelectedValue = DateTime.Now.Month.ToString("00");

                using (PartnerEntities context = new PartnerEntities())
                {
                    allPartners = context.partners.OrderBy(i => i.PartnerName).ToList();
                    allAccounts = context.accounts.ToList();

                    List<invoice> invoices = context.invoices.Where(x => x.PAID_DATE == null).OrderByDescending(x => x.INVOICE_DATE).ToList();
                    generatedInvoices = GetFilteredItems(invoices);
                    gvInvoice.DataSource = generatedInvoices;
                    gvInvoice.DataBind();

                    ddlistPartnerFilter.Items.Clear();
                    ddlistPartnerFilter.Items.Add(new ListItem(" [All]", "-1"));
                    foreach (partner p in allPartners)
                    {
                        ddlistPartnerFilter.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                    }

                    ddlistServiceAccountFilter.Items.Clear();
                    ddlistServiceAccountFilter.Items.Add(new ListItem(" [All]", "-1"));
                    foreach (var kv in serviceAliases)
                    {
                        ddlistServiceAccountFilter.Items.Add(kv.Value);
                    }
                }
            }
            else
            {
                gvInvoice.DataSource = GetFilteredItems(generatedInvoices);
                gvInvoice.DataBind();
            }
        }

        protected void gvInvoice_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                using (PartnerEntities context = new PartnerEntities())
                {
                    int INVOICE_ID = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "INVOICE_ID"));
                    invoice invoice = context.invoices.First(x => x.INVOICE_ID == INVOICE_ID);
                    invoice_item invoiceItem = invoice.invoice_item.Single();
                    Dictionary<string, string> invoiceMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
                    List<KeyValuePair<string, string>> sectionData = invoiceMap.Where(kv => kv.Key.StartsWith("Section-")).ToList();

                    Label lblPartner = (Label)e.Row.FindControl("lblPartner");
                    account account = allAccounts.First(x => x.id == invoice.BILLING_ACCOUNT_ID);
                    lblPartner.Text = allPartners.First(x => x.idPartner == account.idPartner).PartnerName;

                    // Invoice Date
                    Label lblInvoiceDate = (Label)e.Row.FindControl("lblInvoiceDate");
                    var dtInvoice = DataBinder.Eval(e.Row.DataItem, "INVOICE_DATE");
                    if (dtInvoice != null)
                    {
                        DateTime invoiceDate = DateTime.Parse(dtInvoice.ToString());
                        lblInvoiceDate.Text = ((DateTime)invoiceDate).ToString("yyyy-MM-dd");
                    }
                    else lblInvoiceDate.Text = string.Empty;
                    // Due Date
                    Label lblDueDate = (Label)e.Row.FindControl("lblDueDate");
                    var dtDue = DataBinder.Eval(e.Row.DataItem, "Due_DATE");
                    if (dtDue != null)
                    {
                        DateTime dueDate = DateTime.Parse(dtDue.ToString());
                        lblDueDate.Text = ((DateTime)dueDate).ToString("yyyy-MM-dd");
                    }
                    else lblDueDate.Text = string.Empty;


                    LinkButton lbAll = new LinkButton();
                    lbAll.ID = "btnViewSectionAll";
                    lbAll.Text = "All";
                    //lbAll.CommandName = item.Key;
                    //lb.Click += ViewReportOnClick;
                    //lb.OnClientClick = "window.open('PrepareReport.aspx?reportName=" + HttpUtility.HtmlDecode(item.Key) + "&invoiceId=" + DataBinder.Eval(e.Row.DataItem, "INVOICE_ID") + "'); return false;";
                    e.Row.Cells[8].Controls.Add(lbAll);

                    Label lb10 = new Label();
                    lb10.Text = " ";
                    e.Row.Cells[8].Controls.Add(lb10);

                    int sectionNumber = 0;


                    foreach (KeyValuePair<string, string> item in sectionData)
                    {
                        sectionNumber += 1;
                        LinkButton lb = new LinkButton();
                        lb.ID = "btnViewSection_" + sectionNumber;
                        lb.Text = "Section " + sectionNumber;
                        lb.CommandName = item.Key;
                        //lb.Click += ViewReportOnClick;
                        lb.OnClientClick = "window.open('PrepareReport.aspx?reportName=" + HttpUtility.HtmlDecode(item.Key) + "&invoiceId=" + DataBinder.Eval(e.Row.DataItem, "INVOICE_ID") + "'); return false;";
                        e.Row.Cells[8].Controls.Add(lb);

                        Label lbl = new Label();
                        lbl.Text = " ";
                        e.Row.Cells[8].Controls.Add(lbl);
                    }
                }
            }
        }

        protected void btnOK_Click(object sender, EventArgs e)
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                int invoiceId = Convert.ToInt32(hfInvoiceId.Value);
                invoice invoice = context.invoices.First(x => x.INVOICE_ID == invoiceId);
                invoice.REFERENCE_NUMBER = TextBoxReferenceNumber.Text;
                invoice.INVOICE_DATE = Convert.ToDateTime(TextBoxInvoiceDate.Text);
                //invoice.DUE_DATE = Convert.ToDateTime(TextBoxDueDate.Text);
                context.SaveChanges();

                List<invoice> invoices = context.invoices.Where(x => x.PAID_DATE == null).OrderByDescending(x => x.INVOICE_DATE).ToList();
                generatedInvoices = GetFilteredItems(invoices);
                gvInvoice.DataSource = generatedInvoices;
                gvInvoice.DataBind();
            }
            this.mpeInvoice.Hide();
        }

        protected void LinkButtonEdit_Click(object sender, EventArgs e)
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                LinkButton btnEdit = sender as LinkButton;
                GridViewRow gvrow = (GridViewRow)btnEdit.NamingContainer;
                int INVOICE_ID = Convert.ToInt32(gvInvoice.DataKeys[gvrow.RowIndex].Value);
                invoice invoice = context.invoices.First(x => x.INVOICE_ID == INVOICE_ID);
                hfInvoiceId.Value = invoice.INVOICE_ID.ToString();
                LabelDESCRIPTION.Text = invoice.DESCRIPTION;
                TextBoxReferenceNumber.Text = invoice.REFERENCE_NUMBER; 
                if (invoice.INVOICE_DATE != null)
                    TextBoxInvoiceDate.Text = ((DateTime)invoice.INVOICE_DATE).ToString("yyyy-MM-dd");
                if (invoice.DUE_DATE != null)
                    TextBoxDueDate.Text = ((DateTime)invoice.DUE_DATE).ToString("yyyy-MM-dd");
                this.upInner.Update();
                this.mpeInvoice.Show();
            }
        }

        protected void LinkButtonDelete_Click(object sender, EventArgs e)
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                context.Database.Connection.Open();
                System.Data.Common.DbCommand cmd = context.Database.Connection.CreateCommand();
                LinkButton btnEdit = sender as LinkButton;
                GridViewRow gvrow = (GridViewRow)btnEdit.NamingContainer;
                int invoiceId = Convert.ToInt32(gvInvoice.DataKeys[gvrow.RowIndex].Value);
                invoice invoice = context.invoices.First(x => x.INVOICE_ID == invoiceId);

                try
                {
                    cmd.ExecuteCommandText($@" set autocommit = 0; ");
                    cmd.ExecuteCommandText($@" delete from invoice_item where invoice_id ={invoiceId}; ");
                    cmd.ExecuteCommandText($@" delete from invoice where invoice_id = {invoiceId}; ");
                    cmd.ExecuteCommandText($@" delete from acc_ledger_summary_billed where invoiceOrEventId={invoiceId}; ");
                    cmd.ExecuteCommandText($@" delete from acc_temp_transaction where idEvent={invoiceId}; ");
                    cmd.ExecuteCommandText($@" set autocommit = 1; ");
                }
                catch (Exception e1) {
                    cmd.ExecuteCommandText($@" rollback; ");
                    cmd.ExecuteCommandText($@" set autocommit=1; ");
                    throw e1;
                }
                this.upInner.Update();
                //this.mpeInvoice.Show();
            }
        }

        private List<invoice> GetFilteredItems(List<invoice> generatedInvoices)
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                List<invoice> filteredInvoices = generatedInvoices;

                if (cbPartnerFilter.Checked && ddlistPartnerFilter.SelectedIndex != 0)
                {
                    int idPartner = Convert.ToInt32(ddlistPartnerFilter.SelectedValue);
                    List<long> accountIds = context.accounts.Where(x => x.idPartner == idPartner).Select(x => x.id).ToList();
                    filteredInvoices = filteredInvoices
                        .Where(x => accountIds.Contains((long) x.BILLING_ACCOUNT_ID)).ToList();
                }

                if (cbServiceAccountFilter.Checked && ddlistServiceAccountFilter.SelectedIndex != 0)
                {
                    List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                    List<invoice> tempInvoices = new List<invoice>();
                    foreach (invoice filteredInvoice in filteredInvoices)
                    {
                        account account = context.accounts.First(x => x.id == filteredInvoice.BILLING_ACCOUNT_ID);
                        foreach (var kv in serviceAliases)
                        {
                            var regex = kv.Key;
                            if (regex.Matches(account.accountName).Count > 0 && kv.Value == ddlistServiceAccountFilter.SelectedValue)
                            {
                                tempInvoices.Add(filteredInvoice);
                                break;
                            }
                        }
                    }
                    filteredInvoices = tempInvoices;
                }

                if (cbMonthFilter.Checked)
                {
                    DateTime fromDate = new DateTime(Convert.ToInt32(TextBoxYear.Text), Convert.ToInt32(DropDownListMonth.SelectedValue), 1);
                    DateTime tillDate = fromDate.AddMonths(1).AddDays(-1);
                    filteredInvoices = filteredInvoices
                        .Where(x => x.INVOICE_DATE >= fromDate && x.INVOICE_DATE <= tillDate).ToList();
                }

                return filteredInvoices;
            }
        }

        protected void cbPartnerFilter_OnCheckedChanged(object sender, EventArgs e)
        {
            ddlistPartnerFilter.SelectedIndex = -1;
            ddlistPartnerFilter.Enabled = cbPartnerFilter.Checked;
        }

        protected void btnShow_OnClick(object sender, EventArgs e)
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                List<invoice> invoices = context.invoices.Where(x => x.PAID_DATE == null).OrderByDescending(x => x.INVOICE_DATE).ToList();
                generatedInvoices = GetFilteredItems(invoices);
                gvInvoice.DataSource = generatedInvoices;
                gvInvoice.DataBind();
            }
        }

        protected void btnCancel_OnClick(object sender, EventArgs e)
        {
            this.mpeInvoice.Hide();
        }

        protected void cbMonthFilter_OnCheckedChanged(object sender, EventArgs e)
        {
            TextBoxYear.Enabled = cbMonthFilter.Checked;
            DropDownListMonth.Enabled = cbMonthFilter.Checked;
        }

        protected void cbServiceAccountFilter_OnCheckedChanged(object sender, EventArgs e)
        {
            ddlistServiceAccountFilter.SelectedIndex = -1;
            ddlistServiceAccountFilter.Enabled = cbServiceAccountFilter.Checked;
        }
    }
}