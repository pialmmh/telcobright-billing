using TelcobrightMediation;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Collections.Generic;
using MediationModel;
using PortalApp;
using TelcobrightInfra;
using System.Data;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;
using reports;
using LibraryExtensions;
using Newtonsoft.Json;
using System.IO;

public partial class DashboardAspxForIcx : Page
{
    private static int currentPageForJobGrid = 0;
    private static int currentIndexForJobGrid = 20;
    private int offset = currentPageForJobGrid * currentIndexForJobGrid;
    private string icxConnstr;
    TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();


    string targetIcxName = "btrc_cas";
    protected void Page_Load(object sender, EventArgs e)
    {
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

        //dashboard items

        //UpdateInternationalIncoming();
        this.Timer1.Enabled = true;
        //this.Timer2.Enabled = true;
        this.Timer3.Enabled = true;

        BindGridView();
        DomesticCallForPreviousSevenDays1();
        InternationalIncommimng1();
        InternationalOutgoing1();
        //InternationalOutgoing1();
        // Bind the GridView
        UpdateErrorCalls();


        if (!IsPostBack)//initial
        {
            BindGridViewForMissingTg();
            DateTime currentDate = DateTime.Now;
            DateTime sevenDaysAgo = currentDate.AddDays(-7);
            //InternationalOutgoing1(sevenDaysAgo, currentDate);

            DateTime lastDayDisplayed = sevenDaysAgo.AddDays(6);
            if (currentDate.Date == lastDayDisplayed.Date)
            {
                NextButton3.Enabled = false;
            }
        }

    }

    //humayun 
    private void DomesticCallForPreviousSevenDays1()
    {
        Series series1 = DomesticCallForPreviousSevenDays.Series["Series1"];
        DataPointCollection points = series1.Points;
        series1.IsValueShownAsLabel = true;
        series1.LabelFormat = "#,##0";
        

        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A" };

        string connectionString = icxConnstr;
        List<double> durations = new List<double>();
        List<string> durationDate = new List<string>();
        DateTime currentDate = DateTime.Now;
        DateTime sevenDaysAgo = currentDate.AddDays(-6);
        DateTime lastHourMinuteSecond = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);
        string today = lastHourMinuteSecond.ToString("yyyy-MM-dd HH:mm:ss");
        DateTime firstHourMinuteSecondSevenDaysAgo = new DateTime(sevenDaysAgo.Year, sevenDaysAgo.Month, sevenDaysAgo.Day, 0, 0, 0);
        string lastSevenDay = firstHourMinuteSecondSevenDaysAgo.ToString("yyyy-MM-dd HH:mm:ss");

        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = $@"select tup_starttime as DurationDate, sum(duration1) as Duration from sum_voice_day_01 where 
                            tup_starttime >= '{lastSevenDay}' and tup_starttime <= '{today}' group by DurationDate;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {
                    while (read.Read())
                    {
                        //durationDate.Add(read["DurationDate"].ToString().ConvertToDateTimeFromCustomFormat("MM/dd/yyyy HH:mm:ss").ToString("MMMM dd")); // Store date labels
                        DateTime date = DateTime.Parse(read["DurationDate"].ToString());
                        durationDate.Add(date.ToString("MMMM-dd"));
                        durations.Add(read.GetDouble("Duration"));
                    }
                }
            }
        }

        // Ensure that the data is retrieved as expected
        foreach (double value in durations)
        {
            Console.WriteLine("Duration: " + value);
        }

        // Set X-axis labels
        for (int i = 0; i < durationDate.Count; i++)
        {
            points.AddXY(durationDate[i], durations[i]);
        }

        // Set colors
        for (int i = 0; i < durationDate.Count; i++)
        {
            points[i].Color = ColorTranslator.FromHtml(colors[i]);
        }
        Label5.Text = $"Domestic Calls of ({firstHourMinuteSecondSevenDaysAgo.ToString("yyyy-MM-dd")} to {lastHourMinuteSecond.ToString("yyyy-MM-dd")})";

    }

    private void InternationalOutgoing1()
    {
        Series series1 = InternationalOutgoing.Series["Series1"];
        DataPointCollection points = series1.Points;
        series1.IsValueShownAsLabel = true;
        series1.LabelFormat = "#,##0";


        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A" };

        string connectionString = icxConnstr;
        List<double> durations = new List<double>();
        List<string> durationDate = new List<string>();
        DateTime currentDate = DateTime.Now;
        DateTime sevenDaysAgo = currentDate.AddDays(-6);
        DateTime lastHourMinuteSecond = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);
        string today = lastHourMinuteSecond.ToString("yyyy-MM-dd HH:mm:ss");
        DateTime firstHourMinuteSecondSevenDaysAgo = new DateTime(sevenDaysAgo.Year, sevenDaysAgo.Month, sevenDaysAgo.Day, 0, 0, 0);
        string lastSevenDay = firstHourMinuteSecondSevenDaysAgo.ToString("yyyy-MM-dd HH:mm:ss");

        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = $@"select tup_starttime as DurationDate, sum(duration3) as Duration from sum_voice_day_02 where 
                            tup_starttime >= '{lastSevenDay}' and tup_starttime <= '{today}' group by DurationDate;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {
                    while (read.Read())
                    {
                        //durationDate.Add(read["DurationDate"].ToString().ConvertToDateTimeFromCustomFormat("MM/dd/yyyy HH:mm:ss").ToString("MMMM dd")); // Store date labels
                        DateTime date = DateTime.Parse(read["DurationDate"].ToString());
                        durationDate.Add(date.ToString("MMMM-dd"));
                        durations.Add(read.GetDouble("Duration"));
                    }
                }
            }
        }

        // Ensure that the data is retrieved as expected
        foreach (double value in durations)
        {
            Console.WriteLine("Duration: " + value);
        }

        // Set X-axis labels
        for (int i = 0; i < durationDate.Count; i++)
        {
            points.AddXY(durationDate[i], durations[i]);
        }

        // Set colors
        for (int i = 0; i < durationDate.Count; i++)
        {
            points[i].Color = ColorTranslator.FromHtml(colors[i]);
        }
        Label3.Text = $"International Outgoing Calls of ({firstHourMinuteSecondSevenDaysAgo.ToString("yyyy-MM-dd")} to {lastHourMinuteSecond.ToString("yyyy-MM-dd")})";
    }

    private void InternationalIncommimng1()
    {
        Series series1 = InternationalIncommimng.Series["Series1"];
        DataPointCollection points = series1.Points;
        series1.IsValueShownAsLabel = true;
        series1.LabelFormat = "#,##0";
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A" };

        string connectionString = icxConnstr;
        List<double> duration = new List<double>();
        List<string> durationDate = new List<string>();
        DateTime currentDate = DateTime.Now;
        DateTime sevenDaysAgo = currentDate.AddDays(-6);
        DateTime lastHourMinuteSecond = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);
        string today = lastHourMinuteSecond.ToString("yyyy-MM-dd HH:mm:ss");
        DateTime firstHourMinuteSecondSevenDaysAgo = new DateTime(sevenDaysAgo.Year, sevenDaysAgo.Month, sevenDaysAgo.Day, 0, 0, 0);
        string lastSevenDay = firstHourMinuteSecondSevenDaysAgo.ToString("yyyy-MM-dd HH:mm:ss");

        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = $@"select tup_starttime as DurationDate, sum(duration1) as Duration from sum_voice_day_03 where 
                        tup_starttime >= '{lastSevenDay}' and tup_starttime <= '{today}' group by DurationDate;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {
                    while (read.Read())
                    {
                        // Format the date to "Month-Day-Year" format
                        DateTime date = DateTime.Parse(read["DurationDate"].ToString());
                        durationDate.Add(date.ToString("MMMM-dd"));

                        duration.Add(read.GetDouble("Duration"));
                    }
                }
            }
        }

        // Check if there is data
        if (duration.Count > 0)
        {
            // Ensure that the data is retrieved as expected
            foreach (double value in duration)
            {
                Console.WriteLine("Duration: " + value);
            }

            // Set X-axis labels
            for (int i = 0; i < durationDate.Count; i++)
            {
                points.AddXY(durationDate[i], duration[i]);
            }

            // Set colors
            for (int i = 0; i < durationDate.Count; i++)
            {
                points[i].Color = ColorTranslator.FromHtml(colors[i]);
            }

            // Hide the "NO DATA" label when there is data
            //NoDataLabel.Visible = false;
        }
        else
        {
            // If there is no data, show "NO DATA" label
            //NoDataLabel.Visible = true;
        }
        Label1.Text = $"International Incoming Calls of ({firstHourMinuteSecondSevenDaysAgo.ToString("yyyy-MM-dd")} to {lastHourMinuteSecond.ToString("yyyy-MM-dd")})";
    }


    //DataPoint dataPoint = new DataPoint
    //{

    //    Color = ColorTranslator.FromHtml(colors[i])
    //};
    //points.Add(dataPoint);
    private void BindGridView()
    {
        telcobrightpartner thisPartner = null;
        string binpath = System.Web.HttpRuntime.BinDirectory;
        TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
        var databaseSetting = telcobrightConfig.DatabaseSetting;
        string userName = Page.User.Identity.Name;
        string dbName;
        if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
        {
            dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
        }
        else
        {
            dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
        }
        databaseSetting.DatabaseName = dbName;

        using (PartnerEntities conTelco = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
        {
            thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbName).ToList().First();

        }
        this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
        // Calculate the offset based on the current page index and page size
        //int pageIndex = GridViewCompleted.PageIndex;
        //int pageSize = GridViewCompleted.PageSize;
        if (currentPageForJobGrid < 1)
        {
            PreviousButton.Enabled = false;
        }
        else
        {
            PreviousButton.Enabled = true;
        }

        List<GridViewCompletedJob> gridViewCompletedJob = new List<GridViewCompletedJob>();

        string connectionString = DbUtil.getDbConStrWithDatabase(databaseSetting);
        icxConnstr = connectionString;
        // Modify your SQL query to include the OFFSET and FETCH NEXT clauses
        string sqlCommand = $"SELECT id, JobName, CreationTime, CompletionTime " +
                            "FROM job " +
                            $"WHERE idjobdefinition = 1 AND status = 1 " +
                            $"ORDER BY completiontime DESC " +
                            $"LIMIT {offset}, {currentIndexForJobGrid};";

        using (MySqlConnection connection = new MySqlConnection())
{
    connection.ConnectionString = connectionString;
    connection.Open();
    DataSet dataSet = gridViewCompletedData(connection, sqlCommand);

    bool hasData = dataSet.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);

    if (hasData)
    {
        gridViewCompletedJob = ConvertDataSetToList(dataSet);
        GridViewCompleted.DataSource = gridViewCompletedJob;
        GridViewCompleted.DataBind();
        NoDataLabel.Visible = false; // Hide the "NO DATA" label
    }
    else
    {
        GridViewCompleted.DataSource = null;
        GridViewCompleted.DataBind();
        NoDataLabel.Visible = true; // Show the "NO DATA" label
    }
}

    }

    private void BindGridViewForMissingTg()
    {
        telcobrightpartner thisPartner = null;
        string binpath = System.Web.HttpRuntime.BinDirectory;
        TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
        var databaseSetting = telcobrightConfig.DatabaseSetting;
        string userName = Page.User.Identity.Name;
        string dbName;
        if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
        {
            dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
        }
        else
        {
            dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
        }
        databaseSetting.DatabaseName = dbName;

        using (PartnerEntities conTelco = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
        {
            thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbName).ToList().First();

        }
        this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
        // Calculate the offset based on the current page index and page size
        //int pageIndex = GridViewCompleted.PageIndex;
        //int pageSize = GridViewCompleted.PageSize;

        string connectionString = DbUtil.getDbConStrWithDatabase(databaseSetting);

        List<GridViewMissingTg> gridViewMissingTg = new List<GridViewMissingTg>();

        // Modify your SQL query to include the OFFSET and FETCH NEXT clauses
        string sqlCommand = $@"SELECT TGNAME, SWITCHID,SWITCHNAME,CONCAT(SWITCHID,'-',TGNAME) as TG_WITH_SWITCH FROM
                            (SELECT DISTINCT
                              CASE
                                WHEN InPartnerId = 0 THEN IncomingRoute
                                WHEN OutPartnerId = 0 THEN OutgoingRoute
                                ELSE 'others'
                              END AS TgName, SwitchId, ne.SwitchName as SwitchName
                            FROM
                              (SELECT * FROM cdrerror LIMIT 10000) AS x
                              LEFT JOIN ne 
                              ON x.switchid = ne.idSwitch
                            WHERE InPartnerId = 0 OR OutPartnerId = 0) y
                            where  (TGNAME,SWITCHID) NOT IN (SELECT ROUTENAME,SwitchId FROM ROUTE);";

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = connectionString;
            connection.Open();
            DataSet dataSet = gridViewCompletedData(connection, sqlCommand);

            bool hasData = dataSet.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);

            if (hasData == true)
            {
                gridViewMissingTg = ConvertDataSetToListForMissingTg(dataSet);
                GridView11.DataSource = gridViewMissingTg;
                GridView11.DataBind();
            }
            else
            {
                Msg.Enabled = true;
                Msg.Text = "** No Missing TG";
            }
        }
    }
    

    protected void GridViewCompleted_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        // Set the new page index
        GridViewCompleted.PageIndex = e.NewPageIndex;
        BindGridView();

    }

    protected void PreviousButton_Click(object sender, EventArgs e)
    {
        // Go to the previous page
        if (currentPageForJobGrid > 0)
        {
            currentPageForJobGrid--;
            BindGridView();
        }

        DateTime currentDate = DateTime.Now;
        DateTime sevenDaysAgo = currentDate.AddDays(-6);

        // Update the date range for the query
        DateTime newStartDate = sevenDaysAgo.AddDays(0);
        DateTime newEndDate = sevenDaysAgo.AddDays(6);

        // Update the method call with the new date range
        //InternationalOutgoing1(newStartDate, newEndDate);
    }

    protected void NextButton_Click(object sender, EventArgs e)
    {
        // Go to the next page
        currentPageForJobGrid++;
        BindGridView();
        //if (GridViewCompleted.PageIndex < GridViewCompleted.PageCount - 1)
        //{
        //    GridViewCompleted.PageIndex++;
        //    BindGridView();
        //}
        DateTime currentDate = DateTime.Now;
        DateTime sevenDaysAgo = currentDate.AddDays(-6);

        // Update the date range for the query
        DateTime newStartDate = sevenDaysAgo.AddDays(-7);
        DateTime newEndDate = sevenDaysAgo;

        // Update the method call with the new date range
        //InternationalOutgoing1(newStartDate, newEndDate);

    }

    protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Find the DropDownList control in the current row
            DropDownList ddl = (DropDownList)e.Row.FindControl("DropDownListPartner");

            // Check if the DropDownList control is found
            if (ddl != null)
            {
                using (PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(telcobrightConfig.DatabaseSetting))
                {
                    List<partner> iosList = context.partners.ToList();

                    // Clear the DropDownList and add the "Select" option
                    ddl.Items.Clear();
                    ddl.Items.Add(new ListItem("Select", "Select"));

                    foreach (partner p in iosList.OrderBy(x => x.PartnerName))
                    {
                        ddl.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                    }
                }
            }
        }
    }





    private void UpdateErrorCalls()
    {
        List<DashBoard.ErrorCalls> errorCalls = new List<DashBoard.ErrorCalls>();
        string sql = @"SELECT
                        CASE
                            WHEN ErrorCode = 'Mediation error: InPartnerId must be > 0' THEN 'Mediation error: Incoming Route/TG Missing'
                            WHEN ErrorCode = 'Mediation error: OutPartnerId must be > 0' THEN 'Mediation error: Outgoing Route/TG Missing'
                            WHEN ErrorCode = 'Mediation error: ServiceGroup must be > 0' THEN 'Mediation error: Outgoing Route/TG Missing'
                            else ErrorCode
                        END AS ErrorReason,
                        COUNT(*) AS NumberOfCalls
                        FROM cdrerror
                        GROUP BY ErrorReason
                        LIMIT 0,1001;";

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = icxConnstr;
            connection.Open();
            DataSet dataSet = gridViewCompletedData(connection, sql);
            
            bool hasData = dataSet.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);

            if (hasData == true)
            {

                errorCalls = ConvertDataSetToListForErrorCalls(dataSet);
                long totalNumberOfCalls = errorCalls.Select(c => c.NumberOfCalls).Sum();
                if (totalNumberOfCalls > 1000)
                {
                    this.HyperLinkError.Text = "1000+ Calls in Error";
                }
                else
                {
                    this.HyperLinkError.Text = totalNumberOfCalls + " Calls in Error";
                }
                GridViewError.DataSource = errorCalls;
                GridViewError.DataBind();
            }
        }
    }
    protected void Timer1_Tick(object sender, EventArgs e)
    {
        UpdateErrorCalls();
        DomesticCallForPreviousSevenDays1();

    }

    protected void Timer2_Tick(object sender, EventArgs e)
    {
        this.GridViewCompleted.DataBind();
    }

    protected void Timer3_Tick(object sender, EventArgs e)
    {
        UpdateInternationalIncoming();

    }

    protected class IntlIn
    {
        public string PartnerName { get; set; }
        public double Minutes { get; set; }
    }

    protected class GridViewCompletedJob
    {
        public int id { get; set; }
        public string jobName { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime completionTime { get; set; }
    }



    protected class GridViewMissingTg
    {
        public int Id { get; set; }
        public string TgName { get; set; }
        public int switchId { get; set; }
        public string switchName { get; set; }
    }

    protected class GridViewTg
    {
        public int Id { get; set; }
        public string TgName { get; set; }
        public string switchName { get; set; }
        public string zone { get; set; }
        public string partner { get; set; }
    }
    private void UpdateInternationalIncoming()
    {
        return;
        List<IntlIn> intlIn = new List<IntlIn>();
        using (PartnerEntities context = new PartnerEntities())
        {
            intlIn = context.Database.SqlQuery<IntlIn>(
                " select p.PartnerName,round(Minutes,2) as Minutes from " +
                " (select customerid, sum(roundedduration) / 60 Minutes " +
                " from cdrsummary " +
                " where starttime > '" + DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                " and starttime < '" + (DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                " group by customerid " +
                " ) x " +
                " left join partner p " +
                " on x.customerid = p.idpartner " +
                " order by Minutes desc ").ToList();
        }
        this.HyperLinkIntlIn.Text = intlIn.Sum(c => c.Minutes).ToString() + " Minutes Today";
        this.GridViewIntlin.DataSource = intlIn;
        this.GridViewIntlin.DataBind();
    }



    DataSet gridViewCompletedData(MySqlConnection connection, string sql)
    {

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Connection = connection;
        MySqlDataAdapter domDataAdapter = new MySqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        domDataAdapter.Fill(ds);
        return ds;
    }

    List<GridViewCompletedJob> ConvertDataSetToList(DataSet ds)
    {
        bool hasRecords = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);
        List<GridViewCompletedJob> records = new List<GridViewCompletedJob>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    GridViewCompletedJob record = new GridViewCompletedJob();
                    //record.id = int.Parse(row.ItemArray[0].ToString());
                    //record.jobName = row.ItemArray[1].ToString();
                    //record.creationTime = (DateTime) row.ItemArray[2];
                    //record.completionTime = (DateTime)row.ItemArray[3];

                    record.id = Convert.ToInt32(row["Id"]);
                    record.jobName = row["JobName"].ToString();
                    record.creationTime = Convert.ToDateTime(row["CreationTime"]);
                    record.completionTime = Convert.ToDateTime(row["CompletionTime"]);

                    records.Add(record);
                }
            }
        }
        return records;
    }

    List<GridViewMissingTg> ConvertDataSetToListForMissingTg(DataSet ds)
    {
        bool hasRecords = ds.Tables.Cast<DataTable>()
            .Any(table => table.Rows.Count != 0);
        List<GridViewMissingTg> records = new List<GridViewMissingTg>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    GridViewMissingTg record = new GridViewMissingTg();
                    record.TgName = row["TgName"].ToString();
                    record.switchName = row["SwitchName"].ToString();
                    records.Add(record);
                }
            }
        }
        return records;
    }

    List<GridViewTg> ConvertDataSetToListForTg(DataSet ds)
    {
        bool hasRecords = ds.Tables.Cast<DataTable>()
            .Any(table => table.Rows.Count != 0);
        List<GridViewTg> records = new List<GridViewTg>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    GridViewTg record = new GridViewTg();
                    record.TgName = row["TgName"].ToString();
                    record.switchName = row["SwitchName"].ToString();
                    record.zone = row["Zone"].ToString();
                    record.partner = row["Partner"].ToString();
                    records.Add(record);
                }
            }
        }
        return records;
    }
    List<DashBoard.ErrorCalls> ConvertDataSetToListForErrorCalls(DataSet ds)
    {
        bool hasRecords = ds.Tables.Cast<DataTable>()
            .Any(table => table.Rows.Count != 0);
        List<DashBoard.ErrorCalls> records = new List<DashBoard.ErrorCalls>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    DashBoard.ErrorCalls record = new DashBoard.ErrorCalls();
                    record.ErrorReason = row["ErrorReason"].ToString();
                    record.NumberOfCalls = Convert.ToInt32(row["NumberOfCalls"]);
                    records.Add(record);
                }
            }
        }
        return records;
    }
    protected void DropDownListPartner_OnSelectedIndexChanged(object sender, EventArgs e)
    {


    }


    protected void DropDownListOfZone_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    protected void TgAssignment(object sender, EventArgs e)
    {
        string connectionString = icxConnstr;
        {
            LinkButton btn = (LinkButton)sender;
            GridViewRow row = (GridViewRow)btn.NamingContainer;

            if (row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlZone = (DropDownList)row.FindControl("DropDownList1");
                DropDownList ddlPartner = (DropDownList)row.FindControl("DropDownListPartner");

                if (ddlZone.SelectedValue == "Select" || ddlPartner.SelectedValue == "Select")
                {
                    Msg.Enabled = true;
                    Msg.Text = "Please Select Valid Zone or Partner";
                    return;
                }

                string tgName = row.Cells[1].Text;

                string selectedZone = ddlZone.SelectedValue;
                string selectedPartner = ddlPartner.SelectedValue;

                // Perform a database lookup to get the SwitchId based on the selected SwitchName
                string switchName = row.Cells[2].Text; 

                // You need to implement a method to retrieve the SwitchId based on the switchName
                int switchId = LookupSwitchId(switchName);

                string insertQuery = "INSERT INTO route (RouteName, SwitchId, zone,idPartner) VALUES (@RouteName, @SwitchId, @Zone,@PartnerId)";

                try
                {
                    string userName = Page.User.Identity.Name;
                    string dbName;
                    ConfigPathHelper configPathHelper = new ConfigPathHelper(
                                                    "WS_Topshelf_Quartz",
                                                    "portalBTRC",
                                                    "UtilInstallConfig",
                                                    "generators", "");
                    string jsonPath = configPathHelper.GetPortalBtrcBinPath() + @"\text.json";

                    string jsonString = File.ReadAllText(jsonPath);

                    // Deserialize the JSON string into a Dictionary<string, string>
                    Dictionary<string, string> dbVSHostname = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                    if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
                    {
                        dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
                    }
                    else
                    {
                        dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
                    }

                    var databaseSetting = telcobrightConfig.DatabaseSetting;
                    databaseSetting.DatabaseName = dbName;
                    databaseSetting.ServerName = dbVSHostname[dbName];
                    
                    string conString = DbUtil.getDbConStrWithDatabase(databaseSetting);


                    using (MySqlConnection conn = new MySqlConnection(conString))
                    {
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@RouteName", tgName);
                            cmd.Parameters.AddWithValue("@SwitchId", switchId);
                            cmd.Parameters.AddWithValue("@Zone", selectedZone);
                            cmd.Parameters.AddWithValue("@PartnerId", selectedPartner);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    Msg.Enabled = true;
                    Msg.Text = "Tg/Route Name Assign Successfully";
                    Msg.ForeColor = Color.Green;
                    BindGridViewForMissingTg();
                }
                catch (Exception ex)
                {
                    Msg.Enabled = true;
                    Msg.Text = $"{tgName} - Already Exists in Route Table";
                    Msg.ForeColor = Color.Red;
                    return;
                }
            }
        }
    }


    private int LookupSwitchId(string switchName)
    {
        int switchId = 0;
        string sql = $"select idSwitch from ne where SwitchName= @SwitchName;";

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = icxConnstr;
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@SwitchName", switchName);


            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    // Read the switchId from the query result
                    switchId = reader.GetInt32(0);
                }
            }
            return switchId;
        }
    }

}