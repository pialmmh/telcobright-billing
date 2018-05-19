using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;
using PortalApp._myCodes;
using TelcobrightMediation;


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

                //if (e.Row.Cells[4].Text.Equals("Postpaid"))
                //{
                //    lb.Enabled = false;
                   
                //}
                //else
                //{

                //    lb.Enabled = true;


                //}
                e.Row.Cells[4].Controls.Add(lb);
                //LinkButton lb1 = new LinkButton();
                //lb1.ID = "historyBtn";
                //lb1.Text = "History";
                //lb1.OnClientClick = "window.open('PaymentHistory.aspx?id=" + Int32.Parse( e.Row.Cells[0].Text) + "'); return false;";
                //e.Row.Cells[11].Controls.Add(lb1);

            }



        }

    
        
        protected void btnOK_Click(object sender, EventArgs e)
        {
            int partnerId = Int32.Parse(lblID.Text);
            Double amount =Double.Parse( txtAmount.Text);
            string payDate = txtDate.Text;
            string type = ddlistType.SelectedValue;
            string reference = paymentReference.Text;
            string comments = comment.Text;
            List<ServiceAcountStatus> lstPayments = (List<ServiceAcountStatus>)this.GridView.DataSource;

            var partner = lstPayments.Find(p => p.PartnerID == partnerId);
            partner.LastCreditedAmount += amount;
            partner.LastAmountType = type;
            GridView.DataSource = lstPayments;
            GridView.DataBind();
            List<TopUpInfo> lstTopUpInfo = new List<TopUpInfo>();
            lstTopUpInfo.Add(new TopUpInfo() {PartnerID=partnerId,Date=payDate,Type=type ,Amount=amount ,Currency=partner.Currency,PaymentReference=reference,Comment=comments} );

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
            //ServiceAcountStatus s = new ServiceAcountStatus();

            //GridView.DataSource =s.popultateGrid();
            //GridView.DataBind();

        }


        protected void paymentBtn_Click(object sender, EventArgs e)
        {

            LinkButton btndetails = sender as LinkButton;
            GridViewRow gvrow = (GridViewRow)btndetails.NamingContainer;
            lblID.Text = GridView.DataKeys[gvrow.RowIndex].Value.ToString();
//            lblusername.Text = ((List<partner>)Session["allPartners"]).First(x => x.idPartner == Convert.ToInt32(gvrow.Cells[1].Text)).PartnerName;
            lblSer.Text = gvrow.Cells[2].Text;
            this.ModalPopupExtender1.Show();

        }

    }


}


