using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using MediationModel;

public partial class DefaultRtUtil : System.Web.UI.Page
{
    private int _mShowByCountry=0;
    private int _mShowByAns = 0;

    DataTable _dt;

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
            this.txtDate1.Text = endtime.ToString("dd/MM/yyyy HH:mm:ss");
            this.txtDate.Text = starttime.ToString("dd/MM/yyyy HH:mm:ss");

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
        

        using (MySqlConnection connection = new MySqlConnection())
        {
            //connection.ConnectionString = "server=10.0.30.125;User Id=dbreader;password=Takaytaka1#;Persist Security Info=True;default command timeout=3600;database=dbl";
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
            connection.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;

            //IF GROUP BY INTERNATIONAL PARTNER=FALSE
            if (this.DropDownListPartner.SelectedIndex == 0)
            {
                cmd.CommandText = "CALL ErlangByRouteAll(@p_StartDateTime,@p_EndDateTime,@p_W)";

                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                cmd.Parameters.AddWithValue("p_W", this.TextBoxW.Text);
            }
            else if (this.DropDownListPartner.SelectedIndex > 0)
            {
                int idRoute = -1;
                string routeName = "";
                int switchId = -1;
                route thisRoute = null;
                int.TryParse(this.DropDownListPartner.SelectedValue,out idRoute);
                if (idRoute > 0)
                {
                    using (PartnerEntities context = new PartnerEntities())
                    {
                        thisRoute = context.routes.Where(c => c.idroute == idRoute).First();
                        if (thisRoute != null)
                        {
                            routeName = thisRoute.RouteName;
                            switchId = Convert.ToInt32( thisRoute.SwitchId);

                            if (routeName != "" && switchId > 0)
                            {
                                cmd.CommandText = "CALL ErlangByRouteOne(@p_StartDateTime,@p_EndDateTime,@p_W,@p_RouteName,@p_SwitchId)";

                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_W", this.TextBoxW.Text);
                                cmd.Parameters.AddWithValue("p_RouteName", routeName);
                                cmd.Parameters.AddWithValue("p_SwitchId", switchId);
                            }
                            else
                            {
                                this.Label1.Text = "Route Not Found! idRoute=" + idRoute;
                                return;
                            }

                        }
                    }
                }
            }

            //Common Code**************#########################################
            this.Label1.Text = "";
            //GRID VIEW SUM HOLDER, DECLARE TO HANDLE MAX 100 COLUMNS
            decimal[] sumOfGridColumns = new decimal[100];
            int i = 0;
            for (i = 0; i < 100; i++) sumOfGridColumns[i] = 0; //initialize to 0

            //******SUMMARY CALCULATION FOR GRID VIEW
            int[,] summaryColumns = new int[10, 2];//THIS MUST BE CHANGED FOR EACH PAGE
            //If any column flagged in the DATASET to be summarized, but not present in the Grid, EXCEPTION!
            //refer to datasets column index, (not gridview's) those are to summed....
            //2nd dimension=0 means summary type=int, 1 means summary type=round 2 digits single
            summaryColumns[0, 0] = 2;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[0, 1] = 0;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[1, 0] = 3;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[1, 1] = 0;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[2, 0] = 4;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[2, 1] = 0;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[3, 0] = 5;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[3, 1] = 0;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[4, 0] = 6;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[4, 1] = 1;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[5, 0] = 7;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[5, 1] = 0;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[6, 0] = 8;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[6, 1] = 1;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[7, 0] = 9;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[7, 1] = 1;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[8, 0] = 10;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[8, 1] = 1;//THIS MUST BE CHANGED FOR EACH PAGE

            summaryColumns[9, 0] = 11;//THIS MUST BE CHANGED FOR EACH PAGE
            summaryColumns[9, 1] = 1;//THIS MUST BE CHANGED FOR EACH PAGE
            //if daily summary off then the date column will not be in the dataset
            //so decrement each summary column's index by one

            if (this.CheckBoxDailySummary.Checked == false)
            {
                this.GridView1.Columns[0].Visible = false;
                //int j = 0;
                //for (j = 0; j < SummaryColumns.GetLength(0); j++) SummaryColumns[j, 0] = SummaryColumns[j, 0] - 1;

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

                cmd.CommandText = cmd.CommandText.Replace("CALL ", "CALL " + summaryInterval + "Summary");

            }

            //source for report... cdr or summary data
            switch (this.DropDownListReportSource.SelectedValue)
            {
                case "1"://CDR
                    break;
                case "2"://Summary Data
                    cmd.CommandText = cmd.CommandText.Replace("CALL ", "CALL SD");
                    break;
                case "3"://Cdr Error
                    cmd.CommandText = cmd.CommandText.Replace("CALL ", "CALL Err");
                    break;
            }

            //Egress Side
            if (this.CheckBoxShowPerformance.Checked == true)
            {
                cmd.CommandText = cmd.CommandText.Replace("ErlangBy", "ErlangEGBy");
                this.GridView1.Columns[1].HeaderText = "Egress Route";
            }
            else
            {
                this.GridView1.Columns[1].HeaderText = "Ingress Route";
            }
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            da.Fill(dataset);

            List<int> columnSortList = new List<int>();
            List<string> colNamelist = new List<string>();

            this._dt = dataset.Tables[0];
            this.Session["RouteUtilization.aspx.csdt16"] = this._dt; //THIS MUST BE CHANGED FOR EACH PAGE

            //suppress hidden baseColumns of the gridview
            int cnt=0;
            int thisColumnIndexDataGrid = -1;
            colNamelist = new List<string>();
            for(cnt=0;cnt< this.GridView1.Columns.Count;cnt++)
            {
                thisColumnIndexDataGrid = -1;
                int count = 0;
                for (count = 0; count < this._dt.Columns.Count; count++)
                {
                    if (this.GridView1.Columns[cnt].SortExpression.ToLower() == this._dt.Columns[count].ColumnName.ToLower())
                    {
                        thisColumnIndexDataGrid = count;
                        
                        if (this.GridView1.Columns[cnt].Visible == true)
                        {
                            columnSortList.Add(count);
                            colNamelist.Add(this.GridView1.Columns[cnt].HeaderText);   
                        }
                        break;
                    }
                }
                

            }

            this.Session["RouteUtilization.aspx.csdt260"] = colNamelist;//THIS MUST BE CHANGED FOR EACH PAGE
            //now add sort order to override the column list in the dataset and to match gridview setting


            this.Session["RouteUtilization.aspx.csdt360"] = columnSortList;

            this.GridView1.DataSource = dataset;
            bool hasRows = dataset.Tables.Cast<DataTable>()
                               .Any(table => table.Rows.Count != 0);

            if (hasRows == true)
            {
                this.Label1.Text = "";
                this.Button1.Visible = true; //show export

                //Summary calculation for grid view*************************

                foreach (DataRow dr in this._dt.Rows)
                {
                    int r = 0;
                    for (r = 0; r < summaryColumns.GetLength(0); r++)
                    {
                        string thisValue = dr.ItemArray[summaryColumns[r, 0]].ToString();
                        decimal temp;

                        if (decimal.TryParse(thisValue, out temp))
                        {
                            sumOfGridColumns[summaryColumns[r, 0]] += temp;
                        }
                    }
                }

                //SUMMARY CALCULATION FOR GRIDVIEW COMPLETE

                this.GridView1.ShowFooter = true;
                this.GridView1.DataBind();
                //display summary information in the footer
                if (this.CheckBoxDailySummary.Checked == true)
                    this.GridView1.FooterRow.Cells[0].Text = "Total:";
                else
                {
                    int cnt2 = 1;
                    for (cnt2 = 1; cnt2 < this.GridView1.Columns.Count; cnt2++)
                    {//display the test "Total: " in the footer of the first visible cell of the gridview
                        if (this.GridView1.Columns[cnt2].Visible == true)
                        {
                            this.GridView1.FooterRow.Cells[cnt2].Text = "Total:";
                            break;
                        }
                    }
                }

                int r1;
                for (r1 = 0; r1 < summaryColumns.GetLength(0); r1++)
                {
                    string thisDatasetFieldname = this._dt.Columns[summaryColumns[r1, 0]].ColumnName;
                    //find this datasetfield's index in current gridview, its required because the positions
                    //change as some baseColumns of the grid are dynamically made visible/invisible

                    int thisColumnIndexInGridView = -1;
                    int count = 0;
                    for (count = 0; count < this.GridView1.Columns.Count; count++)
                    {
                        if (this.GridView1.Columns[count].SortExpression.ToLower() == thisDatasetFieldname.ToLower())
                        {
                            thisColumnIndexInGridView = count;
                            break;
                        }
                    }
                    if (thisColumnIndexInGridView != -1)
                    {
                        if (summaryColumns[r1, 1] == 1) //column has fraction upto 2 decimal
                        {
                            this.GridView1.FooterRow.Cells[thisColumnIndexInGridView].Text = decimal.Round(sumOfGridColumns[summaryColumns[r1, 0]], 6).ToString("F2");

                        }
                        else // column is integer type, round to 0 decimal place
                        {
                            this.GridView1.FooterRow.Cells[thisColumnIndexInGridView].Text = decimal.Round(sumOfGridColumns[summaryColumns[r1, 0]], 6).ToString("F0");


                        }
                    }
                }


                //End of Common code*******################################################

                ////calculation of Overall ASR,ACD and PDD sum
                
                //    double TotalCalls = Convert.ToDouble(GridView1.FooterRow.Cells[4].Text);
                //    double SuccessfulCalls = Convert.ToDouble(GridView1.FooterRow.Cells[5].Text);
                //    double ConnectedCalls = Convert.ToDouble(GridView1.FooterRow.Cells[6].Text);
                //    double ActualDuration = Convert.ToDouble(GridView1.FooterRow.Cells[7].Text);

                //    double ASR = 100 * SuccessfulCalls / TotalCalls;
                //    double ACD = ActualDuration / SuccessfulCalls;
                //    double PDD = 0;
                //    double CCR=0;
                //    if (ConnectedCalls != 0)
                //    {
                //        //calculate PDD later
                //        CCR = 100*ConnectedCalls / TotalCalls;
                //    }
                //    else
                //    {
                //        PDD = 0;
                //        CCR = 0;
                //    }

                    
                //    GridView1.FooterRow.Cells[15].Text = Convert.ToString(Math.Round(ASR, 2));
                //    GridView1.FooterRow.Cells[16].Text = Convert.ToString(Math.Round(ACD, 2));
                //    GridView1.FooterRow.Cells[17].Text = Convert.ToString(Math.Round(PDD, 2));
                //    GridView1.FooterRow.Cells[18].Text = Convert.ToString(Math.Round(CCR, 2));

                //calculation of Overall ASR,ACD and PDD sum complete
                    //common code for report pages
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
        DataTable dt2;
        List<string> colNameList=new List<string>();
        List<int> columnSortList=new List<int>();
        if (this.Session["RouteUtilization.aspx.csdt16"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            dt2 = (DataTable) this.Session["RouteUtilization.aspx.csdt16"];//THIS MUST BE CHANGED IN EACH PAGE
            
            if (this.Session["RouteUtilization.aspx.csdt260"] != null) //THIS MUST BE CHANGED IN EACH PAGE
            {
                colNameList = (List<string>) this.Session["RouteUtilization.aspx.csdt260"];//THIS MUST BE CHANGED IN EACH PAGE
            }

            if (this.Session["RouteUtilization.aspx.csdt360"] != null) //THIS MUST BE CHANGED IN EACH PAGE
            {
                columnSortList = (List<int>) this.Session["RouteUtilization.aspx.csdt360"];//THIS MUST BE CHANGED IN EACH PAGE
            }
            ExportToSpreadsheet(dt2, "International Incoming",colNameList,columnSortList); //THIS MUST BE CHANGED IN EACH PAGE
            this.Session.Abandon();
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
        //write baseColumns in order specified in ColumnSortedList
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

    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        
    }
}
