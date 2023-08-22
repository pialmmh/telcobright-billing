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
public partial class AlphatechPremium : System.Web.UI.Page
{
    private int _mShowByCountry=0;
    private int _mShowByAns = 0;
    DataTable _dt;
    private string GetQuery()
    {

        string StartDate =txtDate.Text;
        string EndtDate = (txtDate1.Text.ConvertToDateTimeFromMySqlFormat()).AddSeconds(1).ToMySqlFormatWithoutQuote();
        string tableName = DropDownListReportSource.SelectedValue + "05";

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

        string constructedSQL = new SqlHelperIntlInTransit
                        (StartDate,
                         EndtDate,
                         groupInterval,
                         tableName,
                         
                         new List<string>()
                            {
                                //groupInterval=="Hourly"?"tup_starttime":string.Empty,
                                getInterval(groupInterval), 
                                CheckBoxPartner.Checked==true?"tup_inpartnerid":string.Empty,
                                CheckBoxOutPartner.Checked==true?"tup_outpartnerid":string.Empty,
                                CheckBoxInRoute.Checked==true?"tup_incomingroute":string.Empty,
                                CheckBoxOutRoute.Checked==true?"tup_outgoingroute":string.Empty,
                                CheckBoxMatchedCustomerPrefix.Checked==true?"tup_matchedprefixcustomer":string.Empty,
                                CheckBoxMatchedSupplierPrefix.Checked==true?"tup_matchedprefixsupplier":string.Empty,
                                //CheckBoxShowByAns.Checked==true?"tup_destinationId":string.Empty,
                                //CheckBoxShowByIgw.Checked==true?"tup_outpartnerid":string.Empty,
                            },
                         new List<string>()
                            {
                                CheckBoxPartner.Checked==true?DropDownListPartner.SelectedIndex>0?" tup_inpartnerid="+DropDownListPartner.SelectedValue:string.Empty:string.Empty,
                                CheckBoxOutPartner.Checked==true?DropDownListOutPartner.SelectedIndex>0?" tup_outpartnerid="+DropDownListOutPartner.SelectedValue:string.Empty:string.Empty,
                                CheckBoxInRoute.Checked==true?DropDownListInRoute.SelectedIndex>0?" tup_incomingroute='"+DropDownListInRoute.SelectedValue+"'":string.Empty:string.Empty,
                                CheckBoxOutRoute.Checked==true?DropDownListOutRoute.SelectedIndex>0?" tup_outpartnerid='"+DropDownListOutRoute.SelectedValue+"'":string.Empty:string.Empty,
                                //CheckBoxShowByAns.Checked==true?DropDownListAns.SelectedIndex>0?" tup_destinationId="+DropDownListAns.SelectedValue:string.Empty:string.Empty,
                                //CheckBoxShowByIgw.Checked==true?DropDownListIgw.SelectedIndex>0?" tup_outpartnerid="+DropDownListIgw.SelectedValue:string.Empty:string.Empty
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



    protected void submit_Click(object sender, EventArgs e)
    {
        //if (CheckBoxShowByAns.Checked == true)
        //{
        //    GridView1.Columns[3].Visible = true;
        //    //load ANS KPI
        //    Dictionary<string, partner> dicKpiAns = new Dictionary<string, partner>();
        //    using (PartnerEntities context = new PartnerEntities())
        //    {
        //        foreach (partner thisPartner in context.partners.Where(c => c.PartnerType == 1).ToList())
        //        {
        //            dicKpiAns.Add(thisPartner.PartnerName, thisPartner);
        //        }
        //        ViewState["dicKpiAns"] = dicKpiAns;
        //    }
        //}
        //else GridView1.Columns[3].Visible = false;
        if (CheckBoxMatchedCustomerPrefix.Checked)
        {
            GridView1.Columns[5].Visible = true;
        }
        else GridView1.Columns[5].Visible = false;

        //if (CheckBoxShowByIgw.Checked == true)
        //{
        //    GridView1.Columns[2].Visible = true;
        //}
        //else GridView1.Columns[2].Visible = false;
        if (CheckBoxMatchedSupplierPrefix.Checked)
        {
            GridView1.Columns[6].Visible = true;
        }
        else GridView1.Columns[6].Visible = false;

        if (CheckBoxPartner.Checked == true)
        {
            GridView1.Columns[1].Visible = true;
        }
        else GridView1.Columns[1].Visible = false;
        if (CheckBoxInRoute.Checked == true)
        {
            GridView1.Columns[2].Visible = true;
        }
        else GridView1.Columns[2].Visible = false;

        if (CheckBoxOutPartner.Checked == true)
        {
            GridView1.Columns[3].Visible = true;
        }
        else GridView1.Columns[3].Visible = false;
        if (CheckBoxOutRoute.Checked == true)
        {
            GridView1.Columns[4].Visible = true;
        }
        else GridView1.Columns[4].Visible = false;

        if (CheckBoxShowCost.Checked == true)
        {
            GridView1.Columns[12].Visible = true;
            GridView1.Columns[13].Visible = true;
            GridView1.Columns[14].Visible = true;
        }
        else
        {
            GridView1.Columns[12].Visible = false;
            GridView1.Columns[13].Visible = false;
            GridView1.Columns[14].Visible = false;
        }
        if (CheckBoxShowPerformance.Checked == true)
        {
            GridView1.Columns[15].Visible = true;
            GridView1.Columns[16].Visible = true;
            GridView1.Columns[17].Visible = true;
            GridView1.Columns[18].Visible = true;
            GridView1.Columns[19].Visible = true;
            GridView1.Columns[20].Visible = true;

        }
        else
        {
            GridView1.Columns[15].Visible = false;
            GridView1.Columns[16].Visible = false;
            GridView1.Columns[17].Visible = false;
            GridView1.Columns[18].Visible = false;
            GridView1.Columns[19].Visible = false;
            GridView1.Columns[20].Visible = false;

        }
        //make profit invisible, it's useless
        //GridView1.Columns[15].Visible = false;
        //GridView1.Columns[9].Visible = true;//carrier's duration

        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            connection.Open();
           
            MySqlCommand cmd = new MySqlCommand(GetQuery(), connection);

            cmd.Connection = connection; 
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
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataSet dataset = new DataSet();
                da.Fill(dataset);


                GridView1.DataSource = dataset;
                bool hasRows = dataset.Tables.Cast<DataTable>()
                                   .Any(table => table.Rows.Count != 0);
                if (hasRows == true)
                {
                    GridView1.ShowFooter = true;//set it here before setting footer text, setting this to true clears already set footer text
                    Label1.Text = "";
                    Button1.Visible = true; //show export
                                            //Summary calculation for grid view*************************
                    TrafficReportDatasetBased tr = new TrafficReportDatasetBased(dataset);
                    tr.Ds = dataset;
                    List<NoOfCallsVsPdd> callVsPdd = new List<NoOfCallsVsPdd>();
                    foreach (DataRow dr in tr.Ds.Tables[0].Rows)
                    {
                        tr.CallStat.TotalCalls += tr.ForceConvertToLong(dr["Total Calls"]);
                        tr.CallStat.ConnectedCalls += tr.ForceConvertToLong(dr["Connected Calls"]);
                        //tr.CallStat.ConnectedCallsbyCauseCodes += tr.ForceConvertToLong(dr["ConectbyCC"]);
                        tr.CallStat.SuccessfullCalls += tr.ForceConvertToLong(dr["Successful Calls"]);
                        tr.CallStat.TotalActualDuration += tr.ForceConvertToDouble(dr["Customer Duration"]);
                        tr.CallStat.TotalRoundedDuration += tr.ForceConvertToDouble(dr["Supplier Duration"]);
                        //tr.CallStat.TotalDuration1 += tr.ForceConvertToDouble(dr["Duration1"]);
                        tr.CallStat.PartnerCost += tr.ForceConvertToDouble(dr["Cost"]);
                        tr.CallStat.BtrcRevShare += tr.ForceConvertToDouble(dr["Margin"]);
                        tr.CallStat.IgwRevenue += tr.ForceConvertToDouble(dr["Revenue"]);
                        NoOfCallsVsPdd cpdd = new NoOfCallsVsPdd(tr.ForceConvertToLong(dr["Successful Calls"]), tr.ForceConvertToDouble(dr["PDD"]));
                        callVsPdd.Add(cpdd);
                    }
                    tr.CallStat.TotalActualDuration = Math.Round(tr.CallStat.TotalActualDuration, 2);
                    //tr.CallStat.TotalDuration1 = Math.Round(tr.CallStat.TotalDuration1, 2);
                    //tr.CallStat.TotalDuration2 = Math.Round(tr.CallStat.TotalDuration2, 2);
                    //tr.CallStat.TotalDuration3 = Math.Round(tr.CallStat.TotalDuration3, 2);
                    //tr.CallStat.TotalDuration4 = Math.Round(tr.CallStat.TotalDuration4, 2);
                    tr.CallStat.TotalRoundedDuration = Math.Round(tr.CallStat.TotalRoundedDuration, 2);
                    tr.CallStat.PartnerCost  = Math.Round(tr.CallStat.PartnerCost, 2);
                    tr.CallStat.BtrcRevShare = Math.Round(tr.CallStat.BtrcRevShare, 2);
                    tr.CallStat.IgwRevenue = Math.Round(tr.CallStat.IgwRevenue, 2);
                    tr.CallStat.CalculateAsr(2);
                    tr.CallStat.CalculateAcd(2);
                    tr.CallStat.CalculateAveragePdd(callVsPdd, 2);
                    tr.CallStat.CalculateCcr(2);
                    tr.CallStat.CalculateCcRbyCauseCode(2);
                    //SUMMARY CALCULATION FOR GRIDVIEW COMPLETE


                    //display summary information in the footer
                    Dictionary<string, dynamic> fieldSummaries = new Dictionary<string, dynamic>();//key=colname,val=colindex in grid
                                                                                                   //all keys have to be lowercase, because db fields are lower case at times
                    fieldSummaries.Add("total calls", tr.CallStat.TotalCalls);
                    fieldSummaries.Add("connected calls", tr.CallStat.ConnectedCalls);
                    //fieldSummaries.Add("connectbycc", tr.CallStat.ConnectedCallsbyCauseCodes);
                    fieldSummaries.Add("successful calls", tr.CallStat.SuccessfullCalls);
                    fieldSummaries.Add("customer duration", tr.CallStat.TotalActualDuration);
                    fieldSummaries.Add("supplier duration", tr.CallStat.TotalRoundedDuration);
                    //fieldSummaries.Add("duration1", tr.CallStat.TotalDuration1);
                    fieldSummaries.Add("cost", tr.CallStat.PartnerCost);
                    fieldSummaries.Add("margin", tr.CallStat.BtrcRevShare);
                    fieldSummaries.Add("revenue", tr.CallStat.IgwRevenue);
                    fieldSummaries.Add("asr", tr.CallStat.Asr);
                    fieldSummaries.Add("acd", tr.CallStat.Acd);
                    fieldSummaries.Add("pdd", tr.CallStat.Pdd);
                    fieldSummaries.Add("ccr", tr.CallStat.Ccr);
                    fieldSummaries.Add("ccrbycc", tr.CallStat.CcRbyCauseCode);
                    tr.FieldSummaries = fieldSummaries;

                    Session["AlfPreTr"] = tr;//save to session

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
    

    protected void Button1_Click(object sender, EventArgs e)
    {
        if (Session["AlfPreTr"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            TrafficReportDatasetBased tr = (TrafficReportDatasetBased)Session["AlfPreTr"];
            DataSetWithGridView dsG = new DataSetWithGridView(tr, GridView1);//invisible columns are removed in constructor
            CreateExcelFileAspNet.CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(tr.Ds, "AlfPremium_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + ".xlsx", Response);
        }
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
        //if (CheckBoxShowByAns.Checked == true)
        //{
        //    DropDownListAns.Enabled = true;
        //}
        //else DropDownListAns.Enabled = false;
    }
    protected void CheckBoxShowByIgw_CheckedChanged(object sender, EventArgs e)
    {
        //if (CheckBoxShowByIgw.Checked == true)
        //{
        //    DropDownListIgw.Enabled = true;
        //}
        //else DropDownListIgw.Enabled = false;
    
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
        
        //if (CheckBoxShowByAns.Checked == true)
        //{
        //    Dictionary<string,partner> dicKpiAns = null;
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        if (ViewState["dicKpiAns"] != null)
        //        {
        //            dicKpiAns = (Dictionary<string,partner>)ViewState["dicKpiAns"];
        //        }
        //        else
        //        {
        //            return;
        //        }
        //        //Label lblCountry = (Label)e.Row.FindControl("Label3");
        //        string thisAnsName = DataBinder.Eval(e.Row.DataItem, "ANS").ToString();
        //        Single thisAsr = 0;
        //        Single thisAcd = 0;
        //        Single thisCcr = 0;
        //        Single thisPdd = 0;
        //        Single thisCcRbyCc = 0;
        //        Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ASR").ToString(),out thisAsr);
        //        Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ACD").ToString(), out thisAcd);
        //        Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCR").ToString(), out thisCcr);
        //        Single.TryParse(DataBinder.Eval(e.Row.DataItem, "PDD").ToString(), out thisPdd);
        //        Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCRByCC").ToString(), out thisCcRbyCc);
        //        partner thisPartner = null;
        //        dicKpiAns.TryGetValue(thisAnsName, out thisPartner);
        //        if (thisPartner != null)
        //        {
        //            Color redColor=ColorTranslator.FromHtml("#FF0000");
        //            //ASR
        //            Single refAsr = 0;
        //            if (Convert.ToSingle(thisPartner.refasr) > 0)
        //            {
        //                refAsr=Convert.ToSingle(thisPartner.refasr);
        //            }
        //            if ((thisAsr < refAsr) || (thisAsr == 0))
        //            {
        //                e.Row.Cells[16].ForeColor = Color.White;
        //                e.Row.Cells[16].BackColor = redColor;
        //                e.Row.Cells[16].Font.Bold = true;
        //            }

        //            //fas detection
        //            double tempDbl = 0;
        //            double refAsrFas = refAsr + refAsr * .5;//fas threshold= 30% of ref asr by default
        //            double.TryParse(thisPartner.refasrfas.ToString(), out tempDbl);
        //            if (tempDbl > 0) refAsrFas = tempDbl;

        //            if (thisAsr > refAsrFas && refAsrFas>0)
        //            {
        //                e.Row.Cells[16].ForeColor = Color.White;
        //                e.Row.Cells[16].BackColor = Color.Blue;
        //                e.Row.Cells[16].Font.Bold = true;
        //            }

        //            //ACD
        //            Single refAcd = 0;
        //            if (Convert.ToSingle(thisPartner.refacd) > 0)
        //            {
        //                refAcd = Convert.ToSingle(thisPartner.refacd);
        //            }
        //            if (thisAcd < refAcd)
        //            {
        //                //e.Row.Cells[16].ForeColor = RedColor;
        //                e.Row.Cells[17].ForeColor = Color.White;
        //                e.Row.Cells[17].BackColor = redColor;
        //                e.Row.Cells[17].Font.Bold = true;
        //            }

        //            //PDD
        //            Single refPdd = 0;
        //            if (Convert.ToSingle(thisPartner.refpdd) > 0)
        //            {
        //                refPdd = Convert.ToSingle(thisPartner.refpdd);
        //            }
        //            if (thisPdd > refPdd)
        //            {
        //                //e.Row.Cells[17].ForeColor = RedColor;
        //                e.Row.Cells[18].ForeColor = Color.White;
        //                e.Row.Cells[18].BackColor = redColor;
        //                e.Row.Cells[18].Font.Bold = true;
        //            }

        //            //CCR
        //            Single refCcr = 0;
        //            if (Convert.ToSingle(thisPartner.refccr) > 0)
        //            {
        //                refCcr = Convert.ToSingle(thisPartner.refccr);
        //            }
        //            if (thisCcr < refCcr)
        //            {
        //                //e.Row.Cells[18].ForeColor = RedColor;
        //                e.Row.Cells[19].ForeColor = Color.White;
        //                e.Row.Cells[19].BackColor = redColor;
        //                e.Row.Cells[19].Font.Bold = true;
        //            }

        //            //CCRByCauseCode
        //            Single refCcrCc = 0;
        //            if (Convert.ToSingle(thisPartner.refccrbycc) > 0)
        //            {
        //                refCcrCc = Convert.ToSingle(thisPartner.refccrbycc);
        //            }
        //            if (thisCcRbyCc < refCcrCc)
        //            {
        //                //e.Row.Cells[20].ForeColor = RedColor;
        //                e.Row.Cells[21].ForeColor = Color.White;
        //                e.Row.Cells[21].BackColor = redColor;
        //                e.Row.Cells[21].Font.Bold = true;
        //            }
        //        }
        //    }
        //}//if checkbox ans
        
        //0 ASR highlighting
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            double asr = 1;
            double.TryParse(e.Row.Cells[16].Text, out asr);
            Color redColor2 = ColorTranslator.FromHtml("#FF0000");
            if (asr <= 0)
            {
                e.Row.Cells[14].ForeColor = Color.White;
                e.Row.Cells[14].BackColor = redColor2;
                e.Row.Cells[14].Font.Bold = true;
            }
        }

    }

    protected void CheckBoxOutPartner_OnCheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxOutPartner.Checked == true)
        {
            DropDownListOutPartner.Enabled = true;
        }
        else DropDownListOutPartner.Enabled = false;
    }

    protected void CheckBoxInRoute_OnCheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxInRoute.Checked == true)
        {
            DropDownListInRoute.Enabled = true;
        }
        else DropDownListInRoute.Enabled = false;
    }

    protected void CheckBoxOutRoute_OnCheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxOutRoute.Checked == true)
        {
            DropDownListOutRoute.Enabled = true;
        }
        else DropDownListOutRoute.Enabled = false;
    }
}
