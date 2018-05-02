using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using LibraryExtensions;
using MediationModel;
using PortalApp;
public partial class ConfigSupplierRatePlan : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            //Retrieve Path from TreeView for displaying in the master page caption label
            TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
            CommonCode commonCodes = new CommonCode();
            commonCodes.LoadReportTemplatesTree(ref masterTree);

            string localPath = this.Request.Url.LocalPath;
            int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");http://localhost:25964/config/rateplan.aspx.cs
            string rootFolder = localPath.Substring(1, pos2NdSlash);
            int endOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(rootFolder);
            string urlWithQueryString = ("~" + "/" + rootFolder +
                                         this.Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length),
                                             this.Request.Url.AbsoluteUri.Length -
                                             (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
            //for some reason url was not including .aspx
            if (urlWithQueryString.EndsWith(".aspx==") == false)
            {
                urlWithQueryString += ".aspx";
            }
            TreeNodeCollection cNodes = masterTree.Nodes;
            TreeNode matchedNode = null;
            foreach (TreeNode n in cNodes) //for each nodes at root level, loop through children
            {
                matchedNode = commonCodes.RetrieveNodes(n, urlWithQueryString);
                if (matchedNode != null)
                {
                    break;
                }
            }
            //set screentile/caption in the master page...
            Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
            if (matchedNode != null)
            {
                lblScreenTitle.Text = matchedNode.ValuePath;
            }
            else
            {
                lblScreenTitle.Text = "";
            }


            //End of Site Map Part *******************************************************************

            //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
            //from Partner

            string thisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString +
                                         "; default command timeout=3600;";

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

            //LoadMainDropDownList();

            //load partners
            using (PartnerEntities contextIgw = new PartnerEntities())
            {
                List<partner> allPartners = contextIgw.partners.OrderBy(i => i.PartnerName).ToList();
                this.Session["sesAllPartners"] = allPartners;

                List<timezone> tz = contextIgw.timezones.Include("zone.country")
                    .OrderBy(o => o.zone.country.country_name).ToList();
                this.Session["sesAllTimeZones"] = tz;

            }
            
        }
    }



    protected void DropDownSupplierRatePlanType_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    void LoadCurrencies()
    {
        //load currencies
        DropDownList ddlistCurrency =
            (DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListCurrency");
        List<uom> currencies = null;
        using (MySqlConnection con =
            new MySqlConnection(ConfigurationManager.ConnectionStrings["Reader"].ConnectionString))
        {
            con.Open();
            using (MySqlCommand cmd = new MySqlCommand("", con))
            {
                cmd.CommandText = $@"select uom_id from uom
                                     Where UOM_TYPE_ID = 'CURRENCY_MEASURE'
                                     order by uom_type_id";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    currencies = reader.ConvertToList(c =>
                        new uom()
                        {
                            UOM_ID = reader[0].ToString()
                        });
                }
            }
            ddlistCurrency.Items.Clear();
            foreach (var currency in currencies)
            {
                ddlistCurrency.Items.Add(new ListItem(currency.UOM_ID, currency.UOM_ID));
            }
            //ddlistCurrency.DataBind();
        }
    }

    protected void DropDownListType_SelectionChanged(object sender, EventArgs e)
    {
        DropDownList typeList = (DropDownList)sender;
        //DropDownList ddlist = (DropDownList)frmSupplierRatePlanInsert.FindControl("ddlistPartner");
        DropDownList ddlistTz = (DropDownList)this.frmSupplierRatePlanInsert.FindControl("ddlistTimeZone");
        DropDownList ddlistCurrency = (DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListCurrency");
        //DropDownList ddlCountryWise = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListCountryWise");

        if (typeList.SelectedValue == "3" || typeList.SelectedValue == "4")//Intl in/out
        {
            //ddlist.Enabled = false;

            //find out default native timezone for this operator
            int thisOperatorId = -1;
            int thisTimeZoneIndex = -1;
            if (this.Session["sesidOperator"] != null)
            {
                thisOperatorId = (int)this.Session["sesidOperator"];
            }
            using (PartnerEntities conMed = new PartnerEntities())
            {
                thisTimeZoneIndex = (from c in conMed.telcobrightpartners
                                     where c.idCustomer == thisOperatorId
                                     select c.NativeTimeZone).First();
            }

            ddlistTz.SelectedValue = thisTimeZoneIndex.ToString();

            ddlistTz.Enabled = false;
            ddlistCurrency.Enabled = false;
            //set fixed currency for international incoming
            switch (typeList.SelectedValue)
            {
                case "3": //international In
                case "4": //international Out
                    ddlistCurrency.SelectedValue = "USD";
                    ddlistCurrency.SelectedValue = "USD";
                    break;
            }
            //ddlCountryWise.Enabled = false;

        }
        else //customer/supplier
        {
            //ddlist.Enabled = true;
            ddlistTz.Enabled = true;
            ddlistCurrency.Enabled = true;
        }
    }




    protected void GridViewSupplierRatePlan_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if ((e.Row.RowState & DataControlRowState.Edit) > 0) return;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton lnkBtn = null;

                //set command argument for delete button tobe retrieved in rowdatabound event...
                lnkBtn = (LinkButton)e.Row.FindControl("LinkButton2");
                lnkBtn.CommandArgument = e.Row.RowIndex.ToString();

                lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonTask");
                lnkBtn.OnClientClick = "window.open('ratetask.aspx?idRatePlan=" + DataBinder.Eval(e.Row.DataItem, "id").ToString() + "'); return false;";

                lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonRate");
                lnkBtn.OnClientClick = "window.open('rates.aspx?idRatePlan=" + DataBinder.Eval(e.Row.DataItem, "id").ToString() + "'); return false;";

                //set command argument for delete button tobe retrieved in rowdatabound event...
                lnkBtn = (LinkButton)e.Row.FindControl("LinkButton2");
                lnkBtn.CommandArgument = e.Row.RowIndex.ToString();

            }




            Label thisLabel = (Label)e.Row.FindControl("lblCurrency");
            if (DataBinder.Eval(e.Row.DataItem, "Currency") != null)
            {
                string currency = DataBinder.Eval(e.Row.DataItem, "Currency").ToString();
                thisLabel.Text = currency;
            }

            //ThisLabel = (Label)e.Row.FindControl("lblType");
            //if (DataBinder.Eval(e.Row.DataItem, "Type") != null)
            //{
            //    string Type = DataBinder.Eval(e.Row.DataItem, "Type").ToString();

            //    switch (Type)
            //    {
            //        case "1":
            //            ThisLabel.Text = "Transit";
            //            break;
            //        case "3":
            //            ThisLabel.Text = "Intl. Incoming";
            //            break;
            //        case "4":
            //            ThisLabel.Text = "Intl. Outgoing";
            //            break;

            //    }
            //}



            //Session["sesAllTimeZones"]
            thisLabel = (Label)e.Row.FindControl("lblTimeZone");
            if (DataBinder.Eval(e.Row.DataItem, "TimeZone") != null)
            {
                int idTimeZone = int.Parse(DataBinder.Eval(e.Row.DataItem, "TimeZone").ToString());
                if (idTimeZone > 0)
                {
                    if (this.Session["sesAllTimeZones"] != null)
                    {
                        List<timezone> allTimeZones = (List<timezone>)this.Session["sesAllTimeZones"];
                        string tzName = (from c in allTimeZones
                                         where c.id == idTimeZone
                                         select c.zone.country.country_name + " " + c.offsetdesc + " [" + c.zone.zone_name + "]").First();
                        thisLabel.Text = tzName;
                    }
                }

            }



        }



    }

    protected void EntityDataSupplierRatePlan_Inserting(object sender, EntityDataSourceChangingEventArgs e)
    {

    }

    protected void EntityDataSupplierRatePlan_QueryCreated(object sender, QueryCreatedEventArgs e)
    {


        var allRatePlan = e.Query.Cast<rateplan>();

        if (this.ddlistSupplierRatePlanType.SelectedIndex == 0 && this.TextBoxRatePlanName.Text == "")
        {
            e.Query = from c in allRatePlan
                      select c;
            return;
        }
        else if (this.ddlistSupplierRatePlanType.SelectedIndex == 0 && this.TextBoxRatePlanName.Text != "")
        {
            int type = int.Parse(this.ddlistSupplierRatePlanType.SelectedValue);
            string searchStr = this.TextBoxRatePlanName.Text.ToLower();
            e.Query = from c in allRatePlan
                      where c.RatePlanName.ToLower().Contains(searchStr)
                      select c;
            return;
        }
        else if (this.ddlistSupplierRatePlanType.SelectedIndex > 0 && this.TextBoxRatePlanName.Text == "")
        {
            int type = int.Parse(this.ddlistSupplierRatePlanType.SelectedValue);
            e.Query = from c in allRatePlan
                      where c.Type == type
                      select c;
        }
        else if (this.ddlistSupplierRatePlanType.SelectedIndex > 0 && this.TextBoxRatePlanName.Text != "")
        {
            int type = int.Parse(this.ddlistSupplierRatePlanType.SelectedValue);
            string searchStr = this.TextBoxRatePlanName.Text.ToLower();
            e.Query = from c in allRatePlan
                      where c.Type == type
                      && c.RatePlanName.ToLower().Contains(searchStr)
                      select c;
            return;
        }



    }

    protected void GridViewSupplierRatePlan_RowCommand(object sender, GridViewCommandEventArgs e)
    {

        if (e.CommandName == "Delete")
        {
            string idSupplierRatePlan = ((Label)this.GridViewSupplierRatePlan.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("lblIdSupplierRatePlan")).Text;
            this.lblIdSupplierRatePlanGlobal.Text = idSupplierRatePlan;
        }

    }

    protected void GridViewSupplierRatePlan_RowEditing(object sender, GridViewEditEventArgs e)
    {


    }

    protected void GridViewSupplierRatePlan_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        //int index = e.RowIndex;
        //GridViewSupplierRatePlan.EditIndex = index;
        //DropDownList ddrList = GridViewSupplierRatePlan.Rows[index].FindControl("DropDownList2") as DropDownList;
        //Label LblTmp = GridViewSupplierRatePlan.Rows[index].FindControl("Label1") as Label;
        //LblTmp.Text = ddrList.SelectedValue;
        //DataRowView rowView = (DataRowView)e.Row.DataItem;
        //DataRowView rowView = (DataRowView)GridViewSupplierRatePlan.Rows[index].DataItem;
        //rowView.Row[12] = LblTmp.Text;
    }

    protected void LinkButton1_Click(object sender, EventArgs e)
    {
        this.frmSupplierRatePlanInsert.DataBind();
        //frmSupplierRatePlanInsert.DefaultMode = FormViewMode.Insert;
        this.frmSupplierRatePlanInsert.Visible = true;
    }



    protected void FormViewCancel_Click(object sender, EventArgs e)
    {
        this.frmSupplierRatePlanInsert.DataBind();
        this.frmSupplierRatePlanInsert.Visible = false;
    }

    protected void frmSupplierRatePlanInsert_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {

    }


    protected void cvAll_Validate(object source, ServerValidateEventArgs args)
    {
        //name

        TextBox txtValid = (TextBox)this.frmSupplierRatePlanInsert.FindControl("RatePlanNameTextBox");
        //DataRowView rowView = (DataRowView)frmSupplierRatePlanInsert.DataItem;
        //int idSupplierRatePlan=rowView["idSupplierRatePlan"

        if (txtValid.Text == "")
        {
            this.cvAll.ErrorMessage = "Field Rate Plan Name can't be empty !";
            args.IsValid = false;
            return;
        }
        else //check duplicate name
        {
            string thisName = txtValid.Text;
            using (PartnerEntities context = new PartnerEntities())
            {
                bool dupName = false;
                dupName = (from c in context.rateplans
                           where c.RatePlanName == thisName
                           select c).Any();
                if (dupName)
                {
                    this.cvAll.ErrorMessage = "Duplicate Rate Plan Name !";
                    args.IsValid = false;
                    return;
                }
            }

        }

        //find out rateplan type

        DropDownList ddlistPlanType = (DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListType");
        int ratePlanType = int.Parse(ddlistPlanType.SelectedValue);
        //partner
        //DropDownList ddlistPartner = (DropDownList)frmSupplierRatePlanInsert.FindControl("ddlistPartner");
        if (ddlistPlanType.SelectedIndex <= 0) //no service family is selected
        {
            this.cvAll.ErrorMessage = "No Service Family Selected !";
            args.IsValid = false;
            return;
        }

        //active date
        txtValid = (TextBox)this.frmSupplierRatePlanInsert.FindControl("ActiveDateTextBox");
        string dateString = txtValid.Text; ; // <-- Valid
        string format = "yyyy-MM-dd";
        DateTime dateTime;
        if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out dateTime) == false)
        {
            //Console.WriteLine(dateTime);
            this.cvAll.ErrorMessage = "Invalid/Empty Creation Date !";
            args.IsValid = false;
            return;
        }



    }

    protected void GridViewSupplierRatePlan_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int idSupplierRatePlan = int.Parse(this.lblIdSupplierRatePlanGlobal.Text);

        if (idSupplierRatePlan > 0)
        {
            //check for route table for child record
            using (PartnerEntities context = new PartnerEntities())
            {
                bool rateExists = false;

                rateExists = (from c in context.rates.AsQueryable()
                              where c.idrateplan == idSupplierRatePlan
                              select c).Any();

                if (rateExists == true)
                {
                    e.Cancel = true;
                    System.Web.HttpContext.Current.Response.Write(

                        "<script type=" + (char)34 + "text/JavaScript" + (char)34 + ">" +
                        "alert('Record cannot be deleted as child record exists in rate table !');" +
                        "</script>"
                        );
                    return;
                }
                var lstTaskRef = context.ratetaskreferences.Where(c => c.idRatePlan == idSupplierRatePlan).Select(c => c.id).ToList();

                int taskExists = 1;
                if (lstTaskRef.Count > 0)
                {
                    taskExists = context.Database.SqlQuery<int>(" select (select exists (select * from ratetask where idrateplan in(" +
                        string.Join(",", lstTaskRef)
                        + "))) as existence ", typeof(int)).First();
                }
                else
                {
                    taskExists = 0;
                }
                if (taskExists == 1)
                {
                    e.Cancel = true;
                    System.Web.HttpContext.Current.Response.Write(

                        "<script type=" + (char)34 + "text/JavaScript" + (char)34 + ">" +
                        "alert('Record cannot be deleted as child record exists in ratetask table!');" +
                        "</script>"
                        );
                    return;
                }

                bool rateplanAssignmentExists = context.rateassigns.Any(c => c.Inactive == idSupplierRatePlan);
                if (rateplanAssignmentExists == true)
                {
                    e.Cancel = true;
                    System.Web.HttpContext.Current.Response.Write(

                        "<script type=" + (char)34 + "text/JavaScript" + (char)34 + ">" +
                        "alert('Record cannot be deleted as this rateplan is currently assigned!');" +
                        "</script>"
                        );
                    return;
                }

                else //no task, delete rate task reference first
                {
                    context.Database.ExecuteSqlCommand(" delete from ratetaskreference where idrateplan=" + idSupplierRatePlan);
                }
            }

        }

    }
    protected void frmSupplierRatePlanInsert_Load(object sender, EventArgs e)
    {

        if (!this.IsPostBack)
        {

            //DropDownList ddlistPartner = (DropDownList)frmSupplierRatePlanInsert.FindControl("ddlistPartner");
            int idOperatorType = 0;

            if (this.Session["sesidOperatorType"] != null)
            {
                idOperatorType = (int)this.Session["sesidOperatorType"];
            }

            string sql = "";
            if (idOperatorType == 4)//igw
            {
                //                ddlistPartner.DataSourceID = "SqlDataFrmDdListIgwOperator";
            }

            else//icx and regular partners
            {
                //ddlistPartner.DataSourceID = "SqlDataFrmDdListICXOperator";
            }



        }
    }

    protected void frmSupplierRatePlanInsert_ItemCreated(object sender, EventArgs e)
    {
        DropDownList ddlistTz = (DropDownList)this.frmSupplierRatePlanInsert.FindControl("ddlistTimeZone");

        //find out default native timezone for this operator
        int thisOperatorId = -1;
        int thisTimeZoneIndex = -1;
        if (this.Session["sesidOperator"] != null)
        {
            thisOperatorId = (int)this.Session["sesidOperator"];
        }

        using (PartnerEntities conMed = new PartnerEntities())
        {
            if (thisOperatorId > 0)
            {
                thisTimeZoneIndex = (from c in conMed.telcobrightpartners
                                     where c.idCustomer == thisOperatorId
                                     select c.NativeTimeZone).First();
                ddlistTz.SelectedValue = thisTimeZoneIndex.ToString();
            }
        }

        //set default billing span=minute
        ((DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListBillingSpan")).SelectedIndex = 1;
        LoadCurrencies();
    }


    protected void frmSupplierRatePlanInsert_ItemInserting(object sender, FormViewInsertEventArgs e)
    {

        int newType = int.Parse(((DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListType")).SelectedValue);
        string newName = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("RatePlanNameTextBox")).Text;
        //int newPartner = int.Parse(((DropDownList)frmSupplierRatePlanInsert.FindControl("ddlistPartner")).SelectedValue);

        int newTimeZoneId = int.Parse(((DropDownList)this.frmSupplierRatePlanInsert.FindControl("ddlistTimeZone")).SelectedValue);
        string newDescription = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("DescriptionTextBox")).Text;
        string newActiveDate = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("ActiveDateTextBox")).Text;
        string newCurrency = ((DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListCurrency")).SelectedValue;

        string newActiveTime = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("TextBoxTime")).Text;
        newActiveDate += " " + newActiveTime;

        //string newCodeDeleteDate = ((TextBox)frmSupplierRatePlanInsert.FindControl("TextBoxPrevious")).Text;
        //string newCodeDeleteTime = ((TextBox)frmSupplierRatePlanInsert.FindControl("TextBoxPreviousTime")).Text;
        //newCodeDeleteDate += " " + newCodeDeleteTime;


        string newServiceType = ((DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListServiceType")).SelectedValue;
        string newSubServiceType = ((DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListSubServiceType")).SelectedValue;
        string newSurchargeTime = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("txtSurchargeTime")).Text;
        string newSurchargeAmount = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("txtSurchargeAmount")).Text;
        string newResolution = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("txtResolution")).Text;
        string newMinDurationSec = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("txtMinDurationSec")).Text;
        string newRateAmountRoundupDecimal = ((TextBox)this.frmSupplierRatePlanInsert.FindControl("TextBoxRoundUp")).Text;
        if (StringExtensions.IsNumeric(newRateAmountRoundupDecimal) == false)
        {
            newRateAmountRoundupDecimal = "";
        }
        rateplan newPlan = new rateplan();
        int temproundUp = -1;
        if (int.TryParse(newRateAmountRoundupDecimal, out temproundUp) == true)
        {
            newPlan.RateAmountRoundupDecimal = temproundUp;
        }
        else
        {
            newPlan.RateAmountRoundupDecimal = null;
        }
        newPlan.Type = newType;
        newPlan.RatePlanName = newName;
        //NewPlan.idPartner = newPartner;
        newPlan.Description = newDescription;
        newPlan.TimeZone = newTimeZoneId;
        //NewPlan.field1 = CountryWiseCodeUpdate;
        string strformat = "yyyy-MM-dd HH:mm:ss";
        DateTime dateTime;
        if (DateTime.TryParseExact(newActiveDate, strformat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out dateTime) == true)
        {
            newPlan.date1 = dateTime;
        }

        newPlan.Category = sbyte.Parse(newServiceType);
        newPlan.SubCategory = sbyte.Parse(newSubServiceType);

        decimal tempDecimal = 0;
        if (decimal.TryParse(newSurchargeAmount, out tempDecimal))
        {
            newPlan.SurchargeAmount = tempDecimal;
        }
        else newPlan.SurchargeAmount = 0;

        int tempInt = 0;
        if (int.TryParse(newSurchargeTime, out tempInt))
        {
            newPlan.SurchargeTime = tempInt;
        }
        else newPlan.SurchargeAmount = 0;

        //        int TempInt = 0;
        if (int.TryParse(newResolution, out tempInt))
        {
            newPlan.Resolution = tempInt;
        }
        else newPlan.Resolution = 1;//default
        Single tempSin = 0;
        if (Single.TryParse(newMinDurationSec, out tempSin))
        {
            newPlan.minDurationSec = tempSin;
        }
        else newPlan.minDurationSec = 0;//default

        newPlan.Currency = newCurrency;
        newPlan.BillingSpan = ((DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListBillingSpan")).SelectedValue;

        //field5= Month First (MF) or Day First (DF) for Ambiguous Date Handling
        newPlan.field5 = ((DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListAmbiguous")).SelectedValue;

        //field3=Mark as Ref Rate Plan for LCR
        newPlan.field3 = Convert.ToInt32(((DropDownList)this.frmSupplierRatePlanInsert.FindControl("DropDownListLcrRef")).SelectedValue);
        using (PartnerEntities context = new PartnerEntities())
        {

            context.rateplans.Add(newPlan);
            context.SaveChanges();

            this.frmSupplierRatePlanInsert.Visible = false;
            //LoadMainDropDownList();
            this.GridViewSupplierRatePlan.DataBind();

        }


    }

    protected void frmSupplierRatePlanInsert_DataBound(object sender, EventArgs e)
    {

    }
    protected void frmSupplierRatePlanInsert_ModeChanged(object sender, EventArgs e)
    {

    }
    protected void ButtonFind_Click(object sender, EventArgs e)
    {
        this.GridViewSupplierRatePlan.DataBind();
    }
    protected void frmSupplierRatePlanInsert_ItemCommand(object sender, FormViewCommandEventArgs e)
    {

    }
}