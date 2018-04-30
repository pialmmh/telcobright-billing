using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LibraryExtensions;
using MediationModel;
using PortalApp;
using TelcobrightMediation.Config;

public partial class DefaultRates : Page
{

    override protected void OnInit(EventArgs e)
    {
        Load += new EventHandler(Page_Load);
        base.OnInit(e);
    }

  

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            //Retrieve Path from TreeView for displaying in the master page caption label

            TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
            CommonCode commonCodes = new CommonCode();
            commonCodes.LoadReportTemplatesTree(ref masterTree);

            string localPath = this.Request.Url.LocalPath;
            int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
            string rootFolder = localPath.Substring(1, pos2NdSlash);
            int endOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(rootFolder);
            string urlWithQueryString = ("~" + "/" + rootFolder + this.Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), this.Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
            //for some reason url was not including .aspx
            //if (UrlWithQueryString.EndsWith(".aspx==") == false)
            //{
            //    UrlWithQueryString += ".aspx";
            //}
            //TreeNodeCollection cNodes = MasterTree.Nodes;
            //TreeNode MatchedNode = null;
            //foreach (TreeNode N in cNodes)//for each nodes at root level, loop through children
            //{
            //    MatchedNode = CommonCodes.RetrieveNodes(N, UrlWithQueryString);
            //    if (MatchedNode != null)
            //    {
            //        break;
            //    }
            //}
            ////set screentile/caption in the master page...
            //Label lblScreenTitle = (Label)Master.FindControl("lblScreenTitle");
            //if (MatchedNode != null)
            //{
            //    lblScreenTitle.Text = MatchedNode.ValuePath;
            //}
            //else
            //{
            //    lblScreenTitle.Text = "";
            //}
            //****CAPTION SET Manually
            Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
            lblScreenTitle.Text = "Configuration/Rates";
            //End of Site Map Part *******************************************************************

            //set default service type to wholesale voice
            // set default service to whole sale voice
            this.DropDownListservice.SelectedIndex = 1;
            this.DropDownListPartner.Enabled = true;
            this.DropDownListRoute.Enabled = true;

            this.txtDate.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
            this.txtDate1.Text = DateTime.Today.AddDays(31).ToString("yyyy-MM-dd");
            this.ViewState["qstring"] = urlWithQueryString;//save because a re-direct is needed because gridview is not
            //refreshing after delete all rates...sucks.

            var dicCountry = new Dictionary<string, string>();
            var dicRatePlan = new Dictionary<string, string>();
            var dicPartner = new Dictionary<string, string>();
            var dicRoute = new Dictionary<string, string>();
            using (var context = new PartnerEntities())
            {
                foreach (countrycode cc in context.countrycodes.ToList())
                {
                    dicCountry.Add(cc.Code, cc.Name);
                }
                foreach (rateplan rp in context.rateplans.ToList())
                {
                    dicRatePlan.Add(rp.id.ToString(), rp.RatePlanName);
                }
                foreach (partner cr in context.partners.ToList())
                {
                    dicPartner.Add(cr.idPartner.ToString(), cr.PartnerName);
                }
                foreach (route r in context.routes.ToList())
                {
                    dicRoute.Add(r.idroute.ToString(), r.RouteName);
                }

                this.Session["rates.dicCountry"] = dicCountry;
                this.Session["rates.dicRatePlan"] = dicRatePlan;
                this.Session["rates.dicPartner"] = dicPartner;
                this.Session["rates.dicRoute"] = dicRoute;
                //redirected from rate plan page, view rate by rate plan
                HttpRequest q = this.Request;
                NameValueCollection n = q.QueryString;
                int v = -1;
                if (n.HasKeys())
                {
                    string k = n.GetKey(0);
                    if (k == "idRatePlan")// view rates by one rateplan
                    {
                        v = Convert.ToInt32(n.Get(0));
                        if (n.Keys.Count == 1)
                        {
                            this.DropDownListRatePlan.DataBind();
                            this.DropDownListRatePlan.SelectedValue = v.ToString();
                            this.LinkButtonRate.OnClientClick = "window.open('ratetask.aspx?idRatePlan=" + v.ToString() + "')";
                            //disable partner and assign direction fields
                            this.DropDownListPartner.Enabled = false;
                            this.DropDownListRoute.Enabled = false;
                            //corresponding service has to be set in the service dropdown
                            this.DropDownListservice.DataBind();
                            this.DropDownListservice.SelectedValue = context.rateplans.Where(c => c.id == v).First().Type.ToString();


                            this.DropDownListAssignedDirection.Enabled = false;

                            this.LinkButtonDelete.OnClientClick = "return confirm('Are you sure to delete all rates under this rate plan?');";
                            this.LinkButtonDelete.CommandArgument = v.ToString();

                            MyGridViewDataBound();
                            return;
                        }

                    }
                }
            }//using context
        }//if !postback
    }

    protected void ButtonFind_Click(object sender, EventArgs e)
    {   
        MyGridViewDataBound();
    }

    void MyGridViewDataBound()
    {
        try
        {

            if (this.DropDownListRatePlan.SelectedIndex > 0)// a rate plan is selected
            {
                //allow editing and deleting rates only if by rateplan is selected
                this.GridViewSupplierRates.Columns[0].Visible = true;
                this.GridViewSupplierRates.Columns[1].Visible = true;
                this.GridViewSupplierRates.Columns[5].Visible = false;//partner
                this.GridViewSupplierRates.Columns[6].Visible = false;//route
                this.GridViewSupplierRates.Columns[4].Visible = false;//order
            }
            else
            {
                //allow editing and deleting rates only if by rateplan is selected
                this.GridViewSupplierRates.Columns[0].Visible = false;
                this.GridViewSupplierRates.Columns[1].Visible = false;
                this.GridViewSupplierRates.Columns[5].Visible = true;//partner
                this.GridViewSupplierRates.Columns[6].Visible = true;//route
                this.GridViewSupplierRates.Columns[4].Visible = true;//order

                //also if service is not assignable then hide the partner and route field
                using (var context = new PartnerEntities())
                {
                    int idService = int.Parse(this.DropDownListservice.SelectedValue);
                    int? assigningNotRequired = context.enumservicefamilies.First(c => c.id == idService).PartnerAssignNotNeeded;
                    if (assigningNotRequired != 1)//assignable
                    {
                        this.GridViewSupplierRates.Columns[5].Visible = true;//partner
                        this.GridViewSupplierRates.Columns[6].Visible = true;//route
                    }
                    else//not assignable
                    {
                        this.GridViewSupplierRates.Columns[5].Visible = false;//partner
                        this.GridViewSupplierRates.Columns[6].Visible = false;//route
                    }
                }

            }


            this.lblStatus.Text = "";
            string startdate = this.txtDate.Text;
            string enddate = this.txtDate1.Text;

            DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
            DateTime denddate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
            DateTime comparedate = dstartdate;

            if (startdate.Length == 10)
            {
                DateTime.TryParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate);
            }
            else if (startdate.Length > 10)
            {
                DateTime.TryParseExact(startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate);
            }

            if (enddate.Length == 10)
            {
                DateTime.TryParseExact(enddate + " 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out denddate);
            }
            else if (enddate.Length > 10)
            {
                DateTime.TryParseExact(enddate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out denddate);
            }

            if (dstartdate > comparedate && denddate > comparedate)
            {
                startdate = dstartdate.ToString("yyyy-MM-dd HH:mm:ss");
                enddate = denddate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                throw new Exception("Invalid Date!");
            }

            ServiceAssignmentDirection assignDir = ServiceAssignmentDirection.Customer;
            if (this.DropDownListAssignedDirection.SelectedValue != "customer")
            {
                assignDir = ServiceAssignmentDirection.Supplier;
            }
            RateChangeType changetype = RateChangeType.All;
            switch (this.ddlistChangetype.SelectedValue)
            {
                case "-1":
                    changetype = RateChangeType.All;
                    break;
                case "2":
                    changetype = RateChangeType.New;
                    break;
                case "3":
                    changetype = RateChangeType.Increase;
                    break;
                case "4":
                    changetype = RateChangeType.Decrease;
                    break;
                case "5":
                    changetype = RateChangeType.Unchanged;
                    break;
            }

            int idservice = -1;
            int.TryParse(this.DropDownListservice.SelectedValue, out idservice);

            DateRange dRange = new DateRange();
            dRange.StartDate = dstartdate;
            dRange.EndDate = denddate;

            int idRatePlan = -1;
            int.TryParse(this.DropDownListRatePlan.SelectedValue, out idRatePlan);

            int idPartner = -1;
            int.TryParse(this.DropDownListPartner.SelectedValue, out idPartner);

            int idRoute = -1;
            int.TryParse(this.DropDownListRoute.SelectedValue, out idRoute);

            int priority = 0;//all
            int.TryParse(this.TextBoxPriority.Text.Trim(), out priority);

            int idServiceType = -1;
            int.TryParse(this.ddlistServiceType.SelectedValue, out idServiceType);

            int idSubServiceType = -1;
            int.TryParse(this.ddlistSubServiceType.SelectedValue, out idSubServiceType);

            if (this.CheckBoxAllTime.Checked == true && idRatePlan > 0)//viewing rates in a rate plan
            {
                //set startdate to 0001-01-01 00:00:00
                dRange.StartDate = new DateTime(1, 1, 1, 0, 0, 0);
                //set end date to 9999-12-31 23:59:59
                dRange.EndDate = new DateTime(9999, 12, 31, 23, 59, 59);
            }

            //get any ne of this telcobright partner, required by rate handling objects
            string conStrPartner = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;
            string dbNameAppConf = "";
            foreach (string param in conStrPartner.Split(';'))
            {
                if (param.ToLower().Contains("database"))
                {
                    dbNameAppConf = param.Split('=')[1].Trim('"');
                    break;
                }
            }
            
            TelcobrightConfig tbc = PageUtil.GetTelcobrightConfig();
            using(PartnerEntities context=new PartnerEntities())
            {
                context.Database.Connection.Open();
                RateContainerInMemoryLocal rateContainer = new RateContainerInMemoryLocal(context);
                RateDictionaryGeneratorByTuples dicGenerator = new RateDictionaryGeneratorByTuples(idservice,
                    dRange, assignDir, idRatePlan, idPartner, idRoute, priority, this.TextBoxPrefix.Text.Trim(), this.TextBoxDescription.Text.Trim(),
                    idServiceType,
                    idSubServiceType, changetype,context,rateContainer);
            
                //DicGenerator.QueryCacheUpdateInDB(ConPartner, dRange);
                //order by prefix ascending and startdate descending
                Dictionary<TupleByPeriod, List<Rateext>> dicRateList = dicGenerator.GetRateDict(useInMemoryTable:false);//Get the rate dictionary
                List<Rateext> combinedList = new List<Rateext>();
                foreach (KeyValuePair<TupleByPeriod, List<Rateext>> kv in dicRateList)
                {
                    combinedList.AddRange(kv.Value);
                }
                this.GridViewSupplierRates.DataSource = combinedList;
                this.GridViewSupplierRates.DataBind();
                if (combinedList.Count < 1)
                {
                    this.lblStatus.Text = " No Rate Found!";
                }
                else
                {
                    this.lblStatus.Text = combinedList.Count + " Rates";
                }
            }
        }
        catch (Exception e1)
        {
            this.lblStatus.Text = e1.Message + "<br/>" + e1.InnerException;
            this.lblStatus.ForeColor = Color.Red;
        }
    }


    
    protected void GridViewSupplierRates_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //bind country code
            Label lblCountry = (Label)e.Row.FindControl("lblCountry");
            Label lblPartner = (Label)e.Row.FindControl("lblPartner");
            Label lblRoute = (Label)e.Row.FindControl("lblRoute");
            var dicCountry = (Dictionary<string, string>) this.Session["rates.dicCountry"];
            var dicPartner = (Dictionary<string, string>) this.Session["rates.dicPartner"];
            var dicRoute = (Dictionary<string, string>) this.Session["rates.dicRoute"];
            string thisCc = DataBinder.Eval(e.Row.DataItem, "countrycode").ToString();
            int thisidCarier = Convert.ToInt32( DataBinder.Eval(e.Row.DataItem, "idpartner").ToString());
            int thisidRoute = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "idroute").ToString());
            if (lblCountry != null && thisCc!="")
            {
                string countryName = "";
                dicCountry.TryGetValue(thisCc, out countryName);
                lblCountry.Text = countryName;
            }
            if (lblPartner != null && thisidCarier > 0)
            {
                lblPartner.Text = dicPartner[thisidCarier.ToString()];
            }
            else { lblPartner.Text = "All"; }
            if (lblRoute != null && thisidRoute > 0)
            {
                lblRoute.Text = dicRoute[thisidRoute.ToString()];
            }
            else { lblRoute.Text = "All"; }
            Label lblRatePlan = (Label)e.Row.FindControl("lblRatePlan");
            var dicRatePlan = (Dictionary<string, string>) this.Session["rates.dicRatePlan"];
            string thisidRatePlan = DataBinder.Eval(e.Row.DataItem, "idrateplan").ToString();
            if (lblRatePlan != null)
            {
                lblRatePlan.Text = dicRatePlan[thisidRatePlan];
            }
            if ((e.Row.RowState & DataControlRowState.Edit) > 0)
            {   

                //set default values of start/end date controls to their default values before editing

                string thisdate = this.lblRateGlobal.Text;
                string[] allDates = thisdate.Split('#');

                AjaxControlToolkit.CalendarExtender calDate = (AjaxControlToolkit.CalendarExtender)e.Row.FindControl("CalendarStartDate");
                TextBox txtTime = (TextBox)e.Row.FindControl("TextBoxStartDateTimePicker");

                string strCalDate = allDates[0];
                string format = "yyyy-MM-dd";
                DateTime dateTime;
                //if (DateTime.TryParseExact(strCalDate, format, CultureInfo.InvariantCulture,
                //    DateTimeStyles.None, out dateTime))
                //{
                //    CalDate.SelectedDate = dateTime;
                //    //CalDate.vi = dateTime;
                //    txtTime.Text = AllDates[1];
                //}
                //else
                //{
                //    CalDate.SelectedDate = DateTime.Now;
                //    //CalDate.VisibleDate = DateTime.Now;
                //    txtTime.Text = "00:00:00";
                //}


                AjaxControlToolkit.CalendarExtender calDateEnd = (AjaxControlToolkit.CalendarExtender)e.Row.FindControl("CalendarEndDate");
                TextBox txtTimeEnd = (TextBox)e.Row.FindControl("TextBoxEndDateTimePicker");

                string strCalDateEnd = allDates[2];
                DateTime dateTimeEnd;
                if (DateTime.TryParseExact(strCalDateEnd, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTimeEnd))
                {
                    calDateEnd.SelectedDate = dateTimeEnd;

                    txtTime.Text = allDates[3];
                }
                else
                {
                    //CalDateEnd.SelectedDate = DateTime.Today;

                    txtTimeEnd.Text = "00:00:00";
                }
            }//edit mode data binding   
            else
            {//not edit mode, binding during normal gridview mode

                

                //Label lblResolution = (Label)e.Row.FindControl("lblResolution");
                //lblResolution.Text = DataBinder.Eval(e.Row.DataItem, "Resolution").ToString();

                CheckBox chkbox = (CheckBox)e.Row.FindControl("CheckBox1");
                //set command argument for link button edit to be retrieved in rowcommand event,seperated by #
                LinkButton lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonEdit");

                //for change completed rate tasks, disable the edit button
                bool changeCommitted = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "changecommitted"));
                if (changeCommitted == true)
                {
                    lnkBtn.Enabled = false;
                    chkbox.Checked = true;
                }
                else
                {
                    lnkBtn.Enabled = true;
                    chkbox.Checked = false;
                }

                
                string effDate = ";";
                string effTime = ";";
                string endDate = ";";
                string endTime = ";";
                if (DataBinder.Eval(e.Row.DataItem, "startdate") != null)
                {
                    try//exception may occur due to invalid date string
                    {
                        effDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("yyyy-MM-dd");
                        effTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("HH:mm:ss");
                    }
                    catch (Exception e1)
                    {
                        //do nothing, jsut allow the program to continue
                    }
                }

                if (DataBinder.Eval(e.Row.DataItem, "p_enddate") != null && DataBinder.Eval(e.Row.DataItem, "p_enddate").ToString().Trim() != "")
                {
                    try
                    {
                        endDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "p_enddate")).ToString("yyyy-MM-dd");
                        endTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "p_enddate")).ToString("HH:mm:ss");
                    }
                    catch (Exception e1)
                    {
                        //do nothing, jsut allow the program to continue
                    }
                }
                //LnkBtn.CommandArgument = e.Row.RowIndex.ToString();
                lnkBtn.CommandArgument = effDate + "#" + effTime + "#" + endDate + "#" + endTime;

                //set country here....
                //dropdown ThisLabel = (Label)e.Row.FindControl("lblRateAmount");

                //country
                Label thisLabel = (Label)e.Row.FindControl("lblCountry");
                if (DataBinder.Eval(e.Row.DataItem, "CountryCode") != null && DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString() != "")
                {
                    string thisCountryCode = DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString();
                    if (thisLabel != null)
                    {
                        if (this.Session["rates.rates.sesCountryCodes"] != null)
                        {
                            List<countrycode> countryCodes = (List<countrycode>) this.Session["rates.rates.sesCountryCodes"];
                            if ((countryCodes.Any(c => c.Code == thisCountryCode)) == true)
                            {
                                thisLabel.Text = (from c in countryCodes
                                                  where c.Code == thisCountryCode
                                                  select c.Name + " (" + c.Code + ")").First();
                            }
                        }
                    }
                }


                //lblRateAmount
                thisLabel = (Label)e.Row.FindControl("lblRateAmount");
                if (DataBinder.Eval(e.Row.DataItem, "RateAmount") != null)
                {
                    decimal thisRateAmount = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "RateAmount"));
                    if (thisLabel != null)
                    {
                        //if(ThisRateAmount
                        thisLabel.Text = thisRateAmount.ToString("0.#00000");
                    }
                }
                else //null rateamount
                {
                    if (thisLabel != null)
                    {
                        //if(ThisRateAmount
                        thisLabel.Text = "";
                    }
                }

                //ThisLabel
                //set change types e.g. new, delete, increase, decrease etc.
                thisLabel = (Label)e.Row.FindControl("lblRateChangeType");
                if (DataBinder.Eval(e.Row.DataItem, "Status") != null)
                {
                    int changeType = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Status"));
                    switch (changeType)
                    {
                        case 0:
                        case 7:
                        case 8:
                            thisLabel.Text = "";//committed & uncommitteds are handled by changecommitted flag
                            //errors are handled by field2
                            break;
                        case 1:
                            thisLabel.Text = "Code End";
                            break;
                        case 2:
                            thisLabel.Text = "New";
                            break;
                        case 3:
                            thisLabel.Text = "Increase";
                            break;
                        case 4:
                            thisLabel.Text = "Decrease";
                            break;
                        case 5:
                            thisLabel.Text = "Unchanged";
                            break;
                        case 9:
                            thisLabel.Text = "Overlap";
                            break;
                        case 10:
                            thisLabel.Text = "Overlap Adjusted";
                            break;
                        case 11:
                            thisLabel.Text = "Rate Param Conflict";
                            break;
                        case 12:
                            thisLabel.Text = "Rate Position not found";
                            break;
                        case 13:
                            thisLabel.Text = "Existing";
                            break;
                    }

                }
                else
                {
                    thisLabel.Text = "Unknown";
                }

                

            }//else not edit mode, binding during normal gridview mode
        }// if data row
        else if (e.Row.RowType == DataControlRowType.Footer)
        {
            
        }
    }

    protected void DropDownListPartner_SelectedIndexChanged(Object sender, EventArgs e)
    {
        int thisidPartner=Convert.ToInt32(this.DropDownListPartner.SelectedValue);
        if (this.DropDownListPartner.SelectedIndex > 0)//one selected partner
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                List<route> lstRoute = context.routes.Where(c => c.idPartner == thisidPartner).ToList();
                this.DropDownListRoute.Items.Clear();
                this.DropDownListRoute.Items.Add(new ListItem(" [All]", "-1"));
                foreach (route r in lstRoute)
                {
                    this.DropDownListRoute.Items.Add(new ListItem(r.RouteName, r.idroute.ToString()));
                }
            }
            this.DropDownListRoute.Enabled = true;
        }
        else//no partner selected
        {
            this.DropDownListRoute.Items.Clear();
            this.DropDownListRoute.Items.Add(new ListItem(" [All]", "-1"));
            this.DropDownListRoute.SelectedIndex = 0;
            this.DropDownListRoute.Enabled = false;
        }
    }

    protected void ddlservice_SelectedIndexChanged(Object sender, EventArgs e)
    {
        if (this.DropDownListservice.SelectedIndex > 0)
        {
            //DropDownListPartner.SelectedIndex = 0;
            //DropDownListPartner.Enabled = false;
            //RadioButtonCustomer.Enabled = false;
            //RadioButtonSupplier.Enabled = false;
            using (PartnerEntities conMed = new PartnerEntities())
            {
                using (PartnerEntities context = new PartnerEntities())
                {
                    int type = Convert.ToInt32(this.DropDownListservice.SelectedValue);
                    List<rateplan> lstRatePlan = context.rateplans.Where(c => c.Type == type).ToList();
                    this.DropDownListRatePlan.Items.Clear();
                    this.DropDownListRatePlan.Items.Add(new ListItem(" All", "-1"));
                    foreach (rateplan rp in lstRatePlan)
                    {
                        this.DropDownListRatePlan.Items.Add(new ListItem(rp.RatePlanName, rp.id.ToString()));
                    }

                    //some rating rule may not need to be assigned to partners
                    int idservice = Convert.ToInt32(this.DropDownListservice.SelectedValue);
                    int? partnerAssgnNotNeeded = conMed.enumservicefamilies.Where(c => c.id == idservice).First().PartnerAssignNotNeeded;
                    if (Convert.ToInt32(partnerAssgnNotNeeded) == 1)
                    {
                        this.DropDownListPartner.SelectedIndex = 0;
                        this.DropDownListPartner.Enabled = false;
                        this.DropDownListRoute.Enabled = false;

                        this.DropDownListAssignedDirection.SelectedIndex = 0;
                        this.DropDownListAssignedDirection.Enabled = false;
                    }
                    else//partner assignable
                    {
                        this.DropDownListPartner.SelectedIndex = 0;
                        this.DropDownListAssignedDirection.SelectedIndex = 0;

                        if (this.DropDownListRatePlan.SelectedIndex == 0)
                        {
                            this.DropDownListPartner.Enabled = true;
                            this.DropDownListRoute.Enabled = true;
                            this.DropDownListAssignedDirection.Enabled = true;
                        }
                    }

                }
            }

            
        }
        else
        {
            this.DropDownListservice.DataBind();
            this.DropDownListPartner.Enabled = false;
            this.DropDownListRoute.Enabled = false;
        }
    }

    protected void ddlRateplan_SelectedIndexChanged(Object sender, EventArgs e)
    {
        if (this.DropDownListRatePlan.SelectedIndex > 0)
        {
            this.DropDownListPartner.SelectedIndex = 0;
            this.DropDownListPartner.Enabled = false;
            this.DropDownListRoute.Enabled = false;
            this.DropDownListAssignedDirection.Enabled = false;
            this.TextBoxPriority.Text = "0";
            this.TextBoxPriority.Enabled = false;
            this.LinkButtonRate.OnClientClick = "window.open('rates.aspx?idRatePlan=" + this.DropDownListRatePlan.SelectedValue + "')";
            this.LinkButtonDelete.OnClientClick = "confirm('Are you sure to delete all rates under this rate plan?')";
            this.LinkButtonDelete.CommandArgument = this.DropDownListRatePlan.SelectedValue;
            this.LinkButtonRate.Visible = true;
            this.LinkButtonDelete.Visible = true;
        }
        else
        {
            //check if the current service is partner assignable
            using (var context = new PartnerEntities())
            {
                int idService= int.Parse(this.DropDownListservice.SelectedValue);
                int? assigningNotRequired = context.enumservicefamilies.First(c => c.id == idService).PartnerAssignNotNeeded;
                if (assigningNotRequired != 1)//assignable
                {
                    this.DropDownListPartner.Enabled = true;
                    this.DropDownListRoute.Enabled = true;
                    this.DropDownListAssignedDirection.Enabled = true;
                    this.TextBoxPriority.Enabled = true;
                }
            }
            this.LinkButtonRate.Visible = false;
            this.LinkButtonDelete.Visible = false;
        }
    }

    
    protected void LinkButtonDelete_Click(object sender, EventArgs e)
    {
        using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
        {
            con.Open();
            using (MySqlCommand cmd = new MySqlCommand(" delete from rate where idrateplan= " + ((LinkButton)sender).CommandArgument,con))
            {
                cmd.ExecuteNonQuery();
            }
        }
        
        //MyGridViewDataBound();//not refresing the gridview
        this.Response.Redirect((string) this.ViewState["qstring"]);
    }//submit click

    protected void Button1_Click(object sender, EventArgs e)
    {
        DataTable dt2;
        List<string> colNameList=new List<string>();
        List<int> columnSortList=new List<int>();
        if (this.Session["rates.Rates.aspx.csdt16"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            dt2 = (DataTable) this.Session["rates.Rates.aspx.csdt16"];//THIS MUST BE CHANGED IN EACH PAGE
            
            if (this.Session["rates.Rates.aspx.csdt26"] != null) //THIS MUST BE CHANGED IN EACH PAGE
            {
                colNameList = (List<string>) this.Session["rates.Rates.aspx.csdt26"];//THIS MUST BE CHANGED IN EACH PAGE
            }

            if (this.Session["rates.Rates.aspx.csdt36"] != null) //THIS MUST BE CHANGED IN EACH PAGE
            {
                columnSortList = (List<int>) this.Session["rates.Rates.aspx.csdt36"];//THIS MUST BE CHANGED IN EACH PAGE
            }
            ExportToSpreadsheet(dt2, "International Incoming",colNameList,columnSortList); //THIS MUST BE CHANGED IN EACH PAGE
            this.Session.Abandon();
        }
    }

    public static void ExportToSpreadsheet(DataTable table, string name, List<string> colNameList, List<int> columnSortlist)
    {
        HttpContext context = HttpContext.Current;
        context.Response.Clear();

        string thisRow = "";
        
        //write columns in order specified in ColumnSortedList
        int ii = 0;
        for (ii=0; ii<colNameList.Count;ii++ )
        {  
            //ThisRow +=  table.Columns[ColumnSortlist[ii]].ColumnName + ",";
            thisRow += colNameList[ii]+ ",";
        }

        thisRow = thisRow.Substring(0, thisRow.Length - 1) + Environment.NewLine;
        context.Response.Write(thisRow);


        foreach (DataRow row in table.Rows)
        {
            thisRow = "";
            for (ii = 0; ii < columnSortlist.Count; ii++) //for each column
            {  
                thisRow += row[columnSortlist[ii]].ToString().Replace(",", string.Empty) + ",";
            }
            thisRow = thisRow.Substring(0, thisRow.Length - 1) + Environment.NewLine;
            context.Response.Write(thisRow);
        }
        
        context.Response.ContentType = "application/ms-excel";
        context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + name + ".csv");
        context.Response.End();
    }



    protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
    {
        
    }


    public void DateInitialize()
    {
        //else
        //{
        //    txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
        //    txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
        //}
    }

    protected void TextBoxDuration_TextChanged(object sender, EventArgs e)
    {
        long a;
        if (!long.TryParse(this.TextBoxDuration.Text, out a))
        {
            // If Not Integer Clear Textbox text or you can also Undo() Last Operation :)

            this.TextBoxDuration.Text = "30";
        }
    }

    protected void GridViewSupplierRates_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        GridViewRow thisRow = this.GridViewSupplierRates.Rows[e.RowIndex];
        string thisId = ((Label)thisRow.FindControl("lblId")).Text;
        using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["Partner"].ToString()))
        {
            con.Open();
            using (MySqlCommand cmd = new MySqlCommand(" delete from rate where id= " + thisId, con))
            {
                cmd.ExecuteNonQuery();
            }
        }
        MyGridViewDataBound();
    
    }
    protected void GridViewSupplierRates_RowEditing(object sender, GridViewEditEventArgs e)
    {
        this.GridViewSupplierRates.EditIndex = e.NewEditIndex;
        MyGridViewDataBound();
        
        
    }
    protected void GridViewSupplierRates_RowUpdated(object sender, GridViewUpdatedEventArgs e)
    {
        if (this.hidvaluerowcolorchange.Value != "")
        {
            //couldnot change row color after validation fail in row updating
            //taking help of hiddenfield
            string rowcolorchange = "";
            rowcolorchange = this.hidvaluerowcolorchange.Value;
            if (rowcolorchange.Length > 1 && rowcolorchange.Contains(","))
            {
                int targetRow = -1;
                int.TryParse(rowcolorchange.Split(',')[0], out targetRow);
                if (targetRow > -1)
                {
                    string msg = rowcolorchange.Split(',')[1];
                    this.lblStatus.ForeColor = Color.Red;
                    this.lblStatus.Text = msg;
                }
            }
            e.KeepInEditMode = true;
        }
        MyGridViewDataBound();
    }
    protected void GridViewSupplierRates_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        try
        {
            this.hidvaluerowcolorchange.Value = "";

            GridViewRow row = this.GridViewSupplierRates.Rows[e.RowIndex];
            long newId = Convert.ToInt64(((Label)row.FindControl("lblId")).Text);
            string newDesc = ((TextBox)row.FindControl("txtDescription")).Text;
            double newRateAmount = Convert.ToDouble(((TextBox)row.FindControl("txtRateAmount")).Text);
            int newResolution = Convert.ToInt32(((TextBox)row.FindControl("txtResolution")).Text);
            Single newMinDurationSec = Convert.ToSingle(((TextBox)row.FindControl("txtMinDurationSec")).Text);
            string newEndDate = ((TextBox)row.FindControl("TextBoxEndDatePicker")).Text;
            string newEndTime = ((TextBox)row.FindControl("TextBoxEndDateTimePicker")).Text;
            string newEndDateAndTime = "";
            if (newEndDate != "")
            {
                newEndDateAndTime = newEndDate + " " + newEndTime;
            }
            int newInactive = Convert.ToInt32(((DropDownList)row.FindControl("DropDownListInactive")).SelectedValue);
            int newSurchargeTime = Convert.ToInt32(((TextBox)row.FindControl("txtSurchargeTime")).Text);

            Single newSurchargeAmount = 0;
            Single newOtherAmount1 = 0;
            Single newOtherAmount2 = 0;
            Single newOtherAmount3 = 0;
            Single newOtherAmount4 = 0;
            Single newOtherAmount5 = 0;
            Single newOtherAmount6 = 0;
            Single newOtherAmount7 = 0;
            Single newOtherAmount8 = 0;
            Single newOtherAmount9 = 0;
            Single newOtherAmount10 = 0;

            Single.TryParse(((TextBox)row.FindControl("txtSurchargeAmount")).Text, out newSurchargeAmount);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount1")).Text, out newOtherAmount1);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount2")).Text, out newOtherAmount2);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount3")).Text, out newOtherAmount3);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount4")).Text, out newOtherAmount4);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount5")).Text, out newOtherAmount5);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount6")).Text, out newOtherAmount6);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount7")).Text, out newOtherAmount7);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount8")).Text, out newOtherAmount8);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount9")).Text, out newOtherAmount9);
            Single.TryParse(((TextBox)row.FindControl("txtOtherAmount10")).Text, out newOtherAmount10);

            int newRoundDigit = 0;
            int.TryParse(((TextBox)row.FindControl("txtRoundDigits")).Text, out newRoundDigit);

            rate thisInstance = null;
            rate nextInstance = null;
            using (PartnerEntities context = new PartnerEntities())
            {
                thisInstance = context.rates.Where(c => c.id == newId).FirstOrDefault();
                nextInstance = context.rates.Where(c => c.idrateplan == thisInstance.idrateplan && c.Prefix == thisInstance.Prefix
                                                                        && c.startdate > thisInstance.startdate
                                                                        && c.Category == thisInstance.Category
                                                                       && c.SubCategory == thisInstance.SubCategory).OrderBy(c => c.startdate).Take(1).ToList().FirstOrDefault();
               
                
                if (thisInstance != null)
                {
                    DateTime newEndDateAsDate = new DateTime();
                    if (DateTime.TryParseExact(newEndDateAndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out newEndDateAsDate))
                    {
                        if (newEndDateAsDate < thisInstance.startdate)
                        {
                            this.hidvaluerowcolorchange.Value = e.RowIndex + "," + "End datetime must be greater than start datetime";
                            return;

                        }
                        if (nextInstance != null && (newEndDateAsDate > nextInstance.startdate))
                        {
                            this.hidvaluerowcolorchange.Value = e.RowIndex + "," + "End datetime cannot overlap next effective date.";
                            return;
                        }
                        //otherwise
                        thisInstance.enddate = newEndDateAsDate;
                    }
                    else//new enddatetime is given but, format invalid OR, this is rate is latest having no end date
                    {
                        if (nextInstance != null)
                        {
                            this.hidvaluerowcolorchange.Value = e.RowIndex + "," + "Invalid End Datetime.";
                            return;
                        }
                    }

                    //support only selected fields during rate update for consistency
                    thisInstance.description = newDesc;
                    thisInstance.rateamount = Convert.ToDecimal(newRateAmount);
                    thisInstance.Resolution = newResolution;
                    thisInstance.MinDurationSec = newMinDurationSec;
                    thisInstance.Inactive = newInactive;
                    thisInstance.SurchargeTime = newSurchargeTime;
                    thisInstance.SurchargeAmount = Convert.ToDecimal(newSurchargeAmount);
                    thisInstance.OtherAmount1 = Convert.ToDecimal(newOtherAmount1);
                    thisInstance.OtherAmount2 = Convert.ToDecimal(newOtherAmount2);
                    thisInstance.OtherAmount3 = Convert.ToDecimal(newOtherAmount3);
                    thisInstance.OtherAmount4 = Convert.ToDecimal(newOtherAmount4);
                    thisInstance.OtherAmount5 = Convert.ToDecimal(newOtherAmount5);
                    thisInstance.OtherAmount6 = Convert.ToDecimal(newOtherAmount6);
                    thisInstance.OtherAmount7 = newOtherAmount7;
                    thisInstance.OtherAmount8 = newOtherAmount8;
                    thisInstance.OtherAmount9 = newOtherAmount9;
                    thisInstance.OtherAmount10 = newOtherAmount10;
                    thisInstance.RateAmountRoundupDecimal = newRoundDigit;

                    context.SaveChanges();
                    this.GridViewSupplierRates.EditIndex = -1;
                    MyGridViewDataBound();

                }
            }
        }//try   
        catch (Exception e1)
        {
            this.lblStatus.ForeColor = Color.Red;
            this.lblStatus.Text = e1.Message + "<br>" + e1.InnerException;
        }

    }
    protected void GridViewSupplierRates_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Edit")
        {
            string effEndDate = e.CommandArgument.ToString();
            this.lblRateGlobal.Text = effEndDate;
        }
    }
    protected void GridViewSupplierRates_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        this.GridViewSupplierRates.EditIndex = -1;
        MyGridViewDataBound();
        this.lblStatus.Text = "";
        this.lblStatus.ForeColor = Color.Black;
    }
    protected void GridViewSupplierRates_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.GridViewSupplierRates.PageIndex = e.NewPageIndex;
        MyGridViewDataBound();
    }
    protected void GridViewSupplierRates_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
}
