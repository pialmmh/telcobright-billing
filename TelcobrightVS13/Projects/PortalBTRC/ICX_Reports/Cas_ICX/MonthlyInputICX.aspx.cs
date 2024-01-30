using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TelcobrightMediation.Config;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using TelcobrightMediation;
using InstallConfig;

namespace PortalApp.ICX_Reports.Cas_ICX
{
    public partial class MonthlyInputICX : System.Web.UI.Page
    {
        static List<icxdailyinput> icxDailyInputs = new List<icxdailyinput>();
        //static List<icxdailyinput> icxDailyInputsCalc = new List<icxdailyinput>();
        private static Dictionary<string, string> dbVSHostname = new Dictionary<string, string>();

        PartnerEntities context;
        telcobrightpartner thisPartner;
        List<telcobrightpartner> telcoTelcobrightpartners;
        TelcobrightConfig telcobrightConfig;
        DatabaseSetting databaseSetting;
        protected void Page_Load(object sender, EventArgs e)
        {
            telcobrightConfig = PageUtil.GetTelcobrightConfig();
            databaseSetting = telcobrightConfig.DatabaseSetting;
            string dbName;
            string userName = Page.User.Identity.Name;
            if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
            {
                dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
            }
            else
            {
                dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
            }

            dbVSHostname = CasDockerDbHelper.IcxVsdbHostNames;

            databaseSetting.DatabaseName = dbName;
            //databaseSetting.ServerName = dbVSHostname[dbName];
            databaseSetting.ServerName = "localhost";

            context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting);
            telcoTelcobrightpartners = context.telcobrightpartners.ToList();
            this.thisPartner = telcoTelcobrightpartners.Where(c => c.databasename == dbName).ToList().First();


            if (!IsPostBack)
            {
                string[] ICXName =
                                        {
                                            "Agni ICX",
                                            "BTCL",
                                            "Bangla ICX",
                                            "Bangla Telecom Ltd",
                                            "Bantel Limited",
                                            "Cross World Telecom Limited",
                                            "GETCO Telecommunications Ltd",
                                            "Gazi Networks Limited",
                                            "Imam Network Ltd",
                                            "Integrated Services Limited (Sheba ICX)",
                                            "JibonDhara Solutions Limited",
                                            "M&H Telecom Limited",
                                            "Mother Telecommunication",
                                            "New Generation Telecom Limited",
                                            "Paradise Telecom Limited",
                                            "Purple Telecom Limited",
                                            "RingTech(Bangladesh) Limited",
                                            "SR Telecom Limited",
                                            "Softex communication Ltd",
                                            "Summit Communications Ltd (Vertex)",
                                            "Tele Exchange Limited",
                                            "Teleplus Newyork Limited",
                                            "Voicetel Ltd"
                                        };


                for (int year = 2023; year <= 2031; year++)
                {
                    DropDownYear.Items.Add(year.ToString());
                }

                //foreach (string name in ICXName)
                //{
                //    DropDownICX.Items.Add(name);
                //}

                string[] months = {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};
                foreach(string month in months)
                {
                    DropDownMonth.Items.Add(month);
                }

                // initializing default value to dropdown
                DropDownYear.SelectedValue = DateTime.Now.Year.ToString(); ;
                DropDownMonth.SelectedValue = months[DateTime.Now.Month - 1];

                LoadData();
                GridViewDataBound();

            }
        }
        

      
        
        void GridViewDataBound()
        {
            GridView2.DataSource = icxDailyInputs;
            //LoadData();
            GridView2.DataBind();
        }
        

        protected void ddlMonth_SelectedIndexChanged(object sender, EventArgs e)
        {

            LoadData();
            GridViewDataBound();
        }
        
        void LoadData()
        {
            string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            Dictionary<string, int> monthDictionary = months.ToDictionary(month => month, month => Array.IndexOf(months, month) + 1);

            //DateTime currentDate = DateTime.Now;
            int year1 = int.Parse(DropDownYear.SelectedValue);
            int month1 = monthDictionary[DropDownMonth.SelectedValue];
            Dictionary<string, string> ICXDictionary = new Dictionary<string, string>
                                                            {
                                                                { "Agni ICX", "agni_cas" },
                                                                { "BTCL", "btcl_cas" },
                                                                { "Bangla ICX", "banglaicx_cas" },
                                                                { "Bangla Telecom Ltd", "banglatelecom_cas" },
                                                                { "Bantel Limited", "bantel_cas" },
                                                                { "Cross World Telecom Limited", "crossworld_cas" },
                                                                { "GETCO Telecommunications Ltd", "getco_cas" },
                                                                { "Gazi Networks Limited", "gazinetworks_cas" },
                                                                { "Imam Network Ltd", "imamnetwork_cas" },
                                                                { "Integrated Services Limited (Sheba ICX)", "sheba_cas" },
                                                                { "JibonDhara Solutions Limited", "jibondhara_cas" },
                                                                { "M&H Telecom Limited", "mnh_cas" },
                                                                { "Mother Telecommunication", "mothertelecom_cas" },
                                                                { "New Generation Telecom Limited", "newgenerationtelecom_cas" },
                                                                { "Paradise Telecom Limited", "paradise_cas" },
                                                                { "Purple Telecom Limited", "purple_cas" },
                                                                { "RingTech(Bangladesh) Limited", "ringtech_cas" },
                                                                { "SR Telecom Limited", "srtelecom_cas" },
                                                                { "Softex communication Ltd", "softex_cas" },
                                                                { "Summit Communications Ltd (Vertex)", "summit_cas" },
                                                                { "Tele Exchange Limited", "teleexchange_cas" },
                                                                { "Teleplus Newyork Limited", "teleplusnetwork_cas" },
                                                                { "Voicetel Ltd", "voicetel_cas" }
                                                            };



            //string dbName = ICXDictionary[DropDownICX.SelectedItem.ToString()];
            //string date = year1 + "-" + month1 + "-" + "01";
            string sql = getSqlQuery(year1, month1);
            icxDailyInputs = context.Database.SqlQuery<icxdailyinput>(sql).ToList();

            //string date1 = year1 + "-" + month1 + "-" + "01";
            //if (month1 == 12)
            //{
            //    month1 = 0;
            //    year1 += 1;
            //}

            Dictionary<string, icxdailyinput> reportAsDic = icxDailyInputs.ToDictionary (r => r.callDateCalc);
        }

        

        protected void GridViewSupplierRates_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header) {
                int i = (int)GridView2.Columns[2].ItemStyle.Width.Value; 
            }
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton LinkButtonSubmit = (LinkButton)e.Row.FindControl("LinkButtonSubmit");
                Label callDate = (Label)e.Row.FindControl("lblDate");
                var icxData = icxDailyInputs.FirstOrDefault(item => item.submitted == "YES" && item.callDateSub == callDate.Text);

                if (icxData != null && LinkButtonSubmit!= null && icxData.callDateSub == callDate.Text)
                { 
                    if (LinkButtonSubmit.Enabled != false)
                    {
                        LinkButtonSubmit.Enabled = false;
                        LinkButtonSubmit.Text = "Submitted";
                        LinkButtonSubmit.ForeColor = System.Drawing.Color.Green;
                    }

                    if (LinkButtonSubmit.OnClientClick != null)
                    {
                        LinkButtonSubmit.OnClientClick = null;
                    }
                }
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                // Format the decimals with two decimal places
                decimal? sumDom = icxDailyInputs.Where(obj => obj.DomesticDurationCalc != null).Sum(obj => obj.DomesticDurationCalc);
                decimal? sumIntIn = icxDailyInputs.Where(obj => obj.IntlInDurationCalc != null).Sum(obj => obj.IntlInDurationCalc);
                decimal? sumIntOut = icxDailyInputs.Where(obj => obj.IntlOutDurationCalc != null).Sum(obj => obj.IntlOutDurationCalc);

                decimal sumDomICX = icxDailyInputs.Sum(obj => obj.DomesticICX);
                decimal sumIntInICX = icxDailyInputs.Sum(obj => obj.IntInICX);
                decimal sumIntOutICX = icxDailyInputs.Sum(obj => obj.IntOutICX);




                // Format the decimals with two decimal places

                e.Row.Cells[0].Text = "Total";
                e.Row.Cells[1].Text = sumDom.HasValue ? sumDom.Value.ToString("F2") : string.Empty;
                e.Row.Cells[2].Text = sumDomICX.ToString("F2");
                e.Row.Cells[3].Text = sumDom.HasValue ? (sumDom - sumDomICX).Value.ToString("F2") : (0 - sumDomICX).ToString("F2");

                e.Row.Cells[4].Text = sumIntIn.HasValue ? sumIntIn.Value.ToString("F2") : string.Empty;
                e.Row.Cells[5].Text = sumIntInICX.ToString("F2");
                e.Row.Cells[6].Text = sumIntIn.HasValue ? (sumIntIn - sumIntInICX).Value.ToString("F2") : (0 - sumIntInICX).ToString("F2");

                e.Row.Cells[7].Text = sumIntOut.HasValue ? sumIntOut.Value.ToString("F2") : string.Empty;
                e.Row.Cells[8].Text = sumIntOutICX.ToString("F2");
                e.Row.Cells[9].Text = sumIntOut.HasValue ? (sumIntOut - sumIntOutICX).Value.ToString("F2") : (0 - sumIntOutICX).ToString("F2");



                //e.Row.Cells[10].Text = domIntOut.ToString("F2");


            }

        }
        string getSqlQuery(int year, int month)
        {
            string date1 = year + "-" + month + "-" + "01";
            if (month == 12)
            {
                month = 0;
                year += 1;
            }
            string date2 = year + "-" + (month+1) + "-" + "01";

            string localdb = $@"select 'Gazi Networks Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   gazinetworks_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   gazinetworks_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2";
            string livedb = $@"
select 'Agni ICX' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  agni_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   agni_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   agni_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'BTCL' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  btcl_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   btcl_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   btcl_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Bangla ICX' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  banglatelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   banglatelecom_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   banglatelecom_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Bangla Telecom Ltd' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  banglaicx_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   banglaicx_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   banglaicx_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Bantel Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  bantel_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   bantel_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   bantel_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Cross World Telecom Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  crossworld_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   crossworld_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   crossworld_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Gazi Networks Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   gazinetworks_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   gazinetworks_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Imam Network Ltd' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  imamnetwork_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   imamnetwork_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   imamnetwork_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Integrated Services Limited (Sheba ICX)' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  sheba_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   sheba_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   sheba_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'JibonDhara Solutions Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  jibondhara_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   jibondhara_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   jibondhara_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'M&H Telecom Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  mnh_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   mnh_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   mnh_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Mother Telecommunication' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  mothertelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   mothertelecom_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   mothertelecom_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'New Generation Telecom Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  newgenerationtelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   newgenerationtelecom_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   newgenerationtelecom_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Paradise Telecom Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  paradise_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   paradise_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   paradise_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Purple Telecom Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  purple_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   purple_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   purple_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'RingTech(Bangladesh) Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  ringtech_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   ringtech_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   ringtech_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'SR Telecom Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  srtelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   srtelecom_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   srtelecom_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Softex communication Ltd' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  softex_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   softex_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   softex_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Summit Communications Ltd (Vertex)' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  summit_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   summit_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   summit_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Tele Exchange Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  teleexchange_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   teleexchange_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   teleexchange_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Teleplus Newyork Limited' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from teleplusnewyork_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from  teleplusnewyork_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from  teleplusnewyork_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 union all
select 'Voicetel Ltd' as callDateCalc, domestic DomesticDurationCalc, intIn IntlOutDurationCalc, intOut IntlInDurationCalc  from(select yearMonth, domestic, intIn from(select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth, sum(duration1) domestic from  voicetel_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}'  group by concat(year(tup_starttime),'-' ,month(tup_starttime))) dom left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth1, sum(duration1) IntIn from   voicetel_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intIn  on dom.yearMonth = intIn.yearMonth1 )domIntOut left join (select concat(year(tup_starttime),'-' ,month(tup_starttime)) as yearMonth2, sum(duration1) IntOut from   voicetel_cas.sum_voice_day_02 where tup_starttime >= '' and tup_starttime < '{date2}' group by concat(year(tup_starttime),'-' ,month(tup_starttime))) intOut on domIntOut.yearMonth = intOut.yearMonth2 
";
            return livedb;
        }
    }

}
