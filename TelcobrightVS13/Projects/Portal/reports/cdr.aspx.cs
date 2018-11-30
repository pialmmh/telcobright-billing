using TelcobrightMediation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using ExportToExcel;
using MySql.Data.MySqlClient;
using System.Web;
using DocumentFormat.OpenXml.Drawing;
using MediationModel;
using PortalApp;
using System.IO;
using DevExpress.Web;
using DevExpress.Web.Data;
using DevExpress.Xpo;

public partial class ConfigCdr : Page
{

    public List<int> DateColumnIndexes = new List<int>();//for date formatting in row-databound

    //string startdate = "";
    //string enddate = "";
    //string country = "";
    //string destination = "";
    //string ans = "";
    //string ansid = "";
    //string icx = "";
    //string icxid = "";
    //string partner = "";
    //string partnerid = "";
    //string callsstatus = "";
    //string causecode = "";
    string _totalCdrCount = "";
    
    //string IncomingRoute = "";
    //int? SwitchIdInco = -1;
    //string OutgoingRoute = "";
    //int? SwitchIdOut = -1;
    //int startindex = 0;
    ////int endindex = startindex + 99;
    
    DataTable _dt = null;
    
    //GridView Implement var
    long _totalNumRows = 0;
    int _pageRowIndex = 0;

    int _maxNUmRows = 100;//number of rows per grid
    long _totalNumberOfRows = 0;
    int _maxPageVisible = 10;//maximum number of pages visible in grid footer
    long _totalpage = 0; long _numberOfSlot = 0;

    string _placeholderString = "";
    long _totalNumRowsTemp = 0;//TotalNumRows;
    int _gridActiveIndexTemp = 0;//index;

    override protected void OnInit(EventArgs e)
    {
        Load += new EventHandler(Page_Load);
        base.OnInit(e);
    }

    
    public DateTime FirstDayOfMonthFromDateTime(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    public DateTime LastDayOfMonthFromDateTime(DateTime dateTime)
    {
        DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
        return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
    }

    protected void DropDownListMonth_SelectedIndexChanged(object sender, EventArgs e)
    {
        //select 15th of month to find out first and last day of a month as it exists in all months.
        DateTime anyDayOfMonth = new DateTime(int.Parse(this.TextBoxYear.Text), int.Parse(this.DropDownListMonth.SelectedValue), 15);
        this.txtDate.Text = FirstDayOfMonthFromDateTime(anyDayOfMonth).ToString("yyyy-MM-dd");
    }
    protected void DropDownListMonth1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //select 15th of month to find out first and last day of a month as it exists in all months.
        DateTime anyDayOfMonth = new DateTime(int.Parse(this.TextBoxYear1.Text), int.Parse(this.DropDownListMonth1.SelectedValue), 15);
        this.txtDate1.Text = LastDayOfMonthFromDateTime(anyDayOfMonth).AddDays(1).ToString("yyyy-MM-dd");
    }

    protected void DropDownListSwitch_SelectedIndexChanged(object sender, EventArgs e)
    {
        int switchId = Convert.ToInt32(this.DropDownListSwitch.SelectedValue);
        PopulatePartner(switchId, 0);
        PopulatePartner(switchId, 1);
        PopulateRoute(switchId,-1, 0);
        PopulateRoute(switchId,-1, 1);
    }
    protected void DropDownListInPartner_SelectedIndexChanged(object sender, EventArgs e)
    {
        int switchId = Convert.ToInt32(this.DropDownListSwitch.SelectedValue);
        int idPartner = Convert.ToInt32(this.DropDownListInPartner.SelectedValue);
        PopulateRoute(switchId,idPartner, 0);
    }
    protected void DropDownListOutPartner_SelectedIndexChanged(object sender, EventArgs e)
    {
        int switchId = Convert.ToInt32(this.DropDownListSwitch.SelectedValue);
        int idPartner = Convert.ToInt32(this.DropDownListOutPartner.SelectedValue);
        PopulateRoute(switchId,idPartner, 1);
    }
    protected class ErrorSummary
    {
        public string Reason { get; set; }
        public long Count { get; set; }
    }

    protected void DropDownListSource_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(this.DropDownListSource.SelectedIndex==0)//cdr
        {
            this.DropDownListErrorReason.Items.Clear();
            this.DropDownListErrorReason.Items.Add(new ListItem("N/A", "0"));
            this.DropDownListFieldList.SelectedIndex = 0;//basic fields
        }
        else//cdrerror
        {
            this.DropDownListErrorReason.Items.Clear();
            this.DropDownListErrorReason.Items.Add(new ListItem(" [All]", "0"));
            List<ErrorSummary> lstEs = new List<ErrorSummary>();
            using (PartnerEntities context = new PartnerEntities())
            {
                lstEs = context.Database.SqlQuery<ErrorSummary>(@"select ifnull(Reason,'Unknown') as Reason, Count from
                                                                (select errorcode as Reason,count(*) as Count
                                                                from cdrerror group by errorcode) x
                                                                order by Count desc,Reason").ToList();
                foreach(ErrorSummary es in lstEs)
                {
                    this.DropDownListErrorReason.Items.Add(new ListItem(es.Reason + " [" + es.Count.ToString() + " Calls]", es.Reason));
                }
            }
            this.DropDownListFieldList.SelectedIndex = 2;//basic error
        }
    }
    

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {

            //PageUtil.ApplyPageSettings(this, Server.MapPath("~/reports/cdr.spring.json"));   
            //set screentile/caption in the master page...
            Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
            lblScreenTitle.Text = "Reports/CDR";

            this.TextBoxYear.Text = DateTime.Now.Year.ToString();
            this.TextBoxYear1.Text = DateTime.Now.Year.ToString();

            this.txtDate.Text = (DateTime.Now.AddHours(-1)).ToString("yyyy-MM-dd HH:mm:ss");
            this.txtDate1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
            //from Partner

            string thisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

            MySqlConnection connection = new MySqlConnection(thisConectionString);
            string database = connection.Database.ToString();
            List<ne> lstSwitch;
            TelcobrightConfig tbc = PortalApp.PageUtil.GetTelcobrightConfig();
            ServiceGroupComposer serviceGroupComposer = new ServiceGroupComposer();
            serviceGroupComposer.ComposeFromPath(PageUtil.GetPortalBinPath() +
                                                 System.IO.Path.DirectorySeparatorChar + ".." +
                                                 System.IO.Path.DirectorySeparatorChar + "Extensions");
            Dictionary<int, IServiceGroup> serviceGroups =
                serviceGroupComposer.ServiceGroups.ToDictionary(c => c.Id);
            using (PartnerEntities context = new PartnerEntities())
            {
                telcobrightpartner thisCustomer = (from c in context.telcobrightpartners
                    where c.databasename == database
                    select c).First();
                int thisOperatorId = thisCustomer.idCustomer;
                lstSwitch = context.nes.Where(c => c.idCustomer == thisOperatorId).ToList();

                if (lstSwitch.Count <= 0)
                {
                    this.lblStatus.Text = "No Switch found for customer: " + thisCustomer.CustomerName;
                    return;
                }
                //populate service group
                this.DropDownListServiceGroup.Items.Clear();
                this.DropDownListServiceGroup.Items.Add(new ListItem(" [All]", "-1"));

                foreach (KeyValuePair<int, ServiceGroupConfiguration> kv in tbc.CdrSetting.ServiceGroupConfigurations)
                {
                    if (serviceGroups.ContainsKey(kv.Key))
                    {
                        string serviceGroupName = serviceGroups[kv.Key].RuleName;
                        this.DropDownListServiceGroup.Items.Add(new ListItem(serviceGroupName,
                            serviceGroups[kv.Key].Id.ToString()));
                    }
                }
            }

            this.DropDownListSwitch.Items.Clear();
            this.DropDownListSwitch.Items.Add(new ListItem(" [All]", "-1"));
            foreach (ne ne in lstSwitch)
            {
                this.DropDownListSwitch.Items.Add(new ListItem(ne.SwitchName, ne.idSwitch.ToString()));
            }

            int switchId = Convert.ToInt32(this.DropDownListSwitch.SelectedValue);
            PopulatePartner(switchId, 0);
            PopulatePartner(switchId, 1);

            PopulateRoute(0, -1, 0);
            PopulateRoute(0, -1, 1);

            this.DropDownListFieldList.Items.Clear();
            // deserialize JSON directly from a file
            var fieldTemplates = (List<CdrFieldTemplate>) tbc.PortalSettings.DicConfigObjects["CdrFieldTemplate"];
            //foreach (JObject obj in jArray)
            //{
            //    CdrFieldTemplate cf = obj.ToObject<CdrFieldTemplate>();
            //    fieldTemplates.Add(cf);
            //    this.DropDownListFieldList.Items.Add(new ListItem(cf.FieldTemplateName, cf.FieldTemplateName));
            //}
            foreach (CdrFieldTemplate cf in fieldTemplates)
            {
                this.DropDownListFieldList.Items.Add(new ListItem(cf.FieldTemplateName, cf.FieldTemplateName));
            }
            this.Session["cdrfieldtemplate"] = fieldTemplates;


        } //if !postback
    }

    void PopulatePartner(int switchId,int inOrOut)
    {
        DropDownList ddlPartner = null;
        switch(inOrOut)
        {
            case 0:
                ddlPartner = this.DropDownListInPartner;
                break;
            case 1:
                ddlPartner = this.DropDownListOutPartner;
                break;
            default:
                break;
        }
        ddlPartner.Items.Clear();
        ddlPartner.Items.Add(new ListItem(" [All]", "-1"));
        using (PartnerEntities context = new PartnerEntities())
        {
            List<int> partnersWithRoutes = new List<int>();
            if (switchId<=0)
            {
                partnersWithRoutes = context.routes.GroupBy(test => test.idPartner)
                   .Select(grp => grp.FirstOrDefault())
                   .ToList().Select(c => c.idPartner).ToList();
            }
            else
            {
                partnersWithRoutes = context.routes.Where(r=>r.SwitchId==switchId).GroupBy(test => test.idPartner)
                   .Select(grp => grp.FirstOrDefault())
                   .ToList().Select(c => c.idPartner).ToList();
            }
            
            List<partner> lstPartners = context.partners.Where(p => partnersWithRoutes.Contains(
                p.idPartner
                )).OrderBy(c=>c.PartnerName).ToList();
            foreach (partner p in lstPartners)
            {
                ddlPartner.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
            }
        }
    }
    void PopulateRoute(int switchId, int idPartner, int inOrOut)
    {
        DropDownList ddlRoute = null;
        switch (inOrOut)
        {
            case 0:
                ddlRoute = this.DropDownListInRoute;
                break;
            case 1:
                ddlRoute = this.DropDownListOutRoute;
                break;
        }
        ddlRoute.Items.Clear();
        ddlRoute.Items.Add(new ListItem(" [All]", "-1"));
        using (PartnerEntities context = new PartnerEntities())
        {
            List<route> partnerRoutes = new List<route>();
            if (idPartner <= 0)
            {
                if (switchId > 0)
                {
                    partnerRoutes = context.routes.Where(c => c.SwitchId == switchId).ToList();
                }
                else
                {
                    partnerRoutes = context.routes.ToList();
                }
            }
            else
            {
                if (switchId > 0)
                {
                    partnerRoutes = context.routes.Where(r => r.SwitchId == switchId &&
                    r.idPartner == idPartner).OrderBy(c => c.RouteName).ToList();
                }
                else
                {
                    partnerRoutes = context.routes.Where(r => r.idPartner == idPartner).OrderBy(c => c.RouteName).ToList();
                }

            }

            foreach (route r in partnerRoutes)
            {
                ddlRoute.Items.Add(new ListItem(r.SwitchId + "-" + r.RouteName.ToString(), r.idroute.ToString()));
            }
        }
    }

    protected void ButtonExport_Click(object sender, EventArgs e)
    {

    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        //export
        long records = -1;
        if (long.TryParse(this.TextBoxNoOfRecords.Text, out records) == false)
        {
            this.lblStatus.Text = "Invalid number of records!";
            return;
        }
        else
        {
            if (records <= 0)
            {
                this.lblStatus.Text = "Invalid number of records!";
                return;
            }
        }
        ExportFirstOrAll(records);
    }
    public DataSet GetDataSet(string connectionString, string sql)
    {
        MySqlConnection conn = new MySqlConnection(connectionString);
        MySqlDataAdapter da = new MySqlDataAdapter();
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        da.SelectCommand = cmd;
        DataSet ds = new DataSet();

        conn.Open();
        da.Fill(ds);
        conn.Close();

        return ds;
    }
    public void ExportFirstOrAll(long noOfRecords)
    {
        string sql = "";
        if (noOfRecords == -1)//all records
        {
            sql = (string) this.ViewState["squery"];
        }
        else//first N
        {
            sql = ((string) this.ViewState["squery"]).Replace(";", "") + " limit 0," + this.TextBoxNoOfRecords.Text + ";";
        }
        DataTable dt = GetDataSet(ConfigurationManager.ConnectionStrings["reader"].ConnectionString, sql).Tables[0];
        ExportToSpreadsheet(dt, "callview_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }
    public static void ExportToSpreadsheet(DataTable dt, string name)
    {
        foreach (DataColumn dc in dt.Columns)
        {
            //add summary value if found for this column
            if (dc.ColumnName.ToLower().Contains("time"))//start time, end time etc.
            {
                dc.ExtendedProperties.Add("NumberFormat", "yyyy-MM-dd HH:mm:ss");
            }
            
        }
        HttpContext context = HttpContext.Current;
        context.Response.Clear();
        CreateExcelFileAspNet.CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(dt, "cdr_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + ".xlsx", context.Response);
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        //export
        ExportFirstOrAll(-1);
    }

    

    private string GetNumberFilterExpression(DropDownList ddlist, string fieldName, string value)
    {
        if (ddlist.SelectedValue == "0")//startswith
        {
            return " and " + fieldName + " like '" + value + "%' ";
        }
        else if (ddlist.SelectedValue == "1")//exact match
        {
            return " and " + fieldName + " like '" + value + "' ";
        }
        else//contains
        {
            return " and instr(" + fieldName + ",'" + value + "')>0 "; ;
        }
    }


    


    


    protected void ButtonFind_Click(object sender, EventArgs e)
    {

        DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
        DateTime denddate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
        DateTime comparedate = dstartdate;
        string startdate = this.txtDate.Text;
        string enddate = this.txtDate1.Text;
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
            DateTime.TryParseExact(enddate + " 23:59:59", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out denddate);
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
            this.lblStatus.Text = "Invalid Date!";
            return;
        }

        Dictionary<string, route> dicRoutes = null;
        using (PartnerEntities context = new PartnerEntities())
        {
            dicRoutes = context.routes.ToDictionary(c => c.idroute.ToString());
        }

        int serviceGroup = Convert.ToInt32(this.DropDownListServiceGroup.SelectedValue);
        int? chargingStatus = this.DropDownListChargingStatus.SelectedIndex == 0 ? -1 : Convert.ToInt32(this.DropDownListChargingStatus.SelectedValue);

        string sourceTable = this.DropDownListSource.SelectedIndex == 0 ? "cdr c " : "cdrerror c ";
        string sql = " select * from " + sourceTable + " left join partner inpartner on c.inpartnerid=inpartner.idpartner " +
                     " left join partner outpartner on c.outpartnerid=outpartner.idpartner " +//basic string
                     " where starttime>='" + startdate + "' and starttime<='" + enddate + "' " +
                     (serviceGroup >= 0 ? " and ServiceGroup=" + serviceGroup : "")+
                     (chargingStatus >= 0 ? " and chargingstatus=" + chargingStatus : "");

        int idSwitch = this.DropDownListSwitch.SelectedIndex == 0 ? 0 : Convert.ToInt32(this.DropDownListSwitch.SelectedValue);
        if (idSwitch > 0) sql += " and switchid=" + idSwitch.ToString();

        int customerId = this.DropDownListInPartner.SelectedIndex == 0 ? 0 : Convert.ToInt32(this.DropDownListInPartner.SelectedValue);
        if (customerId > 0) sql += " and inpartnerid=" + customerId;

        int supplierId = this.DropDownListOutPartner.SelectedIndex == 0 ? 0 : Convert.ToInt32(this.DropDownListOutPartner.SelectedValue);
        if (supplierId > 0) sql += " and outpartnerid=" + supplierId;

        string IncomingRoute = this.DropDownListInRoute.SelectedIndex == 0 ? "-1" : this.DropDownListInRoute.SelectedItem.Value;
        if (IncomingRoute != "-1")
        {
            string routeSwitchIdIn = dicRoutes[IncomingRoute].SwitchId.ToString();
            string routeNameIn = dicRoutes[IncomingRoute].RouteName;
            sql += " and ( switchid=" + routeSwitchIdIn + " and IncomingRoute='" + routeNameIn + "' ) ";
        }

        string OutgoingRoute = this.DropDownListOutRoute.SelectedIndex == 0 ? "-1" : this.DropDownListOutRoute.SelectedItem.Value;
        if (OutgoingRoute != "-1")
        {
            string routeSwitchIdOut = dicRoutes[OutgoingRoute].SwitchId.ToString();
            string routeNameOut = dicRoutes[OutgoingRoute].RouteName;
            sql += " and (switchid=" + routeSwitchIdOut + " and OutgoingRoute='" + routeNameOut + "' ) ";
        }

        if (!string.IsNullOrEmpty(this.TextBoxIngressCalled.Text.Trim()))
        {
            sql += GetNumberFilterExpression(this.ddlistIngressCalled, "originatingcallednumber", this.TextBoxIngressCalled.Text.Trim());
        }


        if (!string.IsNullOrEmpty(this.TextBoxIngressCalling.Text.Trim()))
        {
            sql += GetNumberFilterExpression(this.ddlistIngressCalling, "originatingcallingnumber", this.TextBoxIngressCalling.Text.Trim());
        }

        if (!string.IsNullOrEmpty(this.TextBoxEgressCalled.Text.Trim()))
        {
            sql += GetNumberFilterExpression(this.ddlistEgressCalled, "terminatingcallednumber", this.TextBoxEgressCalled.Text.Trim());
        }

        if (!string.IsNullOrEmpty(this.TextBoxEgressCalling.Text.Trim()))
        {
            sql += GetNumberFilterExpression(this.ddlistEgressCalling, "terminatingcallingnumber", this.TextBoxEgressCalling.Text.Trim());
        }

        string errorReasonField4 = this.DropDownListErrorReason.SelectedIndex == 0 ? "" : this.DropDownListErrorReason.SelectedValue;
        if (errorReasonField4 != "")
        {
            sql += " and ErrorCode='" + errorReasonField4 + "'";
        }
        //
        //Sql +=
        GridDataBinding2(sql);
    }

    private void GridDataBinding2(string sql)
    {
        using (PartnerEntities context = new PartnerEntities())
        {
            string countSql = sql.Replace("select *", "select count(*) as cnt ");
            this._totalNumRows = context.Database.SqlQuery<long>(countSql).ToList().First();
            this.lblStatus.Text = this._totalNumRows.ToString() + " Records";
            this.ViewState["totalnumrows"] = this._totalNumRows;
        }

        //fetch field template
        var cdrFieldTemplates = (List<CdrFieldTemplate>) this.Session["cdrfieldtemplate"];
        CdrFieldTemplate selectedTemplate = cdrFieldTemplates.Where(c => c.FieldTemplateName == this.DropDownListFieldList.SelectedValue).First();
        //replace * with selected fields only...
        sql = sql.Replace("select *", " select " + string.Join(",", selectedTemplate.Fields) + " ");
        if(selectedTemplate.FieldTemplateName== "Basic")
        {

        }


        this.ViewState["squery"] = sql;
        this.ViewState["totalnumrows"] = this._totalNumRows;
        this.ViewState["gridactiveindex"] = this._gridActiveIndexTemp;

        int.TryParse(this.ViewState["gridactiveindex"].ToString(), out this._gridActiveIndexTemp);
        this._pageRowIndex = this._gridActiveIndexTemp;

        string query = sql;
        this._dt = GetDataSource((this._pageRowIndex * this._maxNUmRows), this._maxNUmRows, query);

        this.DateColumnIndexes.Clear();
        for (int i = 0; i < this._dt.Columns.Count; i++)
        {
            DataColumn dc = this._dt.Columns[i];
            //add summary value if found for this column
            if (dc.ColumnName.ToLower().Contains("time"))//start time, end time etc.
            {
                this.DateColumnIndexes.Add(i);
            }

        }
        if (this._dt.Rows.Count < (this._maxNUmRows + 1))
        {
            DataRow dtExtra = this._dt.NewRow();
            this._dt.Rows.Add(dtExtra);

        }
        // int cccnnntt = dtTable2.Rows.Count;

        this.Session["dtCDR"] = _dt;
        this.gridViewDx.DataBind();

/*
        this.gridView.DataSource = this._dt;// dtSet;
        this.gridView.AllowPaging = true;
        if (this._dt.Rows.Count > 1)
            this.gridView.PageSize = this._dt.Rows.Count - 1;// maxNUmRows;
        else
            this.gridView.PageSize = 1;
        // if(dtTable2.Rows.count==maxNUmRows+1)
        this.gridView.PageIndex = 0;// pageRowIndex;
        this.gridView.DataBind();
        //init pager control
        this._placeholderString = "placeholder";
        this._totalNumRowsTemp = this._totalNumRows;
        //_gridActiveIndexTemp = gridactiveindex;
        SetPaging1(this.gridView);
*/
    }



    private DataTable GetDataSourceAll(string queryString)
    {
        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            connection.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    private void SetPaging1(GridView tempGrid)
    {
        //D R PV TP SL
        //totalrow=getcount();maxnumrows;maxPageVisible;Totalpage;Slot==start,end,prev,next
        //
        if (this._gridActiveIndexTemp > tempGrid.PageIndex)
        {
            tempGrid.PageIndex = this._gridActiveIndexTemp;
        }

        bool prevflag = false; bool nextflag = false;
        GridViewRow row1 = tempGrid.BottomPagerRow;
        // int numOfPage1 = _totalNumRowsTemp / maxNUmRows;
        long start = this._gridActiveIndexTemp + 1, slot = 1;
        /* if (_gridActiveIndexTemp >= maxNUmRows)
         {
             slot = (_gridActiveIndexTemp + 1) / maxNUmRows;
             start = maxNUmRows * slot + 1;
         }

         slot = _gridActiveIndexTemp / maxNUmRows;
         start = maxNUmRows * slot + 1;

         int end = start + maxNUmRows;

         if (end > numOfPage1)
             end = numOfPage1;*/

        ////////////
        PagingPreProcess();
        int pi = this._gridActiveIndexTemp + 1;


        long currentSlot = pi / this._maxPageVisible;
        if (pi % this._maxPageVisible > 0)
        {
            currentSlot++;
        }
        if (currentSlot > this._numberOfSlot)
        {
            currentSlot = this._numberOfSlot;
        }

        start = (currentSlot - 1) * this._maxPageVisible + 1;
        long end = start + this._maxPageVisible - 1;

        if (this._numberOfSlot > 1)
        {
            if (currentSlot == 1)
            {
                prevflag = false;
                nextflag = true;
                // start = 1; end = start + maxPageVisible-1;
            }
            else if (currentSlot == this._numberOfSlot)
            {
                prevflag = true;
                nextflag = false;
                // start = currentSlot;
                end = this._totalpage;
            }
            else
            {
                prevflag = true;
                nextflag = true;
            }
        }
        else
        {
            end = this._totalpage;
        }


        ///////////////
        if (row1 != null)
        {
            if (prevflag)
            {
                LinkButton btn = new LinkButton();
                btn.CommandName = "G1Page";
                btn.CommandArgument = (start - 1).ToString();
                btn.Text = "<..";
                btn.ToolTip = "Page " + (start - 1).ToString();

                PlaceHolder place = (PlaceHolder)row1.Cells[0].FindControl(this._placeholderString);
                place.Controls.Add(btn);

                Label lbl = new Label();
                lbl.Text = "| ";
                place.Controls.Add(lbl);
            }
            for (long i = start; i <= end; i++)
            {
                if (i <= end)
                {
                    LinkButton btn = new LinkButton();
                    btn.CommandName = "G1Page";
                    btn.CommandArgument = i.ToString();

                    if (i == this._gridActiveIndexTemp + 1)
                    {
                        btn.BackColor = System.Drawing.Color.Khaki;
                    }

                    btn.Text = i.ToString();
                    btn.ToolTip = "Page " + i.ToString();

                    PlaceHolder place = (PlaceHolder)row1.Cells[0].FindControl(this._placeholderString);
                    place.Controls.Add(btn);

                    Label lbl = new Label();
                    lbl.Text = "| ";
                    place.Controls.Add(lbl);

                    place.Visible = true;

                    /*  bool a = place.Visible;
                      if (!a)
                      {
                          place.Visible = true;
                      }*/
                }

                //if (i + 1 == numOfPage1)
                //  i = end + 1;
            }
            if (nextflag)
            {
                LinkButton btn = new LinkButton();
                btn.CommandName = "G1Page";
                btn.CommandArgument = (end + 1).ToString();

                btn.Text = "..>";
                btn.ToolTip = "Page " + (end + 1).ToString();

                PlaceHolder place = (PlaceHolder)row1.Cells[0].FindControl(this._placeholderString);
                place.Controls.Add(btn);

                // Label lbl = new Label();
                // lbl.Text = "| ";
                // place.Controls.Add(lbl);
            }
        }

    }
    private void PagingPreProcess()
    {
        //D R PV TP SL
        //totalrow=getcount();maxnumrows;maxPageVisible;Totalpage;Slot==start,end,prev,next

        bool checknum1 = Int64.TryParse(this.ViewState["totalnumrows"].ToString(), out this._totalNumRows);
        if (!checknum1)
        {
            this._totalNumRows = Convert.ToInt32(this._totalCdrCount);//GetCount("banglatel.re_rate");
            this.ViewState["totalnumrows"] = this._totalNumRows;
        }


        int.TryParse(this.ViewState["gridactiveindex"].ToString(), out this._gridActiveIndexTemp);
        //pageRowIndex = gridactiveindex3;

        this._totalNumberOfRows = this._totalNumRows;//D
        long d = this._totalNumberOfRows;
        int r = this._maxNUmRows;
        int pv = this._maxPageVisible;

        long tpResult = d / r;
        if (d % r > 0)
        {
            this._totalpage = tpResult + 1;
        }
        else
        {
            this._totalpage = tpResult;
        }

        long slotResult = this._totalpage / this._maxPageVisible;
        if (this._totalpage % this._maxPageVisible > 0)
        {
            this._numberOfSlot = slotResult + 1;
        }
        else
        {
            this._numberOfSlot = slotResult;
        }
    }
    private DataTable GetDataSource(int startRowIndex, int maximumRows, string queryString)
    {
        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            connection.Open();

            // string strSelectCmd = "SELECT * FROM CDRListed order by fileserialnumber desc ";
            string strSelectCmd = "Call CDRLGetRows1(@RowIndex,@MaxRows,@QueryString)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;

            //cmd.CommandText = "SELECT * FROM CDRListed";
            cmd.CommandText = strSelectCmd;

            cmd.Parameters.AddWithValue("RowIndex", startRowIndex);
            cmd.Parameters.AddWithValue("MaxRows", maximumRows);
            cmd.Parameters.AddWithValue("QueryString", queryString);
            //string strSelectCmd0 = "SELECT * FROM CDRReceived order by fileserialnumber desc ";
            //MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            // MySqlDataAdapter da0 = new MySqlDataAdapter(strSelectCmd0, connection);
            //conn.Open();
            //da.Fill(dsPerson, "Person"); 

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

  
    protected void gridView_PreRender(object sender, EventArgs e)
    {
       
    }
    protected void gridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (this.DateColumnIndexes.Count > 0)
            {
                for (int i = 0; i < this.DateColumnIndexes.Count; i++)
                {
                    int columnIndex = this.DateColumnIndexes[i];
                    string strDate = DataBinder.Eval(e.Row.DataItem, this._dt.Columns[columnIndex].ColumnName).ToString();
                    DateTime tempDate = new DateTime();
                    if (DateTime.TryParse(strDate, out tempDate) == true)
                    {
                        e.Row.Cells[columnIndex].Text = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }
        }
    }

    protected void gridView_DataBound(object sender, EventArgs e)
    {
        
    }

    protected void gridViewDx_OnCustomColumnDisplayText(object sender, ASPxGridViewColumnDisplayTextEventArgs e)
    {
        if (e.Column.GetType() == typeof(GridViewDataDateColumn))
        {
            ((GridViewDataDateColumn)e.Column).PropertiesDateEdit.DisplayFormatString = "yyyy-MM-dd HH:mm:ss";
        }
    }

    protected void gridViewDx_OnDataBound(object sender, EventArgs e)
    {
        ASPxGridView grid = sender as ASPxGridView;
        if (grid.Columns.IndexOf(grid.Columns["CommandColumn"]) != -1)
            return;
        GridViewCommandColumn col = new GridViewCommandColumn();
        col.Name = "CommandColumn";
        col.ShowEditButton = true;
        col.VisibleIndex = 0;
        grid.Columns.Insert(0, col);
    }

    protected void gridViewDx_OnDataBinding(object sender, EventArgs e)
    {
        //BindingList<partner> _data = new BindingList<partner>();
        //_data.Add(new partner()
        //{
        //    idPartner = 1,
        //    PartnerName = "First Partner"
        //});
        this._dt = (DataTable)this.Session["dtCDR"];
        this.gridViewDx.DataSource = this._dt;
    }

    protected void gridViewDx_OnCellEditorInitialize(object sender, ASPxGridViewEditorEventArgs e)
    {
        if (e.Column.GetType() == typeof(GridViewDataDateColumn))
        {
            (e.Column.PropertiesEdit as DateEditProperties).DisplayFormatString = "yyyy-MM-dd HH:mm:ss";
            (e.Column.PropertiesEdit as DateEditProperties).EditFormatString = "yyyy-MM-dd HH:mm:ss";
            (e.Column.PropertiesEdit as DateEditProperties).TimeSectionProperties.Visible = true;
        }
        else
        {
            switch (e.Column.FieldName)
            {
                case "StartTime":
                case "IdCall":
                    e.Editor.ReadOnly = true;
                    break;;
            }
        }
    }

    protected void gridViewDx_OnRowUpdating(object sender, ASPxDataUpdatingEventArgs e)
    {
        string sourceTable = this.DropDownListSource.SelectedIndex == 0 ? "cdr " : "cdrerror ";
        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
            connection.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;

            string strUpdateCmd = "update " + sourceTable;
            strUpdateCmd += " where IdCall = @IdCall and StartTime = @StartTime";
            cmd.CommandText = strUpdateCmd;
            //cmd.ExecuteNonQuery();
        }
    }
}
