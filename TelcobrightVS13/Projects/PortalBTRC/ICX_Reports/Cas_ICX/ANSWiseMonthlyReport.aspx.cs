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
    public partial class ANSWiseMonthlyReport : System.Web.UI.Page
    {
        static List<ANSWiseMonthlyReportBTRC> icxDailyInputs = new List<ANSWiseMonthlyReportBTRC>();
        static List<ANSWiseMonthlyReportBTRC> icxDailyInputsCalc = new List<ANSWiseMonthlyReportBTRC>();
        private static Dictionary<string, string> dbVSHostname = new Dictionary<string, string>();

        PartnerEntities context;
        telcobrightpartner thisPartner;
        TelcobrightConfig telcobrightConfig;
        string dbName { get; set; }
        DatabaseSetting databaseSetting;
        protected void Page_Load(object sender, EventArgs e)
        {
            telcobrightConfig = PageUtil.GetTelcobrightConfig();
            databaseSetting = telcobrightConfig.DatabaseSetting;

            string userName = Page.User.Identity.Name;

            if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
            {
                this.dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
            }
            else
            {
                this.dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
            }
            databaseSetting.DatabaseName = this.dbName;

            this.context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting);
            List<telcobrightpartner> telcoTelcobrightpartners = context.telcobrightpartners.ToList();
            thisPartner = telcoTelcobrightpartners.Where(c => c.databasename == dbName).ToList().First();

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
            icxDailyInputs = context.Database.SqlQuery<ANSWiseMonthlyReportBTRC>(sql).ToList();

            //string date1 = year1 + "-" + month1 + "-" + "01";
            //if (month1 == 12)
            //{
            //    month1 = 0;
            //    year1 += 1;
            //}
        }

        

        protected void GridViewSupplierRates_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header) {
                int i = (int)GridView2.Columns[2].ItemStyle.Width.Value; 
            }
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //LinkButton LinkButtonSubmit = (LinkButton)e.Row.FindControl("LinkButtonSubmit");
                //Label callDate = (Label)e.Row.FindControl("lblDate");
                //var icxData = icxDailyInputs.FirstOrDefault(item => item.submitted == "YES" && item.callDateSub == callDate.Text);

                //if (icxData != null && LinkButtonSubmit!= null && icxData.callDateSub == callDate.Text)
                //{ 
                //    if (LinkButtonSubmit.Enabled != false)
                //    {
                //        LinkButtonSubmit.Enabled = false;
                //        LinkButtonSubmit.Text = "Submitted";
                //        LinkButtonSubmit.ForeColor = System.Drawing.Color.Green;
                //    }

                //    if (LinkButtonSubmit.OnClientClick != null)
                //    {
                //        LinkButtonSubmit.OnClientClick = null;
                //    }
                //}
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                // Format the decimals with two decimal places
                decimal? sumDomIn = icxDailyInputs.Where(obj => obj.DomIn != null).Sum(obj => obj.DomIn);
                decimal? sumDomOut = icxDailyInputs.Where(obj => obj.DomOut != null).Sum(obj => obj.DomOut);


                decimal? sumIntIn = icxDailyInputs.Where(obj => obj.IntIn != null).Sum(obj => obj.IntIn);
                decimal? sumIntOut = icxDailyInputs.Where(obj => obj.IntOut != null).Sum(obj => obj.IntOut);





                // Format the decimals with two decimal places

                e.Row.Cells[0].Text = "Total";
                e.Row.Cells[1].Text = sumDomIn.HasValue ? sumDomIn.Value.ToString("F2") : string.Empty;
                e.Row.Cells[2].Text = sumDomOut.HasValue ? sumDomOut.Value.ToString("F2") : string.Empty;

                e.Row.Cells[3].Text = sumIntIn.HasValue ? sumIntIn.Value.ToString("F2") : string.Empty;
                e.Row.Cells[4].Text = sumIntOut.HasValue ? sumIntOut.Value.ToString("F2") : string.Empty;

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

            string localdb = $@"select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  {dbName}.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  {dbName}.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  {dbName}.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid ;
                            ";
            string livedb = $@"
                            select PartnerName AS PartnerName,
                            SUM(domIn) AS domIn,
                            SUM(domOut) AS domOut,
                            SUM(IntIn) AS IntIn,
                            SUM(Intout) AS Intout from
                        (
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  agni_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  agni_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  agni_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  agni_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  btcl_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  btcl_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  btcl_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  btcl_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  banglatelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  banglatelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  banglatelecom_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  banglatelecom_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  banglaicx_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  banglaicx_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  banglaicx_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  banglaicx_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  bantel_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  bantel_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  bantel_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  bantel_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  crossworld_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  crossworld_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  crossworld_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  crossworld_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  gazinetworks_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  gazinetworks_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  gazinetworks_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  imamnetwork_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  imamnetwork_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  imamnetwork_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  imamnetwork_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  sheba_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  sheba_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  sheba_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  sheba_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  jibondhara_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  jibondhara_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  jibondhara_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  jibondhara_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  mnh_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  mnh_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  mnh_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  mnh_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  mothertelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  mothertelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  mothertelecom_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  mothertelecom_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  newgenerationtelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  newgenerationtelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  newgenerationtelecom_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  newgenerationtelecom_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  paradise_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  paradise_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  paradise_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  paradise_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  purple_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  purple_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  purple_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  purple_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  ringtech_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  ringtech_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  ringtech_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  ringtech_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  srtelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  srtelecom_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  srtelecom_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  srtelecom_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  softex_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  softex_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  softex_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  softex_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  summit_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  summit_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  summit_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  summit_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  teleexchange_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  teleexchange_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  teleexchange_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  teleexchange_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from teleplusnewyork_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from teleplusnewyork_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from teleplusnewyork_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from teleplusnewyork_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid union all
                        select PartnerName, domIn, domOut, IntIn, Intout from (select PartnerName, idPartner, domIn, domOut, IntIn from (select partner.PartnerName, partner.idPartner idpartner, domIn, domOut from (select domOut.domOut, domIn.domIn, domIn.tup_outpartnerid from (select sum(duration1) domOut, tup_inpartnerid from  voicetel_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) domOut left join (select sum(duration1) domIn, tup_outpartnerid from  voicetel_cas.sum_voice_day_01 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) domIn on domOut.tup_inpartnerid = domIn.tup_outpartnerid) domInOut left join (select idPartner, partnerName from partner) partner on domInOut.tup_outpartnerid = partner.idPartner) domestic left join (select sum(duration1) IntIn, tup_outpartnerid from  voicetel_cas.sum_voice_day_03 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_outpartnerid) IntIn on domestic.idPartner = IntIn.tup_outpartnerid) domIntIn left join (select sum(duration3) IntOut, tup_inpartnerid from  voicetel_cas.sum_voice_day_02 where tup_starttime >= '{date1}' and tup_starttime < '{date2}' group by tup_inpartnerid) IntIn on domIntIn.idPartner = IntIn.tup_inpartnerid 

                        )x
                        group by PartnerName
                        order by PartnerName;
                            ";
            return livedb;
        }
    }

}
