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
                    List<InvoiceDataCollector> summaryForInvoiceGenerations =
                        new List<InvoiceDataCollector>();
                    List<BillingRule> billingRules = context.jsonbillingrules.ToList()
                        .Select(c => JsonConvert.DeserializeObject<BillingRule>(c.JsonExpression)).ToList();
                    List<long> accountIds = context.accounts.Where(x => x.isCustomerAccount == 1 && x.isBillable == 1)
                        .Select(x => x.id).ToList();
                    List<acc_ledger_summary> accLedgerSummaries =
                        context.acc_ledger_summary.Where(x => accountIds.Contains(x.idAccount) && x.AMOUNT != 0)
                            .ToList();
                    bool isAlreadyExists = false;
                    foreach (acc_ledger_summary ledgerSummary in accLedgerSummaries)
                    {
                        var serviceGroup = context.accounts.First(x => x.id == ledgerSummary.idAccount).serviceGroup;
                        var idBillingRule = context.billingruleassignments.First(x => x.idServiceGroup == serviceGroup)
                            .idBillingRule;
                        BillingRule billingRule = billingRules.First(x => x.Id == idBillingRule);
                        TimeRange timeRange =
                            billingRule.GetBillingCycleByBillableItemsDate(ledgerSummary.transactionDate);
                        account account = allAccounts.First(x => x.id == ledgerSummary.idAccount);
                        partner partner = allPartners.First(x => x.idPartner == account.idPartner);

                        if (summaryForInvoiceGenerations.Count > 0)
                        {
                            InvoiceDataCollector cycle = summaryForInvoiceGenerations
                                .FirstOrDefault(x => x.AccountId == ledgerSummary.idAccount &&
                                                     x.PartnerId == partner.idPartner &&
                                                     x.StartDateTime.Equals(timeRange.Start) &&
                                                     x.EndDateTime.Equals(timeRange.End));
                            if (cycle != null)
                            {
                                cycle.Amount += ledgerSummary.AMOUNT;
                                cycle.InvoiceDates.Add(ledgerSummary.transactionDate);
                                isAlreadyExists = true;
                            }
                        }

                        if (!isAlreadyExists)
                        {
                            InvoiceDataCollector invoiceGeneration = new InvoiceDataCollector();
                            invoiceGeneration.PartnerId = partner.idPartner;
                            invoiceGeneration.PartnerName = partner.PartnerName;
                            invoiceGeneration.AccountId = account.id;
                            invoiceGeneration.AccountName = account.accountName;
                            invoiceGeneration.StartDateTime = timeRange.Start;
                            invoiceGeneration.EndDateTime = timeRange.End;
                            invoiceGeneration.Amount = ledgerSummary.AMOUNT;
                            invoiceGeneration.TimeZone = DefaultTimeZoneId;
                            invoiceGeneration.GmtOffset = GmtOffset;
                            invoiceGeneration.InvoiceDates.Add(ledgerSummary.transactionDate);

                            foreach (var kv in serviceAliases)
                            {
                                var regex = kv.Key;
                                if (regex.Matches(invoiceGeneration.AccountName).Count > 0)
                                {
                                    invoiceGeneration.ServiceAccount = kv.Value;
                                    break;
                                }
                            }

                            summaryForInvoiceGenerations.Add(invoiceGeneration);
                        }
                    }

                    BindingList<InvoiceDataCollector> invoiceGenerations =
                        new BindingList<InvoiceDataCollector>(summaryForInvoiceGenerations);
                    this.Session["igInvoiceGenList"] = invoiceGenerations;
                    gvInvoice.DataSource = invoiceGenerations;
                    gvInvoice.DataBind();

                }
            }
        }

        protected void gvInvoice_OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            gvInvoice.EditIndex = e.NewEditIndex;
            BindingList<InvoiceDataCollector> invoiceGenerations =
                (BindingList<InvoiceDataCollector>) this.Session["igInvoiceGenList"];
            gvInvoice.DataSource = invoiceGenerations;
            gvInvoice.DataBind();
        }

        protected void gvInvoice_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                    DropDownList ddlTimeZone = (DropDownList) e.Row.FindControl("ddlistTimeZone");
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
    }
}