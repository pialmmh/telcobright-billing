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
using PortalApp._myCodes;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;
using System.ComponentModel;

namespace PortalApp.config
{
    public partial class PaymentManagement : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }
        private static List<AccountAction> availableActions { get; set; }

        private BindingList<AccActionEx> actions = new BindingList<AccActionEx>();

        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlPartner = (DropDownList)e.Row.FindControl("ddlPartner");
                ddlPartner.DataSource = Session["allPartners"];
                ddlPartner.DataBind();
                string idPartner = DataBinder.Eval(e.Row.DataItem, "idPartner").ToString();
                ddlPartner.Items.FindByValue(idPartner).Selected = true;

                LinkButton lb = new LinkButton();
                lb.ID = "paymentBtn";
                lb.Text = "Add Payment";
                lb.CommandName = "ShowPopup";
                lb.Click += new System.EventHandler(paymentBtn_Click);
                e.Row.Cells[5].Controls.Add(lb);
            }
        }
        
        protected void btnOK_Click(object sender, EventArgs e)
        {
            string log = string.Empty;
            try
            {
                int accountId = Int32.Parse(lblID.Text);
                decimal amount = decimal.Parse(txtAmount.Text);
                DateTime payDate = Convert.ToDateTime(txtDate.Text);
                using (PartnerEntities context = new PartnerEntities())
                {
                    var con = context.Database.Connection;
                    using (DbCommand cmd=con.CreateCommand())
                    {
                        if (con.State != ConnectionState.Open) con.Open();
                        account account = context.accounts.Where(x => x.id == accountId).ToList().First();
                        TempTransactionCreator.CreateTempTransaction(accountId, amount, payDate, cmd, account);
                    }

                    //context.Database.Log = logInfo => FileLogger.Log(logInfo);
                    //context.acc_temp_transaction.Add(transaction);
                    //context.SaveChanges();

                    actions = (BindingList<AccActionEx>)this.Session["pmActions"];
                    List<acc_action> existingActions = context.acc_action.Where(x => x.idAccount == accountId).ToList();
                    foreach (acc_action item in existingActions)
                    {
                        context.acc_action.Remove(item);
                    }
                    foreach (AccActionEx item in actions)
                    {
                        acc_action action = new acc_action();
                        action.idAccount = accountId;
                        action.idAccountAction = item.idAccountAction;
                        action.threshhold_value = item.threshhold_value;
                        context.acc_action.Add(action);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Tbc = PageUtil.GetTelcobrightConfig();
            }
            List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
            List<string> billableType = new List<string>()
            {
                "/custBilled", "/suppBilled", "/billable"
            };

            // TODO: get this from config
            availableActions = new List<AccountAction>();
            availableActions.Add(new SendAlertEmailAccountAction());
            availableActions.Add(new SendSMSAccountAction());
            availableActions.Add(new BlockAccountAction());

            using (PartnerEntities context = new PartnerEntities())
            {
                List<partner> allPartners = context.partners.OrderBy(p => p.PartnerName).ToList();
                Session["allPartners"] = allPartners;
                List<account> payableAccounts = context.accounts.Where(x => billableType.Contains(x.billableType)).ToList();
                foreach (account account in payableAccounts)
                {
                    foreach (var kv in serviceAliases)
                    {
                        var regex = kv.Key;
                        if (regex.Matches(account.accountName).Count > 0)
                        {
                            account.accountName = kv.Value;
                            break;
                        }
                    }
                }

                GridView.DataSource = payableAccounts;
                GridView.DataBind();
            }
        }

        protected void paymentBtn_Click(object sender, EventArgs e)
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                LinkButton btndetails = sender as LinkButton;
                GridViewRow gvrow = (GridViewRow)btndetails.NamingContainer;
                int accountId = Convert.ToInt32(GridView.DataKeys[gvrow.RowIndex].Value);
                account account = context.accounts.First(x => x.id == accountId);
                lblID.Text = accountId.ToString();
                lblusername.Text = ((List<partner>)Session["allPartners"]).First(x => x.idPartner == account.idPartner).PartnerName;
                lblSer.Text = gvrow.Cells[2].Text;

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
                gvThreshhold.DataSource = actions;
                gvThreshhold.DataBind();

                this.ModalPopupExtender1.Show();
            }
        }

        protected void gvThreshhold_RowDataBound(object sender, GridViewRowEventArgs e)
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
                lb.ID = "ruleBtn";
                lb.Text = "Rule";
                lb.CommandName = "ShowPopup";
                lb.Click += new System.EventHandler(ruleBtn_Click);
                e.Row.Cells[2].Controls.Add(lb);

            }

        }

        protected void ruleBtn_Click(object sender, EventArgs e)
        {
            //this.ModalPopupExtender1.Hide();
            this.ModalPopupExtender2.Show();
        }

        protected void txtAmount_TextChanged(object sender, EventArgs e)
        {
            actions = (BindingList<AccActionEx>)this.Session["pmActions"];
            decimal Amount = Convert.ToDecimal(txtAmount.Text);
            foreach (AccActionEx item in actions)
            {
                if (item.Rule.IsPercent)
                {
                    item.threshhold_value = Amount * item.Rule.Amount / 100;
                }
            }

            this.Session["pmActions"] = actions;
            gvThreshhold.DataSource = actions;
            gvThreshhold.DataBind();

            this.ModalPopupExtender1.Show();
        }

        protected void txtLimit_TextChanged(object sender, EventArgs e)
        {
            TextBox txtLimit = sender as TextBox;
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            actions = (BindingList<AccActionEx>)this.Session["pmActions"];
            AccActionEx editRow = actions[row.RowIndex];
            editRow.threshhold_value = Convert.ToDecimal(txtLimit.Text);
            editRow.Rule = new AccountActionRule() { IsFixedAmount = true };
            this.Session["pmActions"] = actions;
            gvThreshhold.DataSource = actions;
            gvThreshhold.DataBind();

            this.ModalPopupExtender1.Show();
        }

        protected void ddlAccountAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlAccountAction = sender as DropDownList;
            GridViewRow row = (GridViewRow)((Control)sender).NamingContainer;
            actions = (BindingList<AccActionEx>)this.Session["pmActions"];
            AccActionEx editRow = actions[row.RowIndex];
            editRow.idAccountAction = Convert.ToInt32(ddlAccountAction.SelectedValue);
            this.Session["pmActions"] = actions;
            gvThreshhold.DataSource = actions;
            gvThreshhold.DataBind();

            this.ModalPopupExtender1.Show();
        }
    }

    //public class FileLogger
    //{
    //    public static void Log(string logInfo)
    //    {
    //        File.AppendAllText(@"C:\Users\Gigabyte\Desktop\Logger.txt", logInfo);
    //    }
    //}
}


