using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using MediationModel;
using PortalApp;
using TelcobrightMediation;

public partial class DefaultMediation : System.Web.UI.Page
{
    private int _mShowByCountry = 0;
    private int _mShowByAns = 0;

    DataTable _dt;
    static TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
    Dictionary<string, string> userVsDbName = telcobrightConfig.DeploymentProfile.UserVsDbName;



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


    public DataSet GetDataSet(string connectionString, string sql)
    {

        MySqlConnection conn = new MySqlConnection(connectionString);
        MySqlDataAdapter da = new MySqlDataAdapter();
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        da.SelectCommand = cmd;
        DataSet ds = new DataSet();

        conn.Open();
        da.Fill(ds);
        conn.Close();

        return ds;
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        //export
        ExportFirstOrAll(-1);
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        //export
        long records = -1;
        if (long.TryParse(this.TextBoxNoOfRecords.Text, out records) == false)
        {
            this.lblStatus.Text = "Invalid number of records!";
            return;
        }
        else
        {
            if (records <= 0)
            {
                this.lblStatus.Text = "Invalid number of records!";
                return;
            }
        }
        ExportFirstOrAll(records);
    }

    public void ExportFirstOrAll(long noOfRecords)
    {
        string logIdentityName = this.User.Identity.Name;
        String selectedIcx = logIdentityName;
        string selectedUserdbName;
        if (userVsDbName.ContainsKey(logIdentityName))
        {
            selectedUserdbName = userVsDbName[logIdentityName];
        }
        else
        {
            selectedUserdbName = telcobrightConfig.DatabaseSetting.DatabaseName;
        }
        string sql = "";
        if (noOfRecords == -1)//all records
        {
            sql = (string)this.ViewState["jobs.squery"];
        }
        else//first N
        {
            sql = ((string)this.ViewState["jobs.squery"]).Replace(";", "") + " limit 0," + this.TextBoxNoOfRecords.Text + ";";
        }
        string con =  ConfigurationManager.ConnectionStrings["Partner"].ConnectionString;
        if (!selectedUserdbName.Contains("btrc_cas"))
        {
            con = con.Replace("btrc_cas", selectedUserdbName);
        }
        DataTable dt = GetDataSet(con, sql).Tables[0];

        ExportToSpreadsheet(dt, "jobs_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    public static void ExportToSpreadsheet(DataTable table, string name)
    {
        HttpContext context = HttpContext.Current;
        context.Response.Clear();

        string thisRow = "";
        foreach (DataColumn column in table.Columns)
        {

            thisRow += column.ColumnName + ",";
        }
        thisRow = thisRow.Substring(0, thisRow.Length - 1) + Environment.NewLine;
        context.Response.Write(thisRow);


        foreach (DataRow row in table.Rows)
        {
            thisRow = "";
            for (int i = 0; i < table.Columns.Count; i++)
            {

                thisRow += row[i].ToString().Replace(",", string.Empty) + ",";
                //context.Response.Write(row[i].ToString());
                //context.Response.Write(row[i]);
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

    protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.GridView1.PageIndex = e.NewPageIndex;
        submit_Click(sender, e);
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

        long totalSequence = 0;
        List<job> lstAllCdr = null;
        var databaseSetting = telcobrightConfig.DatabaseSetting;
        databaseSetting.DatabaseName = DropDownListViewIncomingRoute.SelectedValue;
        using (PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
        {
            //context.CommandTimeout = 3600;
            string sql = "";
            sql = "select * from job where CreationTime >= '" + dstartdate.ToString("yyyy-MM-dd HH:mm:ss") + "' and CreationTime <= '" +
                denddate.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
            //switch filter
            if (this.DropDownListPartner.SelectedValue != "-1")
            {
                sql += " and idne=" + this.DropDownListPartner.SelectedValue;
            }
            //job type
            if (this.DropDownListJobtype.SelectedValue != "-1")
            {
                sql += " and idjobdefinition= " + this.DropDownListJobtype.SelectedValue;
            }
            //job status
            if (this.DropDownListJobStatus.SelectedValue != "-1")
            {
                if (CheckBoxNegateStatus.Checked == false)
                {
                    sql += " and status=" + this.DropDownListJobStatus.SelectedValue;
                }
                else
                {
                    sql += " and status!=" + this.DropDownListJobStatus.SelectedValue;
                }

            }
            //jobname contains
            if (this.TextBoxJobName.Text.Trim() != "")
            {
                if (CheckBoxJobNameContains.Checked == false)
                {
                    sql += " and lower(jobname) like '%" + this.TextBoxJobName.Text.Trim().ToLower() + "%'";
                }
                else
                {
                    sql += " and lower(jobname) not like '%" + this.TextBoxJobName.Text.Trim().ToLower() + "%'";
                }
            }
            //order by latest completed first, latest listed first
            sql += " order by ifnull(completiontime,'0001-01-01') desc,creationtime desc ";
            lstAllCdr = context.jobs.SqlQuery(sql, typeof(job)).ToList();
            this.ViewState["jobs.squery"] = sql;
            //include ne, jobdefinition and job status descriptions to show value in gridview
            var dicJobdef = context.enumjobdefinitions.ToDictionary(c => c.id);
            var dicJobType = context.enumjobtypes.ToDictionary(c => c.id);
            var dicJobStatus = context.enumjobstatus.ToDictionary(c => c.id);
            var dicNe = context.nes.ToDictionary(c => c.idSwitch);


            foreach (job j in lstAllCdr)
            {
                long tempLong = 0;
                double tempDouble = 0;
                double tempDouble1 = 0;
                long.TryParse(j.NoOfSteps.ToString(), out tempLong);
                totalSequence += tempLong;

                ne ne = null;
                dicNe.TryGetValue(Convert.ToInt32(j.idNE), out ne);
                j.ne = ne;

                enumjobstatu jstat = null;
                dicJobStatus.TryGetValue(j.Status, out jstat);
                j.enumjobstatu = jstat;

                enumjobdefinition jdef = null;
                dicJobdef.TryGetValue(j.idjobdefinition, out jdef);
                enumjobtype jtype = null;
                dicJobType.TryGetValue(jdef.jobtypeid, out jtype);
                jdef.enumjobtype = jtype;
                j.enumjobdefinition = jdef;
            }
        }
        this.GridView1.DataSource = lstAllCdr;
        this.GridView1.DataBind();

        if (lstAllCdr.Count <= 0)
        {
            this.lblStatus.ForeColor = Color.Red;
            this.lblStatus.Text = "No Data!";
            this.Button1.Visible = false; //show export
            this.Button2.Enabled = false;
            this.Button3.Enabled = false;
        }
        else
        {
            this.lblStatus.ForeColor = Color.Green;
            this.lblStatus.Text = " Total Jobs: " + lstAllCdr.Count + ", "
                + "Total Sequence: " + totalSequence + ", ";

            this.Button1.Visible = true; //show export
            this.lblStatus.Visible = true;
            this.Button2.Enabled = true;
            this.Button3.Enabled = true;
        }

        return;

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
        for (ii = 0; ii < colNameList.Count; ii++)
        {
            //ThisRow +=  table.Columns[ColumnSortlist[ii]].ColumnName + ",";
            thisRow += colNameList[ii] + ",";
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

    protected void EntityDataSwitch_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        string partnerConStr = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
        int posDatabase = partnerConStr.IndexOf("database");
        //make sure to keep databasename at the last of the connection string
        string dbName = partnerConStr.Substring(posDatabase + 9, partnerConStr.Length - posDatabase - 9);
        //find TB customerid
        var databaseSetting = telcobrightConfig.DatabaseSetting;
        using (PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
        {
            int idOperator = context.telcobrightpartners.Where(c => c.databasename == dbName).First().idCustomer;
            var allSwitches = e.Query.Cast<ne>();
            e.Query = allSwitches.Where(c => c.idCustomer == idOperator);
        }

    }

    protected void CheckBoxSwithcName_CheckedChanged(object sender, EventArgs e)
    {

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
    protected void DropDownListViewIncomingRoute_SelectedChanged(object sender, EventArgs e)
    {
        setSwitchListDropDown(DropDownListPartner, EventArgs.Empty);
    }
    protected void setICXListDropDown(object sender, EventArgs e)
    {
        string logIdentityName = this.User.Identity.Name;
        String selectedIcx = logIdentityName;
        TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
        string selectedUserdbName;
        Dictionary<string, string> userVsDbName = telcobrightConfig.DeploymentProfile.UserVsDbName;
        if (userVsDbName.ContainsKey(logIdentityName))
        {
            selectedUserdbName = userVsDbName[logIdentityName];
        }
        else
        {
            selectedUserdbName = telcobrightConfig.DatabaseSetting.DatabaseName;
        }
        DropDownListViewIncomingRoute.Items.Clear();
        if (selectedUserdbName.Contains("btrc"))
        {
            foreach (var kv in telcobrightConfig.DeploymentProfile.UserVsDbName)
            {
                if (!kv.Value.Contains("btrc"))
                {
                    string username = kv.Key;
                    string dbNameAsRouteName = kv.Value;
                    string icxName = dbNameAsRouteName.Split('_')[0];
                    DropDownListViewIncomingRoute.Items.Add(new ListItem(icxName, dbNameAsRouteName));
                }

            }
        }
        else
        {
            string individualIcxName = selectedUserdbName.Split('_')[0];
            DropDownListViewIncomingRoute.Items.Add(new ListItem(individualIcxName, selectedUserdbName));
           
        }

    }

    protected void setSwitchListDropDown(object sender, EventArgs e)
    {


        TelcobrightConfig tb = telcobrightConfig;
        tb.DatabaseSetting.DatabaseName = DropDownListViewIncomingRoute.SelectedValue;

        using (PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(tb.DatabaseSetting))
        {
            //populate switch
            List<ne> lstNe = context.nes.ToList();
            this.DropDownListPartner.Items.Clear();
            //this.DropDownListPartner.Items.Add(new ListItem(" [All]", "-1"));
            this.DropDownListPartner.Items.Add(new ListItem(" [All]", "-1"));
            foreach (ne nE in lstNe)
            {
                if (!nE.SwitchName.Contains("dummy"))
                {
                    this.DropDownListPartner.Items.Add(new ListItem(nE.SwitchName, nE.idSwitch.ToString()));
                }
            }
        }
    }
}
