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
using Image = System.Web.UI.WebControls.Image;


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

        string sqlCommand = "select id, JobName, CreationTime, CompletionTime,status " +
                                       "from srtelecom_cas.job where idjobdefinition = 1 " +
                                       "and status = 1 " +
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
                gridViewCompletedJob = ConvertDataSetToList(dataSet,e);
               
          

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



       

        

        if (!IsPostBack)//initial
        {
            

        }

        PopulateIpTdmPieChart();
        PopulateDomesticDistribution();
        PopulateIpTdmDistribution();



    }



    //humayun
    private void PopulateIpTdmDistribution()
    {
        //PositiveSeries
        Series series1 = IpTdmDistribution.Series["PositiveSeries"];
        DataPointCollection points1 = series1.Points;
        DataPoint dataPoint1 = new DataPoint
        {
            AxisLabel = "Sylhet",
            YValues = new double[] { 45 },
            Color = ColorTranslator.FromHtml("#08605c")
        };
        points1.Add(dataPoint1);

        dataPoint1 = new DataPoint
        {
            AxisLabel = "Bogura",
            YValues = new double[] { 9 },
            Color = ColorTranslator.FromHtml("#08605c")
        };
        points1.Add(dataPoint1);
        dataPoint1 = new DataPoint
        {
            AxisLabel = "Khulna",
            YValues = new double[] { 5 },
            Color = ColorTranslator.FromHtml("#08605c")
        };
        points1.Add(dataPoint1);
        dataPoint1 = new DataPoint
        {
            AxisLabel = "Chattogram",
            YValues = new double[] { 35 },
            Color = ColorTranslator.FromHtml("#08605c")
        };
        points1.Add(dataPoint1);
        dataPoint1 = new DataPoint
        {
            AxisLabel = "Dhaka",
            YValues = new double[] { 33 },
            Color = ColorTranslator.FromHtml("#08605c")
        };
        points1.Add(dataPoint1);

        ////NegativeSeries
        Series series2 = IpTdmDistribution.Series["NegativeSeries"];
        DataPointCollection points2 = series2.Points;
        DataPoint dataPoint2 = new DataPoint
        {
            AxisLabel = "Sylhet",
            YValues = new double[] { 90 },
            Color = ColorTranslator.FromHtml("#e40613")
        };
        points2.Add(dataPoint2);

        dataPoint2 = new DataPoint
        {
            AxisLabel = "Bogura",
            YValues = new double[] { 55 },
            Color = ColorTranslator.FromHtml("#e40613")
        };
        points2.Add(dataPoint2);
        dataPoint2 = new DataPoint
        {
            AxisLabel = "Khulna",
            YValues = new double[] { 15 },
            Color = ColorTranslator.FromHtml("#e40613")
        };
        points2.Add(dataPoint2);
        dataPoint2 = new DataPoint
        {
            AxisLabel = "Chattogram",
            YValues = new double[] { 30 },
            Color = ColorTranslator.FromHtml("#e40613")
        };
        points2.Add(dataPoint2);
        dataPoint2 = new DataPoint
        {
            AxisLabel = "Dhaka",
            YValues = new double[] { 60 },
            Color = ColorTranslator.FromHtml("#e40613")
        };
        points2.Add(dataPoint2);
    }


    private void PopulateDomesticDistribution()
    {
        Series series1 = DomesticDistribution.Series["Series1"];

        DataPointCollection points = series1.Points;
        DataPoint dataPoint = new DataPoint
        {
            AxisLabel = "Agni",
            YValues = new double[] { 90 },
            Color = ColorTranslator.FromHtml("#08605c")
        };
        points.Add(dataPoint);

        dataPoint = new DataPoint
        {
            AxisLabel = "Banglatelecom",
            YValues = new double[] { 10 },
            Color = ColorTranslator.FromHtml("#e40613")
        };
        points.Add(dataPoint);


        dataPoint = new DataPoint
        {
            AxisLabel = "Bangla",
            YValues = new double[] { 19 },
            Color = ColorTranslator.FromHtml("#F86F03")
        };
        points.Add(dataPoint);

        dataPoint = new DataPoint
        {
            AxisLabel = "Bantel",
            YValues = new double[] { 78 },
            Color = ColorTranslator.FromHtml("#FFA41B")
        };

        points.Add(dataPoint);


    }

    private void PopulateIpTdmPieChart()
    {

        var dataPoint1 = PieChartIpTdm.Series["Series1"].Points[0];
        var dataPoint2 = PieChartIpTdm.Series["Series1"].Points[1];

        var data1 = dataPoint1.YValues[0] = 80;
        dataPoint1.AxisLabel = "IP" + " " + data1 + "%";

        var data2 = dataPoint2.YValues[0] = 20;
        dataPoint2.AxisLabel = "TDM" + " " + data2 + "%";

        PieChartIpTdm.DataBind();
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
        PopulateIpTdmPieChart();
        

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
        public Image image { get; set; }


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

    List<GridViewCompletedJob> ConvertDataSetToList(DataSet ds , EventArgs e)
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
                    string status = row["status"].ToString();
                   
                    // Determine the dynamic image URL based on your condition or data

      
                     // Find the StatusImage control and set its ImageUrl      

                     records.Add(record);
                }
            }
        }
        return records;
    }
    private string GetImageUrlBasedOnStatus(int  status)
    {
        // Add your logic here to determine the image URL based on the status
         if (status%2 == 1)
        {
            return "https://i.postimg.cc/Rh0G70KG/5610944.png";
        }
        else
        {
            return "https://i.postimg.cc/Y0kkn8rT/Flat-cross-icon-svg.png";
        }
    }


    protected void onGridViewCompleted_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Assuming you have a DataSource with a field named "Status" to determine the image URL
            int i = 0;

            foreach (GridViewRow row in GridViewCompleted.Rows)
                {

                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        // Determine the dynamic image URL based on your condition or data
                        string imageUrl = GetImageUrlBasedOnStatus(i++);
                        Image img = row.FindControl("StatusImage") as Image;
                        img.ImageUrl = imageUrl;

                    }
                }
            }
        
    }


}