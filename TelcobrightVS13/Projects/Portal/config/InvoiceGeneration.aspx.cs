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
                                    invoiceGeneration.ServiceAccountAlias = kv.Value;
                                    break;
                                }
                            }

                            summaryForInvoiceGenerations.Add(invoiceGeneration);
                        }
                    }

                    BindingList<InvoiceDataCollector> invoiceGenerations =
                        new BindingList<InvoiceDataCollector>(summaryForInvoiceGenerations.OrderBy(x => x.PartnerName)
                            .ThenBy(x => x.ServiceAccountAlias).ToList());
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
                        string zoneName = t.zone.country.country_name + " " + t.offsetdesc + " [" + t.zone.zone_name +
                                          "]";
                        ddlistTimeZone.Items.Add(new ListItem(zoneName, t.id.ToString()));
                    }
                    ddlistTimeZone.SelectedValue = DefaultTimeZoneId.ToString();
                    txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
                    txtDate1.Text = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                }
            }
        }

        protected void gvInvoice_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // time zone
                DropDownList ddlTimeZone = (DropDownList)e.Row.FindControl("ddlistTimeZone");
                if (DataBinder.Eval(e.Row.DataItem, "TimeZone") != null)
                {
                    if (this.Session["sesAllTimeZones"] != null)
                    {
                        List<timezone> allTimeZones = (List<timezone>)this.Session["sesAllTimeZones"];
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

                // start date time
                TextBox txtStartDate = (TextBox)e.Row.FindControl("txtStartDate");
                if (DataBinder.Eval(e.Row.DataItem, "StartDateTime") != null)
                {
                    DateTime startDate = DateTime.Parse(DataBinder.Eval(e.Row.DataItem, "StartDateTime").ToString());
                    txtStartDate.Text = startDate.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // end date time
                TextBox txtEndDate = (TextBox)e.Row.FindControl("txtEndDate");
                if (DataBinder.Eval(e.Row.DataItem, "EndDateTime") != null)
                {
                    DateTime startDate = DateTime.Parse(DataBinder.Eval(e.Row.DataItem, "EndDateTime").ToString());
                    txtEndDate.Text = startDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        }

        protected void btnAddInvoiceRow_OnClick(object sender, EventArgs e)
        {
            List<timezone> allTimeZones = (List<timezone>)this.Session["sesAllTimeZones"];
            BindingList<InvoiceDataCollector> invoiceGenerations =
                (BindingList<InvoiceDataCollector>)this.Session["igInvoiceGenList"];
            InvoiceDataCollector ledgerSummary = new InvoiceDataCollector();
            ledgerSummary.PartnerId = Convert.ToInt32(ddlistPartner.SelectedValue);
            ledgerSummary.PartnerName = ddlistPartner.SelectedItem.Text;
            ledgerSummary.ServiceAccountAlias = ddlistServiceAccount.Text;
            ledgerSummary.StartDateTime = Convert.ToDateTime(txtDate.Text);
            ledgerSummary.EndDateTime = Convert.ToDateTime(txtDate1.Text);
            ledgerSummary.TimeZone = Convert.ToInt32(ddlistTimeZone.SelectedValue);
            ledgerSummary.GmtOffset = allTimeZones.First(x => x.id == ledgerSummary.TimeZone).gmt_offset;
            ledgerSummary.InvoiceDates.Add(ledgerSummary.StartDateTime);
            invoiceGenerations.Add(ledgerSummary);
            invoiceGenerations =
                new BindingList<InvoiceDataCollector>(invoiceGenerations.OrderBy(x => x.PartnerName)
                    .ThenBy(x => x.ServiceAccountAlias).ToList());
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = invoiceGenerations;
            gvInvoice.DataBind();
        }

        protected void btnGenerateInvoice_OnClick(object sender, EventArgs e)
        {
            try
            {
                int batchSizeForJobSegments = 10000;
                BindingList<InvoiceDataCollector> boundRowsForInvoiceGeneration =
                    (BindingList<InvoiceDataCollector>)this.Session["igInvoiceGenList"];
                List<job> invoicingJobs = new List<job>();
                if (this.gvInvoice.Rows.Count <= 0)
                {
                    throw new Exception("No row selected for invoice generation.");
                }
                using (PartnerEntities context = new PartnerEntities())
                {
                    for (var index = 0; index < this.gvInvoice.Rows.Count; index++)
                    {
                        GridViewRow invoiceRow = this.gvInvoice.Rows[index];
                        CheckBox cbSelect = (CheckBox)invoiceRow.FindControl("cbSelect");
                        if (cbSelect.Checked)
                        {
                            var invoiceDataCollector = boundRowsForInvoiceGeneration[invoiceRow.RowIndex];
                            if (invoiceDataCollector.EndDateTimeLocal <
                                invoiceDataCollector.StartDateTimeLocal)
                            {
                                throw new Exception("EndDatetime must be >= StartDateTime for all rows, check row no:" +
                                                    index + 1);
                            }
                            if (invoiceDataCollector.AccountId <= 0)
                            {
                                throw new Exception("AccountId must be >=0, check row no:" + index + 1);
                            }
                            job invoicingJob = CreateInvoiceGenerationJob(invoiceDataCollector, context,
                                batchSizeForJobSegments);
                            invoicingJobs.Add(invoicingJob);
                        }
                    }
                    context.jobs.AddRange(invoicingJobs);
                    context.SaveChanges();
                    this.lblStatus.ForeColor = Color.Black;
                    this.lblStatus.Text = invoicingJobs.Count + " invoicing job(s) created.";
                }
            }
            catch (Exception exception)
            {
                this.lblStatus.ForeColor = Color.Red;
                this.lblStatus.Text = exception.Message;
            }
        }


        protected job CreateInvoiceGenerationJob(InvoiceDataCollector invoiceDataCollector,
            PartnerEntities context, int batchSizeForJobSegment)
        {
            string serviceAccount = invoiceDataCollector.ServiceAccountAlias;
            int jobDefinition = 12;
            int prevJobCountWithSameName =
                context.jobs.Count(j => j.idjobdefinition == jobDefinition && j.idjobdefinition == jobDefinition);
            long glAccountId = invoiceDataCollector.AccountId;
            Dictionary<string, string> jobParamsMap = new Dictionary<string, string>();
            string sourceTable = "acc_transaction";
            jobParamsMap.Add("sourceTable", sourceTable);
            jobParamsMap.Add("serviceAccountId", glAccountId.ToString());
            jobParamsMap.Add("startDate", invoiceDataCollector.StartDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote());
            jobParamsMap.Add("endDate", invoiceDataCollector.EndDateTime.ToMySqlStyleDateTimeStrWithoutQuote());
            jobParamsMap.Add("timeZoneOffsetSec", invoiceDataCollector.GmtOffset.ToString());
            
            if (prevJobCountWithSameName > 0)
            {
                serviceAccount = serviceAccount + "_" + prevJobCountWithSameName;
            }
            string jobName = invoiceDataCollector.PartnerName + "/" + serviceAccount
                             + "/" + invoiceDataCollector.StartDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote()
                             + " to " +
                             invoiceDataCollector.EndDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote();

            List<SqlSingleWhereClauseBuilder> singleWhereClauses = new List<SqlSingleWhereClauseBuilder>();
            List<SqlMultiWhereClauseBuilder> multipleWhereClauses = new List<SqlMultiWhereClauseBuilder>();
            SqlSingleWhereClauseBuilder newParam = null;
            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.FirstBeforeAndOr);
            newParam.Expression = "transactionTime>=";
            newParam.ParamType = SqlWhereParamType.Datetime;
            newParam.ParamValue = invoiceDataCollector.StartDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote();
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "transactionTime<=";
            newParam.ParamType = SqlWhereParamType.Datetime;
            newParam.ParamValue = invoiceDataCollector.EndDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote();
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "glAccountId=";
            newParam.ParamType = SqlWhereParamType.Numeric;
            newParam.ParamValue = glAccountId.ToString();
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "isBillable=";
            newParam.ParamType = SqlWhereParamType.Numeric;
            newParam.ParamValue = "1";
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "isBilled is null or isBilled<>";
            newParam.ParamType = SqlWhereParamType.Numeric;
            newParam.ParamValue = "1";
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "cancelled is null or cancelled<>";
            newParam.ParamType = SqlWhereParamType.Numeric;
            newParam.ParamValue = "1";
            singleWhereClauses.Add(newParam);

            BatchSqlJobParamJson sqlParam = new BatchSqlJobParamJson
            (
                sourceTable,
                batchSizeForJobSegment,
                singleWhereClauses,
                multipleWhereClauses,
                columnExpressions: new List<string>() { "id as RowId", "transactionTime as RowDateTime" }
            );

            int jobPriority = context.enumjobdefinitions.Where(j => j.id == 12).Select(j => j.Priority).First();
            jobParamsMap.Add("sqlParam", JsonConvert.SerializeObject(sqlParam));
            job newjob = new job();
            newjob.Progress = 0;
            newjob.idjobdefinition = 12; //invoicing job
            newjob.Status = 6; //created
            newjob.JobName = jobName;
            newjob.idjobdefinition = jobDefinition;
            newjob.CreationTime = DateTime.Now;
            newjob.idNE = 0;
            newjob.JobParameter = JsonConvert.SerializeObject(jobParamsMap);
            newjob.priority = jobPriority;
            return newjob;
        }

        protected void ddlistTimeZone_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlTimeZone = sender as DropDownList;
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            List<timezone> allTimeZones = (List<timezone>)this.Session["sesAllTimeZones"];
            BindingList<InvoiceDataCollector> invoiceGenerations =
                (BindingList<InvoiceDataCollector>)this.Session["igInvoiceGenList"];
            InvoiceDataCollector editRow = invoiceGenerations[row.RowIndex];
            editRow.TimeZone = Convert.ToInt32(ddlTimeZone.SelectedValue);
            editRow.GmtOffset = allTimeZones.First(x => x.id == editRow.TimeZone).gmt_offset;
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = invoiceGenerations;
            gvInvoice.DataBind();
        }

        protected void txtStartDate_TextChanged(object sender, EventArgs e)
        {
            TextBox txtStartDate = sender as TextBox;
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            BindingList<InvoiceDataCollector> invoiceGenerations =
                (BindingList<InvoiceDataCollector>)this.Session["igInvoiceGenList"];
            InvoiceDataCollector editRow = invoiceGenerations[row.RowIndex];
            editRow.StartDateTime = DateTime.Parse(txtStartDate.Text);
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = invoiceGenerations;
            gvInvoice.DataBind();
        }

        protected void txtEndDate_TextChanged(object sender, EventArgs e)
        {
            TextBox txtEndDate = sender as TextBox;
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            BindingList<InvoiceDataCollector> invoiceGenerations =
                (BindingList<InvoiceDataCollector>)this.Session["igInvoiceGenList"];
            InvoiceDataCollector editRow = invoiceGenerations[row.RowIndex];
            editRow.EndDateTime = DateTime.Parse(txtEndDate.Text);
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = invoiceGenerations;
            gvInvoice.DataBind();
        }
    }
}