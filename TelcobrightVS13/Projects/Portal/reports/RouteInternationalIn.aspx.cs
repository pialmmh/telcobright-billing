using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using reports;
using ExportToExcel;
using MediationModel;
using PortalApp.ReportHelper;
using LibraryExtensions;
public partial class DefaultRtIntlIn : Page
{
    private int _mShowByCountry=0;
    private int _mShowByAns = 0;

    DataTable _dt;
    private string GetQuery()
    {

        string StartDate = txtDate.Text;
        string EndtDate = (txtDate1.Text.ConvertToDateTimeFromMySqlFormat()).AddSeconds(1).ToMySqlStyleDateTimeStrWithoutQuote();
        string tableName = DropDownListReportSource.SelectedValue + "03";

        string groupInterval = getSelectedRadioButtonText();



        string constructedSQL = new SqlHelperInIGWRouteWise
                        (StartDate,
                         EndtDate,
                         groupInterval,
                         tableName,

                         new List<string>()
                            {
                                groupInterval=="Hourly"?"tup_starttime":string.Empty,
                                CheckBoxPartner.Checked==true?"tup_incomingroute":string.Empty,
                              //  CheckBoxShowByAns.Checked==true?"tup_incomingroute":string.Empty,
                                CheckBoxShowByIgw.Checked==true?"tup_outgoingroute":string.Empty,
                            },
                         new List<string>()
                            {
                                CheckBoxPartner.Checked==true?DropDownListPartner.SelectedIndex>0?" tup_incomingroute="+DropDownListPartner.SelectedValue.EncloseWith("'"):string.Empty:string.Empty,
                              //  CheckBoxShowByAns.Checked==true?DropDownListAns.SelectedIndex>0?" tup_destinationId="+DropDownListAns.SelectedValue:string.Empty:string.Empty,
                                CheckBoxShowByIgw.Checked==true?DropDownListIgw.SelectedIndex>0?" tup_outgoingroute="+DropDownListIgw.SelectedValue.EncloseWith("'"):string.Empty:string.Empty
                            }).getSQLString();


        return constructedSQL;
    }

    public string getSelectedRadioButtonText()
    {
        string interval = "";
        if (RadioButtonHalfHourly.Checked)
            return interval = "" + RadioButtonHalfHourly.Text;
        else if (RadioButtonHourly.Checked)
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



    protected void CheckBoxRealTimeUpdate_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxRealTimeUpdate.Checked)
        {
            //Disable DailySummary,Destination, Dates& Months

            this.CheckBoxDailySummary.Checked = false;
            this.CheckBoxDailySummary.Enabled = false;

            // CheckBoxShowByDestination.Checked = false;
            //CheckBoxShowByDestination.Enabled = false;

            this.TextBoxYear.Enabled = false;
            this.DropDownListMonth.Enabled = false;
            this.txtDate.Enabled = false;

            this.TextBoxYear1.Enabled = false;
            this.DropDownListMonth1.Enabled = false;
            this.txtDate1.Enabled = false;

            //Enable Timers,Duration,country
            //CheckBoxShowByCountry.Checked = true;
            this.TextBoxDuration.Enabled = true;
            //TextBoxDuration.Text = "30";
            //timerflag = true;



            //dateInitialize
        }
        else
        {
            //Enable DailySummary,Destination, Dates& Months

            //CheckBoxDailySummary.Checked = true;
            this.CheckBoxDailySummary.Enabled = true;

            // CheckBoxShowByDestination.Checked = true;
            // CheckBoxShowByDestination.Enabled = true;

            this.TextBoxYear.Enabled = true;
            this.DropDownListMonth.Enabled = true;
            this.txtDate.Enabled = true;

            this.TextBoxYear1.Enabled = true;
            this.DropDownListMonth1.Enabled = true;
            this.txtDate1.Enabled = true;

            //Disable Timers,Duration,
            //CheckBoxShowByCountry.Checked = false;
            this.TextBoxDuration.Enabled = false;
            //TextBoxDuration.Text = "30";
            //timerflag = false;
        }
        //CheckBoxShowByCountry_CheckedChanged(sender, e);
        DateInitialize();
    }

    public void DateInitialize()
    {
        if (this.CheckBoxRealTimeUpdate.Checked)
        {
            long a;
            if (!long.TryParse(this.TextBoxDuration.Text, out a))
            {
                // If Not Integer Clear Textbox text or you can also Undo() Last Operation :)

                this.TextBoxDuration.Text = "30";
                a = 30;
            }

            DateTime endtime = DateTime.Now;
            DateTime starttime = endtime.AddMinutes(a * (-1));
            this.txtDate1.Text = endtime.ToString("yyyy-MM-dd HH:mm:ss");
            this.txtDate.Text = starttime.ToString("yyyy-MM-dd HH:mm:ss");

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
        if (!long.TryParse(this.TextBoxDuration.Text, out a))
        {
            // If Not Integer Clear Textbox text or you can also Undo() Last Operation :)

            this.TextBoxDuration.Text = "30";
        }
    }

    protected void Timer1_Tick(object sender, EventArgs e)
    {
        if (this.CheckBoxRealTimeUpdate.Checked)
        {
            submit_Click(sender, e);
        }
    }



    protected void submit_Click(object sender, EventArgs e)
    {


        if (this.CheckBoxShowByAns.Checked == true)
        {
            this.GridView1.Columns[3].Visible = true;
            //load ANS KPI
            Dictionary<string, partner> dicKpiAns = new Dictionary<string, partner>();
            using (PartnerEntities context = new PartnerEntities())
            {
                foreach (partner thisPartner in context.partners.Where(c => c.PartnerType == 1).ToList())
                {
                    dicKpiAns.Add(thisPartner.PartnerName, thisPartner);
                }
                this.ViewState["dicKpiAns"] = dicKpiAns;
            }
        }
        else this.GridView1.Columns[3].Visible = false;

        if (this.CheckBoxShowByIgw.Checked == true)
        {
            this.GridView1.Columns[2].Visible = true;
        }
        else this.GridView1.Columns[2].Visible = false;

        if (this.CheckBoxPartner.Checked == true)
        {
            this.GridView1.Columns[1].Visible = true;
        }
        else this.GridView1.Columns[1].Visible = false;
        if (this.CheckBoxShowCost.Checked == true)
        {
            this.GridView1.Columns[9].Visible = true;
            this.GridView1.Columns[10].Visible = true;
            this.GridView1.Columns[11].Visible = true;
            this.GridView1.Columns[12].Visible = true;
            this.GridView1.Columns[13].Visible = false;
            this.GridView1.Columns[14].Visible = true;
        }
        else
        {
            this.GridView1.Columns[9].Visible = false;
            this.GridView1.Columns[10].Visible = false;
            this.GridView1.Columns[11].Visible = false;
            this.GridView1.Columns[12].Visible = false;
            this.GridView1.Columns[13].Visible = false;
            this.GridView1.Columns[14].Visible = false;
        }
        if (this.CheckBoxShowPerformance.Checked == true)
        {
            this.GridView1.Columns[15].Visible = true;
            this.GridView1.Columns[16].Visible = true;
            this.GridView1.Columns[17].Visible = true;
            this.GridView1.Columns[18].Visible = true;
            //GridView1.Columns[19].Visible = true;
            this.GridView1.Columns[20].Visible = true;
        }
        else
        {
            this.GridView1.Columns[15].Visible = false;
            this.GridView1.Columns[16].Visible = false;
            this.GridView1.Columns[17].Visible = false;
            this.GridView1.Columns[18].Visible = false;
            //GridView1.Columns[19].Visible = false;
            this.GridView1.Columns[20].Visible = false;

        }
        //make profit invisible, it's useless
        this.GridView1.Columns[15].Visible = false;
        this.GridView1.Columns[9].Visible = true;//carrier's duration
        using (MySqlConnection connection = new MySqlConnection())
        {
            //connection.ConnectionString = "server=10.0.30.125;User Id=dbreader;password=Takaytaka1#;Persist Security Info=True;default command timeout=3600;database=dbl";
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
            connection.Open();

            MySqlCommand cmd = new MySqlCommand(GetQuery(), connection);
            cmd.Connection = connection;


          


            //this piece of code is important, reqport queries don't have date column when not summarized by day/hour etc.
            //need to hide gridview column[0] at times then
            this.Label1.Text = "";
            if (this.CheckBoxDailySummary.Checked == false)
            {
                this.GridView1.Columns[0].Visible = false;
            }
            else
            {
                this.GridView1.Columns[0].Visible = true;
            }


            if (this.CheckBoxDailySummary.Checked == true)
            {
                string summaryInterval = "";
                if (this.RadioButtonHalfHourly.Checked == true)
                {
                    summaryInterval = "Halfhourly";
                    this.GridView1.Columns[0].HeaderText = "Half Hour";
                }
                else if (this.RadioButtonHourly.Checked == true)
                {
                    summaryInterval = "Hourly";
                    this.GridView1.Columns[0].HeaderText = "Hour";
                }
                else if (this.RadioButtonDaily.Checked == true)
                {
                    summaryInterval = "Daily";
                    this.GridView1.Columns[0].HeaderText = "Date";
                }
                else if (this.RadioButtonWeekly.Checked == true)
                {
                    summaryInterval = "Weekly";
                    this.GridView1.Columns[0].HeaderText = "Week";
                }
                else if (this.RadioButtonMonthly.Checked == true)
                {
                    summaryInterval = "Monthly";
                    this.GridView1.Columns[0].HeaderText = "Month";
                }
                else if (this.RadioButtonYearly.Checked == true)
                {
                    summaryInterval = "Yearly";
                    this.GridView1.Columns[0].HeaderText = "Year";
                }

              

            }

            


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            da.Fill(dataset);

            List<int> columnSortList = new List<int>();
            List<string> colNamelist = new List<string>();

            this._dt = dataset.Tables[0];

            this.GridView1.DataSource = dataset;
            bool hasRows = dataset.Tables.Cast<DataTable>()
                               .Any(table => table.Rows.Count != 0);

            if (hasRows == true)
            {
                this.GridView1.ShowFooter = true;//set it here before setting footer text, setting this to true clears already set footer text
                this.Label1.Text = "";
                this.Button1.Visible = true; //show export
                //Summary calculation for grid view*************************
                TrafficReportDatasetBased tr = new TrafficReportDatasetBased(dataset);
                tr.Ds = dataset;
                List<NoOfCallsVsPdd> callVsPdd = new List<NoOfCallsVsPdd>();
                foreach (DataRow dr in tr.Ds.Tables[0].Rows)
                {
                    tr.CallStat.TotalCalls += tr.ForceConvertToLong(dr["CallsCount"]);
                    tr.CallStat.ConnectedCalls += tr.ForceConvertToLong(dr["ConnectedCount"]);
                    tr.CallStat.ConnectedCallsbyCauseCodes += tr.ForceConvertToLong(dr["ConectbyCC"]);
                    tr.CallStat.SuccessfullCalls += tr.ForceConvertToLong(dr["Number Of Calls (International Incoming)"]);
                    tr.CallStat.TotalActualDuration += tr.ForceConvertToDouble(dr["Paid Minutes (International Incoming)"]);
                    tr.CallStat.TotalRoundedDuration += tr.ForceConvertToDouble(dr["RoundedDuration"]);
                    tr.CallStat.TotalDuration1 += tr.ForceConvertToDouble(dr["Duration1"]);
                    NoOfCallsVsPdd cpdd = new NoOfCallsVsPdd(tr.ForceConvertToLong(dr["Number Of Calls (International Incoming)"]), tr.ForceConvertToDouble(dr["PDD"]));
                    callVsPdd.Add(cpdd);
                }
                tr.CallStat.TotalActualDuration = Math.Round(tr.CallStat.TotalActualDuration, 2);
                tr.CallStat.TotalDuration1 = Math.Round(tr.CallStat.TotalDuration1, 2);
                tr.CallStat.TotalDuration2 = Math.Round(tr.CallStat.TotalDuration2, 2);
                tr.CallStat.TotalDuration3 = Math.Round(tr.CallStat.TotalDuration3, 2);
                tr.CallStat.TotalDuration4 = Math.Round(tr.CallStat.TotalDuration4, 2);
                tr.CallStat.TotalRoundedDuration = Math.Round(tr.CallStat.TotalRoundedDuration, 2);
                tr.CallStat.CalculateAsr(2);
                tr.CallStat.CalculateAcd(2);
                tr.CallStat.CalculateAveragePdd(callVsPdd, 2);
                tr.CallStat.CalculateCcr(2);
                tr.CallStat.CalculateCcRbyCauseCode(2);
                //SUMMARY CALCULATION FOR GRIDVIEW COMPLETE


                //display summary information in the footer
                Dictionary<string, dynamic> fieldSummaries = new Dictionary<string, dynamic>();//key=colname,val=colindex in grid
                //all keys have to be lowercase, because db fields are lower case at times
                fieldSummaries.Add("callscount", tr.CallStat.TotalCalls);
                fieldSummaries.Add("connectedcount", tr.CallStat.ConnectedCalls);
                fieldSummaries.Add("connectbycc", tr.CallStat.ConnectedCallsbyCauseCodes);
                fieldSummaries.Add("number of calls (international incoming)", tr.CallStat.SuccessfullCalls);
                fieldSummaries.Add("paid minutes (international incoming)", tr.CallStat.TotalActualDuration);
                fieldSummaries.Add("roundedduration", tr.CallStat.TotalRoundedDuration);
                fieldSummaries.Add("duration1", tr.CallStat.TotalDuration1);
                fieldSummaries.Add("asr", tr.CallStat.Asr);
                fieldSummaries.Add("acd", tr.CallStat.Acd);
                fieldSummaries.Add("pdd", tr.CallStat.Pdd);
                fieldSummaries.Add("ccr", tr.CallStat.Ccr);
                fieldSummaries.Add("ccrbycc", tr.CallStat.CcRbyCauseCode);
                tr.FieldSummaries = fieldSummaries;

                this.Session["IntlInRoute"] = tr;//save to session, ***CHANGE IN EACH PAGE

                //populate footer
                //clear first
                bool captionSetForTotal = false;
                for (int c = 0; c < this.GridView1.Columns.Count; c++)
                {
                    this.GridView1.Columns[c].FooterText = "";
                }
                for (int c = 0; c < this.GridView1.Columns.Count; c++)
                {
                    if (captionSetForTotal == false && this.GridView1.Columns[c].Visible == true)
                    {
                        this.GridView1.Columns[c].FooterText = "Total: ";//first visible column
                        captionSetForTotal = true;
                    }
                    string key = this.GridView1.Columns[c].SortExpression.ToLower();
                    if (key == "") continue;
                    if (tr.FieldSummaries.ContainsKey(key))
                    {
                        this.GridView1.Columns[c].FooterText += (tr.GetDataColumnSummary(key)).ToString();//+ required to cocat "Total:"
                    }
                }
                this.GridView1.DataBind();//call it here after setting footer, footer text doesn't show sometime otherwise, may be a bug
                this.GridView1.ShowFooter = true;//don't set it now, set before footer text setting, weird! it clears the footer text
                                            //hide filters...
                this.Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDivSubmit();", true);
                this.hidValueSubmitClickFlag.Value = "false";

            }//if has rows

            else
            {
                this.GridView1.DataBind();
                this.Label1.Text = "No Data!";
                this.Button1.Visible = false; //hide export
            }

            }//using mysql connection
        

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        if (this.Session["IntlInRoute"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            TrafficReportDatasetBased tr = (TrafficReportDatasetBased) this.Session["IntlInRoute"];
            DataSetWithGridView dsG = new DataSetWithGridView(tr, this.GridView1);//invisible columns are removed in constructor
            CreateExcelFileAspNet.CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(tr.Ds, "IntlIncomingRoute_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + ".xlsx", this.Response);
        }
    }

    public static void ExportToSpreadsheet(DataTable table, string name, List<string> colNameList, List<int> columnSortlist)
    {
        HttpContext context = HttpContext.Current;
        context.Response.Clear();

        string thisRow = "";

        //foreach (DataColumn column in table.Columns)
        //{

        //    ThisRow += column.ColumnName + ",";
        //}
        //write columns in order specified in ColumnSortedList
        int ii = 0;
        for (ii=0; ii<colNameList.Count;ii++ )
        {  
            //ThisRow +=  table.Columns[ColumnSortlist[ii]].ColumnName + ",";
            thisRow += colNameList[ii]+ ",";
        }

        thisRow = thisRow.Substring(0, thisRow.Length - 1) + Environment.NewLine;
        context.Response.Write(thisRow);

        
        //foreach (DataRow row in table.Rows)
        //{
        //    ThisRow = "";
        //    for (int i = 0; i < table.Columns.Count; i++)
        //    {
        //        ThisRow += row[i].ToString().Replace(",", string.Empty) + ",";
        //    }
        //    ThisRow = ThisRow.Substring(0, ThisRow.Length - 1) + Environment.NewLine;
        //    context.Response.Write(ThisRow);
        //}

        foreach (DataRow row in table.Rows)
        {
            thisRow = "";
            for (ii = 0; ii < columnSortlist.Count; ii++) //for each column
            {  
                thisRow += row[columnSortlist[ii]].ToString().Replace(",", string.Empty) + ",";
            }
            thisRow = thisRow.Substring(0, thisRow.Length - 1) + Environment.NewLine;
            context.Response.Write(thisRow);
        }

        //context.Response.ContentType = "text/csv";
        //context.Response.ContentType = "application/vnd.ms-excel";
        context.Response.ContentType = "application/ms-excel";
        context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + name + ".csv");
        context.Response.End();
    }

    protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxPartner.Checked == true)
        {
            this.DropDownListPartner.Enabled = true;
        }
        else this.DropDownListPartner.Enabled = false;
    }


    protected void CheckBoxShowByAns_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxShowByAns.Checked == true)
        {
            this.DropDownListAns.Enabled = true;
        }
        else this.DropDownListAns.Enabled = false;
    }
    protected void CheckBoxShowByIgw_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxShowByIgw.Checked == true)
        {
            this.DropDownListIgw.Enabled = true;
        }
        else this.DropDownListIgw.Enabled = false;
    
    }

    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        if (this.CheckBoxShowByAns.Checked == true)
        {
            Dictionary<string, partner> dicKpiAns = null;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (this.ViewState["dicKpiAns"] != null)
                {
                    dicKpiAns = (Dictionary<string, partner>) this.ViewState["dicKpiAns"];
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
                    if (e.Row.RowType == DataControlRowType.DataRow || e.Row.RowType == DataControlRowType.Footer)
                    {
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
            }

        }//if checkbox ans

        //0 ASR highlighting
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

    protected void Page_Load(object sender, EventArgs e)
    {

    }
}
