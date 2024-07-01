using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;
using System.ComponentModel;
using PortalApp._portalHelper;

namespace PortalApp.config
{
    public partial class AddPayment : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }
        private static List<IAutomationAction> availableActions { get; set; }
        private account account { get; set; }

        private BindingList<AccActionEx> actions = new BindingList<AccActionEx>();
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Tbc = PageUtil.GetTelcobrightConfig();
                List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                int accountId = Convert.ToInt32(Request.QueryString["accountId"]);
                using (PartnerEntities context = new PartnerEntities()) {
                    List<partner> allPartners = context.partners.OrderBy(p => p.PartnerName).ToList();
                    Session["allPartners"] = allPartners;

                    account = context.accounts.First(x => x.id == accountId);
                    foreach (var kv in serviceAliases)
                    {
                        var regex = kv.Key;
                        if (regex.Matches(account.accountName).Count > 0)
                        {
                            account.accountName = kv.Value;
                            break;
                        }
                    }
                    this.Session["pmAccount"] = account;
                }

                ddlistType.Items.Clear();
                ddlistType.Items.Add("PrevBalance");
                switch (account.billableType)
                {
                    case "/billable":           // postpaid
                        ddlistType.Items.Add("Payment");
                        ddlistType.SelectedValue = "Payment";
                        break;
                    case "/custBilled":         // prepaid
                        ddlistType.Items.Add("TopUp");
                        ddlistType.Items.Add("Credit");
                        ddlistType.SelectedValue = "TopUp";
                        AddDefaultAccountActions();
                        break;
                    default:
                        throw new Exception("Unsupported account type");
                }
                txtDate.Text = DateTime.Now.Date.ToString("yyyy-MM-dd");
            }

            account = (account)this.Session["pmAccount"];
            lblID.Text = account.id.ToString();
            lblusername.Text = ((List<partner>)Session["allPartners"]).First(x => x.idPartner == account.idPartner).PartnerName;
            lblSer.Text = account.accountName;
            lblCurrentBalance.Text = account.getCurrentBalanceWithTempTransaction().ToString();
            actions = (BindingList<AccActionEx>)this.Session["pmActions"];
            gvThreshold.DataSource = actions;
            gvThreshold.DataBind();

            lbAddRule.Visible = account.billableType == "/custBilled";
            cbThresholdSettings.Visible = account.billableType == "/custBilled";
        }

        private void AddDefaultAccountActions()
        {
            // TODO: Replace 4 with correct variable
            List<int> serviceGroupWithActions = new List<int>() {4, 100};
            availableActions = Tbc.CdrSetting.ServiceGroupConfigurations
                .First(x => serviceGroupWithActions.Contains(x.Key)).Value.AccountActions;

            actions.Add(new AccActionEx()
            {
                idAccount = account.id,
                threshhold_value = 0,
                idAccountAction = 1,
                Rule = new AccountActionRule()
                {
                    IsPercent = true,
                    Amount = 50
                }
            });
            actions.Add(new AccActionEx()
            {
                idAccount = account.id,
                threshhold_value = 0,
                idAccountAction = 1,
                Rule = new AccountActionRule()
                {
                    IsPercent = true,
                    Amount = 20
                }
            });
            actions.Add(new AccActionEx()
            {
                idAccount = account.id,
                threshhold_value = 0,
                idAccountAction = 3,
                Rule = new AccountActionRule()
                {
                    IsPercent = true,
                    Amount = 10
                }
            });
            this.Session["pmActions"] = actions;

            ddlAccountAction.DataSource = availableActions;
            ddlAccountAction.DataBind();
        }


        protected void gvThreshold_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                TextBox txtLimit = (TextBox)e.Row.FindControl("txtLimit");
                String threshhold_value = DataBinder.Eval(e.Row.DataItem, "threshhold_value").ToString();
                txtLimit.Text = threshhold_value;

                DropDownList ddlAccountAction = (DropDownList)e.Row.FindControl("ddlAccountAction");
                ddlAccountAction.DataSource = availableActions;
                ddlAccountAction.DataBind();
                string idAccountAction = DataBinder.Eval(e.Row.DataItem, "idAccountAction").ToString();
                ddlAccountAction.Items.FindByValue(idAccountAction).Selected = true;

                String RuleDescription = DataBinder.Eval(e.Row.DataItem, "RuleDescription").ToString();
                Label lblRule = (Label)e.Row.FindControl("lblRule");
                lblRule.Text = RuleDescription;

                LinkButton lb = new LinkButton();
                lb.ID = "btnShowRule";
                lb.Text = "Change";
                lb.CommandName = "ShowPopup";
                lb.Click += new System.EventHandler(ruleBtn_Click);
                e.Row.Cells[2].Controls.Add(lb);
            }

        }

        protected void ruleBtn_Click(object sender, EventArgs e)
        {
            LinkButton btndetails = sender as LinkButton;
            GridViewRow gvrow = (GridViewRow)btndetails.NamingContainer;
            hfRowIndex.Value = gvrow.RowIndex.ToString();
            AccActionEx action = actions[gvrow.RowIndex];
            ddlAccountAction.SelectedValue = action.idAccountAction.ToString();
            if (action.Rule.IsFixedAmount)
            {
                rbIsFixedAmount.Checked = true;
                txtFixedAmount.Text = action.threshhold_value.ToString();
            }
            else if (action.Rule.IsPercent)
            {
                rbIsPercent.Checked = true;
                txtPercentage.Text = action.Rule.Amount.ToString();
            }
            else if (action.Rule.IsFormulaBased)
            {
                rbIsFormulaBased.Checked = true;
                txtACD.Text = action.Rule.ACD.ToString();
                txtACR.Text = action.Rule.ACR.ToString();
                txtMinute.Text = action.Rule.Amount.ToString();
                txtNoOfPorts.Text = action.Rule.NoOfPorts.ToString();
            }
            txtResult.Text = action.threshhold_value.ToString();

            this.mpeRule.Show();
        }

        protected void txtAmount_TextChanged(object sender, EventArgs e)
        {
            account = (account)this.Session["pmAccount"];
            switch (account.billableType)
            {
                case "/custBilled":
                    actions = (BindingList<AccActionEx>)this.Session["pmActions"];
                    decimal amount = Convert.ToDecimal(txtAmount.Text) + account.getCurrentBalanceWithTempTransaction();
                    foreach (AccActionEx item in actions)
                    {
                        if (item.Rule.IsPercent)
                        {
                            item.threshhold_value = (amount) * item.Rule.Amount / 100;
                        }
                        else if (item.Rule.IsFormulaBased)
                        {
                            item.threshhold_value = (item.Rule.Amount * 60 / item.Rule.ACD) * item.Rule.ACR * item.Rule.NoOfPorts;
                        }
                    }

                    this.Session["pmActions"] = actions;
                    gvThreshold.DataSource = actions;
                    gvThreshold.DataBind();
                    break;
                case "/billable": break;
                default:
                    throw new Exception("Unsupported account type");
            }
        }

        protected void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                account = (account)this.Session["pmAccount"];
                decimal amount = decimal.Parse(txtAmount.Text);
                if (amount <= 0) throw new Exception("Please enter a positive amount");
                DateTime payDate = Convert.ToDateTime(txtDate.Text);
                using (PartnerEntities context = new PartnerEntities())
                {
                    if (cbThresholdSettings.Checked)
                    {
                        actions = (BindingList<AccActionEx>)this.Session["pmActions"];
                        List<acc_action> existingActions = context.acc_action.Where(x => x.idAccount == account.id).ToList();
                        foreach (acc_action item in existingActions)
                        {
                            context.acc_action.Remove(item);
                        }
                        foreach (AccActionEx item in actions)
                        {
                            acc_action action = new acc_action();
                            action.idAccount = account.id;
                            action.idAccountAction = item.idAccountAction;
                            action.threshhold_value = item.threshhold_value;
                            context.acc_action.Add(action);
                        }
                        context.SaveChanges();
                    }

                    var con = context.Database.Connection;
                    using (DbCommand cmd = con.CreateCommand())
                    {
                        if (con.State != ConnectionState.Open) con.Open();
                        TempTransactionHelper.CreateTempTransaction(account.id, amount, payDate, ddlistType.SelectedValue, cmd, account);
                        Response.Redirect("~/config/PaymentManagement.aspx", false);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/config/PaymentManagement.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnRuleOK_Click(object sender, EventArgs e)
        {
            int RowIndex = Convert.ToInt32(hfRowIndex.Value);
            AccActionEx action = actions[RowIndex];
            AccountActionRule aar = action.Rule;
            aar.IsFixedAmount = rbIsFixedAmount.Checked;
            aar.IsPercent = rbIsPercent.Checked;
            aar.IsFormulaBased = rbIsFormulaBased.Checked;
            if (aar.IsFixedAmount)
            {
                aar.Amount = Convert.ToDecimal(txtFixedAmount.Text);
            }
            else if (aar.IsPercent)
            {
                aar.Amount = Convert.ToDecimal(txtPercentage.Text);
            }
            else if (aar.IsFormulaBased)
            {
                aar.ACR = Convert.ToDecimal(txtACR.Text);
                aar.ACD = Convert.ToDecimal(txtACD.Text);
                aar.Amount = Convert.ToDecimal(txtMinute.Text);
                aar.NoOfPorts = Convert.ToInt32(txtNoOfPorts.Text);
            }
            action.idAccountAction = Convert.ToInt32(ddlAccountAction.SelectedValue);
            action.threshhold_value = Convert.ToDecimal(txtResult.Text);
            this.Session["pmActions"] = actions;
            gvThreshold.DataSource = actions;
            gvThreshold.DataBind();
        }

        protected void CalculateThresholdValue(object sender, EventArgs e)
        {
            account = (account)this.Session["pmAccount"];
            decimal Amount = Convert.ToDecimal(txtAmount.Text) + account.getCurrentBalanceWithTempTransaction();
            if (rbIsFixedAmount.Checked)
            {
                txtResult.Text = txtFixedAmount.Text;
            }
            else if (rbIsPercent.Checked)
            {
                decimal percent = Convert.ToDecimal(txtPercentage.Text);
                txtResult.Text = ((Amount) * percent / 100).ToString();
            }
            else if (rbIsFormulaBased.Checked)
            {
                decimal ACD = Convert.ToDecimal(txtACD.Text);
                decimal ACR = Convert.ToDecimal(txtACR.Text);
                decimal Minute = Convert.ToDecimal(txtMinute.Text);
                int NoOfPorts = Convert.ToInt32(txtNoOfPorts.Text);
                txtResult.Text = ((Minute * 60 / ACD) * ACR * NoOfPorts).ToString();
            }
        }

        protected void lbAddRule_Click(object sender, EventArgs e)
        {
            account = (account)this.Session["pmAccount"];
            actions = (BindingList<AccActionEx>)this.Session["pmActions"];
            actions.Add(new AccActionEx()
            {
                idAccount = account.id,
                threshhold_value = 0,
                idAccountAction = 1,
                Rule = new AccountActionRule()
                {
                    IsFixedAmount = true,
                    Amount = 0
                }
            });
            this.Session["pmActions"] = actions;
            gvThreshold.DataSource = actions;
            gvThreshold.DataBind();
        }
    }
}


