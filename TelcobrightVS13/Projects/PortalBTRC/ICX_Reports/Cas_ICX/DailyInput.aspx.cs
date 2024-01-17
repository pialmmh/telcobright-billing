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
    public partial class WebForm1 : System.Web.UI.Page
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
                for(int year = 2023; year <= 2031; year++)
                {
                    DropDownYear.Items.Add(year.ToString());
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
                GridViewDataBound();

            }
        }
        

        //void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.Header)
        //    {
        //        e.Row.Visible = false;
        //        //AddHeaderRow1();
        //        //AddHeaderRow2();
        //    }
        //}

        //void AddHeaderRow1()
        //{
        //    GridViewRow gr = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Insert);

        //    TableHeaderCell thc0 = new TableHeaderCell();
        //    TableHeaderCell thc1 = new TableHeaderCell();
        //    TableHeaderCell thc2 = new TableHeaderCell();
        //    //TableHeaderCell thc3 = new TableHeaderCell();
        //    TableHeaderCell thc4 = new TableHeaderCell();
        //    TableHeaderCell thc5 = new TableHeaderCell();


        //    thc0.Text = "";
        //    thc1.Text = "";
        //    thc2.Text = "Domestic Minutes";
        //    //thc3.Text = "LTFS Minutes";
        //    thc4.Text = "Int. In Minutes";
        //    thc5.Text = "Int. Out Minutes";

        //    thc1.ColumnSpan = 1;
        //    thc2.ColumnSpan = 3;
        //    //thc3.ColumnSpan = 3;
        //    thc4.ColumnSpan = 3;
        //    thc5.ColumnSpan = 3;




        //    gr.Cells.AddRange(new TableCell[] {thc0, thc1, thc2, thc4, thc5});

        //    GridView2.Controls[0].Controls.AddAt(0, gr);
        //}

        //void AddHeaderRow2()
        //{
        //    GridViewRow gr = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Insert);

        //    TableHeaderCell thc0 = new TableHeaderCell();
        //    TableHeaderCell thc1 = new TableHeaderCell();
        //    TableHeaderCell thc2 = new TableHeaderCell();
        //    TableHeaderCell thc3 = new TableHeaderCell();
        //    TableHeaderCell thc4 = new TableHeaderCell();
        //    //TableHeaderCell thc5 = new TableHeaderCell();
        //    //TableHeaderCell thc6 = new TableHeaderCell();
        //    //TableHeaderCell thc7 = new TableHeaderCell();
        //    TableHeaderCell thc8 = new TableHeaderCell();
        //    TableHeaderCell thc9 = new TableHeaderCell();
        //    TableHeaderCell thc10 = new TableHeaderCell();
        //    TableHeaderCell thc11 = new TableHeaderCell();
        //    TableHeaderCell thc12 = new TableHeaderCell();
        //    TableHeaderCell thc13 = new TableHeaderCell();

        //    thc0.Text = "Action";
        //    thc1.Text = "Date";
        //    thc2.Text = "CAS";
        //    thc3.Text = "ICX";
        //    thc4.Text = "Difference";
        //    //thc5.Text = "CAS";
        //    //thc6.Text = "ICX";
        //    //thc7.Text = "Difference";
        //    thc8.Text = "CAS";
        //    thc9.Text = "ICX";
        //    thc10.Text = "Difference";
        //    thc11.Text = "CAS";
        //    thc12.Text = "ICX";
        //    thc13.Text = "Difference";



        //    gr.Cells.AddRange(new TableCell[] {thc0, thc1, thc2, thc3, thc4,  thc8, thc9, thc10, thc11, thc12, thc13 });

        //    GridView2.Controls[0].Controls.AddAt(1, gr);
        //}

        

        //protected void UpdateButton_Click(object sender, EventArgs e)
        //{
        //    foreach (GridViewRow row in GridView2.Rows)
        //    {
        //        TextBox textBox = (TextBox)row.FindControl("ICXDomInput");

        //        if (textBox != null)
        //        {
        //            string value = textBox.Text;
        //            // Do something with the value, e.g., print it or use it in further processing
        //            Response.Write("Value from ICXDomInput: " + value);
        //        }
        //    }
        //}

       

        //protected void GridView2_RowCommand(object sender, GridViewCommandEventArgs e)
        //{
        //    if (e.CommandName == "SubmitReport")
        //    {
        //        int rowIndex = Convert.ToInt32(e.CommandArgument);
        //        GridViewRow row = GridView2.Rows[rowIndex];

        //        TextBox icxTextBox = (TextBox)row.FindControl("ICXDomInput");

        //        if (icxTextBox != null)
        //        {
        //            string cellValue = icxTextBox.Text;

        //            // Process or store the cell value as needed
        //            Console.WriteLine("Cell Value: " + cellValue);
        //        }
        //    }
        //}


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
                        throw new Exception("Int.Out. not in correct format");
                    }
                }
                
            }
            catch (Exception e1)
            {
                Console.WriteLine("Row Update error");
            }
            this.GridView2.EditIndex = -1;
            //LoadData();
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
        bool alreadyRowUpdated(string callDate)
        {
            icxDailyInputs = icxDailyInputs;  
            //if(icxDailyInputs)

            return false;
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
            string date2 = year1 + "-" + (month1+1) + "-" + "01";
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

        protected void GridViewRowSubmitting(object sender, GridViewDeleteEventArgs e)
        {

            GridViewRow row = this.GridView2.Rows[e.RowIndex];
            string callDate = ((Label)row.FindControl("lblDate")).Text;

            foreach(var icxDailyinput in icxDailyInputs)
            {
                if(icxDailyinput.callDateSub == callDate && icxDailyinput.submitted == "NO")
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

            return $@"select date_format(tup_starttime,'%Y-%m-%d') callDateCalc, domestic DomesticDurationCalc, intOut IntlOutDurationCalc, intIn IntlInDurationCalc  from
                        (
	                        select tup_starttime, domestic, intOut from
		                        (select tup_starttime, sum(duration1) domestic 
			                        from sum_voice_day_01 
			                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
			                        group by tup_starttime
		                        ) dom
			
		                        left join
		
		                        (select tup_starttime tup_starttime1, sum(duration1) intOut
			                        from sum_voice_day_02 
			                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
			                        group by tup_starttime1
		                        ) intOut    
		                        on dom.tup_starttime = intOut.tup_starttime1 
                        )domIntOut

                        left join 
                        (
	                        select tup_starttime tup_starttime3, sum(duration1) intIn
		                        from sum_voice_day_03
		                        where tup_starttime >= '{date1}' and tup_starttime < '{date2}' 
		                        group by tup_starttime3
                        ) intIn

                        on domIntOut.tup_starttime = intIn.tup_starttime3;";
        }
    }

}
