using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using ExportToExcel;
//using InstallConfig;
using MediationModel;
using LibraryExtensions;
using PortalApp;
using PortalApp.ReportHelper;
using reports;
using TelcobrightInfra;
using TelcobrightMediation;

//
using System;
using System.IO;
using PortalApp;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PortalApp._myCodes;
using PortalApp._portalHelper;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using TelcobrightMediation;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
//

public partial class CasDefaultRptAllTrafic : System.Web.UI.Page
{
    string StartDate;
    string EndtDate;
    private int _mShowByCountry = 0;
    private int _mShowByAns = 0;
    DataTable _dt;
    TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
    public TelcobrightConfig tbc;
    private string GetQuery()
    {

        StartDate = txtDate.Text;
        EndtDate = (txtDate1.Text.ConvertToDateTimeFromMySqlFormat()).AddSeconds(1).ToMySqlFormatWithoutQuote();
        string tableName = DropDownListReportSource.SelectedValue + "01";

        string groupInterval = getSelectedRadioButtonText();
        switch (groupInterval)
        {
            case "Half Hourly":
            case "Hourly":
                tableName = tableName.Replace("_day_", "_hr_");
                break;
            default:
                tableName = tableName.Replace("_hr_", "_day_");
                break;

        }


        string constructedSQL = new SqlHelperIntlInIcxAllTrafic
        (StartDate,
            EndtDate,
            groupInterval,
            tableName,

            new List<string>()
            {
                groupInterval=="Hourly"?"Date":string.Empty,
                getInterval(groupInterval),
                CheckBoxPartner.Checked==true?"tup_inpartnerid":string.Empty,
                CheckBoxShowByAns.Checked==true?"tup_destinationId":string.Empty,
                CheckBoxShowByIgw.Checked==true?"tup_outpartnerid":string.Empty,
                //CheckBoxViewIncomingRoute.Checked==true?"tup_incomingroute":string.Empty,
                CheckBoxViewOutgoingRoute.Checked==true?"tup_outgoingroute":string.Empty,
                ViewBySwitch.Checked==true?"tup_switchid":string.Empty
            },
            new List<string>()
            {
                ViewBySwitch.Checked==true? DropDownListShowBySwitch.SelectedIndex>0?"tup_switchid="+DropDownListShowBySwitch.SelectedItem.Value:string.Empty:string.Empty,
                CheckBoxPartner.Checked==true?DropDownListPartner.SelectedIndex>0?" tup_inpartnerid="+DropDownListPartner.SelectedValue:string.Empty:string.Empty,
                CheckBoxShowByAns.Checked==true?DropDownListAns.SelectedIndex>0?" tup_destinationId="+DropDownListAns.SelectedValue:string.Empty:string.Empty,
                CheckBoxShowByIgw.Checked==true?DropDownListIgw.SelectedIndex>0?" tup_outpartnerid="+DropDownListIgw.SelectedValue:string.Empty:string.Empty,
                //CheckBoxViewIncomingRoute.Checked==true?DropDownListViewIncomingRoute.SelectedIndex>0?" tup_incomingroute="+"'"+DropDownListViewIncomingRoute.SelectedItem.Value:string.Empty:string.Empty,
                CheckBoxViewOutgoingRoute.Checked==true?DropDownListViewOutgoingRoute.SelectedIndex>0?" tup_outgoingroute="+DropDownListViewOutgoingRoute.SelectedItem.Value:string.Empty:string.Empty,

            }).getSQLString();


        return constructedSQL;
    }

    private string getInterval(string groupInterval)
    {
        switch (groupInterval)
        {
            case "Hourly":
            case "Daily":
                return "Date";
            case "Weekly":
                return "concat(year(Date),'-W',week(Date))";
            case "Monthly":
                return "concat(year(Date),'-',date_format(Date,'%b'))";
            case "Yearly":
                return "DATE_FORMAT(Date,'%Y')";
            default:
                return string.Empty;
        }
    }

    public string getSelectedRadioButtonText()
    {
        if (CheckBoxDailySummary.Checked)
        {
            string interval = "";
            //if (RadioButtonHalfHourly.Checked)
            //    return interval = "" + RadioButtonHalfHourly.Text;
            //else 
            if (RadioButtonHourly.Checked)
                return interval = "" + RadioButtonHourly.Text;
            else if (RadioButtonDaily.Checked)
                return interval = "" + RadioButtonDaily.Text;
            else if (RadioButtonWeekly.Checked)
                return interval = "" + RadioButtonWeekly.Text;
            else if (RadioButtonMonthly.Checked)
                return interval = "" + RadioButtonMonthly.Text;
            else if (RadioButtonYearly.Checked)
                return interval = "" + RadioButtonYearly.Text;
            else
                return "";
        }
        else return string.Empty;
    }
    private int GetColumnIndexByName(GridView grid, string name)
    {
        foreach (DataControlField col in grid.Columns)
        {
            if (col.SortExpression.ToLower().Trim() == name.ToLower().Trim())
            {
                return grid.Columns.IndexOf(col);
            }
        }

        return -1;
    }




    protected void submit_Click(object sender, EventArgs e)
    {
        /*
        if (CheckBoxShowByAns.Checked == true)
        {
            GridView1.Columns[3].Visible = true;
            //load ANS KPI
            Dictionary<string, partner> dicKpiAns = new Dictionary<string, partner>();
            using (PartnerEntities context = new PartnerEntities())
            {
                foreach (partner thisPartner in context.partners.Where(c => c.PartnerType == 1).ToList())
                {
                    dicKpiAns.Add(thisPartner.PartnerName, thisPartner);
                }
                ViewState["dicKpiAns"] = dicKpiAns;
            }
        }
        else GridView1.Columns[3].Visible = false;
        */

        //GridView1.Columns[GetColumnIndexByName(GridView1, "International Partner")].Visible = CheckBoxPartner.Checked;
        //GridView1.Columns[GetColumnIndexByName(GridView1, "icxName")].Visible = CheckBoxViewIncomingRoute.Checked;
        //GridView1.Columns[GetColumnIndexByName(GridView1, "IGW")].Visible = CheckBoxShowByIgw.Checked;
        //GridView1.Columns[GetColumnIndexByName(GridView1, "tup_outgoingroute")].Visible = CheckBoxViewOutgoingRoute.Checked;
        //GridView1.Columns[GetColumnIndexByName(GridView1, "Paid Minutes (International Incoming)")].Visible = true;
        //if (CheckBoxShowCost.Checked == true)
        //{
        //    GridView1.Columns[14].Visible = false;
        //    GridView1.Columns[15].Visible = false;
        //}
        //else
        //{
        //    GridView1.Columns[14].Visible = false;
        //    GridView1.Columns[15].Visible = false;
        //}
        //if (CheckBoxShowPerformance.Checked == true)
        //{
        //    GridView1.Columns[19].Visible = true;
        //}
        //else
        //{
        //    GridView1.Columns[19].Visible = false;
        //}
        ////make profit invisible, it's useless
        //GridView1.Columns[17].Visible = false;
        ////GridView1.Columns[9].Visible = true;//carrier's duration



        ///
        TelcobrightConfig telcobrightConfig1 = PageUtil.GetTelcobrightConfig();
        DatabaseSetting databaseSetting = telcobrightConfig1.DatabaseSetting;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // or LicenseContext.Commercial

        telcobrightConfig1 = PageUtil.GetTelcobrightConfig();
        databaseSetting = telcobrightConfig1.DatabaseSetting;

        string userName = Page.User.Identity.Name;
        string dbName;
        if (telcobrightConfig1.DeploymentProfile.UserVsDbName.ContainsKey(userName))
        {
            dbName = telcobrightConfig1.DeploymentProfile.UserVsDbName[userName];
        }
        else
        {
            dbName = telcobrightConfig1.DatabaseSetting.DatabaseName;
        }
        databaseSetting.DatabaseName = dbName;


        PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting);
        List<telcobrightpartner> telcoTelcobrightpartners = context.telcobrightpartners.ToList();
        telcobrightpartner thisPartner = telcoTelcobrightpartners.Where(c => c.databasename == dbName).ToList().First();
       

        using (MySqlConnection connection = new MySqlConnection())
        {
            Dictionary<string, string> userVsDbName = tbc.DeploymentProfile.UserVsDbName;


            List<string> tableNames = new List<string>();
            string logIdentityName = this.User.Identity.Name;
            String selectedIcx = logIdentityName;
            TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
            string selectedUserdbName;
            if (userVsDbName.ContainsKey(logIdentityName))
            {
                selectedUserdbName = userVsDbName[logIdentityName];
            }
            else
            {
                selectedUserdbName = telcobrightConfig.DatabaseSetting.DatabaseName;
            }

            if (selectedIcx.Contains("btrc"))
            {
                foreach (var db in userVsDbName)
                {
                    if (!db.Value.Contains("btrc"))
                    {
                        tableNames.Add(db.Value + ".sum_voice_day_01");
                        tableNames.Add(db.Value + ".sum_voice_day_04");
                    }

                }
            }
            else
            {
                tableNames.Add(selectedUserdbName + ".sum_voice_day_01");
                tableNames.Add(selectedUserdbName + ".sum_voice_day_04");
            }

            connection.ConnectionString = PortalConnectionHelper.GetReadOnlyConnectionString(this.tbc.DatabaseSetting);
            connection.ConnectionString = connection.ConnectionString.Replace("btrc_cas",selectedUserdbName);
            connection.Open();
            string sql = GetQuery();

            /////


            string domesticSQL = $@"select sum(duration) duration from(
	                                    select sum(duration1) as duration from gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all	
	                                    select sum(duration1) as duration from gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' ) 
                                    as domestic;";
            string intOutSQL = $@"select sum(duration) duration from(
	                                    select sum(duration1) as duration from gazinetworks_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all	
	                                    select sum(duration1) as duration from gazinetworks_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' ) 
                                    as domestic;";
            string intInSQL = $@"select sum(duration) duration from(
	                                    select sum(duration1) as duration from gazinetworks_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all	
	                                    select sum(duration1) as duration from gazinetworks_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' ) 
                                    as domestic;";
            string domesticSQL1 = $@"select ROUND(sum(duration), 3) duration from(
	                            select ROUND(sum(duration1)/60, 3) as duration from agni_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from banglaicx_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from banglatelecom_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from bantel_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from imamnetwork_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from jibondhara_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from mnh_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from btcl_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from paradise_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from purple_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from ringtech_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from crossworld_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from srtelecom_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from sheba_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from softex_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from teleexchange_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from newgenerationtelecom_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from mothertelecom_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from teleplusnewyork_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from summit_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                            select ROUND(sum(duration1)/60, 3) as duration from voicetel_cas.sum_voice_day_01 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' 

                                ) 
                                as domestic;";
            string intOutSQL1 = $@"select ROUND(sum(duration), 3) duration from(
                                select ROUND(sum(roundedduration)/60, 3) as duration from agni_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from banglaicx_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from banglatelecom_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from bantel_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from gazinetworks_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from imamnetwork_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from jibondhara_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from mnh_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from btcl_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from paradise_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from purple_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from ringtech_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from crossworld_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from srtelecom_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from sheba_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from softex_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from teleexchange_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from newgenerationtelecom_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from mothertelecom_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from teleplusnewyork_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select ROUND(sum(roundedduration)/60, 3) as duration from summit_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
                                select sum(roundedduration)/60 as duration from voicetel_cas.sum_voice_day_02 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' 
                                )
                            as IntOut;";
            string intInSQL1 = $@"select ROUND(sum(duration), 3) duration from(
	                        select ROUND(SUM(duration1) / 60, 3) as duration from agni_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from banglaicx_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from banglatelecom_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from bantel_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from gazinetworks_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from imamnetwork_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from jibondhara_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from mnh_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from btcl_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from paradise_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from purple_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from ringtech_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from crossworld_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from srtelecom_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from sheba_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from softex_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from teleexchange_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from newgenerationtelecom_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from mothertelecom_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from teleplusnewyork_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from summit_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}' union all
	                        select ROUND(SUM(duration1) / 60, 3) as duration from voicetel_cas.sum_voice_day_03 where tup_starttime >= '{StartDate}' and tup_starttime < '{EndtDate}')
 
                        as IntIn;";

            List <string> Domestic = context.Database.SqlQuery<string>(domesticSQL1).ToList();
            List<string> IntOut = context.Database.SqlQuery<string>(intOutSQL1).ToList();
            List<string> IntIn = context.Database.SqlQuery<string>(intInSQL1).ToList();


            string domesticResult = Domestic[0];
            string intOutResult = IntOut[0];
            string intInResult = IntIn[0];


            Dictionary<string, AllTrafficData> allTrafficData = new Dictionary<string, AllTrafficData>();
            string[] callType = { "Domestic", "Int. Outgoing", "Int. Incoming" };
            foreach (string type in callType)
            {
                AllTrafficData allTraffic = new AllTrafficData();
                allTraffic.CallType = type;                
                allTrafficData[type] = allTraffic;
            }
            //for domestic data
            if (domesticResult.IsNullOrEmptyOrWhiteSpace())
                domesticResult = "0";
            if (intOutResult.IsNullOrEmptyOrWhiteSpace())
                intOutResult = "0";
            if (intInResult.IsNullOrEmptyOrWhiteSpace())
                intInResult = "0";
            allTrafficData["Domestic"].WholeCountryMinute = domesticResult;
            allTrafficData["Int. Outgoing"].WholeCountryMinute = intOutResult;
            allTrafficData["Int. Incoming"].WholeCountryMinute = intInResult;



            //DomesticLabel.Text = $"ToTal Domestic Minute Count From All ICX is: {domesticResult}";
            //IntOutLabel.Text =   $"ToTal Int. Out Minute Count From All ICX is: {intOutResult}";
            //IntInLabel.Text =    $"ToTal Int. In Minute Count From All ICX is:  {intInResult}";

            ////



            //string domesticSQL = $@"select sum(duration) duration from(
            //                     select sum(duration1) as duration from gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '2023-01-01' and tup_starttime < '2023-12-01' union all

            //                     select sum(duration1) as duration from gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '2023-01-01' and tup_starttime < '2023-12-01' )
            //                    domestic;
            //                    ";



            //Dictionary<string, string> dbVsDbName = AllDeploymenProfiles.getDeploymentprofiles()
            //    .FindAll(p => p.profileName == "cas")[0].UserVsDbName;




            //use sql aggregator
            //SqlAggregator sqlAggregator =
            //    new SqlAggregator(nonUnionSql: sql.Replace("sum_voice_day_01", "<basetable>").Replace("sum_voice_hr_01", "<basetable>"),
            //        tableNames: tableNames,
            //        _baseSqlStartsWith: "(",
            //        _baseSqlEndsWith: ") x");

            //string aggregatedSql = sqlAggregator.getFinalSql();
            string aggregatedSql = sql;
            MySqlCommand cmd = new MySqlCommand(aggregatedSql, connection);

            cmd.Connection = connection;
            //if (CheckBoxDailySummary.Checked == false)
            //{
            //    GridView1.Columns[GetColumnIndexByName(GridView1, "Date")].Visible = false;
            //}
            //else
            //{
            //    GridView1.Columns[GetColumnIndexByName(GridView1, "Date")].Visible = true;


            //}


            //common code
            if (CheckBoxDailySummary.Checked == true)
            {
                string summaryInterval = "";
                //if (RadioButtonHalfHourly.Checked == true)
                //{
                //    summaryInterval = "Halfhourly";
                //    GridView1.Columns[0].HeaderText = "Half Hour";
                //}
                //else 
                if (RadioButtonHourly.Checked == true)
                {
                    summaryInterval = "Hourly";
                    GridView1.Columns[GetColumnIndexByName(GridView1, "Date")].HeaderText = "Hour";
                }
                else if (RadioButtonDaily.Checked == true)
                {
                    summaryInterval = "Daily";
                    GridView1.Columns[GetColumnIndexByName(GridView1, "Date")].HeaderText = "Date";
                }
                else if (RadioButtonWeekly.Checked == true)
                {
                    summaryInterval = "Weekly";
                    GridView1.Columns[GetColumnIndexByName(GridView1, "Date")].HeaderText = "Week";
                }
                else if (RadioButtonMonthly.Checked == true)
                {
                    summaryInterval = "Monthly";
                    GridView1.Columns[GetColumnIndexByName(GridView1, "Date")].HeaderText = "Month";
                }
                else if (RadioButtonYearly.Checked == true)
                {
                    summaryInterval = "Yearly";
                    GridView1.Columns[GetColumnIndexByName(GridView1, "Date")].HeaderText = "Year";
                }
            }
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            da.Fill(dataset);


            GridView1.DataSource = dataset;
            bool hasRows = dataset.Tables.Cast<DataTable>()
                .Any(table => table.Rows.Count != 0);
            if (hasRows == true)
            {
                //GridView1.ShowFooter = true;//set it here before setting footer text, setting this to true clears already set footer text
                Label1.Text = "";
                Button1.Visible = true; //show export
                //Summary calculation for grid view*************************
                TrafficReportDatasetBased tr = new TrafficReportDatasetBased(dataset);
                tr.Ds = dataset;
                List<NoOfCallsVsPdd> callVsPdd = new List<NoOfCallsVsPdd>();

                DataRow dataRow = tr.Ds.Tables[0].Rows[0];

                double numericValue;
                if (double.TryParse(dataRow.ItemArray[1].ToString(), out numericValue))
                {
                    string formattedValue = numericValue.ToString("0.000");
                    allTrafficData["Domestic"].OwnICXMinute = formattedValue;
                }
                if (double.TryParse(dataRow.ItemArray[3].ToString(), out numericValue))
                {
                    string formattedValue = numericValue.ToString("0.000");
                    allTrafficData["Int. Outgoing"].OwnICXMinute = formattedValue;
                }
                if (double.TryParse(dataRow.ItemArray[5].ToString(), out numericValue))
                {
                    string formattedValue = numericValue.ToString("0.000");
                    allTrafficData["Int. Incoming"].OwnICXMinute = formattedValue;
                }


                //allTrafficData["Domestic"].OwnICXMinute = dataRow.ItemArray[1].ToString();
                //allTrafficData["Int. Outgoing"].OwnICXMinute = dataRow.ItemArray[3].ToString();
                //allTrafficData["Int. Incoming"].OwnICXMinute = dataRow.ItemArray[5].ToString();


                allTrafficData["Domestic"].PercentShareOfWholeCountryMinute = (double.Parse(allTrafficData["Domestic"].OwnICXMinute)*100 / double.Parse(allTrafficData["Domestic"].WholeCountryMinute)).ToString("0.00")+"%";
                allTrafficData["Int. Outgoing"].PercentShareOfWholeCountryMinute = (double.Parse(allTrafficData["Int. Outgoing"].OwnICXMinute)*100 / double.Parse(allTrafficData["Int. Outgoing"].WholeCountryMinute)).ToString("0.00") + "%";
                allTrafficData["Int. Incoming"].PercentShareOfWholeCountryMinute = (double.Parse(allTrafficData["Int. Incoming"].OwnICXMinute)*100 / double.Parse(allTrafficData["Int. Incoming"].WholeCountryMinute)).ToString("0.00") + "%";

                GridView2.DataSource = allTrafficData;
                GridView2.DataBind();
                foreach (DataRow dr in tr.Ds.Tables[0].Rows)
                {
                    tr.CallStat.TotalCalls += tr.ForceConvertToLong(dr["noofcalls1"]);
                    tr.CallStat.ConnectedCalls += tr.ForceConvertToLong(dr["minutes1"]);

                    tr.CallStat.TotalCalls += tr.ForceConvertToLong(dr["noofcalls2"]);
                    tr.CallStat.ConnectedCalls += tr.ForceConvertToLong(dr["minutes2"]);

                    tr.CallStat.TotalCalls += tr.ForceConvertToLong(dr["noofcalls3"]);
                    tr.CallStat.ConnectedCalls += tr.ForceConvertToLong(dr["minutes3"]);
                    //tr.CallStat.ConnectedCallsbyCauseCodes += tr.ForceConvertToLong(dr["ConectbyCC"]);
                    //tr.CallStat.SuccessfullCalls += tr.ForceConvertToLong(dr["Number Of Calls (International Incoming)"]);
                    //tr.CallStat.TotalActualDuration += tr.ForceConvertToDouble(dr["Paid Minutes (International Incoming)"]);
                    //tr.CallStat.TotalRoundedDuration += tr.ForceConvertToDouble(dr["RoundedDuration"]);
                    //tr.CallStat.TotalDuration1 += tr.ForceConvertToDouble(dr["Duration1"]);
                    //tr.CallStat.TotalCustomerCost += tr.ForceConvertToDouble(dr["customercost"]);
                    //tr.CallStat.BtrcRevShare += tr.ForceConvertToDouble(dr["tax1"]);
                    //NoOfCallsVsPdd cpdd = new NoOfCallsVsPdd(tr.ForceConvertToLong(dr["Number Of Calls (International Incoming)"]), tr.ForceConvertToDouble(dr["PDD"]));
                    //callVsPdd.Add(cpdd);
                }
                //tr.CallStat.TotalActualDuration = Math.Round(tr.CallStat.TotalActualDuration, 2);
                //tr.CallStat.TotalDuration1 = Math.Round(tr.CallStat.TotalDuration1, 2);
                //tr.CallStat.TotalDuration2 = Math.Round(tr.CallStat.TotalDuration2, 2);
                //tr.CallStat.TotalDuration3 = Math.Round(tr.CallStat.TotalDuration3, 2);
                //tr.CallStat.TotalDuration4 = Math.Round(tr.CallStat.TotalDuration4, 2);
                //tr.CallStat.TotalRoundedDuration = Math.Round(tr.CallStat.TotalRoundedDuration, 2);
                //tr.CallStat.TotalCustomerCost = Math.Round(tr.CallStat.TotalCustomerCost, 2);
                //tr.CallStat.CalculateAsr(2);
                //tr.CallStat.CalculateAcd(2);
                //tr.CallStat.CalculateAveragePdd(callVsPdd, 2);
                //tr.CallStat.CalculateCcr(2);
                //tr.CallStat.CalculateCcRbyCauseCode(2);
                //SUMMARY CALCULATION FOR GRIDVIEW COMPLETE


                //display summary information in the footer
                Dictionary<string, dynamic> fieldSummaries = new Dictionary<string, dynamic>();//key=colname,val=colindex in grid
                //all keys have to be lowercase, because db fields are lower case at times
                fieldSummaries.Add("noofcalls1", tr.CallStat.TotalCalls);
                fieldSummaries.Add("minutes1", tr.CallStat.ConnectedCalls);

                fieldSummaries.Add("noofcalls2", tr.CallStat.TotalCalls);
                fieldSummaries.Add("minutes2", tr.CallStat.ConnectedCalls);

                fieldSummaries.Add("noofcalls3", tr.CallStat.TotalCalls);
                fieldSummaries.Add("minutes3", tr.CallStat.ConnectedCalls);
                //fieldSummaries.Add("connectbycc", tr.CallStat.ConnectedCallsbyCauseCodes);
                //fieldSummaries.Add("number of calls (international incoming)", tr.CallStat.SuccessfullCalls);
                //fieldSummaries.Add("paid minutes (international incoming)", tr.CallStat.TotalActualDuration);
                //fieldSummaries.Add("roundedduration", tr.CallStat.TotalRoundedDuration);
                //fieldSummaries.Add("duration1", tr.CallStat.TotalDuration1);
                //fieldSummaries.Add("asr", tr.CallStat.Asr);
                //fieldSummaries.Add("acd", tr.CallStat.Acd);
                //fieldSummaries.Add("pdd", tr.CallStat.Pdd);
                //fieldSummaries.Add("ccr", tr.CallStat.Ccr);
                //fieldSummaries.Add("ccrbycc", tr.CallStat.CcRbyCauseCode);
                //fieldSummaries.Add("customercost", tr.CallStat.TotalCustomerCost);
                //fieldSummaries.Add("tax1", tr.CallStat.BtrcRevShare);
                tr.FieldSummaries = fieldSummaries;

                Session["IntlIn"] = tr;//save to session

                //populate footer
                //clear first
                bool captionSetForTotal = false;
                for (int c = 0; c < GridView1.Columns.Count; c++)
                {
                    GridView1.Columns[c].FooterText = "";
                }
                for (int c = 0; c < GridView1.Columns.Count; c++)
                {
                    if (captionSetForTotal == false && GridView1.Columns[c].Visible == true)
                    {
                        GridView1.Columns[c].FooterText = "Total: ";//first visible column
                        captionSetForTotal = true;
                    }
                    string key = GridView1.Columns[c].SortExpression.ToLower();
                    if (key == "") continue;
                    if (tr.FieldSummaries.ContainsKey(key))
                    {
                        GridView1.Columns[c].FooterText += (tr.GetDataColumnSummary(key)).ToString();//+ required to cocat "Total:"
                    }
                }
                GridView1.DataBind();//call it here after setting footer, footer text doesn't show sometime otherwise, may be a bug
                GridView1.ShowFooter = true;//don't set it now, set before footer text setting, weird! it clears the footer text
                //hide filters...
                Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDivSubmit();", true);
                hidValueSubmitClickFlag.Value = "false";

            }//if has rows

            else
            {
                GridView1.DataBind();
                Label1.Text = "No Data!";
                Button1.Visible = false; //hide export
            }

        }//using mysql connection



    }


    protected void Button1_Click(object sender, EventArgs e)//export to excel
    {
        if (Session["IntlIn"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            TrafficReportDatasetBased tr = (TrafficReportDatasetBased)Session["IntlIn"];
            DataSetWithGridView dsG = new DataSetWithGridView(tr, GridView1);//invisible baseColumns are removed in constructor
            CreateExcelFileAspNet.CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(tr.Ds, "IntlIncoming_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                                                                                + ".xlsx", Response);
        }
    }


    protected void CheckBoxShowBySwitch_CheckedChanged(object sender, EventArgs e)
    {
        if (ViewBySwitch.Checked == true)
        {
            DropDownListShowBySwitch.Enabled = true;
            setSwitchListDropDown(DropDownListViewIncomingRoute, EventArgs.Empty);
        }
        else DropDownListShowBySwitch.Enabled = false;
    }


    protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxPartner.Checked == true)
        {
            DropDownListPartner.Enabled = true;
        }
        else DropDownListPartner.Enabled = false;
    }


    protected void CheckBoxShowByAns_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxShowByAns.Checked == true)
        {
            DropDownListAns.Enabled = true;
        }
        else DropDownListAns.Enabled = false;
    }
    protected void CheckBoxShowByIgw_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxShowByIgw.Checked == true)
        {
            DropDownListIgw.Enabled = true;
        }
        else DropDownListIgw.Enabled = false;

    }



    protected void CheckBoxRealTimeUpdate_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxRealTimeUpdate.Checked)
        {
            //Disable DailySummary,Destination, Dates& Months

            CheckBoxDailySummary.Checked = false;
            CheckBoxDailySummary.Enabled = false;

            // CheckBoxShowByDestination.Checked = false;
            //CheckBoxShowByDestination.Enabled = false;

            TextBoxYear.Enabled = false;
            DropDownListMonth.Enabled = false;
            txtDate.Enabled = false;

            TextBoxYear1.Enabled = false;
            DropDownListMonth1.Enabled = false;
            txtDate1.Enabled = false;

            //Enable Timers,Duration,country
            //CheckBoxShowByCountry.Checked = true;
            TextBoxDuration.Enabled = true;
            //TextBoxDuration.Text = "30";
            //timerflag = true;



            //dateInitialize
        }
        else
        {
            //Enable DailySummary,Destination, Dates& Months

            //CheckBoxDailySummary.Checked = true;
            CheckBoxDailySummary.Enabled = true;

            // CheckBoxShowByDestination.Checked = true;
            // CheckBoxShowByDestination.Enabled = true;

            TextBoxYear.Enabled = true;
            DropDownListMonth.Enabled = true;
            txtDate.Enabled = true;

            TextBoxYear1.Enabled = true;
            DropDownListMonth1.Enabled = true;
            txtDate1.Enabled = true;

            //Disable Timers,Duration,
            //CheckBoxShowByCountry.Checked = false;
            TextBoxDuration.Enabled = false;
            //TextBoxDuration.Text = "30";
            //timerflag = false;
        }
        //CheckBoxShowByCountry_CheckedChanged(sender, e);
        DateInitialize();
    }

    public void DateInitialize()
    {
        if (CheckBoxRealTimeUpdate.Checked)
        {
            long a;
            if (!long.TryParse(TextBoxDuration.Text, out a))
            {
                // If Not Integer Clear Textbox text or you can also Undo() Last Operation :)

                TextBoxDuration.Text = "30";
                a = 30;
            }

            DateTime endtime = DateTime.Now;
            DateTime starttime = endtime.AddMinutes(a * (-1));
            txtDate1.Text = endtime.ToString("dd/MM/yyyy HH:mm:ss");
            txtDate.Text = starttime.ToString("dd/MM/yyyy HH:mm:ss");

            //return true;
        }
        //else
        //{
        //    txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
        //    txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
        //}
    }

    protected void TextBoxDuration_TextChanged(object sender, EventArgs e)
    {
        long a;
        if (!long.TryParse(TextBoxDuration.Text, out a))
        {
            // If Not Integer Clear Textbox text or you can also Undo() Last Operation :)

            TextBoxDuration.Text = "30";
        }
    }

    protected void Timer1_Tick(object sender, EventArgs e)
    {
        if (CheckBoxRealTimeUpdate.Checked)
        {
            submit_Click(sender, e);
        }
    }
    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Footer)
        {
            return;
        }

        if (CheckBoxShowByAns.Checked == true)
        {
            Dictionary<string, partner> dicKpiAns = null;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (ViewState["dicKpiAns"] != null)
                {
                    dicKpiAns = (Dictionary<string, partner>)ViewState["dicKpiAns"];
                }
                else
                {
                    return;
                }
                //Label lblCountry = (Label)e.Row.FindControl("Label3");
                string thisAnsName = DataBinder.Eval(e.Row.DataItem, "ANS").ToString();
                Single thisAsr = 0;
                Single thisAcd = 0;
                Single thisCcr = 0;
                Single thisPdd = 0;
                Single thisCcRbyCc = 0;
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ASR").ToString(), out thisAsr);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ACD").ToString(), out thisAcd);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCR").ToString(), out thisCcr);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "PDD").ToString(), out thisPdd);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCRByCC").ToString(), out thisCcRbyCc);
                partner thisPartner = null;
                dicKpiAns.TryGetValue(thisAnsName, out thisPartner);
                if (thisPartner != null)
                {
                    Color redColor = ColorTranslator.FromHtml("#FF0000");
                    //ASR
                    Single refAsr = 0;
                    if (Convert.ToSingle(thisPartner.refasr) > 0)
                    {
                        refAsr = Convert.ToSingle(thisPartner.refasr);
                    }
                    if ((thisAsr < refAsr) || (thisAsr == 0))
                    {
                        e.Row.Cells[16].ForeColor = Color.White;
                        e.Row.Cells[16].BackColor = redColor;
                        e.Row.Cells[16].Font.Bold = true;
                    }

                    //fas detection
                    double tempDbl = 0;
                    double refAsrFas = refAsr + refAsr * .5;//fas threshold= 30% of ref asr by default
                    double.TryParse(thisPartner.refasrfas.ToString(), out tempDbl);
                    if (tempDbl > 0) refAsrFas = tempDbl;

                    if (thisAsr > refAsrFas && refAsrFas > 0)
                    {
                        e.Row.Cells[16].ForeColor = Color.White;
                        e.Row.Cells[16].BackColor = Color.Blue;
                        e.Row.Cells[16].Font.Bold = true;
                    }

                    //ACD
                    Single refAcd = 0;
                    if (Convert.ToSingle(thisPartner.refacd) > 0)
                    {
                        refAcd = Convert.ToSingle(thisPartner.refacd);
                    }
                    if (thisAcd < refAcd)
                    {
                        //e.Row.Cells[16].ForeColor = RedColor;
                        e.Row.Cells[17].ForeColor = Color.White;
                        e.Row.Cells[17].BackColor = redColor;
                        e.Row.Cells[17].Font.Bold = true;
                    }

                    //PDD
                    Single refPdd = 0;
                    if (Convert.ToSingle(thisPartner.refpdd) > 0)
                    {
                        refPdd = Convert.ToSingle(thisPartner.refpdd);
                    }
                    if (thisPdd > refPdd)
                    {
                        //e.Row.Cells[17].ForeColor = RedColor;
                        e.Row.Cells[18].ForeColor = Color.White;
                        e.Row.Cells[18].BackColor = redColor;
                        e.Row.Cells[18].Font.Bold = true;
                    }

                    //CCR
                    Single refCcr = 0;
                    if (Convert.ToSingle(thisPartner.refccr) > 0)
                    {
                        refCcr = Convert.ToSingle(thisPartner.refccr);
                    }
                    if (thisCcr < refCcr)
                    {
                        //e.Row.Cells[18].ForeColor = RedColor;
                        e.Row.Cells[19].ForeColor = Color.White;
                        e.Row.Cells[19].BackColor = redColor;
                        e.Row.Cells[19].Font.Bold = true;
                    }

                    //CCRByCauseCode
                    Single refCcrCc = 0;
                    if (Convert.ToSingle(thisPartner.refccrbycc) > 0)
                    {
                        refCcrCc = Convert.ToSingle(thisPartner.refccrbycc);
                    }
                    if (thisCcRbyCc < refCcrCc)
                    {
                        //e.Row.Cells[20].ForeColor = RedColor;
                        e.Row.Cells[21].ForeColor = Color.White;
                        e.Row.Cells[21].BackColor = redColor;
                        e.Row.Cells[21].Font.Bold = true;
                    }
                }
            }
        }//if checkbox ans

        //0 ASR highlighting
        //if (e.Row.RowType == DataControlRowType.DataRow)
        //{
        //    double asr = 1;
        //    double.TryParse(e.Row.Cells[16].Text, out asr);
        //    Color redColor2 = ColorTranslator.FromHtml("#FF0000");
        //    if (asr <= 0)
        //    {
        //        e.Row.Cells[16].ForeColor = Color.White;
        //        e.Row.Cells[16].BackColor = redColor2;
        //        e.Row.Cells[16].Font.Bold = true;
        //    }
        //}

    }

    protected void CheckBoxViewIncomingRoute_CheckedChanged(object sender, EventArgs e)
    {
        DropDownListViewIncomingRoute.Enabled = CheckBoxViewIncomingRoute.Checked;
    }

    protected void CheckBoxViewOutgoingRoute_CheckedChanged(object sender, EventArgs e)
    {
        DropDownListViewOutgoingRoute.Enabled = CheckBoxViewOutgoingRoute.Checked;
    }

    protected void DropDownListViewIncomingRoute_SelectedChanged(object sender, EventArgs e)
    {
        setSwitchListDropDown(DropDownListViewIncomingRoute, EventArgs.Empty);
    }

    protected void setSwitchListDropDown(object sender, EventArgs e)
    {


        TelcobrightConfig tb = telcobrightConfig;
        tb.DatabaseSetting.DatabaseName = DropDownListViewIncomingRoute.SelectedValue;
        if (tb.DatabaseSetting.DatabaseName != "-1")
        {
            this.ViewBySwitch.Enabled = true;

            //this.ViewBySwitch.Checked = true;
            using (PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(tb.DatabaseSetting))
            {
                //populate switch
                List<ne> lstNe = context.nes.ToList();
                this.DropDownListShowBySwitch.Items.Clear();
                this.DropDownListShowBySwitch.Items.Add(new ListItem(" [All]", "-1"));
                foreach (ne nE in lstNe)
                {
                    if (!nE.SwitchName.Contains("dummy"))
                    {
                        this.DropDownListShowBySwitch.Items.Add(new ListItem(nE.SwitchName, nE.idSwitch.ToString()));
                    }

                }
            }
        }
        else
        {
            this.ViewBySwitch.Enabled = false;
            this.ViewBySwitch.Checked = false;
            this.DropDownListShowBySwitch.Enabled = false;
        }
    }


    protected void DropDownListPartner_OnSelectedIndexChanged(object sender, EventArgs e)
    {
        string logIdentityName = this.User.Identity.Name;
        String selectedIcx = logIdentityName;
        TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
        string selectedUserdbName;
        Dictionary<string, string> userVsDbName = telcobrightConfig.DeploymentProfile.UserVsDbName;
        if (userVsDbName.ContainsKey(logIdentityName))
        {
            selectedUserdbName = userVsDbName[logIdentityName];
        }
        else
        {
            selectedUserdbName = telcobrightConfig.DatabaseSetting.DatabaseName;
        }
        DropDownListViewIncomingRoute.Items.Clear();
        if (selectedUserdbName.Contains("btrc"))
        {
            foreach (var kv in telcobrightConfig.DeploymentProfile.UserVsDbName)
            {
                if (!kv.Value.Contains("btrc"))
                {
                    string username = kv.Key;
                    string dbNameAsRouteName = kv.Value;
                    string icxName = dbNameAsRouteName.Split('_')[0];
                    DropDownListViewIncomingRoute.Items.Add(new ListItem(icxName, dbNameAsRouteName));
                }

            }
        }
        else
        {
            string individualIcxName = selectedUserdbName.Split('_')[0];
            DropDownListViewIncomingRoute.Items.Add(new ListItem(individualIcxName, selectedUserdbName));

        }
    }

    protected void DropDownListIgw_OnSelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownListViewOutgoingRoute.Items.Clear();
        DropDownListViewOutgoingRoute.Items.Add(new ListItem("[All]", "-1"));
        if (DropDownListIgw.SelectedValue != String.Empty)
        {
            if (DropDownListIgw.SelectedValue == "-1")
            {
                using (PartnerEntities contex = new PartnerEntities())
                {
                    List<int> ansList = contex.partners.Where(c => c.PartnerType == 2).Select(c => c.idPartner).ToList();
                    foreach (route route in contex.routes.Where(x => ansList.Contains(x.idPartner)))
                    {
                        DropDownListViewOutgoingRoute.Items.Add(new ListItem($"{route.Description} ({route.RouteName})", route.RouteName));
                    }
                }
            }
            else
            {
                using (PartnerEntities contex = new PartnerEntities())
                {
                    int idPartner = Convert.ToInt32(DropDownListIgw.SelectedValue);
                    foreach (route route in contex.routes.Where(x => x.idPartner == idPartner))
                    {
                        DropDownListViewOutgoingRoute.Items.Add(new ListItem($"{route.Description} ({route.RouteName})", route.RouteName));
                    }
                }
            }
        }
    }
}
