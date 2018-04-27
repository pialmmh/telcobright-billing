using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;

namespace PortalApp.config
{
    public partial class InvoiceGeneration : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }

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
                        "idaccount as AccountId, StartDate, EndDate, Amount, 3250 as TimeZone " +
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

                    this.Session["igInvoiceGenList"] = summaryForInvoiceGenerations;
                    gvInvoice.DataSource = summaryForInvoiceGenerations;
                    gvInvoice.DataBind();

                    ddlistPartner.DataSource = allPartners;
                    ddlistPartner.DataBind();

                    foreach (var kv in serviceAliases)
                    {
                        ddlistServiceAccount.Items.Add(kv.Value);
                    }
                }
            }
        }

        protected void gvInvoice_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            
        }

        protected void gvInvoice_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label thisLabel = (Label)e.Row.FindControl("lblTimeZone");
                if (DataBinder.Eval(e.Row.DataItem, "TimeZone") != null)
                {
                    int idTimeZone = int.Parse(DataBinder.Eval(e.Row.DataItem, "TimeZone").ToString());
                    if (idTimeZone > 0)
                    {
                        if (this.Session["sesAllTimeZones"] != null)
                        {
                            List<timezone> allTimeZones = (List<timezone>)this.Session["sesAllTimeZones"];
                            string tzName = (from c in allTimeZones
                                where c.id == idTimeZone
                                select c.zone.country.country_name + " " + c.offsetdesc + " [" + c.zone.zone_name + "]").First();
                            thisLabel.Text = tzName;
                        }
                    }

                }

            }
        }

        protected void btnAddInvoiceRow_OnClick(object sender, EventArgs e)
        {
            List<LedgerSummaryForInvoiceGeneration> summaryForInvoiceGenerations = (List<LedgerSummaryForInvoiceGeneration>)this.Session["igInvoiceGenList"];
            LedgerSummaryForInvoiceGeneration ledgerSummary = new LedgerSummaryForInvoiceGeneration();
            ledgerSummary.ServiceAccount = ddlistServiceAccount.Text;
            ledgerSummary.TimeZone = Convert.ToInt32(ddlistTimeZone.SelectedValue);
            summaryForInvoiceGenerations.Add(ledgerSummary);
            gvInvoice.DataSource = summaryForInvoiceGenerations;
            gvInvoice.DataBind();

        }
    }
}