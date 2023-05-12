using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using reports;
using ExportToExcel;
using MediationModel;
using LibraryExtensions;
using PortalApp.ReportHelper;
public partial class DefaultRptDomesticWeeklyIcx : System.Web.UI.Page
{
    private int _mShowByCountry=0;
    private int _mShowByAns = 0;
    DataTable _dt;
    private string GetQuery()
    {

        string StartDate =txtStartDate.Text;
        string EndtDate = (txtEndDate.Text.ConvertToDateTimeFromMySqlFormat()).AddSeconds(1).ToMySqlFormatWithoutQuote();
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

        string constructedSQL = new SqlHelperIntlInIcx
                        (StartDate,
                         EndtDate,
                         groupInterval,
                         tableName,
                         
                         new List<string>()
                            {
                                // groupInterval=="Hourly"?"tup_starttime":string.Empty,
                                getInterval(groupInterval),
                                CheckBoxPartner.Checked==true?"tup_inpartnerid":string.Empty,
                                CheckBoxShowByAns.Checked==true?"tup_destinationId":string.Empty,
                                CheckBoxShowByIgw.Checked==true?"tup_outpartnerid":string.Empty,
                                CheckBoxViewIncomingRoute.Checked==true?"tup_incomingroute":string.Empty,
                                CheckBoxViewOutgoingRoute.Checked==true?"tup_outgoingroute":string.Empty,
                            },
                         new List<string>()
                            {
                                CheckBoxPartner.Checked==true?DropDownListPartner.SelectedIndex>0?" tup_inpartnerid="+DropDownListPartner.SelectedValue:string.Empty:string.Empty,
                                CheckBoxShowByAns.Checked==true?DropDownListAns.SelectedIndex>0?" tup_destinationId="+DropDownListAns.SelectedValue:string.Empty:string.Empty,
                                CheckBoxShowByIgw.Checked==true?DropDownListIgw.SelectedIndex>0?" tup_outpartnerid="+DropDownListIgw.SelectedValue:string.Empty:string.Empty,
                                CheckBoxViewIncomingRoute.Checked==true?DropDownListViewIncomingRoute.SelectedIndex>0?" tup_incomingroute="+DropDownListViewIncomingRoute.SelectedItem.Value:string.Empty:string.Empty,
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
                return "tup_starttime";
            case "Weekly":
                return "concat(year(tup_starttime),'-W',week(tup_starttime))";
            case "Monthly":
                return "concat(year(tup_starttime),'-',date_format(tup_starttime,'%b'))";
            case "Yearly":
                return "DATE_FORMAT(tup_starttime,'%Y')";
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

    DataSet getBtrcReport(MySqlConnection connection, string sql)
    {
        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Connection = connection;
        MySqlDataAdapter domDataAdapter = new MySqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        domDataAdapter.Fill(ds);
        return ds;    
    }
    List<BtrcReportRow> ConvertBtrcDataSetToList(DataSet ds, Dictionary<int, string> partnerNames) {
        bool hasRecords = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);
        List<BtrcReportRow> records = new List<BtrcReportRow>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    BtrcReportRow record = new BtrcReportRow();
                    int partnerId = row.Field<int>("partnerid");
                    string partnerName = "";
                    if (partnerNames.TryGetValue(partnerId, out partnerName) == false)
                    {
                        throw new Exception("Could not find partner name for partner id=" + partnerId);
                    }
                    record.partnerName = partnerName;
                    record.minutes = row.Field<Decimal>("minutes");
                    records.Add(record);
                }
            }
        }
        return records;
    }
    DataSet getDomesticWeeklyReport(MySqlConnection connection)
    {
        string domSql =
          $@"select tup_inpartnerid as partnerid,sum(duration1)/60 as minutes 
        from 
        (select * from sum_voice_day_01
        where tup_starttime >= '{txtStartDate.Text}' and tup_starttime < '{txtEndDate.Text}'
        union all 
        select * from sum_voice_day_04
        where tup_starttime >= '{txtStartDate.Text}' and tup_starttime < '{txtEndDate.Text}') x
        group by tup_inpartnerid;";

        DataSet ds= getBtrcReport(connection, domSql);
        return ds;
    }


    protected void submit_Click(object sender, EventArgs e)
    {
        
        List<BtrcReportRow> domesticWeeklyRecords = new List<BtrcReportRow>();
        
       
        Dictionary<int, string> partnerNames = null;
        using (PartnerEntities context = new PartnerEntities()) {
            partnerNames = context.partners.ToList().ToDictionary(p => p.idPartner, p => p.PartnerName);
        }
        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
            connection.Open();

            DataSet domesticDs = getDomesticWeeklyReport(connection);
            domesticWeeklyRecords = ConvertBtrcDataSetToList(domesticDs,partnerNames);
            DomHeader.Text = "Weekly Domestic Calls";
            Gvdom.DataSource = domesticWeeklyRecords;
            Gvdom.DataBind();

            
            return;




            if (CheckBoxDailySummary.Checked == false)
            {

                GridView1.Columns[0].Visible = false;
            }
            else
            {
                GridView1.Columns[0].Visible = true;

            }


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
                    GridView1.Columns[0].HeaderText = "Hour";
                }
                else if (RadioButtonDaily.Checked == true)
                {
                    summaryInterval = "Daily";
                    GridView1.Columns[0].HeaderText = "Date";
                }
                else if (RadioButtonWeekly.Checked == true)
                {
                    summaryInterval = "Weekly";
                    GridView1.Columns[0].HeaderText = "Week";
                }
                else if (RadioButtonMonthly.Checked == true)
                {
                    summaryInterval = "Monthly";
                    GridView1.Columns[0].HeaderText = "Month";
                }
                else if (RadioButtonYearly.Checked == true)
                {
                    summaryInterval = "Yearly";
                    GridView1.Columns[0].HeaderText = "Year";
                }
            }
            //MySqlCommand cmd = new MySqlCommand(Sql, connection);

            //MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            //     DataSet dataset = new DataSet();
            //    da.Fill(dataset);
            //    GridView1.DataSource = dataset;
            //    bool hasRows = dataset.Tables.Cast<DataTable>()
            //                      .Any(table => table.Rows.Count != 0);
            //    if (hasRows == true)
            //    {
            //  GridView1.ShowFooter = true;//set it here before setting footer text, setting this to true clears already set footer text
            //Label1.Text = "";
            // Button1.Visible = true; //show export
            //                         //Summary calculation for grid view*************************
            // TrafficReportDatasetBased tr = new TrafficReportDatasetBased(dataset);
            // tr.Ds = dataset;
            // List<NoOfCallsVsPdd> callVsPdd = new List<NoOfCallsVsPdd>();
            // foreach (DataRow dr in tr.Ds.Tables[0].Rows)
            // {
            //     tr.CallStat.TotalCalls += tr.ForceConvertToLong(dr["CallsCount"]);
            //     tr.CallStat.ConnectedCalls += tr.ForceConvertToLong(dr["ConnectedCount"]);
            //     tr.CallStat.ConnectedCallsbyCauseCodes += tr.ForceConvertToLong(dr["ConectbyCC"]);
            //     tr.CallStat.SuccessfullCalls += tr.ForceConvertToLong(dr["Number Of Calls (International Incoming)"]);
            //     tr.CallStat.TotalActualDuration += tr.ForceConvertToDouble(dr["Paid Minutes (International Incoming)"]);
            //     tr.CallStat.TotalRoundedDuration += tr.ForceConvertToDouble(dr["RoundedDuration"]);
            //     tr.CallStat.TotalDuration1 += tr.ForceConvertToDouble(dr["Duration1"]);
            //     tr.CallStat.TotalCustomerCost += tr.ForceConvertToDouble(dr["customercost"]);
            //     tr.CallStat.BtrcRevShare += tr.ForceConvertToDouble(dr["tax1"]);
            // NoOfCallsVsPdd cpdd = new NoOfCallsVsPdd(tr.ForceConvertToLong(dr["Number Of Calls (International Incoming)"]), tr.ForceConvertToDouble(dr["PDD"]));
            //     callVsPdd.Add(cpdd);
            // }
            // tr.CallStat.TotalActualDuration = Math.Round(tr.CallStat.TotalActualDuration, 2);
            // tr.CallStat.TotalDuration1 = Math.Round(tr.CallStat.TotalDuration1, 2);
            // tr.CallStat.TotalDuration2 = Math.Round(tr.CallStat.TotalDuration2, 2);
            // tr.CallStat.TotalDuration3 = Math.Round(tr.CallStat.TotalDuration3, 2);
            // tr.CallStat.TotalDuration4 = Math.Round(tr.CallStat.TotalDuration4, 2);
            // tr.CallStat.TotalRoundedDuration = Math.Round(tr.CallStat.TotalRoundedDuration, 2);
            // tr.CallStat.TotalCustomerCost = Math.Round(tr.CallStat.TotalCustomerCost, 2);
            // tr.CallStat.CalculateAsr(2);
            // tr.CallStat.CalculateAcd(2);
            // tr.CallStat.CalculateAveragePdd(callVsPdd, 2);
            // tr.CallStat.CalculateCcr(2);
            // tr.CallStat.CalculateCcRbyCauseCode(2);
            // //SUMMARY CALCULATION FOR GRIDVIEW COMPLETE


            // //display summary information in the footer
            // Dictionary<string, dynamic> fieldSummaries = new Dictionary<string, dynamic>();//key=colname,val=colindex in grid
            //                                                                                //all keys have to be lowercase, because db fields are lower case at times
            // fieldSummaries.Add("callscount", tr.CallStat.TotalCalls);
            // fieldSummaries.Add("connectedcount", tr.CallStat.ConnectedCalls);
            // fieldSummaries.Add("connectbycc", tr.CallStat.ConnectedCallsbyCauseCodes);
            // fieldSummaries.Add("number of calls (international incoming)", tr.CallStat.SuccessfullCalls);
            // fieldSummaries.Add("paid minutes (international incoming)", tr.CallStat.TotalActualDuration);
            // fieldSummaries.Add("roundedduration", tr.CallStat.TotalRoundedDuration);
            // fieldSummaries.Add("duration1", tr.CallStat.TotalDuration1);
            // fieldSummaries.Add("asr", tr.CallStat.Asr);
            // fieldSummaries.Add("acd", tr.CallStat.Acd);
            // fieldSummaries.Add("pdd", tr.CallStat.Pdd);
            // fieldSummaries.Add("ccr", tr.CallStat.Ccr);
            // fieldSummaries.Add("ccrbycc", tr.CallStat.CcRbyCauseCode);
            // fieldSummaries.Add("customercost", tr.CallStat.TotalCustomerCost);
            // fieldSummaries.Add("tax1", tr.CallStat.BtrcRevShare);
            // tr.FieldSummaries = fieldSummaries;

            // Session["IntlIn"] = tr;//save to session

            // //populate footer
            // //clear first
            // bool captionSetForTotal = false;
            // for (int c = 0; c < GridView1.Columns.Count; c++)
            // {
            //     GridView1.Columns[c].FooterText = "";
            // }
            // for (int c = 0; c < GridView1.Columns.Count; c++)
            // {
            //     if (captionSetForTotal == false && GridView1.Columns[c].Visible == true)
            //     {
            //         GridView1.Columns[c].FooterText = "Total: ";//first visible column
            //         captionSetForTotal = true;
            //     }
            //     string key = GridView1.Columns[c].SortExpression.ToLower();
            //     if (key == "") continue;
            //     if (tr.FieldSummaries.ContainsKey(key))
            //     {
            //         GridView1.Columns[c].FooterText += (tr.GetDataColumnSummary(key)).ToString();//+ required to cocat "Total:"
            //     }
            // }
            // GridView1.DataBind();//call it here after setting footer, footer text doesn't show sometime otherwise, may be a bug
            // GridView1.ShowFooter = true;//don't set it now, set before footer text setting, weird! it clears the footer text
            //                             //hide filters...
            // Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDivSubmit();", true);
            // hidValueSubmitClickFlag.Value = "false";

            //}//if has rows

            else
            {
                GridView1.DataBind();
                Label1.Text = "No Data!";
                Button1.Visible = false; //hide export
            }

        }//using mysql connection



        }
    

    protected void Button1_Click(object sender, EventArgs e)
    {
        //if (Session["IntlIn"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        //{
        //    TrafficReportDatasetBased tr = (TrafficReportDatasetBased)Session["IntlIn"];
        //    DataSetWithGridView dsG = new DataSetWithGridView(tr, GridView1);//invisible columns are removed in constructor
        //    CreateExcelFileAspNet.CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(tr.Ds, "IntlIncoming_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        //            + ".xlsx", Response);
        //}

        List<BtrcReportRow> domesticWeeklyRecords = new List<BtrcReportRow>();
   
        Dictionary<int, string> partnerNames= null;
        using (PartnerEntities context = new PartnerEntities())
        {
            partnerNames = context.partners.ToList().ToDictionary(p => p.idPartner, p => p.PartnerName);
        }

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
            connection.Open();


            DataSet domesticDs = getDomesticWeeklyReport(connection);
            domesticWeeklyRecords = ConvertBtrcDataSetToList(domesticDs, partnerNames);


            ExcelExporterForBtrcReport.ExportToExcelDomesticWeeklyReport("Domestic_Weekly_Report_From_" + txtStartDate.Text + "_To_" + txtEndDate.Text
                    + ".xlsx", Response, domesticWeeklyRecords);

            return;
        }
        }

    private List<BtrcReportRow> ConvertBtrcDataSetToList(DataSet domesticDs, object partnerName)
    {
        throw new NotImplementedException();
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
            txtStartDate.Enabled = false;

            TextBoxYear1.Enabled = false;
            DropDownListMonth1.Enabled = false;
            txtEndDate.Enabled = false;
            
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
            txtStartDate.Enabled = true;

            TextBoxYear1.Enabled = true;
            DropDownListMonth1.Enabled = true;
            txtEndDate.Enabled = true;

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
            txtEndDate.Text = endtime.ToString("dd/MM/yyyy HH:mm:ss");
            txtStartDate.Text = starttime.ToString("dd/MM/yyyy HH:mm:ss");

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

    protected void CalendarEndDate_TextChanged(object sender, EventArgs e)
    {
        CalendarStartDate.SelectedDate = txtStartDate.Text.ConvertToDateTimeFromMySqlFormat();
        DateTime startDate = CalendarStartDate.SelectedDate ?? DateTime.Now;
        DateTime endDate = startDate.AddDays(6);
        CalendarEndDate.SelectedDate = endDate;
        //txtStartDate.Text = startDate.ToString("yyyy-MM-dd 00:00:00");
        //txtEndDate.Text = endDate.ToString("yyyy-MM-dd 23:59:59");
    }
    protected void CalendarStartDate_TextChanged(object sender, EventArgs e)
    {
        CalendarEndDate.SelectedDate = txtEndDate.Text.ConvertToDateTimeFromMySqlFormat();
        DateTime endDate = CalendarEndDate.SelectedDate ?? DateTime.Now;
        DateTime startDate = endDate.AddDays(-6);
        CalendarStartDate.SelectedDate = startDate;
        //txtStartDate.Text = startDate.ToString("yyyy-MM-dd 00:00:00");
        //txtEndDate.Text = endDate.ToString("yyyy-MM-dd 23:59:59");
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
            Dictionary<string,partner> dicKpiAns = null;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (ViewState["dicKpiAns"] != null)
                {
                    dicKpiAns = (Dictionary<string,partner>)ViewState["dicKpiAns"];
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
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ASR").ToString(),out thisAsr);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ACD").ToString(), out thisAcd);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCR").ToString(), out thisCcr);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "PDD").ToString(), out thisPdd);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCRByCC").ToString(), out thisCcRbyCc);
                partner thisPartner = null;
                dicKpiAns.TryGetValue(thisAnsName, out thisPartner);
                if (thisPartner != null)
                {
                    Color redColor=ColorTranslator.FromHtml("#FF0000");
                    //ASR
                    Single refAsr = 0;
                    if (Convert.ToSingle(thisPartner.refasr) > 0)
                    {
                        refAsr=Convert.ToSingle(thisPartner.refasr);
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

                    if (thisAsr > refAsrFas && refAsrFas>0)
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
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            double asr = 1;
            double.TryParse(e.Row.Cells[16].Text, out asr);
            Color redColor2 = ColorTranslator.FromHtml("#FF0000");
            if (asr <= 0)
            {
                e.Row.Cells[16].ForeColor = Color.White;
                e.Row.Cells[16].BackColor = redColor2;
                e.Row.Cells[16].Font.Bold = true;
            }
        }

    }

    protected void CheckBoxViewIncomingRoute_CheckedChanged(object sender, EventArgs e)
    {
        DropDownListViewIncomingRoute.Enabled = CheckBoxViewIncomingRoute.Checked;
    }

    protected void CheckBoxViewOutgoingRoute_CheckedChanged(object sender, EventArgs e)
    {
        DropDownListViewOutgoingRoute.Enabled = CheckBoxViewOutgoingRoute.Checked;
    }

    protected void DropDownListPartner_OnSelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownListViewIncomingRoute.Items.Clear();
        DropDownListViewIncomingRoute.Items.Add(new ListItem("[All]", "-1"));
        if (DropDownListPartner.SelectedValue != String.Empty)
        {
            if (DropDownListPartner.SelectedValue == "-1")
            {
                using (PartnerEntities contex = new PartnerEntities())
                {
                    List<int> ansList = contex.partners.Where(c => c.PartnerType == 2).Select(c => c.idPartner).ToList();
                    foreach (route route in contex.routes.Where(x => ansList.Contains(x.idPartner)))
                    {
                        DropDownListViewIncomingRoute.Items.Add(new ListItem($"{route.Description} ({route.RouteName})", route.RouteName));
                    }
                }
            }
            else
            {
                using (PartnerEntities contex = new PartnerEntities())
                {
                    int idPartner = Convert.ToInt32(DropDownListPartner.SelectedValue);
                    foreach (route route in contex.routes.Where(x => x.idPartner == idPartner))
                    {
                        DropDownListViewIncomingRoute.Items.Add(new ListItem($"{route.Description} ({route.RouteName})", route.RouteName));
                    }
                }
            }
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
