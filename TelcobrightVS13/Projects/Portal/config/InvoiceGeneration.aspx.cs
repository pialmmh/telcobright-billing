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
using System.IO;
namespace PortalApp.config
{
    public partial class InvoiceGeneration : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }
        private static int DefaultTimeZoneId = 3251;
        private static int GmtOffset = 21600;
        private static Dictionary<int, IServiceGroup> MefServiceGroups { get; set; }
        private static List<partner> allPartners { get; set; }
        private static List<timezone> allTimeZones { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Tbc = PageUtil.GetTelcobrightConfig();
                using (PartnerEntities context = new PartnerEntities())
                {

                    allPartners = context.partners.OrderBy(i => i.PartnerName).ToList();
                    List<account> allAccounts = context.accounts.ToList();

                    allTimeZones = context.timezones.Include("zone.country")
                        .OrderBy(o => o.zone.country.country_name).ToList();

                    List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                    List<InvoiceGenRowDataCollector> summaryForInvoiceGenerations =
                        new List<InvoiceGenRowDataCollector>();
                    List<BillingRule> billingRules = context.jsonbillingrules.ToList()
                        .Select(c => JsonConvert.DeserializeObject<BillingRule>(c.JsonExpression)).ToList();
                    List<long> accountIds = context.accounts.Where(x => x.isCustomerAccount == 1 && x.isBillable == 1)
                        .Select(x => x.id).ToList();
                    List<acc_ledger_summary> accLedgerSummaries =
                        context.acc_ledger_summary.Where(x => accountIds.Contains(x.idAccount) && x.AMOUNT != 0)
                            .ToList();
                    bool isAlreadyExists = false;
                    ServiceGroupComposer composer=new ServiceGroupComposer();
                    var dir=new DirectoryInfo(PageUtil.GetPortalBinPath());
                    composer.ComposeFromPath(dir.Parent.GetDirectories().Single(d => d.Name == "Extensions")
                        .FullName);
                    MefServiceGroups = composer.ServiceGroups.ToDictionary(s=>s.Id);
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
                            InvoiceGenRowDataCollector cycle = summaryForInvoiceGenerations
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
                            decimal billedSum = context.acc_ledger_summary_billed
                                .Where(x => x.idAccount == account.id && x.transactionDate >= timeRange.Start &&
                                            x.transactionDate <= timeRange.End)
                                            .Select(l => l.billedAmount)
                                            .DefaultIfEmpty(0)
                                            .Sum();
                            ledgerSummary.AMOUNT += billedSum;

                            if (ledgerSummary.AMOUNT != 0)
                            {
                                InvoiceGenRowDataCollector invoiceDataCollector =
                                    new InvoiceGenRowDataCollector(MefServiceGroups)
                                    {
                                        PartnerId = partner.idPartner,
                                        PartnerName = partner.PartnerName,
                                        AccountId = account.id,
                                        AccountName = account.accountName,
                                        StartDateTime = timeRange.Start,
                                        EndDateTime = timeRange.End,
                                        Amount = ledgerSummary.AMOUNT,
                                        TimeZone = DefaultTimeZoneId,
                                        GmtOffset = GmtOffset,
                                        Currency = account.uom
                                    };
                                invoiceDataCollector.InvoiceDates.Add(ledgerSummary.transactionDate);

                                foreach (var kv in serviceAliases)
                                {
                                    var regex = kv.Key;
                                    if (regex.Matches(invoiceDataCollector.AccountName).Count > 0)
                                    {
                                        invoiceDataCollector.ServiceAccountAlias = kv.Value;
                                        break;
                                    }
                                }
                                summaryForInvoiceGenerations.Add(invoiceDataCollector);
                            }
                        }
                    }

                    BindingList<InvoiceGenRowDataCollector> invoiceGenerations =
                        new BindingList<InvoiceGenRowDataCollector>(summaryForInvoiceGenerations.Where(x => x.Amount != 0).OrderBy(x => x.PartnerName)
                            .ThenBy(x => x.ServiceAccountAlias).ToList());

                    // show current balance
                    int RowId = -1;
                    foreach (InvoiceGenRowDataCollector item in invoiceGenerations)
                    {
                        RowId += 1;
                        item.RowId = RowId;
                        item.CurrentBalance = allAccounts.First(x => x.id == item.AccountId).getCurrentBalanceWithTempTransaction();
                    }
                    this.Session["igInvoiceGenList"] = invoiceGenerations;
                    gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
                    gvInvoice.DataBind();

                    ddlistPartner.DataSource = allPartners;
                    ddlistPartner.DataBind();

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
                        ddlistServiceAccount.Items.Add(kv.Value);
                        ddlistServiceAccountFilter.Items.Add(kv.Value);
                    }

                    foreach (timezone t in allTimeZones)
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
                    if (allTimeZones != null)
                    {
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

                // is due
                Label lblDue = (Label)e.Row.FindControl("lblDue");
                if (Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "IsDue"))) lblDue.Text = "Yes";
                else lblDue.Text = "No";
            }
        }

        protected void btnAddInvoiceRow_OnClick(object sender, EventArgs e)
        {
            BindingList<InvoiceGenRowDataCollector> invoiceGenerations =
                (BindingList<InvoiceGenRowDataCollector>)this.Session["igInvoiceGenList"];
            InvoiceGenRowDataCollector ledgerSummary =
                new InvoiceGenRowDataCollector(MefServiceGroups)
                {
                    PartnerId = Convert.ToInt32(ddlistPartner.SelectedValue),
                    PartnerName = ddlistPartner.SelectedItem.Text,
                    ServiceAccountAlias = ddlistServiceAccount.Text,
                    StartDateTime = Convert.ToDateTime(txtDate.Text),
                    EndDateTime = Convert.ToDateTime(txtDate1.Text),
                    TimeZone = Convert.ToInt32(ddlistTimeZone.SelectedValue)
                };
            ledgerSummary.GmtOffset = allTimeZones.First(x => x.id == ledgerSummary.TimeZone).gmt_offset;
            ledgerSummary.InvoiceDates.Add(ledgerSummary.StartDateTime);
            ledgerSummary.RowId = invoiceGenerations.Max(x => x.RowId) + 1;
            invoiceGenerations.Add(ledgerSummary);
            invoiceGenerations =
                new BindingList<InvoiceGenRowDataCollector>(invoiceGenerations.OrderBy(x => x.PartnerName)
                    .ThenBy(x => x.ServiceAccountAlias).ToList());
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
            gvInvoice.DataBind();
        }

        protected void btnGenerateInvoice_OnClick(object sender, EventArgs e)
        {
            try
            {
                int batchSizeForJobSegments = 10000;
                BindingList<InvoiceGenRowDataCollector> boundRowsForInvoiceGeneration =
                    (BindingList<InvoiceGenRowDataCollector>)this.Session["igInvoiceGenList"];
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
                            InvoiceGenRowDataCollector invoiceGenRowDataCollector = boundRowsForInvoiceGeneration[invoiceRow.RowIndex];
                            if (invoiceGenRowDataCollector.EndDateTimeLocal <
                                invoiceGenRowDataCollector.StartDateTimeLocal)
                            {
                                throw new Exception("EndDatetime must be >= StartDateTime for all rows, check row no:" +
                                                    index + 1);
                            }
                            if (invoiceGenRowDataCollector.AccountId <= 0)
                            {
                                throw new Exception("AccountId must be >=0, check row no:" + index + 1);
                            }
                            job invoicingJob = CreateInvoiceGenerationJob(invoiceGenRowDataCollector, context,
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


        protected job CreateInvoiceGenerationJob(InvoiceGenRowDataCollector invoiceDataCollector,
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
            jobParamsMap.Add("startDate", invoiceDataCollector.StartDateTimeLocal.ToMySqlFormatWithoutQuote());
            jobParamsMap.Add("endDate", invoiceDataCollector.EndDateTime.ToMySqlFormatWithoutQuote());
            jobParamsMap.Add("timeZoneOffsetSec", invoiceDataCollector.GmtOffset.ToString());
            jobParamsMap.Add("timeZoneId", invoiceDataCollector.TimeZone.ToString());
            if (prevJobCountWithSameName > 0)
            {
                serviceAccount = serviceAccount + "_" + prevJobCountWithSameName;
            }
            string jobName = invoiceDataCollector.PartnerName + "/" + serviceAccount
                             + "/" + invoiceDataCollector.StartDateTimeLocal.ToMySqlFormatWithoutQuote()
                             + " to " + invoiceDataCollector.EndDateTimeLocal.ToMySqlFormatWithoutQuote();
            BatchSqlJobParamJson sqlParam =
                PrepareSqlParams(invoiceDataCollector, batchSizeForJobSegment, glAccountId, sourceTable);

            int jobPriority = context.enumjobdefinitions.Where(j => j.id == 12).Select(j => j.Priority).First();
            jobParamsMap.Add("sqlParam", JsonConvert.SerializeObject(sqlParam));
            job newjob = new job
            {
                Progress = 0,
                idjobdefinition = jobDefinition,
                Status = 6,
                JobName = jobName,
                CreationTime = DateTime.Now,
                idNE = 0,
                JobParameter = JsonConvert.SerializeObject(jobParamsMap),
                priority = jobPriority
            };
            int idServiceGroup = context.accounts.Single(a => a.id == glAccountId).serviceGroup;
            IServiceGroup serviceGroup = null;
            invoiceDataCollector.MefServiceGroups.TryGetValue(idServiceGroup, out serviceGroup);
            InvoiceGenerationValidatorInput validatorInput=new InvoiceGenerationValidatorInput(context,newjob);
            if (serviceGroup == null)
                throw new Exception("Service group not found.");
            serviceGroup.ValidateInvoiceGenerationParams(validatorInput);
            return newjob;
        }

        private static BatchSqlJobParamJson PrepareSqlParams(InvoiceGenRowDataCollector invoiceDataCollector, int batchSizeForJobSegment, long glAccountId, string sourceTable)
        {
            List<SqlSingleWhereClauseBuilder> singleWhereClauses = new List<SqlSingleWhereClauseBuilder>();
            List<SqlMultiWhereClauseBuilder> multipleWhereClauses = new List<SqlMultiWhereClauseBuilder>();
            SqlSingleWhereClauseBuilder newParam = null;
            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.FirstBeforeAndOr);
            newParam.Expression = "transactionTime>=";
            newParam.ParamType = SqlWhereParamType.Datetime;
            newParam.ParamValue = invoiceDataCollector.StartDateTimeLocal.ToMySqlFormatWithoutQuote();
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "transactionTime<=";
            newParam.ParamType = SqlWhereParamType.Datetime;
            newParam.ParamValue = invoiceDataCollector.EndDateTimeLocal.ToMySqlFormatWithoutQuote();
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
            return sqlParam;
        }

        protected void ddlistTimeZone_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlTimeZone = sender as DropDownList;
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            int rowId = Convert.ToInt32(gvInvoice.DataKeys[row.RowIndex].Value);
            BindingList<InvoiceGenRowDataCollector> invoiceGenerations =
                (BindingList<InvoiceGenRowDataCollector>)this.Session["igInvoiceGenList"];
            InvoiceGenRowDataCollector editRow = invoiceGenerations.First(x => x.RowId == rowId);
            editRow.TimeZone = Convert.ToInt32(ddlTimeZone.SelectedValue);
            editRow.GmtOffset = allTimeZones.First(x => x.id == editRow.TimeZone).gmt_offset;
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
            gvInvoice.DataBind();
        }

        protected void txtStartDate_TextChanged(object sender, EventArgs e)
        {
            TextBox txtStartDate = sender as TextBox;
            DateTime startDate = Convert.ToDateTime(txtStartDate.Text);
            txtStartDate.Text = startDate.ToString("yyyy-MM-dd 00:00:00");
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            int rowId = Convert.ToInt32(gvInvoice.DataKeys[row.RowIndex].Value);
            BindingList<InvoiceGenRowDataCollector> invoiceGenerations =
                (BindingList<InvoiceGenRowDataCollector>)this.Session["igInvoiceGenList"];
            InvoiceGenRowDataCollector editRow = invoiceGenerations.First(x => x.RowId == rowId);
            editRow.StartDateTime = DateTime.Parse(txtStartDate.Text);
            editRow.Amount = getAmount(editRow);
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
            gvInvoice.DataBind();
        }

        protected void txtEndDate_TextChanged(object sender, EventArgs e)
        {
            TextBox txtEndDate = sender as TextBox;
            DateTime endDate = Convert.ToDateTime(txtEndDate.Text);
            txtEndDate.Text = endDate.ToString("yyyy-MM-dd 23:59:59");
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            int rowId = Convert.ToInt32(gvInvoice.DataKeys[row.RowIndex].Value);
            BindingList<InvoiceGenRowDataCollector> invoiceGenerations =
                (BindingList<InvoiceGenRowDataCollector>)this.Session["igInvoiceGenList"];
            InvoiceGenRowDataCollector editRow = invoiceGenerations.First(x => x.RowId == rowId);
            editRow.EndDateTime = DateTime.Parse(txtEndDate.Text);
            editRow.Amount = getAmount(editRow);
            this.Session["igInvoiceGenList"] = invoiceGenerations;
            gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
            gvInvoice.DataBind();
        }

        private decimal getAmount(InvoiceGenRowDataCollector editRow)
        {
            decimal amount = 0;
            using (PartnerEntities context = new PartnerEntities())
            {
                List<acc_ledger_summary> accLedgerSummaries =
                    context.acc_ledger_summary.Where(x => x.idAccount == editRow.AccountId && x.AMOUNT != 0)
                        .ToList();
                foreach (acc_ledger_summary ledgerSummary in accLedgerSummaries) {
                    if (ledgerSummary.transactionDate >= editRow.StartDateTime && ledgerSummary.transactionDate <= editRow.EndDateTime)
                        amount += ledgerSummary.AMOUNT;
                }

                decimal billedSum = context.acc_ledger_summary_billed
                    .Where(x => x.idAccount == editRow.AccountId && x.transactionDate >= editRow.StartDateTime &&
                                x.transactionDate <= editRow.EndDateTime)
                    .Select(l => l.billedAmount)
                    .DefaultIfEmpty(0)
                    .Sum();

                amount += billedSum;
            }
            return amount;
        }

        protected void cbPartnerFilter_OnCheckedChanged(object sender, EventArgs e)
        {
            ddlistPartnerFilter.SelectedIndex = -1;
            ddlistPartnerFilter.Enabled = cbPartnerFilter.Checked;
        }

        protected void cbServiceAccountFilter_OnCheckedChanged(object sender, EventArgs e)
        {
            ddlistServiceAccountFilter.SelectedIndex = -1;
            ddlistServiceAccountFilter.Enabled = cbServiceAccountFilter.Checked;
        }

        protected void btnShow_OnClick(object sender, EventArgs e)
        {
            BindingList<InvoiceGenRowDataCollector> invoiceGenerations =
                (BindingList<InvoiceGenRowDataCollector>)this.Session["igInvoiceGenList"];
            gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
            gvInvoice.DataBind();
        }

        private List<InvoiceGenRowDataCollector> GetFilteredItems(
            BindingList<InvoiceGenRowDataCollector> invoiceGenerations)
        {
            List<InvoiceGenRowDataCollector> filteredInvoiceGenerations = invoiceGenerations.ToList();

            if (cbPartnerFilter.Checked && ddlistPartnerFilter.SelectedIndex != 0)
            {
                int idPartner = Convert.ToInt32(ddlistPartnerFilter.SelectedValue);
                filteredInvoiceGenerations = filteredInvoiceGenerations
                    .Where(x => x.PartnerId == idPartner).ToList();
            }

            if (cbServiceAccountFilter.Checked && ddlistServiceAccountFilter.SelectedIndex != 0)
            {
                filteredInvoiceGenerations = filteredInvoiceGenerations
                    .Where(x => x.ServiceAccountAlias == ddlistServiceAccountFilter.SelectedValue.ToString()).ToList();
            }

            if (cbDueOnly.Checked)
            {
                filteredInvoiceGenerations = filteredInvoiceGenerations
                    .Where(x => x.IsDue).ToList();
            }

            return filteredInvoiceGenerations;
        }

    }
}