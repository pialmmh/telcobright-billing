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

namespace PortalApp.ICX_Reports.Cas_ICX
{
    public partial class DailyInputICX : System.Web.UI.Page
    {
        static List<icxdailyinput> icxDailyInputs = new List<icxdailyinput>();
        static List<icxdailyinput> icxDailyInputsCalc = new List<icxdailyinput>();

        PartnerEntities context;
        telcobrightpartner thisPartner;
        List<telcobrightpartner> telcoTelcobrightpartners;
        TelcobrightConfig telcobrightConfig;
        DatabaseSetting databaseSetting;
        protected void Page_Load(object sender, EventArgs e)
        {
            telcobrightConfig = PageUtil.GetTelcobrightConfig();
            databaseSetting = telcobrightConfig.DatabaseSetting;
            telcobrightConfig = PageUtil.GetTelcobrightConfig();
            databaseSetting = telcobrightConfig.DatabaseSetting;

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

                foreach (string name in ICXName)
                {
                    DropDownICX.Items.Add(name);
                }

                string[] months = {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};
                foreach(string month in months)
                {
                    DropDownMonth.Items.Add(month);
                }

                // initializing default value to dropdown
                DropDownYear.SelectedValue = DateTime.Now.Year.ToString(); ;
                DropDownMonth.SelectedValue = months[DateTime.Now.Month - 1];

                //Dictionary<string, int> monthDictionary = months.ToDictionary(month => month, month => Array.IndexOf(months, month) + 1);

                //int year1 = int.Parse(DropDownYear.SelectedValue);
                //int month1 = monthDictionary[DropDownMonth.SelectedValue];

                //string date = year1+"-"+month1+"-"+"01";
                //string sql = getSqlQuery(year1, month1);
                ////sthis.icxDailyInputs = context.Database.SqlQuery<icxdailyinput>(sql).ToList();
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



            string dbName = ICXDictionary[DropDownICX.SelectedItem.ToString()];
            string date = year1 + "-" + month1 + "-" + "01";
            string sql = getSqlQuery(year1, month1, dbName);
            icxDailyInputsCalc = context.Database.SqlQuery<icxdailyinput>(sql).ToList();

            string date1 = year1 + "-" + month1 + "-" + "01";
            if (month1 == 12)
            {
                month1 = 0;
                year1 += 1;
            }
            string date2 = year1 + "-" + (month1+1) + "-" + "01";
            string sqlDailyInput = $@"select date_format(callDate,'%Y-%m-%d') callDateICX, date_format(callDate,'%Y-%m-%d') callDateSub, DomesticICX, LtfsICX, IntInICX, IntOutICX, CasDomSub, CasLtfsSub, CasIntInSub, CasIntOutSub, submitted from {dbName}.icxdailyinput where callDate >= '{date1}' and calldate < '{date2}';";
            icxDailyInputs = context.Database.SqlQuery<icxdailyinput>(sqlDailyInput).ToList();

            //Merging both object
            for (int i = 0; i < icxDailyInputs.Count; i++)
            {
                var icxInput = icxDailyInputs[i];
                for (int j = 0; j < icxDailyInputsCalc.Count; j++)
                {
                    var calcInput = icxDailyInputsCalc[j];
                    if (icxInput.callDateICX == calcInput.callDateCalc)
                    {
                        if(icxInput.callDateICX == calcInput.callDateCalc)
                        {
                            icxDailyInputs[i].DomesticDurationCalc = icxDailyInputsCalc[j].DomesticDurationCalc;
                            icxDailyInputs[i].LtfsDurationCalc = icxDailyInputsCalc[j].LtfsDurationCalc;
                            icxDailyInputs[i].IntlInDurationCalc = icxDailyInputsCalc[j].IntlInDurationCalc;
                            icxDailyInputs[i].IntlOutDurationCalc = icxDailyInputsCalc[j].IntlOutDurationCalc;
                        }
                    }
                }
            }

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
           

                // Accumulate the values from each row
                decimal DomesticCAS = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "DomesticDurationCalc"));
                decimal DomesticICX = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "DomesticICX"));
                decimal DiffDomestic = DomesticCAS - DomesticICX;

                decimal IntlInDurationCAS = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "IntlInDurationCalc"));
                decimal IntInICX = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "IntInICX"));
                decimal DiffIntIn = IntlInDurationCAS - IntInICX;

                decimal IntlOutDurationCAS = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "IntlOutDurationCalc"));
                decimal IntOutICX = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "IntOutICX"));
                decimal DiffIntOut = IntlOutDurationCAS - IntOutICX;


                // ... Add more columns as needed

                // Store the accumulated values in ViewState                
                ViewState["DomTotalCas"] = Convert.ToDecimal(ViewState["DomTotalCas"]) + DomesticCAS;
                ViewState["DomTotalICX"] = Convert.ToDecimal(ViewState["DomTotalICX"]) + DomesticICX;
                ViewState["DiffDomestic"] = Convert.ToDecimal(ViewState["DiffDomestic"]) + DiffDomestic;

                ViewState["IntInTotalCas"] = Convert.ToDecimal(ViewState["IntInTotalCas"]) + IntlInDurationCAS;
                ViewState["IntInTotalICX"] = Convert.ToDecimal(ViewState["IntInTotalICX"]) + IntInICX;
                ViewState["DiffIntIn"] = Convert.ToDecimal(ViewState["DiffIntIn"]) + DiffIntIn;

                ViewState["IntOutTotalCas"] = Convert.ToDecimal(ViewState["IntOutTotalCas"]) + IntlOutDurationCAS;
                ViewState["IntOutTotalICX"] = Convert.ToDecimal(ViewState["IntOutTotalICX"]) + IntOutICX;
                ViewState["DiffIntOut"] = Convert.ToDecimal(ViewState["DiffIntOut"]) + DiffIntOut;

                // ... Add more columns as needed
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                // Assuming you have stored values in these string variables
                string domTotalCasString = ViewState["DomTotalCas"].ToString();
                string domTotalICXString = ViewState["DomTotalICX"].ToString();
                string DiffDomtring = ViewState["DiffIntIn"].ToString();

                string intInTotalCasString = ViewState["IntInTotalCas"].ToString();
                string intInTotalICXString = ViewState["IntInTotalICX"].ToString();
                string DiffIntIntring = ViewState["DiffIntIn"].ToString();

                string intOutTotalCasString = ViewState["IntOutTotalCas"].ToString();
                string intOutTotalICXString = ViewState["IntOutTotalICX"].ToString();
                string DiffIntOurtring = ViewState["DiffIntOut"].ToString();

                // Convert strings to decimals and format with two decimal places
                decimal domTotalCas;
                decimal domTotalICX;
                decimal domDiff;

                decimal intInTotalCas;
                decimal intInTotalICX;
                decimal domIntIn;

                decimal intOutTotalCas;
                decimal intOutTotalICX;
                decimal domIntOut;

                decimal.TryParse(domTotalCasString, out domTotalCas);
                decimal.TryParse(domTotalICXString, out domTotalICX);
                decimal.TryParse(DiffDomtring, out domDiff);

                decimal.TryParse(intInTotalCasString, out intInTotalCas);
                decimal.TryParse(intInTotalICXString, out intInTotalICX);
                decimal.TryParse(DiffIntIntring, out domIntIn);

                decimal.TryParse(intOutTotalCasString, out intOutTotalCas);
                decimal.TryParse(intOutTotalICXString, out intOutTotalICX);
                decimal.TryParse(DiffIntOurtring, out domIntOut);


                // Format the decimals with two decimal places
                e.Row.Cells[0].Text = "Summation";
                e.Row.Cells[1].Text = domTotalCas.ToString("F2");
                e.Row.Cells[2].Text = domTotalICX.ToString("F2");
                e.Row.Cells[3].Text = domDiff.ToString("F2");

                e.Row.Cells[4].Text = intInTotalCas.ToString("F2");
                e.Row.Cells[5].Text = intInTotalICX.ToString("F2");
                e.Row.Cells[6].Text = domIntIn.ToString("F2");

                e.Row.Cells[7].Text = intOutTotalCas.ToString("F2");
                e.Row.Cells[8].Text = intOutTotalICX.ToString("F2");
                e.Row.Cells[9].Text = domIntOut.ToString("F2");


                //e.Row.Cells[10].Text = domIntOut.ToString("F2");


            }

        }
        string getSqlQuery(int year, int month, string dbName)
        {
            string date1 = year + "-" + month + "-" + "01";
            if (month == 12)
            {
                month = 0;
                year += 1;
            }
            string date2 = year + "-" + (month+1) + "-" + "01";

            return $@"select date_format(tup_starttime,'%Y-%m-%d') callDateCalc, domestic DomesticDurationCalc, intOut IntlOutDurationCalc, intIn IntlInDurationCalc  from
                        (
	                        select tup_starttime, domestic, intOut from
		                        (select tup_starttime, sum(duration1) domestic 
			                        from {dbName}.sum_voice_day_01 
			                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
			                        group by tup_starttime
		                        ) dom
			
		                        left join
		
		                        (select tup_starttime tup_starttime1, sum(duration1) intOut
			                        from {dbName}.sum_voice_day_02 
			                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
			                        group by tup_starttime1
		                        ) intOut    
		                        on dom.tup_starttime = intOut.tup_starttime1 
                        )domIntOut

                        left join 
                        (
	                        select tup_starttime tup_starttime3, sum(duration1) intIn
		                        from {dbName}.sum_voice_day_03
		                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
		                        group by tup_starttime3
                        ) intIn

                        on domIntOut.tup_starttime = intIn.tup_starttime3;";
        }
    }

}
