using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Itenso.TimePeriod;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;

namespace PortalApp.config
{
    public partial class InvoiceGeneration : System.Web.UI.Page
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

                    List<timezone> tz = context.timezones.Include("zone.country")
                        .OrderBy(o => o.zone.country.country_name).ToList();
                    this.Session["sesAllTimeZones"] = tz;

                    string sql = "select p.idpartner as PartnerId, p.PartnerName, a.accountName as AccountName, " +
                        "idaccount as AccountId, StartDate, EndDate, Amount, " + DefaultTimeZoneId + " as TimeZone, "+ GmtOffset + " as GmtOffset " +
                        "from (select idaccount, min(transactionDate) StartDate, max(transactiondate) EndDate, sum(amount) Amount " +
                        "from acc_ledger_summary " +
                        "where idaccount in (select id from account where isCustomerAccount = 1 and isBillable = 1) " +
                        "and AMOUNT <> 0 group by idAccount) ls left join account a " +
                        "on ls.idAccount = a.id left join partner p " +
                        "on a.idPartner = p.idPartner;";
                    List<LedgerSummaryForInvoiceGeneration> summaryForInvoiceGenerations = context.Database.SqlQuery<LedgerSummaryForInvoiceGeneration>(sql).ToList();
                    List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                    foreach (LedgerSummaryForInvoiceGeneration summaryItem in summaryForInvoiceGenerations)
                    {
                        foreach (var kv in serviceAliases)
                        {
                            var regex = kv.Key;
                            if (regex.Matches(summaryItem.AccountName).Count > 0)
                            {
                                summaryItem.ServiceAccount = kv.Value;
                                break;
                            }
                        }

                    }

                    //AccountFactory accountFactory = new AccountFactory(null);
                    //foreach (var accounts in summaryForInvoiceGenerations.GroupBy(x => x.AccountName))
                    //{
                    //    List<KeyValuePair<string, string>> accountParts = accountFactory.GetAccountParts(accounts.First().AccountName);
                    //}

                    summaryForInvoiceGenerations = new List<LedgerSummaryForInvoiceGeneration>();
                    List<BillingRule> billingRules = context.jsonbillingrules.ToList().Select(c => JsonConvert.DeserializeObject<BillingRule>(c.JsonExpression)).ToList();
                    List<long> accountIds = context.accounts.Where(x => x.isCustomerAccount == 1 && x.isBillable == 1)
                        .Select(x => x.id).ToList();
                    List<acc_ledger_summary> accLedgerSummaries = context.acc_ledger_summary.Where(x => accountIds.Contains(x.idAccount) && x.AMOUNT != 0).ToList();
                    foreach (acc_ledger_summary ledgerSummary in accLedgerSummaries)
                    {
                        var serviceGroup = context.accounts.First(x => x.id == ledgerSummary.idAccount).serviceGroup;
                        var idBillingRule = context.billingruleassignments.First(x => x.idServiceGroup == serviceGroup).idBillingRule;
                        BillingRule billingRule = billingRules.First(x => x.Id == idBillingRule);
                        TimeRange timeRange = billingRule.GetBillingCycleByBillableItemsDate(ledgerSummary.transactionDate);

                        LedgerSummaryForInvoiceGeneration ledgerSummaryForInvoiceGeneration = new LedgerSummaryForInvoiceGeneration();
                        summaryForInvoiceGenerations.Add(ledgerSummaryForInvoiceGeneration);
                    }

                    BindingList<LedgerSummaryForInvoiceGeneration> invoiceGenerations = new BindingList<LedgerSummaryForInvoiceGeneration>(summaryForInvoiceGenerations);
                    this.Session["igInvoiceGenList"] = invoiceGenerations;
                    gvInvoice.DataSource = invoiceGenerations;
                    gvInvoice.DataBind();

                    ddlistPartner.DataSource = allPartners;
                    ddlistPartner.DataBind();

                    foreach (var kv in serviceAliases)
                    {
                        ddlistServiceAccount.Items.Add(kv.Value);
                    }

                    foreach (timezone t in tz)
                    {
                        string zoneName = t.zone.country.country_name + " " + t.offsetdesc + " [" + t.zone.zone_name + "]";
                        ddlistTimeZone.Items.Add(new ListItem(zoneName, t.id.ToString()));
                    }
                    ddlistTimeZone.SelectedValue = DefaultTimeZoneId.ToString();
                    txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
                    txtDate1.Text = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                }
            }
        }

        protected void gvInvoice_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            gvInvoice.EditIndex = e.NewEditIndex;
            BindingList<LedgerSummaryForInvoiceGeneration> invoiceGenerations = (BindingList<LedgerSummaryForInvoiceGeneration>)this.Session["igInvoiceGenList"];
            gvInvoice.DataSource = invoiceGenerations;
            gvInvoice.DataBind();
        }

        protected void gvInvoice_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                    DropDownList ddlTimeZone = (DropDownList)e.Row.FindControl("ddlistTimeZone");
                    if (DataBinder.Eval(e.Row.DataItem, "TimeZone") != null)
                    {
                        if (this.Session["sesAllTimeZones"] != null)
                        {
                            List<timezone> allTimeZones = (List<timezone>) this.Session["sesAllTimeZones"];
                            foreach (timezone t in allTimeZones)
                            {
                                string zoneName = t.zone.country.country_name + " " + t.offsetdesc + " [" +
                                                  t.zone.zone_name + "]";
                                ddlTimeZone.Items.Add(new ListItem(zoneName, t.id.ToString()));
                            }
                        }
                        int idTimeZone = int.Parse(DataBinder.Eval(e.Row.DataItem, "TimeZone").ToString());
                        ddlTimeZone.SelectedValue = idTimeZone.ToString();
                    }
                }
                else
                {
                    Label thisLabel = (Label) e.Row.FindControl("lblTimeZone");
                    if (DataBinder.Eval(e.Row.DataItem, "TimeZone") != null)
                    {
                        int idTimeZone = int.Parse(DataBinder.Eval(e.Row.DataItem, "TimeZone").ToString());
                        if (idTimeZone > 0)
                        {
                            if (this.Session["sesAllTimeZones"] != null)
                            {
                                List<timezone> allTimeZones = (List<timezone>) this.Session["sesAllTimeZones"];
                                string tzName = (from c in allTimeZones
                                    where c.id == idTimeZone
                                    select c.zone.country.country_name + " " + c.offsetdesc + " [" + c.zone.zone_name +
                                           "]").First();
                                thisLabel.Text = tzName;
                            }
                        }

                    }
                }
            }
        }

        protected void btnAddInvoiceRow_OnClick(object sender, EventArgs e)
        {
            List<timezone> allTimeZones = (List<timezone>)this.Session["sesAllTimeZones"];
            BindingList<LedgerSummaryForInvoiceGeneration> invoiceGenerations = (BindingList<LedgerSummaryForInvoiceGeneration>)this.Session["igInvoiceGenList"];
            LedgerSummaryForInvoiceGeneration ledgerSummary = new LedgerSummaryForInvoiceGeneration();
            ledgerSummary.PartnerId = Convert.ToInt32(ddlistPartner.SelectedValue);
            ledgerSummary.PartnerName = ddlistPartner.SelectedItem.Text;
            ledgerSummary.ServiceAccount = ddlistServiceAccount.Text;
            ledgerSummary.StartDateWithTime = Convert.ToDateTime(txtDate.Text);
            ledgerSummary.EndDateWithTime = Convert.ToDateTime(txtDate1.Text);
            ledgerSummary.TimeZone = Convert.ToInt32(ddlistTimeZone.SelectedValue);
            ledgerSummary.GmtOffset = allTimeZones.First(x => x.id == ledgerSummary.TimeZone).gmt_offset;
            invoiceGenerations.Add(ledgerSummary);
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = invoiceGenerations;
            gvInvoice.DataBind();
        }

        protected void gvInvoice_OnRowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int index = e.RowIndex;
            DropDownList ddlTimeZone = gvInvoice.Rows[index].FindControl("ddlistTimeZone") as DropDownList;
            if (ddlTimeZone != null)
            {
                gvInvoice.EditIndex = -1;
                List<timezone> allTimeZones = (List<timezone>)this.Session["sesAllTimeZones"];
                BindingList<LedgerSummaryForInvoiceGeneration> invoiceGenerations = (BindingList<LedgerSummaryForInvoiceGeneration>)this.Session["igInvoiceGenList"];
                LedgerSummaryForInvoiceGeneration editRow = invoiceGenerations.First(x => x.PartnerId == Convert.ToInt32(e.Keys[0]));
                editRow.TimeZone = Convert.ToInt32(ddlTimeZone.SelectedValue);
                editRow.GmtOffset = allTimeZones.First(x => x.id == editRow.TimeZone).gmt_offset;
                this.Session["igInvoiceGenList"] = invoiceGenerations;
                gvInvoice.DataSource = invoiceGenerations;
                gvInvoice.DataBind();
            }
        }

        protected void gvInvoice_OnRowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvInvoice.EditIndex = -1;
            BindingList<LedgerSummaryForInvoiceGeneration> invoiceGenerations = (BindingList<LedgerSummaryForInvoiceGeneration>)this.Session["igInvoiceGenList"];
            gvInvoice.DataSource = invoiceGenerations;
            gvInvoice.DataBind();
        }

        protected void btnGenerateInvoice_OnClick(object sender, EventArgs e)
        {
            BindingList<LedgerSummaryForInvoiceGeneration> invoiceGenerations = (BindingList<LedgerSummaryForInvoiceGeneration>)this.Session["igInvoiceGenList"];
            List<LedgerSummaryForInvoiceGeneration> summaryForInvoiceGenerations = new List<LedgerSummaryForInvoiceGeneration>();
            foreach (GridViewRow invoiceRow in gvInvoice.Rows)
            {
                CheckBox cbSelect = (CheckBox)invoiceRow.FindControl("cbSelect");
                if (cbSelect.Checked)
                {
                    summaryForInvoiceGenerations.Add(invoiceGenerations[invoiceRow.RowIndex]);
                }
            }
        }
    }
}