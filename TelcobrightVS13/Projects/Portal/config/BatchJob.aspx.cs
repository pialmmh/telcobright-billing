using TelcobrightMediation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using MediationModel;
using System.Data.Entity;
using System.IO;
using PortalApp;
using TelcobrightMediation.Config;

public partial class ConfigBatcJob : System.Web.UI.Page
{

    public List<int> DateColumnIndexes = new List<int>();//for date formatting in row-databound

    string _totalCdrCount = "";
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
        PopulateRoute(switchId, -1, 0);
        PopulateRoute(switchId, -1, 1);
    }
    protected void DropDownListInPartner_SelectedIndexChanged(object sender, EventArgs e)
    {
        int switchId = Convert.ToInt32(this.DropDownListSwitch.SelectedValue);
        int idPartner = Convert.ToInt32(this.DropDownListInPartner.SelectedValue);
        PopulateRoute(switchId, idPartner, 0);
    }
    protected void DropDownListOutPartner_SelectedIndexChanged(object sender, EventArgs e)
    {
        int switchId = Convert.ToInt32(this.DropDownListSwitch.SelectedValue);
        int idPartner = Convert.ToInt32(this.DropDownListOutPartner.SelectedValue);
        PopulateRoute(switchId, idPartner, 1);
    }
    protected class ErrorSummary
    {
        public string Reason { get; set; }
        public long Count { get; set; }
    }

    protected void DropDownListSource_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.DropDownListSource.SelectedIndex == 0)//cdr
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
                                                                (select field4 as Reason,count(*) as Count
                                                                from cdrerror group by field4) x
                                                                order by Count desc,Reason").ToList();
                foreach (ErrorSummary es in lstEs)
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
            Label lblScreenTitle = (Label)this.Master.FindControl("lblScreenTitle");
            lblScreenTitle.Text = "Config/Create Batch Job";

            this.TextBoxYear.Text = DateTime.Now.Year.ToString();
            this.TextBoxYear1.Text = DateTime.Now.Year.ToString();

            this.txtDate.Text = (DateTime.Today.AddDays(-2)).ToString("yyyy-MM-dd HH:mm:ss");
            this.txtDate1.Text = ((DateTime.Today).AddDays(1).AddSeconds(-1)).ToString("yyyy-MM-dd HH:mm:ss");

            //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
            //from Partner

            string thisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

            MySqlConnection connection = new MySqlConnection(thisConectionString);
            string database = connection.Database.ToString();
            List<ne> lstSwitch;
            TelcobrightConfig tbc = PageUtil.GetTelcobrightConfig();
            using (PartnerEntities context = new PartnerEntities())
            {
                //populate jobtype
                List<enumjobdefinition> lstJobDef = context.enumjobdefinitions.Where(c => c.BatchCreatable == 1)
                    .Include(c => c.enumjobtype).ToList();
                this.DropDownListJobType.Items.Clear();
                foreach (enumjobdefinition jobdef in lstJobDef)
                {
                    this.DropDownListJobType.Items.Add(new ListItem(jobdef.Type,
                        jobdef.id.ToString()));
                }
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

                ServiceGroupComposer sgComposer = new ServiceGroupComposer();
                string mefPath = PageUtil.GetPortalBinPath() + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar
                    + "Extensions";
                sgComposer.ComposeFromPath(mefPath);
                Dictionary<string, string>
                    dicConfiguredServiceGroups = sgComposer.ServiceGroups.ToDictionary(c => c.Id.ToString(),
                        c => c.RuleName);
                //populate service group
                this.DropDownListServiceGroup.Items.Clear();
                this.DropDownListServiceGroup.Items.Add(new ListItem(" [All]", "-1"));
                foreach (KeyValuePair<string, string> kv in dicConfiguredServiceGroups)
                {
                    this.DropDownListServiceGroup.Items.Add(new ListItem(kv.Value, kv.Key));
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
            var fieldTemplates = new List<CdrFieldTemplate>();
            var jArray = (JArray)tbc.PortalSettings.DicConfigObjects["CdrFieldTemplate"];
            foreach (JObject obj in jArray)
            {
                CdrFieldTemplate cf = obj.ToObject<CdrFieldTemplate>();
                fieldTemplates.Add(cf);
                this.DropDownListFieldList.Items.Add(new ListItem(cf.FieldTemplateName, cf.FieldTemplateName));
            }
            this.Session["cdrfieldtemplate"] = fieldTemplates;


        }//if !postback
    }

    void PopulatePartner(int switchId, int inOrOut)
    {
        DropDownList ddlPartner = null;
        switch (inOrOut)
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
            if (switchId <= 0)
            {
                partnersWithRoutes = context.routes.GroupBy(test => test.idPartner)
                   .Select(grp => grp.FirstOrDefault())
                   .ToList().Select(c => c.idPartner).ToList();
            }
            else
            {
                partnersWithRoutes = context.routes.Where(r => r.SwitchId == switchId).GroupBy(test => test.idPartner)
                   .Select(grp => grp.FirstOrDefault())
                   .ToList().Select(c => c.idPartner).ToList();
            }

            List<partner> lstPartners = context.partners.Where(p => partnersWithRoutes.Contains(
                p.idPartner
                )).OrderBy(c => c.PartnerName).ToList();
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








    protected void ButtonCreateJob_Click(object sender, EventArgs e)
    {
        try
        {
            //validation
            string jobName = this.TextBoxJobName.Text;
            if (jobName == "")
            {
                this.lblStatus.Text = " Job name cannot be empty!";
                return;
            }
            //duplicate job name...
            string s = ConfigurationManager.ConnectionStrings["partnerEntities"].ConnectionString
                .Replace("database=telcobrightmediation", "database=telcobrightbatch");
            using (PartnerEntities contextbatch = new PartnerEntities())
            {
                if (contextbatch.jobs.Any(c => c.JobName.StartsWith(jobName)))
                {
                    this.lblStatus.Text = "Duplicate job name!";
                    return;
                }
            }

            int batchSize = 0;
            int.TryParse(this.TextBoxBatchSize.Text, out batchSize);
            if (batchSize <= 0)
            {
                this.lblStatus.Text = "Invalid batch size";
                return;
            }

            string sourceTable = "";
            int jobType = -1;
            int.TryParse(this.DropDownListJobType.SelectedValue, out jobType);

            if (jobType <= 0)
            {
                this.lblStatus.Text = "Invalid job type!";
                return;
            }
            TelcobrightConfig tbc = PageUtil.GetTelcobrightConfig();
            string operatorName = tbc.DatabaseSetting.DatabaseName;
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
            List<ne> lstSwitch=new List<ne>();
            Dictionary<string, route> dicRoutes = new Dictionary<string, route>();
            using (PartnerEntities context = new PartnerEntities(entityConStr))
            {
                switch (this.DropDownListJobType.SelectedValue)
                {
                    case "2": //Error Process
                        sourceTable = "cdrerror";
                        break;
                    case "3": //re process
                    case "4": //cdr Eraser
                        if (tbc.CdrSetting.PartialCdrEnabledNeIds.Any())
                        {
                            this.lblStatus.Text =
                                " Cdr erasing is not supported for NEs with partial cdr configuration!";
                            return;
                        }
                        sourceTable = "cdr";
                        break;
                }

                //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
                //from Partner

                string thisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

                MySqlConnection connection = new MySqlConnection(thisConectionString);
                string database = connection.Database.ToString();
                dicRoutes = context.routes.ToDictionary(c => c.idroute.ToString());
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
            }

            List<SqlSingleWhereClauseBuilder> lstWhereParamsSingle = new List<SqlSingleWhereClauseBuilder>();
            List<SqlMultiWhereClauseBuilder> lstWhereParamsMultiple = new List<SqlMultiWhereClauseBuilder>();
            SqlSingleWhereClauseBuilder newParam = null;
            DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
            DateTime denddate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
            DateTime comparedate = dstartdate;
            string startdate = this.txtDate.Text;
            string enddate = this.txtDate1.Text;
            if (startdate.Length == 10)
            {
                DateTime.TryParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out dstartdate);
            }
            else if (startdate.Length > 10)
            {
                DateTime.TryParseExact(startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dstartdate);
            }

            if (enddate.Length == 10)
            {
                DateTime.TryParseExact(enddate + " 23:59:59", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out denddate);
            }
            else if (enddate.Length > 10)
            {
                DateTime.TryParseExact(enddate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out denddate);
            }

            if (dstartdate > comparedate && denddate > comparedate)
            {
                startdate = dstartdate.ToString("yyyy-MM-dd HH:mm:ss");
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.FirstBeforeAndOr);
                newParam.Expression = "starttime>=";
                newParam.ParamType = SqlWhereParamType.Datetime;
                newParam.ParamValue = startdate;
                lstWhereParamsSingle.Add(newParam);

                enddate = denddate.ToString("yyyy-MM-dd HH:mm:ss");
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "starttime<=";
                newParam.ParamType = SqlWhereParamType.Datetime;
                newParam.ParamValue = enddate;
                lstWhereParamsSingle.Add(newParam);
            }
            else
            {
                this.lblStatus.Text = "Invalid Date!";
                return;
            }

            int serviceGroup = this.DropDownListServiceGroup.SelectedIndex == 0
                ? 0
                : Convert.ToInt32(this.DropDownListServiceGroup.SelectedValue);
            if (serviceGroup > 0)
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "ServiceGroup=";
                newParam.ParamType = SqlWhereParamType.Numeric;
                newParam.ParamValue = serviceGroup.ToString();
                lstWhereParamsSingle.Add(newParam);
            }


            int? chargingStatus = this.DropDownListChargingStatus.SelectedIndex == 0
                ? 0
                : Convert.ToInt32(this.DropDownListChargingStatus.SelectedValue);
            if (chargingStatus > 0)
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "chargingstatus=";
                newParam.ParamType = SqlWhereParamType.Numeric;
                newParam.ParamValue = chargingStatus.ToString();
                lstWhereParamsSingle.Add(newParam);
            }

            int idSwitch = this.DropDownListSwitch.SelectedIndex == 0
                ? 0
                : Convert.ToInt32(this.DropDownListSwitch.SelectedValue);
            if (idSwitch > 0)
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "switchid=";
                newParam.ParamType = SqlWhereParamType.Numeric;
                newParam.ParamValue = idSwitch.ToString();
                lstWhereParamsSingle.Add(newParam);
                //Sql += " and switchid=" + idSwitch.ToString();
            }

            int customerId = this.DropDownListInPartner.SelectedIndex == 0
                ? 0
                : Convert.ToInt32(this.DropDownListInPartner.SelectedValue);
            if (customerId > 0)
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "customerid=";
                newParam.ParamType = SqlWhereParamType.Numeric;
                newParam.ParamValue = customerId.ToString();
                lstWhereParamsSingle.Add(newParam);
                //Sql += " and customerid=" + CustomerId;
            }

            int supplierId = this.DropDownListOutPartner.SelectedIndex == 0
                ? 0
                : Convert.ToInt32(this.DropDownListOutPartner.SelectedValue);
            if (supplierId > 0)
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "supplierid=";
                newParam.ParamType = SqlWhereParamType.Numeric;
                newParam.ParamValue = supplierId.ToString();
                lstWhereParamsSingle.Add(newParam);
                //Sql += " and supplierid=" + SupplierId;
            }

            string IncomingRoute = this.DropDownListInRoute.SelectedIndex == 0
                ? "-1"
                : this.DropDownListInRoute.SelectedItem.Value;
            if (IncomingRoute != "-1")
            {
                string routeSwitchIdIn = dicRoutes[IncomingRoute].SwitchId.ToString();
                string routeNameIn = dicRoutes[IncomingRoute].RouteName;

                SqlMultiWhereClauseBuilder multiParam = new SqlMultiWhereClauseBuilder(SqlWhereAndOrType.And);

                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.FirstBeforeAndOr);
                newParam.Expression = "switchid=";
                newParam.ParamType = SqlWhereParamType.Numeric;
                newParam.ParamValue = routeSwitchIdIn.ToString();
                multiParam.SingleParams.Add(newParam);

                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "IncomingRoute=";
                newParam.ParamType = SqlWhereParamType.Text;
                newParam.ParamValue = routeNameIn.ToString();
                multiParam.SingleParams.Add(newParam);

                lstWhereParamsMultiple.Add(multiParam);
            }

            string OutgoingRoute = this.DropDownListOutRoute.SelectedIndex == 0
                ? "-1"
                : this.DropDownListOutRoute.SelectedItem.Value;
            if (OutgoingRoute != "-1")
            {
                string routeSwitchIdOut = dicRoutes[OutgoingRoute].SwitchId.ToString();
                string routeNameOut = dicRoutes[OutgoingRoute].RouteName;

                SqlMultiWhereClauseBuilder multiParam = new SqlMultiWhereClauseBuilder(SqlWhereAndOrType.And);

                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.FirstBeforeAndOr);
                newParam.Expression = "switchid=";
                newParam.ParamType = SqlWhereParamType.Numeric;
                newParam.ParamValue = routeSwitchIdOut.ToString();
                multiParam.SingleParams.Add(newParam);

                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "OutgoingRoute=";
                newParam.ParamType = SqlWhereParamType.Text;
                newParam.ParamValue = routeNameOut.ToString();
                multiParam.SingleParams.Add(newParam);

                lstWhereParamsMultiple.Add(multiParam);
            }

            if (!string.IsNullOrEmpty(this.TextBoxIngressCalled.Text.Trim()))
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = GetNumberFilterExpression(this.ddlistIngressCalled, "originatingcallednumber",
                    this.TextBoxIngressCalled.Text.Trim()).Replace("and", "");
                newParam.ParamType = SqlWhereParamType.Null;
                newParam.ParamValue = "";
                lstWhereParamsSingle.Add(newParam);
                //Sql += GetNumberFilterExpression(ddlistIngressCalled, "originatingcallednumber", TextBoxIngressCalled.Text.Trim());
            }


            if (!string.IsNullOrEmpty(this.TextBoxIngressCalling.Text.Trim()))
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = GetNumberFilterExpression(this.ddlistIngressCalling, "originatingcallingnumber",
                    this.TextBoxIngressCalling.Text.Trim()).Replace("and", "");
                newParam.ParamType = SqlWhereParamType.Null;
                newParam.ParamValue = "";
                lstWhereParamsSingle.Add(newParam);
                //Sql += GetNumberFilterExpression(ddlistIngressCalling, "originatingcallingnumber", TextBoxIngressCalling.Text.Trim());
            }

            if (!string.IsNullOrEmpty(this.TextBoxEgressCalled.Text.Trim()))
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = GetNumberFilterExpression(this.ddlistEgressCalled, "terminatingcallednumber",
                    this.TextBoxEgressCalled.Text.Trim()).Replace("and", "");
                newParam.ParamType = SqlWhereParamType.Null;
                newParam.ParamValue = "";
                lstWhereParamsSingle.Add(newParam);
                //Sql += GetNumberFilterExpression(ddlistEgressCalled, "terminatingcallednumber", TextBoxEgressCalled.Text.Trim());
            }

            if (!string.IsNullOrEmpty(this.TextBoxEgressCalling.Text.Trim()))
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = GetNumberFilterExpression(this.ddlistEgressCalling, "terminatingcallingnumber",
                    this.TextBoxEgressCalling.Text.Trim()).Replace("and", "");
                newParam.ParamType = SqlWhereParamType.Null;
                newParam.ParamValue = "";
                lstWhereParamsSingle.Add(newParam);
                //Sql += GetNumberFilterExpression(ddlistEgressCalling, "terminatingcallingnumber", TextBoxEgressCalling.Text.Trim());
            }

            string errorReasonField4 = this.DropDownListErrorReason.SelectedIndex == 0
                ? ""
                : this.DropDownListErrorReason.SelectedValue;
            if (errorReasonField4 != "")
            {
                newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                newParam.Expression = "field4=";
                newParam.ParamType = SqlWhereParamType.Text;
                newParam.ParamValue = errorReasonField4;
                lstWhereParamsSingle.Add(newParam);
                //Sql += " and field4='" + ErrorReasonField4 + "'";
            }
            //

            BatchSqlJobParamJson baseJobParam = new BatchSqlJobParamJson
            (
                sourceTable,
                batchSize,
                lstWhereParamsSingle,
                lstWhereParamsMultiple,
                columnExpressions: new List<string>() { "IdCall as RowId", "starttime as RowDateTime" }
            );

            int jobCount = 0;
            if (idSwitch <= 0) //all switch
            {

            }
            else //one switch selected
            {
                lstSwitch = lstSwitch.Where(c => c.idSwitch == idSwitch).ToList();
            }

            string conStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
            using (PartnerEntities context = new PartnerEntities(conStr))
            {
                foreach (ne thisSwitch in lstSwitch) //create a job for each selected ne
                {
                    BatchSqlJobParamJson thisJobParam = baseJobParam.GetCopy();
                    //create a switchid param for this job
                    newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
                    newParam.Expression = "switchid=";
                    newParam.ParamType = SqlWhereParamType.Numeric;
                    newParam.ParamValue = thisSwitch.idSwitch.ToString();
                    thisJobParam.LstWhereParamsSingle.Add(newParam);
                    job newjob = new job();
                    newjob.Progress = 0;
                    newjob.idjobdefinition = jobType;
                    newjob.Status = 6; //created
                    newjob.JobName = jobName;
                    newjob.idjobdefinition = jobType;
                    newjob.CreationTime = DateTime.Now;
                    newjob.idNE = thisSwitch.idSwitch;
                    newjob.JobParameter = JsonConvert.SerializeObject(thisJobParam);
                    newjob.priority = 5;
                    context.jobs.Add(newjob);
                    context.SaveChanges();
                    jobCount++;
                } //for each switch
            }
            this.lblStatus.Text = jobCount + " job(s) created.";

        }
        catch (Exception e1)
        {
            this.lblStatus.Text = e1.Message;
        }
    }
}
