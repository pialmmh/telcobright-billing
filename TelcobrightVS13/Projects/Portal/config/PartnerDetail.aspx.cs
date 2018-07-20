using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using MediationModel;
using TelcobrightMediation;
using PortalApp;

public partial class ConfigPartnerDetail : System.Web.UI.Page
{
    private static TelcobrightConfig Tbc;
    protected void cvPrefix_Validate(object source, ServerValidateEventArgs args)
    {

        int thisPartnerType = -1;
        if (this.Session["sesPartnerType"] != null) thisPartnerType = (int) this.Session["sesPartnerType"];
        TextBox txtPrefix = (TextBox) this.FormViewPrefix.FindControl("PrefixTextBox");
        //prefix empty
        if (txtPrefix.Text == "")
        {
            this.cvPrefix.ErrorMessage = "Prefix cannot be empty!";
            args.IsValid = false;
            return;
        }
        //trim space from prefix...
        string thisPrefix = txtPrefix.Text.Trim();

        if (thisPartnerType == 1)//partner type=ans
        {
         //prefix has to be unique
            using (PartnerEntities context = new PartnerEntities())
            {
                bool prefixExists = context.partnerprefixes.Where(c=>c.PrefixType==3 && c.Prefix==thisPrefix).Any();
                if (prefixExists == true)
                {
                    this.cvPrefix.ErrorMessage = "Prefix not unique for same direction!";
                    args.IsValid = false;
                    return;
                }
            }
        }
        else if (thisPartnerType == 2)//partner type=ICX
        {
           
            //
        }
        else if (thisPartnerType == 3)//partner type=foreign partner
        {
            //prefix has to be unique for each direction
            DropDownList thisList = this.FormViewPrefix.FindControl("DropDownListPType") as DropDownList;
            int thisPrefixType = int.Parse( thisList.SelectedValue);
            using (PartnerEntities context = new PartnerEntities())
            {
                bool prefixExists = context.partnerprefixes.Where(c => c.PrefixType == thisPrefixType && c.Prefix == thisPrefix).Any();
                if (prefixExists == true)
                {
                    this.cvPrefix.ErrorMessage = "Prefix not unique for same direction!";
                    args.IsValid = false;
                    return;
                }
            }
            
            thisList = this.FormViewPrefix.FindControl("DropDownListCommonTG") as DropDownList;
            if (thisList.SelectedIndex == 0)
            {
                this.cvPrefix.ErrorMessage = "No Common TG selected for this Prefix!";
                args.IsValid = false;
                return;
            }
        }

    }

    protected void cvAll_Validate(object source, ServerValidateEventArgs args)
    {
        //name

        TextBox txtValid = (TextBox) this.FormViewRouteAdd.FindControl("RouteNameTextBox");
        DropDownList ddlSwitch = (DropDownList) this.FormViewRouteAdd.FindControl("ddlistSwitch");
        int switchId=int.Parse(ddlSwitch.SelectedValue);

        if (txtValid.Text == "")
        {
            this.cvAll.ErrorMessage = "Field Route Name can't be empty !";
            args.IsValid = false;
            return;
        }
        else if (new[] { @",", @"/", @"`", @"@", @"#", @"$", @"%", @"^", @"*", @";", @"\", "\"" }.Any(txtValid.Text.Contains))
        {
            this.cvAll.ErrorMessage = "Field Route Name cannot contain characters" +
                string.Join(" ", new[] { @",", @"/", @"`", @"@", @"#", @"$", @"%", @"^", @"*", @";", @"\", "\"" }) + "!";
            args.IsValid = false;
            return;
        }
        else
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                int count = (from c in context.routes.AsQueryable()
                             where c.RouteName == txtValid.Text
                             && c.SwitchId == switchId
                             select c).Count();
                if (count > 0)
                {
                    this.cvAll.ErrorMessage = "Field Route Name is duplicate for Switch Name: '" + ddlSwitch.SelectedItem.Text +  "' !";
                    args.IsValid = false;
                    return;
                }
            }
        }

        TextBox txt1 = (TextBox) this.FormViewRouteAdd.FindControl("TextBox1");
        TextBox txt2 = (TextBox) this.FormViewRouteAdd.FindControl("TextBox2");
        TextBox txt3 = (TextBox) this.FormViewRouteAdd.FindControl("TextBox3");


        long ingressPort = -1;
        long egressPort = -1;
        long commonPort = -1;


        if (long.TryParse(txt1.Text, out ingressPort) == false)
        {
            ingressPort = -1;
        }
        if (long.TryParse(txt2.Text, out egressPort) == false)
        {
            egressPort = -1;
        }
        if (long.TryParse(txt3.Text, out commonPort) == false)
        {
            commonPort = -1;
        }

        if (ingressPort == -1 || egressPort == -1 || commonPort == -1)
        {
            this.cvAll.ErrorMessage = "Ingress, Egress and Bothway Ports have to be numeric!";
            args.IsValid = false;
            return;
        }

        if (commonPort > 0)
        {
            if (ingressPort > 0 || egressPort > 0)
            {
                this.cvAll.ErrorMessage = "If CommonPort is set, then both Ingress and Egress Ports have to be zero.";
                args.IsValid = false;
                return;
            }

        }

    }


    protected void Page_Load(object sender, EventArgs e)
    {

        if (!this.IsPostBack)
        {
            //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
            //from Partner

            string thisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

            MySqlConnection connection = new MySqlConnection(thisConectionString);
            string database = connection.Database.ToString();

            using (PartnerEntities context = new PartnerEntities())
            {
                telcobrightpartner thisCustomer = (from c in context.telcobrightpartners
                    where c.databasename == database
                    select c).First();
                int thisOperatorId = thisCustomer.idCustomer;
                int idOperatorType = Convert.ToInt32(thisCustomer.idOperatorType);


                this.Session["sesidOperator"] = thisOperatorId;
                this.Session["sesidOperatorType"] = idOperatorType;

            }

            // route type
            DropDownList ddlistRouteType = (DropDownList)this.FormViewRouteAdd.FindControl("ddlistRouteType");
            Tbc = PageUtil.GetTelcobrightConfig();
            foreach (KeyValuePair<string, int> item in Tbc.PortalSettings.RouteTypeEnums)
            {
                ddlistRouteType.Items.Add(new ListItem(item.Key, item.Value.ToString()));
            }

            HttpRequest q = this.Request;
            NameValueCollection n = q.QueryString;
            int v = -1;
            if (n.HasKeys())
            {
                string k = n.GetKey(0);
                if (k == "idpartner")
                {
                    v = Convert.ToInt32(n.Get(0));
                }
            }

            //v = 3;//purple
            this.Session["sesidPartner"] = v;

            using (PartnerEntities context = new PartnerEntities())
            {
                partner thisPartner = (from c in context.partners
                                       where c.idPartner==v
                                       select c).First();
                this.lblPartnerDetail.Text = thisPartner.PartnerName;
                int thisPartnerType = thisPartner.PartnerType;
                this.Session["sesPartnerType"] = thisPartnerType;
                //ans can't insert route
                if (thisPartnerType == 1)//partner type=ans
                {
                    this.LinkButton1.Visible = false;
                    this.lblRoute.Visible = false;

                    //hide common TG and PrefixDirection  in gridview
                    this.GridViewPrefix.Columns[5].Visible = false;
                    this.GridViewPrefix.Columns[6].Visible = false;
                    this.lblPrefix.Visible = true;

                    DropDownList thisList = (DropDownList) this.FormViewPrefix.FindControl("DropDownListCommonTG");
                    thisList.Visible = false;
                    
                    DropDownList thisList2 = (DropDownList) this.FormViewPrefix.FindControl("DropDownListPType");
                    thisList2.Visible = false;

                    Label thisLabel = (Label) this.FormViewPrefix.FindControl("lblCommonTG");
                    thisLabel.Visible = false;

                    thisLabel = (Label) this.FormViewPrefix.FindControl("lblPrefixDirection");
                    thisLabel.Visible = false;

                }
                else if (thisPartnerType == 2)//partner type=ICX
                {
                    this.LinkButtonNewPrefix.Visible = false;
                    this.FormViewPrefix.Visible = false;
                    this.lblPrefix.Visible = false;  
                }
                else if (thisPartnerType == 3)//partner type=foreign partner
                {
                    this.LinkButton1.Visible = true;
                    this.lblRoute.Visible = true;
                    //show common TG and PrefixDirection in gridview
                    this.GridViewPrefix.Columns[5].Visible = true;
                    this.GridViewPrefix.Columns[6].Visible = true;

                    this.lblPrefix.Visible = true;

                    DropDownList thisList = (DropDownList) this.FormViewPrefix.FindControl("DropDownListPType");
                    thisList.Visible = true;
                }

            }

        }
    }


    protected void EntityDataPartner_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        int sesidPartner = -1;
        if (this.Session["sesidPartner"] != null)
        {
            sesidPartner = (Int32) this.Session["sesidPartner"];
        }

        var allPartners = e.Query.Cast<partner>();
        e.Query = from c in allPartners
                  where c.idPartner == sesidPartner
                  select c;
    }

    protected void EntityDataSource1_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        int sesidPartner = -1;
        if (this.Session["sesidPartner"] != null)
        {
            sesidPartner = (Int32) this.Session["sesidPartner"];
        }

        var allRoutes = e.Query.Cast<route>();
        e.Query = from c in allRoutes
                  where c.idPartner == sesidPartner
                  select c;
    }

    protected void EntityDataCustomerSwitch_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        int sesidOperator = -1;
        if (this.Session["sesidOperator"] != null)
        {
            sesidOperator = (Int32) this.Session["sesidOperator"];
        }

        var allSwitches = e.Query.Cast<ne>();
        e.Query = from c in allSwitches
                  where c.idCustomer == sesidOperator
                  select c;
    }

    protected void LinkButton1_Click(object sender, EventArgs e)
    {
        this.FormViewRouteAdd.Visible = true;
    }

    protected void FormViewCancel_Click(object sender, EventArgs e)
    {
        this.FormViewRouteAdd.Visible = false;
    }

    protected void FormViewRouteAdd_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {
        this.FormViewRouteAdd.Visible = false;
        this.GridView1.DataBind();
    }

    protected void FormViewRouteAdd_ItemInserting(object sender, FormViewInsertEventArgs e)
    {

    }

    protected void FormViewRouteAdd_ItemCreated(object sender, EventArgs e)
    {
        TextBox txtidPartner = (TextBox) this.FormViewRouteAdd.FindControl("idPartnerTextBox");
        int sesidPartner = -1;
        if (this.Session["sesidPartner"] != null)
        {
            sesidPartner = (Int32) this.Session["sesidPartner"];
        }
        
        txtidPartner.Text = sesidPartner.ToString();

        TextBox txtNatOrInt = (TextBox) this.FormViewRouteAdd.FindControl("NationalOrInternationalTextBox");
        int sesidOperatorType = -1;
        if (this.Session["sesidOperatorType"] != null)
        {
            sesidOperatorType = (Int32) this.Session["sesidOperatorType"];
        }

        if (sesidOperatorType == 4)//igw
        {
            //1=international
            //2=national
            //3=N/A
            if (txtNatOrInt!=null)
            {
                txtNatOrInt.Text = "1";
            }
            

        }

        //set sip as default
        ((DropDownList) this.FormViewRouteAdd.FindControl("ddlistRouteProtocol")).SelectedIndex = 1;

        //set ports to 0,0,1
        ((TextBox) this.FormViewRouteAdd.FindControl("TextBox1")).Text = "0";
        ((TextBox) this.FormViewRouteAdd.FindControl("TextBox2")).Text = "0";
        ((TextBox) this.FormViewRouteAdd.FindControl("TextBox3")).Text = "1";
    }

    protected void DropDownListPrefixType_SelectionChanged(object sender, EventArgs e)
    {
        
 
    }


    protected void LinkButtonNewPrefix_Click(object sender, EventArgs e)
    {
        this.FormViewPrefix.Visible = true;
    }

    protected void LinkButtonFrmPrefixCancel_Click(object sender, EventArgs e)
    {
        //set PrefixTextBox=""
        TextBox thistext = (TextBox) this.FormViewPrefix.FindControl("PrefixTextBox");
        thistext.Text = "";
        this.FormViewPrefix.Visible = false;
    }

    protected void LinkButtonFrmPrefixInsert_Click(object sender, EventArgs e)
    {
        
    }

    protected void FormViewPrefix_ItemInserting(object sender, FormViewInsertEventArgs e)
    {
        partnerprefix thisPrefix = new partnerprefix();
        
        TextBox thisText = (TextBox) this.FormViewPrefix.FindControl("idPartnerTextBox");
        thisPrefix.idPartner = int.Parse( thisText.Text);

        thisText = (TextBox) this.FormViewPrefix.FindControl("PrefixTextBox");
        thisPrefix.Prefix = thisText.Text;

        DropDownList thisList = (DropDownList) this.FormViewPrefix.FindControl("DropDownListCommonTG");
        thisPrefix.CommonTG = int.Parse( thisList.SelectedValue);

        thisList = (DropDownList) this.FormViewPrefix.FindControl("DropDownListPType");
        thisPrefix.PrefixType = int.Parse(thisList.SelectedValue);

        using (PartnerEntities context = new PartnerEntities())
        {
            context.partnerprefixes.Add(thisPrefix);
            context.SaveChanges();
        }

        this.FormViewPrefix.Visible = false;
        //set PrefixTextBox=""
        TextBox thistext = (TextBox) this.FormViewPrefix.FindControl("PrefixTextBox");
        thistext.Text = "";
        this.GridViewPrefix.DataBind();

    }

    protected void FormViewPrefix_ItemCreated(object sender, EventArgs e)
    {

        int idPartner = -1;
        int partnerType = -1;
        if (this.Session["sesidPartner"] != null)
        {
            idPartner = (int) this.Session["sesidPartner"];
            TextBox thistext = (TextBox) this.FormViewPrefix.FindControl("idPartnerTextBox");
            thistext.Text = idPartner.ToString();
        }
        else
        {
            throw new Exception("Invalid idPartner!");
        }
        if (this.Session["sesPartnerType"] != null)
        {
            //prefixtypes 1=customer,2=supplier, 3=ansprefix
            partnerType = (int) this.Session["sesPartnerType"];
            if (partnerType == 1)//ans
            {
                TextBox thistext = (TextBox) this.FormViewPrefix.FindControl("PrefixTypeTextBox");
                thistext.Text = "3";
            }
        }
        else
        {
            throw new Exception("Invalid PartnerType!");
        }


    }

    protected void FormViewPrefix_ItemInserted(object sender, EventArgs e)
    {
        this.FormViewPrefix.Visible = false;
        //set PrefixTextBox=""
        TextBox thistext = (TextBox) this.FormViewPrefix.FindControl("PrefixTextBox");
        thistext.Text = "";
        this.GridViewPrefix.DataBind();
        
    }

    protected void EntityDataSourcePrefix_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        int idPartner = -1;
        if (this.Session["sesidPartner"] != null)
        {
            idPartner = (int) this.Session["sesidPartner"];
        }
        if (idPartner == -1)
        {
            throw new Exception("idPartner not found!");
        }
        var allPrefix = e.Query.Cast<partnerprefix>();
        e.Query = from c in allPrefix
                  where c.idPartner == idPartner
                  select c;

    }
    protected void FormViewPrefix_ModeChanging(object sender, FormViewModeEventArgs e)
    {

    }

    protected void GridViewPrefix_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        
    }
    protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        GridViewRow r = this.GridView1.Rows[e.RowIndex];

        long ingressPort = -1;
        long egressPort = -1;
        long commonPort = -1;


        if (long.TryParse(((TextBox)r.FindControl("TextBoxIngressPort")).Text, out ingressPort) == false)
        {
            ingressPort = -1;
        }
        if (long.TryParse(((TextBox)r.FindControl("TextBoxEgressPort")).Text, out egressPort) == false)
        {
            egressPort = -1;
        }
        if (long.TryParse(((TextBox)r.FindControl("TextBoxCommonPort")).Text, out commonPort) == false)
        {
            commonPort = -1;
        }

        if (ingressPort ==-1 || egressPort ==-1|| commonPort==-1)
        {
            this.LabelUpdateValidate.Text = "Ingress, Egress and Bothway Ports have to be numeric!";
            e.Cancel = true;
            this.LabelUpdateValidate.Visible = true;
            return;
        }

        if (commonPort > 0)
        {
            if (ingressPort > 0 || egressPort > 0)
            {
                this.LabelUpdateValidate.Text = "If CommonPort is set, then both Ingress and Egress Ports have to be zero.";
                e.Cancel = true;
                this.LabelUpdateValidate.Visible = true;
            }     
                
        }
    }
    protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
    {
        this.GridView1.EditIndex = e.NewEditIndex;
        this.GridView1.DataBind();
    }
    protected void GridView1_RowUpdated(object sender, GridViewUpdatedEventArgs e)
    {
        this.LabelUpdateValidate.Visible = false;
    }
    protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        this.LabelUpdateValidate.Visible = false;
    }

    protected void GridView1_OnRowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DropDownList ddlistRouteType = (DropDownList) e.Row.FindControl("ddlistRouteType");
            foreach (KeyValuePair<string, int> item in Tbc.PortalSettings.RouteTypeEnums)
            {
                ddlistRouteType.Items.Add(new ListItem(item.Key, item.Value.ToString()));
            }

            string routeType = (e.Row.FindControl("lblRouteType") as Label).Text;
            ddlistRouteType.Items.FindByValue(routeType).Selected = true;
        }
    }

    protected void ddlistRouteType_OnSelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddlCurrentDropDownList = (DropDownList)sender;
        GridViewRow grdrDropDownRow = ((GridViewRow)ddlCurrentDropDownList.Parent.Parent);
        Label lblCurrentStatus = (Label)grdrDropDownRow.FindControl("lblRouteType");
        if (lblCurrentStatus != null)
            lblCurrentStatus.Text = ddlCurrentDropDownList.SelectedItem.Value;
    }
}