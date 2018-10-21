using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Wordprocessing;
using PortalApp;
using MediationModel;
using Spring.Expressions;
using TelcobrightMediation;

public partial class ConfigPartner : System.Web.UI.Page
{
    private static TelcobrightConfig Tbc { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Tbc = PageUtil.GetTelcobrightConfig();
            //Retrieve Path from TreeView for displaying in the master page caption label

            TreeView masterTree = (TreeView)Master.FindControl("TreeView1");
            CommonCode commonCodes = new CommonCode();
            commonCodes.LoadReportTemplatesTree(ref masterTree);

            string localPath = Request.Url.LocalPath;
            int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
            string rootFolder = localPath.Substring(1, pos2NdSlash);
            int endOfRootFolder = Request.Url.AbsoluteUri.IndexOf(rootFolder);
            string urlWithQueryString = ("~" + "/" + rootFolder + Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
            //for some reason url was not including .aspx
            if (urlWithQueryString.EndsWith(".aspx==") == false)
            {
                urlWithQueryString += ".aspx";
            }
            TreeNodeCollection cNodes = masterTree.Nodes;
            TreeNode matchedNode = null;
            foreach (TreeNode n in cNodes)//for each nodes at root level, loop through children
            {
                matchedNode = commonCodes.RetrieveNodes(n, urlWithQueryString);
                if (matchedNode != null)
                {
                    break;
                }
            }
            //set screentile/caption in the master page...
            Label lblScreenTitle = (Label)Master.FindControl("lblScreenTitle");
            if (matchedNode != null)
            {
                lblScreenTitle.Text = matchedNode.ValuePath;
            }
            else
            {
                lblScreenTitle.Text = "";
            }


            //End of Site Map Part *******************************************************************
            
            
            using (PartnerEntities context = new PartnerEntities())
            {
                IQueryable<partner> partners = from c in context.partners
                                               select c;
            }

            using (PartnerEntities context = new PartnerEntities())
            {
                IList<enumpartnertype> partnerTypes = (from c in context.enumpartnertypes
                                                       select c).ToList();
                Session["sesPartnerTypes"] = partnerTypes;


            }

        }
    }

    protected void DropDownPartnerType_SelectedIndexChanged(object sender, EventArgs e)
    {
        
    }

    protected void GridViewPartner_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        
        if (e.Row.RowType == DataControlRowType.DataRow)
        {

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonPartnerDetail");
                lnkBtn.OnClientClick = "window.open('PartnerDetail.aspx?idpartner=" + DataBinder.Eval(e.Row.DataItem, "idpartner").ToString() + "')";

                //set command argument for delete button tobe retrieved in rowdatabound event...
                lnkBtn = (LinkButton)e.Row.FindControl("LinkButton2");
                //string StartDate = DataBinder.Eval(e.Row.DataItem, "startdate").ToString();

                lnkBtn.CommandArgument = e.Row.RowIndex.ToString();
            }

            int idPartner = Convert.ToInt32(GridViewPartner.DataKeys[e.Row.RowIndex].Value);
            GridView gvAccount = e.Row.FindControl("gvAccount") as GridView;
            using (PartnerEntities context = new PartnerEntities())
            {
                List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                List<account> accounts = context.accounts.Where(p => p.idPartner == idPartner /*&& p.isBillable == 1 && p.isCustomerAccount == 1*/).ToList();
                foreach (account account in accounts)
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

                    // update balance from acc_temp_transaction
                    account.balanceAfter += context.acc_temp_transaction.Where(x => x.glAccountId == account.id)
                        .Select(x => x.amount)
                        .DefaultIfEmpty(0)
                        .Sum();
                }
                gvAccount.DataSource = accounts;
                gvAccount.DataBind();
            }


            //display partner type instead of enum value
            Label lblCType = (Label)e.Row.FindControl("lblPartnerType");
            if (lblCType != null)
            {
                int type = Convert.ToInt32(lblCType.Text);

                IList<enumpartnertype> sesPartnerType = null;
                if (Session["sesPartnerTypes"] != null)
                {
                    sesPartnerType = (IList<enumpartnertype>)Session["sesPartnerTypes"];
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

        if (TextBoxFind.Text == "")
        {
            if (ddlistPartnerType.SelectedIndex == 0) //show all
            {

                var allPartners = e.Query.Cast<partner>();
                e.Query = from c in allPartners
                          where c.PartnerType > -1
                          select c;
            }
            else
            {
                var allPartners = e.Query.Cast<partner>();
                Int32 x = Convert.ToInt32(ddlistPartnerType.SelectedValue);
                e.Query = from c in allPartners
                          where c.PartnerType == x
                          select c;
            }
        }
        else//find by name
        {
            if (ddlistPartnerType.SelectedIndex == 0) //show all
            {

                var allPartners = e.Query.Cast<partner>();
                e.Query = from c in allPartners
                          where c.PartnerType > -1
                          && c.PartnerName.ToLower().Contains(TextBoxFind.Text.ToLower())
                          select c;
            }
            else
            {
                var allPartners = e.Query.Cast<partner>();
                Int32 x = Convert.ToInt32(ddlistPartnerType.SelectedValue);
                e.Query = from c in allPartners
                          where c.PartnerType == x
                          && c.PartnerName.ToLower().Contains(TextBoxFind.Text.ToLower())
                          select c;
            }
        }

    }

    protected void GridViewPartner_RowCommand(object sender, GridViewCommandEventArgs e)
    {

        if (e.CommandName == "Delete")
        {   
            string idPartner = ((Label)GridViewPartner.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("lblIdPartner")).Text;
            lblIdPartnerGlobal.Text = idPartner;
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
        frmPartnerInsert.Visible = true;
    }

    

    protected void FormViewCancel_Click(object sender, EventArgs e)
    {
        frmPartnerInsert.Visible = false;
    }

    protected void frmPartnerInsert_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {
        //frmPartnerInsert.Visible = false;
        //GridViewPartner.DataBind();
    }
    protected void frmPartnerInsert_ItemCreated(object sender, EventArgs e)
    {
        //TextBox txtBillingDate = (TextBox)frmPartnerInsert.FindControl("BillingDateTextBox");
        //txtBillingDate.Text = "1";
    }


    protected void cvAll_Validate(object source, ServerValidateEventArgs args)
    {
        //verify id, was required for Banglatrac version...
        TextBox txtValid = (TextBox)frmPartnerInsert.FindControl("idTextBox");
        int idPartner = -1;
        //int.TryParse(txtValid.Text, out idPartner);
        //idpartner 0 not allowed
        //if (idPartner==0)
        //{
          //  cvAll.ErrorMessage = "idPartner can't be 0 !" ;
            //args.IsValid = false;
            //return;
        //}

        using (PartnerEntities context = new PartnerEntities())
        {
            bool exists = (from c in context.partners.AsQueryable()
                           where c.idPartner == idPartner
                           select c).Any();
            if (exists == true)
            {
                cvAll.ErrorMessage = "Duplicate id Partner:" + txtValid.Text + " !";
                args.IsValid = false;
                return;
            }
        }
        
        txtValid = (TextBox)frmPartnerInsert.FindControl("PartnerNameTextBox");
        DataRowView rowView = (DataRowView)frmPartnerInsert.DataItem;
        //int idPartner=rowView["idPartner"

        if (txtValid.Text == "")
        {
            cvAll.ErrorMessage = "Field Partner Name can't be empty !";
            args.IsValid = false;
            return;
        }
        else if (new[] { @",", @"/", @"`", @"@", @"#", @"$", @"%", @"^", @"*", @";", @"\","\"" }.Any(txtValid.Text.Contains))
        {
            cvAll.ErrorMessage = "Field Partner Name cannot contain characters" +
                string.Join(" ", new[] { @",", @"/", @"`", @"@", @"#", @"$", @"%", @"^", @"*", @";", @"\", "\"" }) + "!";
            args.IsValid = false;
            return;
        }
        else
        {
            using (PartnerEntities context=new PartnerEntities())
            {
                bool exists = (from c in context.partners.AsQueryable()
                             where c.PartnerName == txtValid.Text
                             select c).Any();
                if (exists==true)
                {
                    cvAll.ErrorMessage = "Duplicate Partner Name:" + txtValid.Text + " !";
                    args.IsValid = false;
                    return;
                }       
            }
        }
        //email
        //txtValid = (TextBox)frmPartnerInsert.FindControl("EmailTextBox");
        //if (txtValid.Text == "")
        //{
        //    cvAll.ErrorMessage = "Email can't be empty !";
        //    args.IsValid = false;
        //    return;
        //}
        //BillingDateTextBox
        //txtValid = (TextBox)frmPartnerInsert.FindControl("BillingDateTextBox");
        //if (txtValid.Text == "")
        //{
        //    cvAll.ErrorMessage = "Billing Date can't be empty !";
        //    args.IsValid = false;
        //    return;
        //}
        //else
        //{
        //    double n;
        //    bool isNumeric = double.TryParse(txtValid.Text, out n);
        //    if (isNumeric == false)
        //    {
        //        cvAll.ErrorMessage = "Billing Date is not numeric !";
        //        args.IsValid = false;
        //        return;
        //    }
        //}
        
        
    }

    protected void GridViewPartner_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int idPartner = int.Parse(lblIdPartnerGlobal.Text);
        
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
    protected void frmPartnerInsert_ItemInserting(object sender, FormViewInsertEventArgs e)
    {

        TextBox thisTextBoxId = (TextBox)frmPartnerInsert.FindControl("idTextBox");
        TextBox thisTextBoxName = (TextBox)frmPartnerInsert.FindControl("PartnerNameTextBox");
        TextBox thisTextBoxTel = (TextBox)frmPartnerInsert.FindControl("TelephoneTextBox");
        TextBox thisTextBoxEmail = (TextBox)frmPartnerInsert.FindControl("EmailTextBox");
        TextBox thisTextBoxAdd1 = (TextBox)frmPartnerInsert.FindControl("Address1TextBox");
        TextBox thisTextBoxAdd2 = (TextBox)frmPartnerInsert.FindControl("Address2TextBox");
        TextBox thisTextBoxCity = (TextBox)frmPartnerInsert.FindControl("CityTextBox");
        TextBox thisTextBoxState = (TextBox)frmPartnerInsert.FindControl("StateTextBox");
        TextBox thisTextBoxPost = (TextBox)frmPartnerInsert.FindControl("PostalCodeTextBox");
        TextBox thisTextBoxCountry = (TextBox)frmPartnerInsert.FindControl("CountryTextBox");

        TextBox thisAlternateNameInvoiceTextBox = (TextBox)frmPartnerInsert.FindControl("AlternateNameInvoiceTextBox");
        TextBox thisAlternateNameOtherTextBox = (TextBox)frmPartnerInsert.FindControl("AlternateNameOtherTextBox");
        TextBox thisvatRegistrationNoTextBox = (TextBox)frmPartnerInsert.FindControl("vatRegistrationNoTextBox");
        TextBox thisInvoiceAddressTextBox = (TextBox)frmPartnerInsert.FindControl("InvoiceAddressTextBox");


        DropDownList ddlPre = (DropDownList)frmPartnerInsert.FindControl("ddlistPrePostAdd");
        DropDownList ddlType = (DropDownList)frmPartnerInsert.FindControl("ddlistCustomerTypeAdd");

        string sql =
        "insert into partner (" +
        "PartnerName" +
        ",Telephone" +
        ",email" +
        ",CustomerPrePaid" +
        ",PartnerType" +
        ",Address1" +
        ",Address2" +
        ",City" +
        ",State" +
        ",PostalCode" +
        ", Country" +
        ", AlternateNameInvoice" +
        ", AlternateNameOther" +
        ", invoiceAddress" +
        ", vatRegistrationNo)" +
        " values( " +
        " '" + thisTextBoxName.Text + "'" +
        " ,'" + thisTextBoxTel.Text + "'" +
        " ,'" + thisTextBoxEmail.Text + "'" +
        " ," + int.Parse(ddlPre.SelectedValue) +
        " ," + int.Parse(ddlType.SelectedValue) +
        " ,'" + thisTextBoxAdd1.Text + "'" +
        " ,'" + thisTextBoxAdd2.Text + "'" +
        " ,'" + thisTextBoxCity.Text + "'" +
        " ,'" + thisTextBoxState.Text + "'" +
        " ,'" + thisTextBoxPost.Text + "'" +
        " ,'" + thisTextBoxCountry.Text + "'" +
        " ,'" + thisAlternateNameInvoiceTextBox.Text + "'" +
        " ,'" + thisAlternateNameOtherTextBox.Text + "'" +
        " ,'" + thisInvoiceAddressTextBox.Text + "'" +
        " ,'" + thisvatRegistrationNoTextBox.Text + "')";

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

            connection.Open();

            using (MySqlCommand cmd = new MySqlCommand(sql,connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql.Replace("''","null");
                cmd.ExecuteNonQuery();
            }
        }

        frmPartnerInsert.Visible = false;
        GridViewPartner.DataBind();

        thisTextBoxName.Text = "";
        thisTextBoxTel.Text = "";
        thisTextBoxEmail.Text = "";
        thisTextBoxAdd1.Text = "";
        thisTextBoxAdd2.Text = "";
        thisTextBoxCity.Text = "";
        thisTextBoxState.Text = "";
        thisTextBoxPost.Text = "";
        thisTextBoxCountry.Text = "";
        thisAlternateNameInvoiceTextBox.Text = "";
        thisAlternateNameOtherTextBox.Text = "";
        thisInvoiceAddressTextBox.Text = "";
        thisvatRegistrationNoTextBox.Text = "";


    }
    protected void frmPartnerInsert_ModeChanging(object sender, FormViewModeEventArgs e)
    {

    }
    protected void Button1_Click(object sender, EventArgs e)
    {
        GridViewPartner.DataBind();
    }
}