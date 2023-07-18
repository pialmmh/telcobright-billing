using System;
using System.Collections.Generic;
//using IgwModel;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using PortalApp;
using MediationModel;

public partial class ConfigKpians : System.Web.UI.Page
{
    
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            //common code for report pages
            string localPath = this.Request.Url.LocalPath;
            int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
            string pagePathInTree = "~" + localPath.Substring(pos2NdSlash + 1, localPath.Length - pos2NdSlash - 1);
            TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
            TreeNodeCollection cNodes = masterTree.Nodes;

            TreeNode matchedNode = null;

            CommonCode commonCodes = new CommonCode();
            foreach (TreeNode n in cNodes)
            {
                matchedNode = commonCodes.RetrieveNodes(n, pagePathInTree);
                if (matchedNode != null)
                {
                    break;
                }
            }

            Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
            if (matchedNode != null)
            {
                lblScreenTitle.Text = matchedNode.ValuePath;
            }
            else
            {
                lblScreenTitle.Text = "";
            }
            //End of Common and Site Map Part *******************************************************************
            
            
            using (PartnerEntities context = new PartnerEntities())
            {
                IQueryable<partner> partners = from c in context.partners
                                               select c;
            }

            using (PartnerEntities context = new PartnerEntities())
            {
                IList<enumpartnertype> partnerTypes = (from c in context.enumpartnertypes
                                                       select c).ToList();
                this.Session["sesPartnerTypes"] = partnerTypes;


            }

        }
    }

    protected void DropDownPartnerType_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.GridViewPartner.DataBind();
    }

    protected void GridViewPartner_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        if (e.Row.RowType == DataControlRowType.DataRow)
        {

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //LinkButton LnkBtn = (LinkButton)e.Row.FindControl("LinkButtonPartnerDetail");
                //LnkBtn.OnClientClick = "window.open('PartnerDetail.aspx?idpartner=" + DataBinder.Eval(e.Row.DataItem, "idpartner").ToString() + "')";

                //set command argument for delete button tobe retrieved in rowdatabound event...
                //LnkBtn = (LinkButton)e.Row.FindControl("LinkButton2");
                //string StartDate = DataBinder.Eval(e.Row.DataItem, "startdate").ToString();

                //LnkBtn.CommandArgument = e.Row.RowIndex.ToString();
            }

            //display partner type instead of enum value
            Label lblCType = (Label)e.Row.FindControl("lblPartnerType");
            if (lblCType != null)
            {
                int type = Convert.ToInt32(lblCType.Text);

                IList<enumpartnertype> sesPartnerType = null;
                if (this.Session["sesPartnerTypes"] != null)
                {
                    sesPartnerType = (IList<enumpartnertype>) this.Session["sesPartnerTypes"];
                }

                enumpartnertype strCType = (from c in sesPartnerType
                                            where c.id == type
                                            select c).First();

                lblCType.Text = strCType.Type;



            }
        }



    }

    protected void EntityDataPartner_QueryCreated(object sender, QueryCreatedEventArgs e)
    {

        
            var allPartners = e.Query.Cast<partner>();
            Int32 x = 1;// Convert.ToInt32(ddlistPartnerType.SelectedValue); ANS only
            e.Query = from c in allPartners
                      where c.PartnerType == x
                      select c;
            //GridViewPartner.DataBind();

    }

    protected void GridViewPartner_RowCommand(object sender, GridViewCommandEventArgs e)
    {

        if (e.CommandName == "Delete")
        {   
            string idPartner = ((Label) this.GridViewPartner.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("lblIdPartner")).Text;
            this.lblIdPartnerGlobal.Text = idPartner;
        }


    }

    protected void GridViewPartner_RowEditing(object sender, GridViewEditEventArgs e)
    {


    }

    protected void GridViewPartner_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        //int index = e.RowIndex;
        //GridViewPartner.EditIndex = index;
        //DropDownList ddrList = GridViewPartner.Rows[index].FindControl("DropDownList2") as DropDownList;
        //Label LblTmp = GridViewPartner.Rows[index].FindControl("Label1") as Label;
        //LblTmp.Text = ddrList.SelectedValue;
        //DataRowView rowView = (DataRowView)e.Row.DataItem;
        //DataRowView rowView = (DataRowView)GridViewPartner.Rows[index].DataItem;
        //rowView.Row[12] = LblTmp.Text;
    }

    protected void LinkButton1_Click(object sender, EventArgs e)
    {
        this.frmPartnerInsert.Visible = true;
    }

    

    protected void FormViewCancel_Click(object sender, EventArgs e)
    {
        this.frmPartnerInsert.Visible = false;
    }

    protected void frmPartnerInsert_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {
        this.frmPartnerInsert.Visible = false;
        this.GridViewPartner.DataBind();
    }
    protected void frmPartnerInsert_ItemCreated(object sender, EventArgs e)
    {
        TextBox txtBillingDate = (TextBox) this.frmPartnerInsert.FindControl("BillingDateTextBox");
        txtBillingDate.Text = "1";
    }


    protected void cvAll_Validate(object source, ServerValidateEventArgs args)
    {
        //name
        
        TextBox txtValid = (TextBox) this.frmPartnerInsert.FindControl("PartnerNameTextBox");
        DataRowView rowView = (DataRowView) this.frmPartnerInsert.DataItem;
        //int idPartner=rowView["idPartner"

        if (txtValid.Text == "")
        {
            this.cvAll.ErrorMessage = "Field Partner Name can't be empty !";
            args.IsValid = false;
            return;
        }
        else
        {
            using (PartnerEntities context=new PartnerEntities())
            {
                int count = (from c in context.partners.AsQueryable()
                             where c.PartnerName == txtValid.Text
                             select c).Count();
                if (count>0)
                {
                    this.cvAll.ErrorMessage = "Duplicate Partner Name:" + txtValid.Text + " !";
                    args.IsValid = false;
                    return;
                }       
            }
        }
        //email
        txtValid = (TextBox) this.frmPartnerInsert.FindControl("EmailTextBox");
        if (txtValid.Text == "")
        {
            this.cvAll.ErrorMessage = "Email can't be empty !";
            args.IsValid = false;
            return;
        }
        //BillingDateTextBox
        txtValid = (TextBox) this.frmPartnerInsert.FindControl("BillingDateTextBox");
        if (txtValid.Text == "")
        {
            this.cvAll.ErrorMessage = "Billing Date can't be empty !";
            args.IsValid = false;
            return;
        }
        else
        {
            double n;
            bool isNumeric = double.TryParse(txtValid.Text, out n);
            if (isNumeric == false)
            {
                this.cvAll.ErrorMessage = "Billing Date is not numeric !";
                args.IsValid = false;
                return;
            }
        }
        
        
    }

    protected void GridViewPartner_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int idPartner = int.Parse(this.lblIdPartnerGlobal.Text);
        
        if (idPartner > 0)
        {
            //check for route table for child record
            using (PartnerEntities context = new PartnerEntities())
            {
                int routeExists=0;
                try
                {
                    route r = (from c in context.routes.AsQueryable()
                               where c.idPartner == idPartner
                               select c).First();
                    routeExists = 1;
                }
                catch(Exception e1)
                {
                    if ((e1.Message.IndexOf("contains no elements")) > 0)
                    {
                        routeExists = 0;
                    }
                    else routeExists = 1;
                }
                if (routeExists==1)
                {
                    e.Cancel = true;
                    System.Web.HttpContext.Current.Response.Write(

                        "<script type=" + (char)34 + "text/JavaScript" + (char)34 + ">" +
                        "alert('Record cannot be deleted as child record exists !');" +
                        "</script>"

                        );
                }
            }
            
        }
        
    }
}