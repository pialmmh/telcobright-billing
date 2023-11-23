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
using System.Web.UI.WebControls;

public partial class JobProcessingStatusICX : Page
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

        BindGridViewForJobProcessingStatus();
        BindGridViewErrorStatus();

        if (!IsPostBack)//initial
        {
        }

    }

    //humayun

    
    private void BindGridViewForJobProcessingStatus()
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
        //this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
        // Calculate the offset based on the current page index and page size
        //int pageIndex = GridViewCompleted.PageIndex;
        //int pageSize = GridViewCompleted.PageSize;

        string connectionString = DbUtil.getDbConStrWithDatabase(databaseSetting);

        List<GridVieJobProcessingStatus> gridViewJobProcessingStatus = new List<GridVieJobProcessingStatus>();

        // Modify your SQL query to include the OFFSET and FETCH NEXT clauses
        string sqlCommand = $@"(SELECT (SELECT 'Agni') AS 'ICX NAME',SwitchName AS 'Switch Name',JobName AS 'Job Name',CompletionTime AS 'Completion Time',NoOfSteps AS 'No Of Records'
                        FROM agni_cas.job c LEFT JOIN agni_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1 ORDER BY CompletionTime DESC LIMIT 0 , 1) 
                        UNION ALL 
                        (SELECT (SELECT 'BanglaICX') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM banglaicx_cas.job c LEFT JOIN banglaicx_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1 ORDER BY CompletionTime DESC LIMIT 0 , 1) 
                        UNION ALL 
                        (SELECT (SELECT 'BanglaTelecom') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM banglatelecom_cas.job c LEFT JOIN banglatelecom_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1 ORDER BY CompletionTime DESC LIMIT 0 , 1) 
                        UNION ALL 
                        (SELECT  (SELECT 'Bantel') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM bantel_cas.job c LEFT JOIN bantel_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1 ORDER BY CompletionTime DESC LIMIT 0 , 1) 
                        UNION ALL 
                        (SELECT  (SELECT 'BTCL') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM btcl_cas.job c LEFT JOIN btcl_cas.ne ON c.idNE = ne.idSwitch WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC LIMIT 0 , 1) 
                        UNION ALL 
                        (SELECT (SELECT 'CrossWorld') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM crossworld_cas.job c LEFT JOIN crossworld_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC LIMIT 0 , 1) 
                        UNION ALL 
                        (SELECT  (SELECT 'GaziNetworks') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM gazinetworks_cas.job c LEFT JOIN gazinetworks_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) 
                        UNION ALL 
                        (SELECT  (SELECT 'ImamNetwork') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM imamnetwork_cas.job c LEFT JOIN imamnetwork_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'Jibondhara') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time',
                        FROM jibondhara_cas.job c LEFT JOIN jibondhara_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'MNH') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM mnh_cas.job c LEFT JOIN mnh_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'MotherTelecom') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM mothertelecom_cas.job c LEFT JOIN mothertelecom_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'NewGeneration') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM newgenerationtelecom_cas.job c LEFT JOIN newgenerationtelecom_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'Paradise') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM paradise_cas.job c LEFT JOIN paradise_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'Purple') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM purple_cas.job c LEFT JOIN purple_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'RingTech') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM ringtech_cas.job c LEFT JOIN ringtech_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT 
                            (SELECT 'Sheba') AS 'ICX NAME',
                            SwitchName AS 'Switch Name',
                            JobName AS 'Job Name',
                            CompletionTime AS 'Completion Time',
                            NoOfSteps AS 'No Of Records'
                        FROM
                            sheba_cas.job c
                                LEFT JOIN
                            sheba_cas.ne ON c.idNE = ne.idSwitch
                        WHERE
                            idjobdefinition = 1 AND idNE != 0
                                AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT 
                            (SELECT 'Softex') AS 'ICX NAME',
                            SwitchName AS 'Switch Name',
                            JobName AS 'Job Name',
                            CompletionTime AS 'Completion Time',
                            NoOfSteps AS 'No Of Records'
                        FROM
                            softex_cas.job c
                                LEFT JOIN
                            softex_cas.ne ON c.idNE = ne.idSwitch
                        WHERE
                            idjobdefinition = 1 AND idNE != 0
                                AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT 
                            (SELECT 'SrTelecom') AS 'ICX NAME',
                            SwitchName AS 'Switch Name',
                            JobName AS 'Job Name',
                            CompletionTime AS 'Completion Time',
                            NoOfSteps AS 'No Of Records'
                        FROM
                            srtelecom_cas.job c
                                LEFT JOIN
                            srtelecom_cas.ne ON c.idNE = ne.idSwitch
                        WHERE
                            idjobdefinition = 1 AND idNE != 0
                                AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT 
                            (SELECT 'Summit') AS 'ICX NAME',
                            SwitchName AS 'Switch Name',
                            JobName AS 'Job Name',
                            CompletionTime AS 'Completion Time',
                            NoOfSteps AS 'No Of Records'
                        FROM summit_cas.job c LEFT JOIN summit_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'TeleExchange') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM teleexchange_cas.job c LEFT JOIN teleexchange_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'TelePlusNewyork') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM teleplusnewyork_cas.job c LEFT JOIN teleplusnewyork_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1
                        ORDER BY CompletionTime DESC
                        LIMIT 0 , 1) UNION ALL (SELECT  (SELECT 'VoiceTel') AS 'ICX NAME', SwitchName AS 'Switch Name', JobName AS 'Job Name', CompletionTime AS 'Completion Time', NoOfSteps AS 'No Of Records'
                        FROM voicetel_cas.job c LEFT JOIN voicetel_cas.ne ON c.idNE = ne.idSwitch
                        WHERE idjobdefinition = 1 AND idNE != 0 AND Status = 1 ORDER BY CompletionTime DESC LIMIT 0 , 1) ORDER BY 4 DESC;";
        //sqlCommand = $@"SELECT 'Agni ICX' AS ICXName, 123 AS SwitchId,'Job XYZ' AS LastJobName, NOW() AS CompletionTime, 1000 AS NoofRecords;";

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = connectionString;
            connection.Open();
            DataSet dataSet = gridViewCompletedData(connection, sqlCommand);

            bool hasData = dataSet.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);

            if (hasData == true)
            {
                gridViewJobProcessingStatus = ConvertDataSetToListForJobProcessingStatus(dataSet);
                GridViewJobProcessingStatus.DataSource = gridViewJobProcessingStatus;
                GridViewJobProcessingStatus.DataBind();
            }
        }
    }


    private void BindGridViewErrorStatus()
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
        //this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
        // Calculate the offset based on the current page index and page size
        //int pageIndex = GridViewCompleted.PageIndex;
        //int pageSize = GridViewCompleted.PageSize;

        string connectionString = DbUtil.getDbConStrWithDatabase(databaseSetting);

        List<GridVieErrorStatus> gridViewErrorStatus = new List<GridVieErrorStatus>();

        // Modify your SQL query to include the OFFSET and FETCH NEXT clauses
        string sqlCommand = $@"(SELECT 
                            'Agni' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            agni_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'BanglaICX' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            banglaicx_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'BanglaTelecom' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            banglatelecom_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Bantel' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            bantel_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Btcl' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            btcl_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'CrossWorld' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            crossworld_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'GaziNetworks' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            gazinetworks_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'ImamNetwork' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            imamnetwork_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Jibondhara' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            jibondhara_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'MNH' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            mnh_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'MotherTelecom' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            mothertelecom_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'NewGeneration' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            newgenerationtelecom_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Paradise' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            paradise_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Purple' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            purple_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'RingTech' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            ringtech_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Sheba' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            sheba_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Softex' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            softex_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'SrTelecom' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            srtelecom_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Summit' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            summit_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'Teleexchange' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            teleexchange_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'TelePlusNewyork' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            teleplusnewyork_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5) UNION ALL (SELECT 
                            'VoiceTel' AS 'Icx Name',
                            ExceptionMessage AS 'Error',
                            ProcessName AS 'Process Name',
                            TimeRaised AS 'Occurrence Time'
                        FROM
                            voicetel_cas.allerror
                        ORDER BY id DESC
                        LIMIT 0 , 5);";
        //sqlCommand = $@"SELECT  'Demo ICX' AS ICXName, 'Sample Error' AS Error, 'Demo Process' AS ProcessName, NOW() AS OccuranceTime;";

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = connectionString;
            connection.Open();
            DataSet dataSet = gridViewCompletedData(connection, sqlCommand);

            bool hasData = dataSet.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);

            if (hasData == true)
            {
                gridViewErrorStatus = ConvertDataSetToListForErrorStatus(dataSet);
                GridViewErrorStatus.DataSource = gridViewErrorStatus;
                GridViewErrorStatus.DataBind();
            }
        }
    }

    protected void GridViewCompleted_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        // Set the new page index
    }
    //protected void btnSearch_Click(object sender, EventArgs e)
    //{
    //    // Get the search term from the input field
    //    string searchTerm = GridViewTgs;

    //    // Implement your search logic here
    //    // You can search the database, filter a list, or perform any other search operation

    //    // Update your UI to display search results
    //    // For example, you can bind search results to a GridView or display them in a list
    //}

    protected void PreviousButton_Click(object sender, EventArgs e)
    {
        // Go to the previous page
        currentPageForJobGrid--;
        //currentIndexForJobGrid--;
        //if (GridViewCompleted.PageIndex > 0)
        //{
        //    GridViewCompleted.PageIndex--;
        //    BindGridView();
        //}
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

    protected void NextButton_Click(object sender, EventArgs e)
    {
        // Go to the next page
           currentPageForJobGrid++;
        //currentIndexForJobGrid++;
        //if (GridViewCompleted.PageIndex < GridViewCompleted.PageCount - 1)
        //{
        //    GridViewCompleted.PageIndex++;
        //    BindGridView();
        //}
    }

    protected void Timer2_Tick(object sender, EventArgs e)
    {

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
    protected class GridVieJobProcessingStatus
    {
        public int Id { get; set; }
        public string ICXName { get; set; }
        public int SwitchId { get; set; }
        public string LastJobName { get; set; }
        public DateTime CompletionTime { get; set; }
        public int NoofRecords { get; set; }
    }
    protected class GridVieErrorStatus
    {
        public int Id { get; set; }
        public string ICXName { get; set; }
        public string Error { get; set; }
        public string ProcessName { get; set; }
        public DateTime OccuranceTime { get; set; }
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
    List<GridVieJobProcessingStatus> ConvertDataSetToListForJobProcessingStatus(DataSet ds)
    {
        bool hasRecords = ds.Tables.Cast<DataTable>()
            .Any(table => table.Rows.Count != 0);
        List<GridVieJobProcessingStatus> records = new List<GridVieJobProcessingStatus>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    GridVieJobProcessingStatus record = new GridVieJobProcessingStatus();
                    record.ICXName = row["ICXName"].ToString();
                    record.SwitchId = Convert.ToInt32(row["SwitchId"]);
                    record.LastJobName = row["LastJobName"].ToString();
                    record.CompletionTime = Convert.ToDateTime(row["CompletionTime"]);
                    record.NoofRecords = Convert.ToInt32(row["NoofRecords"]);
                    records.Add(record);
                }
            }
        }
        return records;
    }

    List<GridVieErrorStatus> ConvertDataSetToListForErrorStatus(DataSet ds)
    {
        bool hasRecords = ds.Tables.Cast<DataTable>()
            .Any(table => table.Rows.Count != 0);
        List<GridVieErrorStatus> records = new List<GridVieErrorStatus>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    GridVieErrorStatus record = new GridVieErrorStatus();
                    record.ICXName = row["ICXName"].ToString();
                    record.Error = row["Error"].ToString();
                    record.ProcessName = row["ProcessName"].ToString();
                    record.OccuranceTime = Convert.ToDateTime(row["OccuranceTime"]);
                    records.Add(record);
                }
            }
        }
        return records;
    }
}

