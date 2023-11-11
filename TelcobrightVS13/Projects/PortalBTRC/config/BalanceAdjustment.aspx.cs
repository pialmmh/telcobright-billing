using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;

namespace PortalApp.config
{
    public partial class BalanceAdjustment : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }
        private static List<partner> AllPartners { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Tbc = PageUtil.GetTelcobrightConfig();
                using (PartnerEntities context = new PartnerEntities())
                {

                    AllPartners = context.partners.OrderBy(i => i.PartnerName).ToList();
                    List<account> allAccounts = context.accounts.ToList();

                    List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                    List<AccBalanceAdjRowDataCollector> adjustments = new List<AccBalanceAdjRowDataCollector>();

                    List<long> accountIds = context.accounts.Where(x => x.isCustomerAccount == 1 && x.isBillable == 1)
                        .Select(x => x.id).ToList();

                    int RowId = -1;
                    foreach (account account in allAccounts.Where(x => accountIds.Contains(x.id)).ToList())
                    {
                        RowId += 1;
                        AccBalanceAdjRowDataCollector balanceAdjDataCollector = new AccBalanceAdjRowDataCollector()
                        {
                            RowId = RowId,
                            AccountId = account.id,
                            AccountName = account.accountName,
                            PartnerId = account.idPartner,
                            PartnerName = AllPartners.First(x => x.idPartner == account.idPartner).PartnerName,
                            StartDateTime = DateTime.Now.Date,
                            Currency = account.uom
                        };

                        foreach (var kv in serviceAliases)
                        {
                            var regex = kv.Key;
                            if (regex.Matches(balanceAdjDataCollector.AccountName).Count > 0)
                            {
                                balanceAdjDataCollector.ServiceAccountAlias = kv.Value;
                                break;
                            }
                        }
                        adjustments.Add(balanceAdjDataCollector);
                    }

                    BindingList<AccBalanceAdjRowDataCollector> balanceAdjustments =
                        new BindingList<AccBalanceAdjRowDataCollector>(adjustments.OrderBy(x => x.PartnerName)
                            .ThenBy(x => x.ServiceAccountAlias).ToList());

                    this.Session["baBalanceAdjList"] = balanceAdjustments;
                    gvInvoice.DataSource = GetFilteredItems(balanceAdjustments);
                    gvInvoice.DataBind();

                    ddlistPartnerFilter.Items.Clear();
                    ddlistPartnerFilter.Items.Add(new ListItem(" [All]", "-1"));
                    foreach (partner p in AllPartners)
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
        }

        protected void gvInvoice_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // start date time
                TextBox txtStartDate = (TextBox)e.Row.FindControl("txtStartDate");
                if (DataBinder.Eval(e.Row.DataItem, "StartDateTime") != null)
                {
                    DateTime startDate = DateTime.Parse(DataBinder.Eval(e.Row.DataItem, "StartDateTime").ToString());
                    txtStartDate.Text = startDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                // Amount
                TextBox txtCurrentBalance = (TextBox) e.Row.FindControl("txtCurrentBalance");
                if (DataBinder.Eval(e.Row.DataItem, "CurrentBalance") != null)
                {
                    decimal CurrentBalance = decimal.Parse(DataBinder.Eval(e.Row.DataItem, "CurrentBalance").ToString());
                    txtCurrentBalance.Text = CurrentBalance.ToString("n4");
                }
            }
        }

        protected void btnBalanceAdjustment_OnClick(object sender, EventArgs e)
        {
            try
            {
                int batchSizeForJobSegments = 10000;
                BindingList<AccBalanceAdjRowDataCollector> boundRowsForBalanceAdjustment =
                    (BindingList<AccBalanceAdjRowDataCollector>)this.Session["baBalanceAdjList"];
                List<job> adjustmentJobs = new List<job>();
                if (this.gvInvoice.Rows.Count <= 0)
                {
                    throw new Exception("No row selected for balance adjustment.");
                }
                using (PartnerEntities context = new PartnerEntities())
                {
                    for (var index = 0; index < this.gvInvoice.Rows.Count; index++)
                    {
                        GridViewRow adjustmentRow = this.gvInvoice.Rows[index];
                        CheckBox cbSelect = (CheckBox)adjustmentRow.FindControl("cbSelect");
                        if (cbSelect.Checked)
                        {
                            AccBalanceAdjRowDataCollector invoiceGenRowDataCollector = boundRowsForBalanceAdjustment[adjustmentRow.RowIndex];
                            if (invoiceGenRowDataCollector.AccountId <= 0)
                            {
                                throw new Exception("AccountId must be >=0, check row no:" + index + 1);
                            }
                            job adjustmentJob = CreateBalanceAdjustmentJob(invoiceGenRowDataCollector, context,
                                batchSizeForJobSegments);

                            adjustmentJobs.Add(adjustmentJob);
                        }
                    }
                    context.jobs.AddRange(adjustmentJobs);
                    context.SaveChanges();
                    this.lblStatus.ForeColor = Color.Black;
                    this.lblStatus.Text = adjustmentJobs.Count + " adjustment job(s) created.";
                }
            }
            catch (Exception exception)
            {
                this.lblStatus.ForeColor = Color.Red;
                this.lblStatus.Text = exception.Message;
            }
        }


        protected job CreateBalanceAdjustmentJob(AccBalanceAdjRowDataCollector adjustmentDataCollector,
            PartnerEntities context, int batchSizeForJobSegment)
        {
            string serviceAccount = adjustmentDataCollector.ServiceAccountAlias;
            int jobDefinition = 13;
            int prevJobCountWithSameName =
                context.jobs.Count(j => j.idjobdefinition == jobDefinition && j.idjobdefinition == jobDefinition);
            long glAccountId = adjustmentDataCollector.AccountId;
            Dictionary<string, string> jobParamsMap = new Dictionary<string, string>();
            jobParamsMap.Add("serviceAccountId", glAccountId.ToString());
            jobParamsMap.Add("startDate", adjustmentDataCollector.StartDateTime.ToMySqlFormatWithoutQuote());
            jobParamsMap.Add("amount", adjustmentDataCollector.CurrentBalance.ToMySqlField());
            if (prevJobCountWithSameName > 0)
            {
                serviceAccount = serviceAccount + "_" + prevJobCountWithSameName;
            }
            string jobName = "Balance Adjustment/" + 
                adjustmentDataCollector.PartnerName + "/" + serviceAccount
                + "/" + adjustmentDataCollector.StartDateTime.ToMySqlFormatWithoutQuote();

            int jobPriority = context.enumjobdefinitions.Where(j => j.id == 12).Select(j => j.Priority).First();
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
            return newjob;
        }

        protected void txtStartDate_TextChanged(object sender, EventArgs e)
        {
            TextBox txtStartDate = sender as TextBox;
            DateTime startDate = Convert.ToDateTime(txtStartDate.Text);
            txtStartDate.Text = startDate.ToString("yyyy-MM-dd 00:00:00");
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            int rowId = Convert.ToInt32(gvInvoice.DataKeys[row.RowIndex].Value);
            BindingList<AccBalanceAdjRowDataCollector> invoiceGenerations =
                (BindingList<AccBalanceAdjRowDataCollector>)this.Session["baBalanceAdjList"];
            AccBalanceAdjRowDataCollector editRow = invoiceGenerations.First(x => x.RowId == rowId);
            editRow.StartDateTime = DateTime.Parse(txtStartDate.Text);
            this.Session["baBalanceAdjList"] = invoiceGenerations;
            gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
            gvInvoice.DataBind();
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
            BindingList<AccBalanceAdjRowDataCollector> invoiceGenerations =
                (BindingList<AccBalanceAdjRowDataCollector>)this.Session["baBalanceAdjList"];
            gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
            gvInvoice.DataBind();
        }

        private List<AccBalanceAdjRowDataCollector> GetFilteredItems(
            BindingList<AccBalanceAdjRowDataCollector> invoiceGenerations)
        {
            List<AccBalanceAdjRowDataCollector> filteredInvoiceGenerations = invoiceGenerations.ToList();

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

            return filteredInvoiceGenerations;
        }

        protected void txtCurrentBalance_OnTextChanged(object sender, EventArgs e)
        {
            TextBox txtCurrentBalance = sender as TextBox;
            Decimal currentBalance = Convert.ToDecimal(txtCurrentBalance.Text);
            txtCurrentBalance.Text = currentBalance.ToString("n4");
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            int rowId = Convert.ToInt32(gvInvoice.DataKeys[row.RowIndex].Value);
            BindingList<AccBalanceAdjRowDataCollector> invoiceGenerations =
                (BindingList<AccBalanceAdjRowDataCollector>)this.Session["baBalanceAdjList"];
            AccBalanceAdjRowDataCollector editRow = invoiceGenerations.First(x => x.RowId == rowId);
            editRow.CurrentBalance = Convert.ToDecimal(txtCurrentBalance.Text);
            this.Session["baBalanceAdjList"] = invoiceGenerations;
            gvInvoice.DataSource = GetFilteredItems(invoiceGenerations);
            gvInvoice.DataBind();
        }
    }
}