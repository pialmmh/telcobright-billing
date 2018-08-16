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


namespace PortalApp.config
{
    public partial class PaymentManagement : System.Web.UI.Page
    {

        private static TelcobrightConfig Tbc { get; set; }

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
                this.ModalPopupExtender1.Show();
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


