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
using LibraryExtensions.ConfigHelper;
using reports;

public partial class DashboardAspx : Page
{


    TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
    string targetIcxName = "btrc_cas";
    private DatabaseSetting databaseSetting = null;
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
        TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
        this.databaseSetting = telcobrightConfig.DatabaseSetting;
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

        if (dbName!="btrc_cas")
        {
            // if not BTRC user route to regular icx dashhboard
            //add limit 0,1000 in error table query for icx dashboard
            Response.Redirect("~/DashboardForIcx.aspx");
        }

        using (PartnerEntities conTelco = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
        {
            thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbName).ToList().First();

        }
        //this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
        //this.lblCustomerDisplayName.Text = "CDR Analyzer System (CAS)";
        //databaseSetting.DatabaseName = this.targetIcxName;
        string connectionString = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);

        string sqlCommand = @"select 'Agni ICX' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from agni_cas.job 
                            left join agni_cas.ne
                            on agni_cas.job.idne = agni_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'BTCL' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from btcl_cas.job 
                            left join btcl_cas.ne
                            on btcl_cas.job.idne = btcl_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Bangla ICX' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from banglaicx_cas.job 
                            left join banglaicx_cas.ne
                            on banglaicx_cas.job.idne = banglaicx_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Bangla Telecom Ltd' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from banglatelecom_cas.job 
                            left join banglatelecom_cas.ne
                            on banglatelecom_cas.job.idne = banglatelecom_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Bantel Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from bantel_cas.job 
                            left join bantel_cas.ne
                            on bantel_cas.job.idne = bantel_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Cross World Telecom Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from crossworld_cas.job 
                            left join crossworld_cas.ne
                            on crossworld_cas.job.idne = crossworld_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Gazi Networks Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from gazinetworks_cas.job 
                            left join gazinetworks_cas.ne
                            on gazinetworks_cas.job.idne = gazinetworks_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Imam Network Ltd' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from imamnetwork_cas.job 
                            left join imamnetwork_cas.ne
                            on imamnetwork_cas.job.idne = imamnetwork_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Sheba ICX' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from sheba_cas.job 
                            left join sheba_cas.ne
                            on sheba_cas.job.idne = sheba_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'JibonDhara Solutions Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from jibondhara_cas.job 
                            left join jibondhara_cas.ne
                            on jibondhara_cas.job.idne = jibondhara_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'M&H Telecom Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from mnh_cas.job 
                            left join mnh_cas.ne
                            on mnh_cas.job.idne = mnh_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Mother Telecom Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from mothertelecom_cas.job 
                            left join mothertelecom_cas.ne
                            on mothertelecom_cas.job.idne = mothertelecom_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'New Generation Telecom Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from newgenerationtelecom_cas.job 
                            left join newgenerationtelecom_cas.ne
                            on newgenerationtelecom_cas.job.idne = newgenerationtelecom_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Paradise Telecom Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from paradise_cas.job 
                            left join paradise_cas.ne
                            on paradise_cas.job.idne = paradise_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Purple Telecom Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from purple_cas.job 
                            left join purple_cas.ne
                            on purple_cas.job.idne = purple_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'RingTech(Bangladesh) Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from ringtech_cas.job 
                            left join ringtech_cas.ne
                            on ringtech_cas.job.idne = ringtech_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'SR Telecom Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from srtelecom_cas.job 
                            left join srtelecom_cas.ne
                            on srtelecom_cas.job.idne = srtelecom_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Softex Communication Ltd' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from softex_cas.job 
                            left join softex_cas.ne
                            on softex_cas.job.idne = softex_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Summit Communication Limited(Vertex)' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from summit_cas.job 
                            left join summit_cas.ne
                            on summit_cas.job.idne = summit_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Tele Exchange Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from teleexchange_cas.job 
                            left join teleexchange_cas.ne
                            on teleexchange_cas.job.idne = teleexchange_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Teleplus Newyork Limited' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from teleplusnewyork_cas.job 
                            left join teleplusnewyork_cas.ne
                            on teleplusnewyork_cas.job.idne = teleplusnewyork_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE
                            union all
                            select 'Voicetel Ltd' as icx,SwitchName as 'SwitchName',count(JobName) as No_Of_Cdrs_in_last_24_hours from voicetel_cas.job 
                            left join voicetel_cas.ne
                            on voicetel_cas.job.idne = voicetel_cas.ne.idSwitch
                            where idjobdefinition=1 and Status=1 and CreationTime >= '2023-10-05'and CompletionTime<= '2023-10-06'
                            group by idNE;";

        //sqlCommand = @"SELECT 'Teleplus Newyork Limited' AS icx, 'ZTE' AS 'SwitchName', 23 AS No_Of_Cdrs_in_last_24_hours
        //                      union all
        //                      SELECT 'Summit Communication Limited(Vertex)' AS icx, 'HUAWEI' AS 'SwitchName', 20 AS No_Of_Cdrs_in_last_24_hours
        //                      union all
        //                      SELECT 'Softex Communication Ltd' AS icx, 'CATALIA' AS 'SwitchName', 10 AS No_Of_Cdrs_in_last_24_hours;";
        List <GridViewJobStatusForICX> gridViewCompletedJobStatus = new List<GridViewJobStatusForICX>();


        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = connectionString;
            connection.Open();
            DataSet dataSet = gridViewCompletedData(connection, sqlCommand);
            bool hasData = dataSet.Tables.Cast<DataTable>()
                .Any(table => table.Rows.Count != 0);
            if (hasData == true)
            {
                gridViewCompletedJobStatus = ConvertDataSetToListStatus(dataSet);
                this.GridViewCompleted.DataSource = gridViewCompletedJobStatus;
                this.GridViewCompleted.DataBind();

            }
        }

        //dashboard items
        UpdateErrorCalls();

        UpdateInternationalIncoming();
        this.Timer1.Enabled = true;
        this.Timer2.Enabled = true;
        this.Timer3.Enabled = true;

        PopulateIpTdmPieChart();
        PopulateDomesticDistribution();
        PopulateIpTdmDistribution();
        PopulateInternationalDistributionIn();
        PopulateInternationalDistributionOut();
        PopulateIcxDistributionSylhet();
        PopulateIcxDistributionBarishal();
        PopulateIcxDistributionRangpur();
        PopulateIcxDistributionMymengshing();
        PopulateIcxDistributionRajshahi();

    }

    //humayun
    private void PopulateIpTdmPieChart()
    {

        var dataPoint1 = PieChartIpTdm.Series["Series1"].Points[0];
        var dataPoint2 = PieChartIpTdm.Series["Series1"].Points[1];

        var data1 = dataPoint1.YValues[0] = 20;
        dataPoint1.AxisLabel = "IP" + " " + data1 + "%";

        var data2 = dataPoint2.YValues[0] = 80;
        dataPoint2.AxisLabel = "TDM" + " " + data2 + "%";

        PieChartIpTdm.DataBind();
    }
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


    private void PopulateInternationalDistributionIn()
    {
        Series series1 = InternationalDistributionIncoming.Series["Series1"];
        DataPointCollection points = series1.Points;
        string[] labels = { "Agni", "Banglatelecom" , "Bangla", "Bantel", "Gazinetworks", "Getco","Immamnetworks",
            "Jibondhara", "Mmcommunication", "M&H", "Btrc", "Paradise", "Purple", "Ringtech", "Crossworld",
            "Sheba", "Softech", "Teleexchange", "Newgeneration", "TeleplusNetwork", "Summit", "Mothertel", "Voicetel"};
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A", "#4E4FEB",
            "#068FFF", "#1D5B79", "#EF6262", "#F3AA60", "#F2EE9D", "#7A9D54", "#557A46", "#8C3333",
            "#252B48", "#448069", "#F7E987", "#8CABFF", "#4477CE", "#512B81", "#35155D" };

        // double[] values = { 90, 10, 19, 78,55,89,96,95,75,65,32,85,14,55,22,33,44,55,6,52,63,45 };

        //string connectionString = "Server=127.0.0.1;Database=btrc_cas;User Id=root;Password='';";
        string connectionString = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);

        List<double> data = new List<double>();
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = @"select (select 'agni_cas') as icxname,(select 10000000.00 ) as duration 
            union all select(select 'banglatelecom_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bangla_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bantel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'gazinetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'getco_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'immamnetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'jibondhara_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mmcommunication_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'm&h_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'btrc_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'paradise_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'purple_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'ringtech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'crossworld_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'sheba_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'softech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleexchange_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'newgeneration_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleplusNetwork_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'summit_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mothertel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'voicetel_cas') as icxname,(select 10000000.00 ) as duration;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {

                    while (read.Read())
                    {
                        data.Add(read.GetDouble("duration"));
                    }
                    read.Close();

                }

            }

        }
        for (int i = 0; i < labels.Length; i++)
        {
            DataPoint dataPoint = new DataPoint
            {
                AxisLabel = labels[i],
                YValues = new double[] { data[i] },
                Color = ColorTranslator.FromHtml(colors[i])
            };
            points.Add(dataPoint);
        }
    }


    private void PopulateDomesticDistribution()
    {
        Series series1 = DomesticDistribution.Series["Series1"];
        DataPointCollection points = series1.Points;
        string[] labels = { "Agni", "Banglatelecom" , "Bangla", "Bantel", "Gazinetworks", "Getco","Immamnetworks",
            "Jibondhara", "Mmcommunication", "M&H", "Btrc", "Paradise", "Purple", "Ringtech", "Crossworld",
            "Sheba", "Softech", "Teleexchange", "Newgeneration", "TeleplusNetwork", "Summit", "Mothertel", "Voicetel"};
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A", "#4E4FEB",
            "#068FFF", "#1D5B79", "#EF6262", "#F3AA60", "#F2EE9D", "#7A9D54", "#557A46", "#8C3333",
            "#252B48", "#448069", "#F7E987", "#8CABFF", "#4477CE", "#512B81", "#35155D" };

        // double[] values = { 90, 10, 19, 78,55,89,96,95,75,65,32,85,14,55,22,33,44,55,6,52,63,45 };

        //string connectionString = "Server=127.0.0.1;Database=btrc_cas;User Id=root;Password='';";
        string connectionString = DbUtil.getDbConStrWithDatabase(this.databaseSetting);

        List<double> data = new List<double>();
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = @"select (select 'agni_cas') as icxname,(select 10000000.00 ) as duration 
            union all select(select 'banglatelecom_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bangla_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bantel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'gazinetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'getco_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'immamnetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'jibondhara_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mmcommunication_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'm&h_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'btrc_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'paradise_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'purple_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'ringtech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'crossworld_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'sheba_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'softech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleexchange_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'newgeneration_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleplusNetwork_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'summit_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mothertel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'voicetel_cas') as icxname,(select 10000000.00 ) as duration;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {

                    while (read.Read())
                    {
                        data.Add(read.GetDouble("duration"));
                    }
                    read.Close();

                }

            }

        }
        for (int i = 0; i < labels.Length; i++)
        {
            DataPoint dataPoint = new DataPoint
            {
                AxisLabel = labels[i],
                YValues = new double[] { data[i] },
                Color = ColorTranslator.FromHtml(colors[i])
            };
            points.Add(dataPoint);
        }
    }



    private void PopulateInternationalDistributionOut()
    {
        Series series1 = InternationalDistributionOutgoing.Series["Series1"];
        DataPointCollection points = series1.Points;
        string[] labels = { "Agni", "Banglatelecom" , "Bangla", "Bantel", "Gazinetworks", "Getco","Immamnetworks",
            "Jibondhara", "Mmcommunication", "M&H", "Btrc", "Paradise", "Purple", "Ringtech", "Crossworld",
            "Sheba", "Softech", "Teleexchange", "Newgeneration", "TeleplusNetwork", "Summit", "Mothertel", "Voicetel"};
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A", "#4E4FEB",
            "#068FFF", "#1D5B79", "#EF6262", "#F3AA60", "#F2EE9D", "#7A9D54", "#557A46", "#8C3333",
            "#252B48", "#448069", "#F7E987", "#8CABFF", "#4477CE", "#512B81", "#35155D" };

        // double[] values = { 90, 10, 19, 78,55,89,96,95,75,65,32,85,14,55,22,33,44,55,6,52,63,45 };

        //string connectionString = "Server=127.0.0.1;Database=btrc_cas;User Id=root;Password='';";
        string connectionString = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);

        List<double> data = new List<double>();
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = @"select (select 'agni_cas') as icxname,(select 10000000.00 ) as duration 
            union all select(select 'banglatelecom_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bangla_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bantel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'gazinetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'getco_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'immamnetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'jibondhara_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mmcommunication_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'm&h_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'btrc_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'paradise_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'purple_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'ringtech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'crossworld_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'sheba_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'softech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleexchange_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'newgeneration_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleplusNetwork_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'summit_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mothertel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'voicetel_cas') as icxname,(select 10000000.00 ) as duration;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {

                    while (read.Read())
                    {
                        data.Add(read.GetDouble("duration"));
                    }
                    read.Close();

                }

            }

        }
        for (int i = 0; i < labels.Length; i++)
        {
            DataPoint dataPoint = new DataPoint
            {
                AxisLabel = labels[i],
                YValues = new double[] { data[i] },
                Color = ColorTranslator.FromHtml(colors[i])
            };
            points.Add(dataPoint);
        }
    }


    private void PopulateIcxDistributionSylhet()
    {
        Series series1 = ICXDistributionSylhet.Series["Series1"];
        DataPointCollection points = series1.Points;
        string[] labels = { "Agni", "Banglatelecom" , "Bangla", "Bantel", "Gazinetworks", "Getco","Immamnetworks",
            "Jibondhara", "Mmcommunication", "M&H", "Btrc", "Paradise", "Purple", "Ringtech", "Crossworld",
            "Sheba", "Softech", "Teleexchange", "Newgeneration", "TeleplusNetwork", "Summit", "Mothertel", "Voicetel"};
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A", "#4E4FEB",
            "#068FFF", "#1D5B79", "#EF6262", "#F3AA60", "#F2EE9D", "#7A9D54", "#557A46", "#8C3333",
            "#252B48", "#448069", "#F7E987", "#8CABFF", "#4477CE", "#512B81", "#35155D" };



        //string connectionString = "Server=127.0.0.1;Database=btrc_cas;User Id=root;Password='';";
        string connectionString = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);

        List<double> data = new List<double>();
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = @"select (select 'agni_cas') as icxname,(select 10000000.00 ) as duration 
            union all select(select 'banglatelecom_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bangla_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bantel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'gazinetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'getco_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'immamnetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'jibondhara_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mmcommunication_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'm&h_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'btrc_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'paradise_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'purple_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'ringtech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'crossworld_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'sheba_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'softech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleexchange_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'newgeneration_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleplusNetwork_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'summit_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mothertel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'voicetel_cas') as icxname,(select 10000000.00 ) as duration;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {

                    while (read.Read())
                    {
                        data.Add(read.GetDouble("duration"));
                    }
                    read.Close();

                }

            }

        }
        for (int i = 0; i < labels.Length; i++)
        {
            DataPoint dataPoint = new DataPoint
            {
                AxisLabel = labels[i],
                YValues = new double[] { data[i] },
                Color = ColorTranslator.FromHtml(colors[i])
            };
            points.Add(dataPoint);
        }
    }

    private void PopulateIcxDistributionBarishal()
    {
        Series series1 = ICXDistributionBarishal.Series["Series1"];
        DataPointCollection points = series1.Points;
        string[] labels = { "Agni", "Banglatelecom" , "Bangla", "Bantel", "Gazinetworks", "Getco","Immamnetworks",
            "Jibondhara", "Mmcommunication", "M&H", "Btrc", "Paradise", "Purple", "Ringtech", "Crossworld",
            "Sheba", "Softech", "Teleexchange", "Newgeneration", "TeleplusNetwork", "Summit", "Mothertel", "Voicetel"};
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A", "#4E4FEB",
            "#068FFF", "#1D5B79", "#EF6262", "#F3AA60", "#F2EE9D", "#7A9D54", "#557A46", "#8C3333",
            "#252B48", "#448069", "#F7E987", "#8CABFF", "#4477CE", "#512B81", "#35155D" };



        //string connectionString = "Server=127.0.0.1;Database=btrc_cas;User Id=root;Password='';";
        string connectionString = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);

        List<double> data = new List<double>();
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = @"select (select 'agni_cas') as icxname,(select 10000000.00 ) as duration 
            union all select(select 'banglatelecom_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bangla_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bantel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'gazinetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'getco_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'immamnetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'jibondhara_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mmcommunication_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'm&h_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'btrc_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'paradise_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'purple_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'ringtech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'crossworld_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'sheba_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'softech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleexchange_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'newgeneration_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleplusNetwork_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'summit_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mothertel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'voicetel_cas') as icxname,(select 10000000.00 ) as duration;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {

                    while (read.Read())
                    {
                        data.Add(read.GetDouble("duration"));
                    }
                    read.Close();

                }

            }

        }
        for (int i = 0; i < labels.Length; i++)
        {
            DataPoint dataPoint = new DataPoint
            {
                AxisLabel = labels[i],
                YValues = new double[] { data[i] },
                Color = ColorTranslator.FromHtml(colors[i])
            };
            points.Add(dataPoint);
        }
    }
    private void PopulateIcxDistributionRangpur()
    {
        Series series1 = ICXDistributionRangpur.Series["Series1"];
        DataPointCollection points = series1.Points;
        string[] labels = { "Agni", "Banglatelecom" , "Bangla", "Bantel", "Gazinetworks", "Getco","Immamnetworks",
            "Jibondhara", "Mmcommunication", "M&H", "Btrc", "Paradise", "Purple", "Ringtech", "Crossworld",
            "Sheba", "Softech", "Teleexchange", "Newgeneration", "TeleplusNetwork", "Summit", "Mothertel", "Voicetel"};
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A", "#4E4FEB",
            "#068FFF", "#1D5B79", "#EF6262", "#F3AA60", "#F2EE9D", "#7A9D54", "#557A46", "#8C3333",
            "#252B48", "#448069", "#F7E987", "#8CABFF", "#4477CE", "#512B81", "#35155D" };



        //string connectionString = "Server=127.0.0.1;Database=btrc_cas;User Id=root;Password='';";
        string connectionString = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);

        List<double> data = new List<double>();
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = @"select (select 'agni_cas') as icxname,(select 10000000.00 ) as duration 
            union all select(select 'banglatelecom_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bangla_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bantel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'gazinetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'getco_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'immamnetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'jibondhara_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mmcommunication_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'm&h_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'btrc_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'paradise_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'purple_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'ringtech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'crossworld_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'sheba_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'softech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleexchange_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'newgeneration_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleplusNetwork_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'summit_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mothertel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'voicetel_cas') as icxname,(select 10000000.00 ) as duration;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {

                    while (read.Read())
                    {
                        data.Add(read.GetDouble("duration"));
                    }
                    read.Close();

                }

            }

        }
        for (int i = 0; i < labels.Length; i++)
        {
            DataPoint dataPoint = new DataPoint
            {
                AxisLabel = labels[i],
                YValues = new double[] { data[i] },
                Color = ColorTranslator.FromHtml(colors[i])
            };
            points.Add(dataPoint);
        }
    }

    private void PopulateIcxDistributionMymengshing()
    {
        Series series1 = ICXDistributionMymenshing.Series["Series1"];
        DataPointCollection points = series1.Points;
        string[] labels = { "Agni", "Banglatelecom" , "Bangla", "Bantel", "Gazinetworks", "Getco","Immamnetworks",
            "Jibondhara", "Mmcommunication", "M&H", "Btrc", "Paradise", "Purple", "Ringtech", "Crossworld",
            "Sheba", "Softech", "Teleexchange", "Newgeneration", "TeleplusNetwork", "Summit", "Mothertel", "Voicetel"};
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A", "#4E4FEB",
            "#068FFF", "#1D5B79", "#EF6262", "#F3AA60", "#F2EE9D", "#7A9D54", "#557A46", "#8C3333",
            "#252B48", "#448069", "#F7E987", "#8CABFF", "#4477CE", "#512B81", "#35155D" };



        //string connectionString = "Server=127.0.0.1;Database=btrc_cas;User Id=root;Password='';";
        string connectionString = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);

        List<double> data = new List<double>();
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = @"select (select 'agni_cas') as icxname,(select 10000000.00 ) as duration 
            union all select(select 'banglatelecom_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bangla_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bantel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'gazinetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'getco_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'immamnetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'jibondhara_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mmcommunication_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'm&h_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'btrc_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'paradise_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'purple_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'ringtech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'crossworld_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'sheba_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'softech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleexchange_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'newgeneration_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleplusNetwork_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'summit_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mothertel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'voicetel_cas') as icxname,(select 10000000.00 ) as duration;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {

                    while (read.Read())
                    {
                        data.Add(read.GetDouble("duration"));
                    }
                    read.Close();

                }

            }

        }
        for (int i = 0; i < labels.Length; i++)
        {
            DataPoint dataPoint = new DataPoint
            {
                AxisLabel = labels[i],
                YValues = new double[] { data[i] },
                Color = ColorTranslator.FromHtml(colors[i])
            };
            points.Add(dataPoint);
        }
    }




    private void PopulateIcxDistributionRajshahi()
    {
        Series series1 = ICXDistributionRajshahi.Series["Series1"];
        DataPointCollection points = series1.Points;
        string[] labels = { "Agni", "Banglatelecom" , "Bangla", "Bantel", "Gazinetworks", "Getco","Immamnetworks",
            "Jibondhara", "Mmcommunication", "M&H", "Btrc", "Paradise", "Purple", "Ringtech", "Crossworld",
            "Sheba", "Softech", "Teleexchange", "Newgeneration", "TeleplusNetwork", "Summit", "Mothertel", "Voicetel"};
        string[] colors = { "#08605c", "#e40613", "#F86F03", "#FFA41B", "#8EAC50", "#898121", "#E7B10A", "#4E4FEB",
            "#068FFF", "#1D5B79", "#EF6262", "#F3AA60", "#F2EE9D", "#7A9D54", "#557A46", "#8C3333",
            "#252B48", "#448069", "#F7E987", "#8CABFF", "#4477CE", "#512B81", "#35155D" };



        //string connectionString = "Server=127.0.0.1;Database=btrc_cas;User Id=root;Password='';";
        string connectionString = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);
        

        List<double> data = new List<double>();
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            string sql = @"select (select 'agni_cas') as icxname,(select 10000000.00 ) as duration 
            union all select(select 'banglatelecom_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bangla_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'bantel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'gazinetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'getco_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'immamnetworks_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'jibondhara_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mmcommunication_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'm&h_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'btrc_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'paradise_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'purple_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'ringtech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'crossworld_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'sheba_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'softech_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleexchange_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'newgeneration_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'teleplusNetwork_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'summit_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'mothertel_cas') as icxname,(select 10000000.00 ) as duration
                union all select(select 'voicetel_cas') as icxname,(select 10000000.00 ) as duration;";
            using (MySqlCommand command = new MySqlCommand(sql, con))
            {
                using (MySqlDataReader read = command.ExecuteReader())
                {

                    while (read.Read())
                    {
                        data.Add(read.GetDouble("duration"));
                    }
                    read.Close();

                }

            }

        }
        for (int i = 0; i < labels.Length; i++)
        {
            DataPoint dataPoint = new DataPoint
            {
                AxisLabel = labels[i],
                YValues = new double[] { data[i] },
                Color = ColorTranslator.FromHtml(colors[i])
            };
            points.Add(dataPoint);
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


    }
    protected class GridViewJobStatusForICX
    {
        public int id { get; set; }
        public string icx { get; set; }
        public string SwitchName { get; set; }
        public int No_Of_Cdrs_in_last_24_hours { get; set; }
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
                    record.jobName = row["icx"].ToString();
                    record.creationTime = Convert.ToDateTime(row["CreationTime"]);
                    record.completionTime = Convert.ToDateTime(row["CompletionTime"]);

                    records.Add(record);
                }
            }
        }
        return records;
    }

    List<GridViewJobStatusForICX> ConvertDataSetToListStatus(DataSet ds)
    {
        bool hasRecords = ds.Tables.Cast<DataTable>()
            .Any(table => table.Rows.Count != 0);
        List<GridViewJobStatusForICX> records = new List<GridViewJobStatusForICX>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    GridViewJobStatusForICX record = new GridViewJobStatusForICX();
                   // record.id = Convert.ToInt32(row["id"]);
                    record.icx= row["icx"].ToString();
                    record.SwitchName = row["SwitchName"].ToString();
                    record.No_Of_Cdrs_in_last_24_hours = Convert.ToInt32(row["No_Of_Cdrs_in_last_24_hours"]);

                    records.Add(record);
                }
            }
        }
        return records;
    }





}