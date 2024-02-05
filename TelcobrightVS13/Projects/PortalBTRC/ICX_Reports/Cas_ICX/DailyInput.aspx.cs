using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LibraryExtensions;
using TelcobrightMediation.Config;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using InstallConfig;

namespace PortalApp.ICX_Reports.Cas_ICX
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        static List<icxdailyinput> icxDailyInputs = new List<icxdailyinput>();
        static List<icxdailyinput> icxDailyInputsCalc = new List<icxdailyinput>();
        private static Dictionary<string, string> dbVSHostname = new Dictionary<string, string>();

        PartnerEntities context;
        telcobrightpartner thisPartner;
        List<telcobrightpartner> telcoTelcobrightpartners;
        TelcobrightConfig telcobrightConfig;
        DatabaseSetting databaseSetting;
        protected void Page_Load(object sender, EventArgs e)
        {
            //Docker connection
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
            databaseSetting.ServerName = dbVSHostname[dbName];
            //databaseSetting.ServerName = "localhost";


            context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting);
            telcoTelcobrightpartners = context.telcobrightpartners.ToList();
            this.thisPartner = telcoTelcobrightpartners.Where(c => c.databasename == dbName).ToList().First();


            if (!IsPostBack)
            {
                for (int year = 2023; year <= 2031; year++)
                {
                    DropDownYear.Items.Add(year.ToString());
                }

                string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                foreach (string month in months)
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




        protected void GridViewRowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow row = this.GridView2.Rows[e.RowIndex];
                decimal domDuration, intInDuration, intOutDuration;
                string domDur = ((TextBox)row.FindControl("ICXDomInput")).Text;
                string intInDur = ((TextBox)row.FindControl("ICXIntInInput")).Text;
                string intOutDur = ((TextBox)row.FindControl("ICXIntOutInput")).Text;
                string callDate = ((Label)row.FindControl("lblDate")).Text;


                if (!string.IsNullOrWhiteSpace(domDur))
                {
                    if (decimal.TryParse(domDur, out domDuration))
                    {
                        string updateSql = $@"update icxdailyinput set DomesticICX = {domDuration} where callDate='{callDate}';";
                        context.Database.ExecuteSqlCommand(updateSql);
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Input value not in correct format');", true);
                        throw new Exception("Domestic not in correct format");
                    }
                }

                if (!string.IsNullOrWhiteSpace(intInDur))
                {
                    if (decimal.TryParse(intInDur, out intInDuration))
                    {
                        string updateSql = $@"update icxdailyinput set IntInICX = {intInDuration} where callDate='{callDate}';";
                        context.Database.ExecuteSqlCommand(updateSql);
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Input value not in correct format');", true);
                        throw new Exception("In.tIn. not in correct format");
                    }
                }

                if (!string.IsNullOrWhiteSpace(intOutDur))
                {
                    if (decimal.TryParse(intOutDur, out intOutDuration))
                    {
                        string updateSql = $@"update icxdailyinput set IntOutICX = {intOutDuration} where callDate='{callDate}';";
                        context.Database.ExecuteSqlCommand(updateSql);
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Input value not in correct format');", true);
                        throw new Exception("Int.Out. not in correct format");
                    }
                }

            }
            catch (Exception e1)
            {
                Console.WriteLine("Row Update error");
            }
            this.GridView2.EditIndex = -1;
            LoadData();
            GridViewDataBound();
        }
        protected void GridViewRowEditing(object sender, GridViewEditEventArgs e)
        {
            this.GridView2.EditIndex = e.NewEditIndex;
            //LoadData();
            GridViewDataBound();
        }
        protected void GridViewRowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.GridView2.EditIndex = -1;
            //LoadData();
            GridViewDataBound();
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

            string date = year1 + "-" + month1 + "-" + "01";
            string sql = getSqlQuery(year1, month1);
            icxDailyInputsCalc = context.Database.SqlQuery<icxdailyinput>(sql).ToList();

            string date1 = year1 + "-" + month1 + "-" + "01";
            if (month1 == 12)
            {
                month1 = 0;
                year1 += 1;
            }
            string date2 = year1 + "-" + (month1 + 1) + "-" + "01";
            string sqlDailyInput = $@"select date_format(callDate,'%Y-%m-%d') callDateICX, date_format(callDate,'%Y-%m-%d') callDateSub, DomesticICX, LtfsICX, IntInICX, IntOutICX, CasDomSub, CasLtfsSub, CasIntInSub, CasIntOutSub, submitted from icxdailyinput where callDate >= '{date1}' and calldate < '{date2}';";
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
                        if (icxInput.callDateICX == calcInput.callDateCalc)
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

        protected void GridViewRowSubmitting(object sender, GridViewDeleteEventArgs e)
        {

            GridViewRow row = this.GridView2.Rows[e.RowIndex];
            string callDate = ((Label)row.FindControl("lblDate")).Text;

            foreach (var icxDailyinput in icxDailyInputs)
            {
                if (icxDailyinput.callDateSub == callDate && icxDailyinput.submitted == "NO")
                {
                    decimal casDomSub = decimal.Parse(((Label)row.FindControl("lblCasDomestic")).Text);
                    //decimal casLtfsSub = decimal.Parse(((Label)row.FindControl("lblCasLtfs")).Text);
                    decimal casIntInSub = decimal.Parse(((Label)row.FindControl("lblCasIntIn")).Text);
                    decimal casIntOutSub = decimal.Parse(((Label)row.FindControl("lblCasIntOut")).Text);

                    string submitSql = $@"update icxdailyinput set CasDomSub = {casDomSub}, CasIntInSub = {casIntInSub}, CasIntOutSub = {casIntOutSub}, submitted = 'YES' where callDate= '{callDate}';";
                    context.Database.ExecuteSqlCommand(submitSql);
                }
            }
            LoadData();
            GridViewDataBound();
        }

        protected void GridViewSupplierRates_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                int i = (int)GridView2.Columns[2].ItemStyle.Width.Value;
            }
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton LinkButtonSubmit = (LinkButton)e.Row.FindControl("LinkButtonSubmit");
                Label callDate = (Label)e.Row.FindControl("lblDate");
                var icxData = icxDailyInputs.FirstOrDefault(item => item.submitted == "YES" && item.callDateSub == callDate.Text);

                if (icxData != null && LinkButtonSubmit != null && icxData.callDateSub == callDate.Text)
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

                decimal? sumDom = icxDailyInputs.Where(obj => obj.DomesticDurationCalc != null).Sum(obj => obj.DomesticDurationCalc);
                decimal? sumIntIn = icxDailyInputs.Where(obj => obj.IntlInDurationCalc != null).Sum(obj => obj.IntlInDurationCalc);
                decimal? sumIntOut = icxDailyInputs.Where(obj => obj.IntlOutDurationCalc != null).Sum(obj => obj.IntlOutDurationCalc);

                decimal sumDomICX = icxDailyInputs.Sum(obj => obj.DomesticICX);
                decimal sumIntInICX = icxDailyInputs.Sum(obj => obj.IntInICX);
                decimal sumIntOutICX = icxDailyInputs.Sum(obj => obj.IntOutICX);




                // Format the decimals with two decimal places
                e.Row.Cells[1].Text = "Total";
                e.Row.Cells[2].Text = sumDom.HasValue ? sumDom.Value.ToString("F2") : string.Empty;
                e.Row.Cells[3].Text = sumDomICX.ToString("F2");
                e.Row.Cells[4].Text = sumDom.HasValue ? (sumDom - sumDomICX).Value.ToString("F2") : (0 - sumDomICX).ToString("F2");

                e.Row.Cells[5].Text = sumIntIn.HasValue ? sumIntIn.Value.ToString("F2") : string.Empty;
                e.Row.Cells[6].Text = sumIntInICX.ToString("F2");
                e.Row.Cells[7].Text = sumIntIn.HasValue ? (sumIntIn - sumIntInICX).Value.ToString("F2") : (0 - sumIntInICX).ToString("F2");

                e.Row.Cells[8].Text = sumIntOut.HasValue ? sumIntOut.Value.ToString("F2") : string.Empty;
                e.Row.Cells[9].Text = sumIntOutICX.ToString("F2");
                e.Row.Cells[10].Text = sumIntOut.HasValue ? (sumIntOut - sumIntOutICX).Value.ToString("F2") : (0 - sumIntOutICX).ToString("F2");


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
            string date2 = year + "-" + (month + 1) + "-" + "01";

            return $@"select date_format(tup_starttime,'%Y-%m-%d') callDateCalc, domestic DomesticDurationCalc, intOut IntlOutDurationCalc, intIn IntlInDurationCalc  from
                        (
	                        select tup_starttime, domestic, intOut from
		                        (select tup_starttime, sum(duration1)/60 domestic 
			                        from sum_voice_day_01 
			                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
			                        group by tup_starttime
		                        ) dom
			
		                        left join
		
		                        (select tup_starttime tup_starttime1, sum(duration1)/60 intOut
			                        from sum_voice_day_02 
			                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
			                        group by tup_starttime1
		                        ) intOut    
		                        on dom.tup_starttime = intOut.tup_starttime1 
                        )domIntOut

                        left join 
                        (
	                        select tup_starttime tup_starttime3, sum(duration1)/60 intIn
		                        from sum_voice_day_03
		                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
		                        group by tup_starttime3
                        ) intIn

                        on domIntOut.tup_starttime = intIn.tup_starttime3;";
        }
    }

}
