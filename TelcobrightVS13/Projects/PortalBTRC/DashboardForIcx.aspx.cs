using TelcobrightMediation;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.UI;
using reports;
using System.Collections.Generic;
using MediationModel;
using PortalApp;
using TelcobrightInfra;
using System.Data;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

public partial class DashboardAspxForIcx : Page
{
    private static int currentPageForJobGrid = 0;
    private static int currentIndexForJobGrid = 20;
    private int offset = currentPageForJobGrid * currentIndexForJobGrid;
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
        //telcobrightpartner thisPartner = null;
        //string binpath = System.Web.HttpRuntime.BinDirectory;
        //TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
        //var databaseSetting = telcobrightConfig.DatabaseSetting;
        //string userName = Page.User.Identity.Name;
        //string dbName;
        //if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
        //{
        //    dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
        //}
        //else
        //{
        //    dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
        //}
        //databaseSetting.DatabaseName = dbName;

        //using (PartnerEntities conTelco = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
        //{
        //    thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbName).ToList().First();

        //}
        //this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
        ////this.lblCustomerDisplayName.Text = "CDR Analyzer System (CAS)";
        ////databaseSetting.DatabaseName = this.targetIcxName;

        //string connectionString = DbUtil.getDbConStrWithDatabase(databaseSetting);

        //string sqlCommand = "select id, JobName, CreationTime, CompletionTime " +
        //                               "from job where idjobdefinition = 1 " +
        //                               "and status = 1 " +
        //                               "order by completiontime desc limit 0,20;";

        //List<GridViewCompletedJob> gridViewCompletedJob = new List<GridViewCompletedJob>();


        //using (MySqlConnection connection = new MySqlConnection())
        //{
        //    connection.ConnectionString = connectionString;
        //    connection.Open();
        //    DataSet dataSet = gridViewCompletedData(connection, sqlCommand);
        //    bool hasData = dataSet.Tables.Cast<DataTable>()
        //                   .Any(table => table.Rows.Count != 0);
        //    if (hasData == true)
        //    {
        //        gridViewCompletedJob = ConvertDataSetToList(dataSet);
        //        this.GridViewCompleted.DataSource = gridViewCompletedJob;
        //        this.GridViewCompleted.DataBind();

        //    }
        //}


        //dashboard items
        UpdateErrorCalls();

        //UpdateInternationalIncoming();
        //this.Timer1.Enabled = true;
        //this.Timer2.Enabled = true;
        //this.Timer3.Enabled = true;

        //GridViewCompleted.PageIndex = 0;
        //GridViewCompleted.PageSize = 10;
        // Set the initial page index to 0 and page size to 20
        BindGridView();
        BindGridViewForMissingTg();
        BindGridViewForTg();
        //getpartners();


        // Bind the GridView


        if (!IsPostBack)//initial
        {


        }

    }

    //private static void getpartners()
    //{
    //    using (PartnerEntities contex = PortalConnectionHelper.GetPartnerEntitiesDynamic(tbc.DatabaseSetting))
    //    {
    //        var IOSList = contex.partners.Where(c => c.PartnerType == 2).ToList();

    //        DropDownList1.Items.Clear();
    //        DropDownListPartner.Items.Add(new ListItem(" [All]", "-1"));
    //        foreach (partner p in IOSList.OrderBy(x => x.PartnerName))
    //        {
    //            DropDownListPartner.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
    //        }
    //        var ANSList = contex.partners.Where(c => c.PartnerType == 1).ToList();
    //        DropDownListAns.Items.Clear();
    //        DropDownListAns.Items.Add(new ListItem("[All]", "-1"));
    //        foreach (partner p in ANSList.OrderBy(x => x.PartnerName))
    //        {
    //            DropDownListAns.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
    //        }

    //        var IGWList = contex.partners.Where(c => c.PartnerType == 2).ToList();
    //        DropDownListIgw.Items.Clear();
    //        DropDownListIgw.Items.Add(new ListItem("[All]", "-1"));
    //        foreach (partner p in IGWList.OrderBy(x => x.PartnerName))
    //        {
    //            DropDownListIgw.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
    //        }

    //        List<ne> nes = contex.nes.ToList();
    //        DropDownListShowBySwitch.Items.Clear();
    //        DropDownListShowBySwitch.Items.Add(new ListItem(" [All]", "-1"));
    //        foreach (ne ns in nes.OrderBy(x => x.SwitchName))
    //        {
    //            DropDownListShowBySwitch.Items.Add(new ListItem(ns.SwitchName, ns.idSwitch.ToString()));
    //        }

    //    }

    //    DropDownListPartner_OnSelectedIndexChanged(DropDownListPartner, EventArgs.Empty);
    //    DropDownListIgw_OnSelectedIndexChanged(DropDownListIgw, EventArgs.Empty);
    //}

    //humayun

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
        if (offset == 0 || offset<0)
        {
            PreviousButton.Enabled = false;  
        }
        else
        {
            PreviousButton.Enabled = true;
        }
        string connectionString = DbUtil.getDbConStrWithDatabase(databaseSetting);

        List<GridViewCompletedJob> gridViewCompletedJob = new List<GridViewCompletedJob>();

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

            if (hasData == true)
            {
                gridViewCompletedJob = ConvertDataSetToList(dataSet);
                GridViewCompleted.DataSource = gridViewCompletedJob;
                GridViewCompleted.DataBind();
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
        string sqlCommand = $@"SELECT DISTINCT
                              CASE
                                WHEN InPartnerId = 0 THEN IncomingRoute
                                WHEN OutPartnerId = 0 THEN OutgoingRoute
                                ELSE 'others'
                              END AS TgName, SwitchId, ne.SwitchName as SwitchName
                            FROM
                              (SELECT * FROM cdrerror LIMIT 10000) AS x
                              LEFT JOIN ne 
                              ON x.switchid = ne.idSwitch
                            WHERE InPartnerId = 0 OR OutPartnerId = 0;";

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
        }
    }
    private void BindGridViewForTg()
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

        List<GridViewTg> gridViewTg = new List<GridViewTg>();

        // Modify your SQL query to include the OFFSET and FETCH NEXT clauses
        string sqlCommand = $@"select idroute as id,RouteName as TgName,ne.SwitchName as SwitchName ,zone as Zone,partner.PartnerName as Partner from route
                                left join ne
                                on ne.idSwitch = route.SwitchId
                                left join partner
                                on partner.idPartner= route.idPartner;";

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = connectionString;
            connection.Open();
            DataSet dataSet = gridViewCompletedData(connection, sqlCommand);

            bool hasData = dataSet.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);

            if (hasData == true)
            {
                gridViewTg = ConvertDataSetToListForTg(dataSet);
                ListViewTgs.DataSource = gridViewTg;
                ListViewTgs.DataBind();
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

        currentPageForJobGrid--;
        //currentIndexForJobGrid--;
        BindGridView();
        //if (GridViewCompleted.PageIndex > 0)
        //{
        //    GridViewCompleted.PageIndex--;
        //    BindGridView();
        //}
    }

    protected void NextButton_Click(object sender, EventArgs e)
    {
        // Go to the next page
        currentPageForJobGrid++;
        //currentIndexForJobGrid++;
        BindGridView();
        //if (GridViewCompleted.PageIndex < GridViewCompleted.PageCount - 1)
        //{
        //    GridViewCompleted.PageIndex++;
        //    BindGridView();
        //}
    }

    private void UpdateErrorCalls()
    {
        List<DashBoard.ErrorCalls> ec = new List<DashBoard.ErrorCalls>();
        using (PartnerEntities conPartner = PortalConnectionHelper.GetPartnerEntitiesDynamic(telcobrightConfig.DatabaseSetting))
        {
            ec = conPartner.Database.SqlQuery<DashBoard.ErrorCalls>(@"select ErrorCode as ErrorReason, count(*) as NumberOfCalls
                                                from cdrerror group by ErrorCode limit 0,1001").ToList();
            long totalNumberOfCalls = ec.Select(c => c.NumberOfCalls).Sum();

            if (totalNumberOfCalls > 1000)
            {
                this.HyperLinkError.Text = "1000+ Calls in Error";
            }
            else
            {
                this.HyperLinkError.Text = totalNumberOfCalls + " Calls in Error";
            }
            this.GridViewError.DataSource = ec;
            this.GridViewError.DataBind();

        }
    }
    protected void Timer1_Tick(object sender, EventArgs e)
    {
        UpdateErrorCalls();
        

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


    protected void DropDownListOfPartners(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected void DropDownListOfZone(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}