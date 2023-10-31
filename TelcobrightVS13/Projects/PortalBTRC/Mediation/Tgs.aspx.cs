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
using System.Data.Entity;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

public partial class TgsOfICX : Page
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

        BindGridViewForTg();

        if (!IsPostBack)//initial
        {
        }

    }

    //humayun

    
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
    }

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
    protected class GridViewTg
    {
        public int Id { get; set; }
        public string TgName { get; set; }
        public string switchName { get; set; }
        public string zone { get; set; }
        public string partner { get; set; }
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
}