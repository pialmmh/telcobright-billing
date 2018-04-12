using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using PortalApp._myCodes;


namespace PortalApp.config
{
    public partial class PaymentManagement : System.Web.UI.Page
    {
      
        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton lb = new LinkButton();
                lb.ID = "paymentBtn";
                lb.Text = "Add Payment";
                lb.CommandName = "ShowPopup";
                lb.Click += new System.EventHandler(paymentBtn_Click);

                if (e.Row.Cells[4].Text.Equals("Postpaid"))
                {
                    lb.Enabled = false;
                   
                }
                else
                {

                    lb.Enabled = true;


                }
                e.Row.Cells[10].Controls.Add(lb);
                LinkButton lb1 = new LinkButton();
                lb1.ID = "historyBtn";
                lb1.Text = "History";
                lb1.OnClientClick = "window.open('PaymentHistory.aspx?id=" + Int32.Parse( e.Row.Cells[0].Text) + "'); return false;";
                e.Row.Cells[11].Controls.Add(lb1);

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
            ServiceAcountStatus s = new ServiceAcountStatus();
            
            GridView.DataSource =s.popultateGrid();
            GridView.DataBind();

        }
       

        protected void paymentBtn_Click(object sender, EventArgs e)
        {

            LinkButton btndetails = sender as LinkButton;
            GridViewRow gvrow = (GridViewRow)btndetails.NamingContainer;
            lblID.Text = GridView.DataKeys[gvrow.RowIndex].Value.ToString();
            lblusername.Text = gvrow.Cells[1].Text;
            lblSer.Text = gvrow.Cells[2].Text;
            this.ModalPopupExtender1.Show();

        }


    }


}


