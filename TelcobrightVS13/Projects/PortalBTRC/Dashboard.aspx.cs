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

public partial class DashboardAspx : Page
{
   
    
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
        telcobrightpartner thisPartner = null;
        string binpath = System.Web.HttpRuntime.BinDirectory;

        var databaseSetting = telcobrightConfig.DatabaseSetting;

        using (PartnerEntities conTelco = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
        {
            thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbNameAppConf).ToList().First();

        }
        //this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
        //this.lblCustomerDisplayName.Text = "CDR Analyzer System (CAS)";
        databaseSetting.DatabaseName = this.targetIcxName;
        string connectionString = DbUtil.getDbConStrWithDatabase(databaseSetting);

        string sqlCommand = "select id, JobName, CreationTime, CompletionTime " +
                                       "from job where idjobdefinition = 1 " +
                                       "order by completiontime desc limit 0,23;";

        List<GridViewCompletedJob> gridViewCompletedJob = new List<GridViewCompletedJob>();


        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = connectionString;
            connection.Open();
            DataSet dataSet = gridViewCompletedData(connection, sqlCommand);
            bool hasData = dataSet.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);
            if (hasData == true)
            {
                gridViewCompletedJob = ConvertDataSetToList(dataSet);
                this.GridViewCompleted.DataSource = gridViewCompletedJob;
                this.GridViewCompleted.DataBind();

            }
        }


        //dashboard items
        UpdateErrorCalls();





        UpdateInternationalIncoming();
        this.Timer1.Enabled = true;
        this.Timer2.Enabled = true;
        this.Timer3.Enabled = true;

       


        //PieChartIpTdm
        var dataPoint1 = PieChartIpTdm.Series["Series1"].Points[0];
         var dataPoint2 = PieChartIpTdm.Series["Series1"].Points[1];

         var data1 = dataPoint1.YValues[0] = 80;
         dataPoint1.AxisLabel = "IP"+" " + data1 +"%";

         var data2 = dataPoint2.YValues[0] = 20;
         dataPoint2.AxisLabel = "TDM" + " " + data2 + "%";

         PieChartIpTdm.DataBind();


        //BarChartIp
        /*var dataPointI1 = BarChartIp.Series["PositiveSeries"].Points[0];
        var dataPointI2 = BarChartIp.Series["PositiveSeries"].Points[1];
        var dataPointI3 = BarChartIp.Series["PositiveSeries"].Points[2];
        var dataPointI4 = BarChartIp.Series["PositiveSeries"].Points[3];
        var dataPointI5 = BarChartIp.Series["PositiveSeries"].Points[4];
        var dataPointI6 = BarChartIp.Series["PositiveSeries"].Points[5];*/


        // Find the bar chart control
        var barChart = FindControl("BarChartIp");

        if (barChart != null)
        {
            // Find the positive and negative series
            var positiveSeries = BarChartIp.Series["PositiveSeries"];
            var negativeSeries = BarChartIp.Series["NegativeSeries"];

            if (positiveSeries != null && negativeSeries != null)
            {
                // Iterate through the data points in the bar chart
                for (int i = 0; i < positiveSeries.Points.Count; i++)
                {
                    // Access AxisLabel and YValues for both positive and negative series
                    string barAxisLabel = positiveSeries.Points[i].AxisLabel;
                    double barPositiveValue = positiveSeries.Points[i].YValues[0];
                    double barNegativeValue = negativeSeries.Points[i].YValues[0];

                    // Do something with the data from the bar chart
                    // For example, you can add them to the same list or display them in a label
                }
            }
        }


    }
    private void UpdateErrorCalls()
    {
        List<DashBoard.ErrorCalls> ec = new List<DashBoard.ErrorCalls>();
        using (PartnerEntities conPartner = PortalConnectionHelper.GetPartnerEntitiesDynamic(telcobrightConfig.DatabaseSetting))
        {
            ec = conPartner.Database.SqlQuery<DashBoard.ErrorCalls>(@"select ErrorCode as ErrorReason, count(*) as NumberOfCalls
                                                from cdrerror group by ErrorCode").ToList();
            this.HyperLinkError.Text = ec.Select(c => c.NumberOfCalls).Sum() + " Calls in Error";
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
                    record.id = int.Parse(row.ItemArray[0].ToString());
                    record.jobName = row.ItemArray[1].ToString();
                    record.creationTime = (DateTime) row.ItemArray[2];
                    record.completionTime = (DateTime)row.ItemArray[3];


                    records.Add(record);
                }
            }
        }
        return records;
    }





}