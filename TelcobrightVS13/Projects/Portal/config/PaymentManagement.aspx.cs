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
        private static List<IAutomationAction> availableActions { get; set; }

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
                e.Row.Cells[6].Controls.Add(lb);
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
                    account.balanceAfter = account.getCurrentBalanceWithTempTransaction();
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
                Response.Redirect("~/config/AddPayment.aspx?accountId=" + accountId, false);
                Context.ApplicationInstance.CompleteRequest();
            }
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


