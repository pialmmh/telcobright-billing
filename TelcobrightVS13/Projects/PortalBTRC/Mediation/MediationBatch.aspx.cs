using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;

public partial class DefaultMedBatch : Page
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
            DateTime starttime = DateTime.Today;//endtime.AddMinutes(a * (-1));
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

    public DateTime FirstDayOfMonthFromDateTime(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    public DateTime LastDayOfMonthFromDateTime(DateTime dateTime)
    {
        DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
        return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
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
        string startdate = this.txtDate.Text;
        string enddate = this.txtDate1.Text;
        
        DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
        DateTime denddate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
        DateTime comparedate = dstartdate;

        if (startdate.Length == 10)
        {
            DateTime.TryParseExact(startdate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate);
        }
        else if (startdate.Length > 10)
        {
            DateTime.TryParseExact(startdate, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate);
        }

        if (enddate.Length == 10)
        {
            DateTime.TryParseExact(enddate + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out denddate);
        }
        else if (enddate.Length > 10)
        {
            DateTime.TryParseExact(enddate, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out denddate);
        }

        if (dstartdate > comparedate && denddate > comparedate)
        {
            startdate = dstartdate.ToString("yyyy-MM-dd HH:mm:ss");
            enddate = denddate.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            this.Label1.Text = "Invalid Date!";
            return;
        }

        string conStrBatch = ConfigurationManager.ConnectionStrings["PartnerEntities"].ConnectionString.Replace("database=telcobrightmediation", "database=telcobrightbatch");

        List<job> lstAllCdr=null;
        using (PartnerEntities context = new PartnerEntities())
        {
            //context.CommandTimeout = 3600;
            lstAllCdr = context.jobs.Where(c => c.CreationTime >= dstartdate && c.CreationTime <= denddate).OrderByDescending(o => o.JobName)
                .ToList();

            this.GridView1.DataSource = lstAllCdr;
            this.GridView1.DataBind();

            if (lstAllCdr.Count <= 0)
            {
                this.Label1.Text = "No Data!";
                this.Button1.Visible = false; //show export
            }
            else
            {
                this.Label1.Text = "";
                this.Button1.Visible = true; //show export
            }
        
        }
        
        return;

        using (MySqlConnection connection = new MySqlConnection())
        {
            //connection.ConnectionString = "server=10.0.30.125;User Id=dbreader;password=Takaytaka1#;Persist Security Info=True;default command timeout=3600;database=dbl";
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
            connection.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;

            //IF GROUP BY INTERNATIONAL CARRIER=FALSE
            








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


            

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            da.Fill(dataset);

            List<int> columnSortList = new List<int>();
            List<string> colNamelist = new List<string>();

            this._dt = dataset.Tables[0];
            this.Session["Mediation.aspx.csdt16"] = this._dt; //THIS MUST BE CHANGED FOR EACH PAGE

            //suppress hidden columns of the gridview
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
                    //change as some columns of the grid are dynamically made visible/invisible

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
        if (this.Session["Mediation.aspx.csdt16"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            dt2 = (DataTable) this.Session["Mediation.aspx.csdt16"];//THIS MUST BE CHANGED IN EACH PAGE
            
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

    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //set command argument for delete button tobe retrieved in rowdatabound event...
            LinkButton lnkBtn = (LinkButton)e.Row.FindControl("LinkButton2");
            //string StartDate = DataBinder.Eval(e.Row.DataItem, "startdate").ToString();

            lnkBtn.CommandArgument = DataBinder.Eval(e.Row.DataItem, "idallcdr").ToString();
            
            Label lblJobType = (Label)e.Row.FindControl("lblJobType");
            int jobType = Convert.ToInt32( DataBinder.Eval(e.Row.DataItem, "StartSequenceNumber").ToString());
            if (lblJobType != null)
            {
                switch (jobType)
                {
                    case 1:
                        lblJobType.Text = "Error Process";
                        break;
                    case 2:
                        lblJobType.Text = "Re-Rate";
                        break;
                    case 3:
                        lblJobType.Text = "Re-Process in DB";
                        break;
                    case 4:
                        lblJobType.Text = "Re-Process File";
                        break;
                    default:
                        lblJobType.Text = "Unknown";
                        break;
                }
                
            }

            //enabled
            CheckBox chkEnabled = (CheckBox)e.Row.FindControl("CheckBox1");
            int enabled = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "EndSequenceNumber").ToString());
            string status = DataBinder.Eval(e.Row.DataItem, "status").ToString();
            //enable checkbox if job is in waiting state only
            if (status == "waiting")
            {
                chkEnabled.Enabled = true;
            }
            else
            {
                chkEnabled.Enabled = false;
            }
            switch (enabled)
            {
                case 1:
                    chkEnabled.Checked = true;
                    break;
                default:
                    chkEnabled.Checked = false;
                    break;
                    
            }
        }
        
    }

    protected void EntityDataSwitch_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        string partnerConStr = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
        int posDatabase = partnerConStr.IndexOf("database");
        //make sure to keep databasename at the last of the connection string
        string dbName = partnerConStr.Substring(posDatabase + 9, partnerConStr.Length - posDatabase - 9);
        //find TB customerid
        using (PartnerEntities context = new PartnerEntities())
        {
            int idOperator = context.telcobrightpartners.Where(c => c.databasename == dbName).First().idCustomer;
            var allSwitches = e.Query.Cast<ne>();
            e.Query = allSwitches.Where(c => c.idCustomer == idOperator);
        }

    }
    protected void EntityDataAllCdr_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        

    }
    protected void LinkButtonToday_Click(object sender, EventArgs e)
    {
        this.txtDate.Text = DateTime.Today.ToString("dd/MM/yyyy");
        this.txtDate1.Text = DateTime.Today.ToString("dd/MM/yyyy");
        submit_Click(sender, e);
    }
    protected void LinkButtonYesterday_Click(object sender, EventArgs e)
    {
        this.txtDate.Text = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy");
        this.txtDate1.Text = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy");
        submit_Click(sender, e);
    }
    protected void LinkButtonLast3_Click(object sender, EventArgs e)
    {
        this.txtDate.Text = DateTime.Today.AddDays(-2).ToString("dd/MM/yyyy");
        this.txtDate1.Text = DateTime.Today.ToString("dd/MM/yyyy");
        submit_Click(sender, e);
    }
    protected void LinkButtonLast7_Click(object sender, EventArgs e)
    {
        this.txtDate.Text = DateTime.Today.AddDays(-6).ToString("dd/MM/yyyy");
        this.txtDate1.Text = DateTime.Today.ToString("dd/MM/yyyy");
        submit_Click(sender, e);
    }
    protected void LinkButtonThisMonth_Click(object sender, EventArgs e)
    {
        this.txtDate.Text = FirstDayOfMonthFromDateTime(DateTime.Today).ToString("dd/MM/yyyy");
        this.txtDate1.Text = LastDayOfMonthFromDateTime(DateTime.Today).ToString("dd/MM/yyyy");
        submit_Click(sender, e);
    }
    protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Delete")
        {
            
            long idallcdr = -1;
            long.TryParse(e.CommandArgument.ToString(), out idallcdr);
            if (idallcdr >= 0)
            {
                string constrBatch = ConfigurationManager.ConnectionStrings["telcobrightmediationSql"].ConnectionString.Replace("database=telcobrightmediation", "database=telcobrightbatch");
                using (MySqlConnection con = new MySqlConnection(constrBatch))
                {
                    try{
                    con.Open();
                    }
                    catch(Exception e1)
                    {
                        this.Label1.Text=e1.Message;
                        return;
                    }
                    
                    using (MySqlCommand cmd = new MySqlCommand("", con))
                    {
                        try
                        {
                            cmd.CommandText = " set autocommit=0;";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = " delete from cdrbatch where fileserialno=" + idallcdr + ";";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = " delete from allcdr where idallcdr=" + idallcdr + ";";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = " commit; ";
                            cmd.ExecuteNonQuery();

                            

                        }
                        catch (Exception e2)
                        {
                            cmd.CommandText = " rollback; ";
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                }//using mysql connection
            }
            else
            {
                this.Label1.Text = "Unable to find id column's value!";
                
            }
            
        }
    }
    protected void GridView1_RowDeleted(object sender, GridViewDeletedEventArgs e)
    {
        
    }
    protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        submit_Click(sender, e);
    }

    protected void Save_Click(object sender, EventArgs e)
    {
        
        string constrBatch = ConfigurationManager.ConnectionStrings["telcobrightmediationSql"].ConnectionString.Replace("database=telcobrightmediation", "database=telcobrightbatch");
        using (MySqlConnection con = new MySqlConnection(constrBatch))
        {
            try
            {
                con.Open();
            }
            catch (Exception e1)
            {
                this.Label1.Text = e1.Message;
                this.Label1.Text = e1.Message;
                return;
            }

            using (MySqlCommand cmd = new MySqlCommand("", con))
            {
                try
                {
                    cmd.CommandText = " set autocommit=0;";
                    cmd.ExecuteNonQuery();

                    foreach (GridViewRow row in this.GridView1.Rows)
                    {
                        LinkButton lnkBtn = (LinkButton)row.FindControl("LinkButton2");
                        long idallcdr = long.Parse(lnkBtn.CommandArgument.ToString());
                        string status = ((Label)row.FindControl("lblStatus")).Text;
                        if (status == "waiting")
                        {
                            if (((CheckBox)row.FindControl("CheckBox1")).Checked)
                            {
                                cmd.CommandText = " update allcdr set endsequencenumber=1 where idallcdr=" + idallcdr;
                            }
                            else
                            {
                                cmd.CommandText = " update allcdr set endsequencenumber=0 where idallcdr=" + idallcdr;
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }

                    cmd.CommandText = " commit; ";
                    cmd.ExecuteNonQuery();
                    this.Label1.ForeColor = Color.Green;
                    this.Label1.Text = "Changes saved.";


                }
                catch (Exception e2)
                {
                    this.Label1.Text = e2.Message;
                    cmd.CommandText = " rollback; ";
                    cmd.ExecuteNonQuery();
                }
            }

        }//using mysql connection
        
        
    }
}
