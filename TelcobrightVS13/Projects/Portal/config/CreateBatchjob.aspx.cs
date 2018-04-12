using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PortalApp;
using TelcobrightMediation;
public partial class _DefaultBatch : System.Web.UI.Page
{
    private int m_ShowByCountry=0;
    private int m_ShowByANS = 0;
   
    DataTable dt;


    protected void Page_Load(object sender, EventArgs e)
    {
        //common code for report pages
        //view state of ParamBorder div
        string TempText = hidValueFilter.Value;
        bool LastVisible = hidValueFilter.Value == "invisible" ? false : true;
        if (hidValueSubmitClickFlag.Value == "false")
        {
            if (LastVisible)
            {
                //show filters...
                Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "ShowParamBorderDiv();", true);
            }
            else
            {
                //hide filters...
                Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDiv();", true);
            }
        }
        //set this month's start and End Date [Time] in the date picker controls...
        if (!IsPostBack)
        {
            
            

            //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
            //from Partner

            string ThisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

            MySqlConnection connection = new MySqlConnection(ThisConectionString);
            string database = connection.Database.ToString();

            using (PartnerEntities Context = new PartnerEntities())
            {
                //populate jobtype
                List<enumjobdefinition> lstJobDef = Context.enumjobdefinitions.Where(c=>c.BatchCreatable==1).Include(c=>c.enumjobtype).ToList();
                DropDownListJobType.Items.Clear();
                foreach (enumjobdefinition jobdef in lstJobDef)
                {
                    DropDownListJobType.Items.Add(new ListItem(jobdef.enumjobtype.Type+"-"+jobdef.Type, jobdef.id.ToString()));
                }
                telcobrightpartner ThisCustomer = (from c in Context.telcobrightpartners
                                                   where c.databasename == database
                                                   select c).First();
                int ThisOperatorId = ThisCustomer.idCustomer;
                int idOperatorType = Convert.ToInt32(ThisCustomer.idOperatorType);

                Session["sesidOperator"] = ThisOperatorId;
                Session["sesidOperatorType"] = idOperatorType;

                //populate the switch combobox for this TB customer
                List<ne> SwitchList = Context.nes.Where(c => c.idCustomer == ThisOperatorId).ToList();
                DropDownListSwitch.Items.Add(new ListItem(" All", "-1"));
                foreach (ne Sw in SwitchList)
                {
                    DropDownListSwitch.Items.Add(new ListItem(Sw.SwitchName, Sw.idSwitch.ToString()));
                }
                //populate incoming route
                using (PartnerEntities conpartner = new PartnerEntities())
                {
                    List<route> RouteList = conpartner.routes.ToList();
                    Dictionary<int,ne> dicNE = null;
                    using (PartnerEntities ConTelco = new PartnerEntities())
                    {
                        dicNE = ConTelco.nes.ToDictionary(c => c.idSwitch);
                    }
                     
                    DropDownListInRoute.Items.Add(new ListItem(" All", "-1"));
                    foreach (route Sw in RouteList)
                    {
                        ne ThisSwitch = null;
                        dicNE.TryGetValue(Convert.ToInt32(Sw.SwitchId), out ThisSwitch);
                        DropDownListInRoute.Items.Add(new ListItem(ThisSwitch.SwitchName+"-" + Sw.RouteName, Sw.idroute.ToString()));
                    }
                    //error code, casting to any enumtype will do, can't cast to mediationchecklist as it requires id, calldirection both to be not null
                    List<enumjobtype> ErrorCodeList = conpartner.Database.SqlQuery<enumjobtype>(" SELECT distinct fieldnumber as id,expression as Type FROM mediationchecklist order by id ").ToList();
                    DropDownListError.Items.Add(new ListItem("[ All]", "-1"));
                    foreach (enumjobtype Sw in ErrorCodeList)
                    {
                        DropDownListError.Items.Add(new ListItem(Sw.Type + " (" + Sw.id + ")", Sw.id.ToString()));
                    }
                }
                


            }

            //set summary as report source default
            DropDownListReportSource.SelectedIndex = 1;

            TextBoxYear.Text = System.DateTime.Now.ToString("yyyy");
            TextBoxYear1.Text = System.DateTime.Now.ToString("yyyy");
            DropDownListMonth.SelectedIndex = int.Parse(System.DateTime.Now.ToString("MM")) - 1;
            DropDownListMonth1.SelectedIndex = int.Parse(System.DateTime.Now.ToString("MM")) - 1;
            //txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
            //txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
            txtDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtDate1.Text = DateTime.Now.ToString("dd/MM/yyyy");


            //set controls if page is called for a template
            TreeView MasterTree = (TreeView)Master.FindControl("TreeView1");
            NameValueCollection n = Request.QueryString;
            CommonCode CommonCodes = new CommonCode();
            if (n.HasKeys())
            {
                string TemplateName = "";
                var items = n.AllKeys.SelectMany(n.GetValues, (k, v) => new { key = k, value = v });
                foreach (var ThisParam in items)
                {
                    if (ThisParam.key == "templ")
                    {
                        TemplateName = ThisParam.value;
                        break;
                    }
                }
                if (TemplateName != "")
                {
                    //set controls here ...
                    string RetVal = CommonCodes.SetTemplateControls(this, TemplateName);
                    if (RetVal != "success")
                    {
                        string script = "alert('Error occured while loading template: " + TemplateName
                            + "! " + Environment.NewLine + RetVal + "');";
                        ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                        return;
                    }
                }
                //Load Report Templates in TreeView dynically from database.
                CommonCode CommonCode = new CommonCode();
                CommonCode.LoadReportTemplatesTree(ref MasterTree);
            }
            //Retrieve Path from TreeView for displaying in the master page caption label
            string LocalPath = Request.Url.LocalPath;
            int Pos2ndSlash = LocalPath.Substring(1, LocalPath.Length - 1).IndexOf("/");
            string Root_Folder = LocalPath.Substring(1, Pos2ndSlash);
            int EndOfRootFolder = Request.Url.AbsoluteUri.IndexOf(Root_Folder);
            string UrlWithQueryString = ("~" +"/"+Root_Folder + Request.Url.AbsoluteUri.Substring((EndOfRootFolder + Root_Folder.Length), Request.Url.AbsoluteUri.Length - (EndOfRootFolder + Root_Folder.Length))).Replace("%20", " ");
            TreeNodeCollection cNodes = MasterTree.Nodes;
            TreeNode MatchedNode = null;
            foreach (TreeNode N in cNodes)//for each nodes at root level, loop through children
            {
                MatchedNode = CommonCodes.RetrieveNodes(N, UrlWithQueryString);
                if (MatchedNode != null)
                {
                    break;
                }
            }
            //set screentile/caption in the master page...
            Label lblScreenTitle = (Label)Master.FindControl("lblScreenTitle");
            if (MatchedNode != null)
            {
                lblScreenTitle.Text = MatchedNode.ValuePath;
            }
            else
            {
                lblScreenTitle.Text = "";
            }


            //End of Site Map Part *******************************************************************

        }

        hidValueSubmitClickFlag.Value = "false";
    }

    protected void submit_Click(object sender, EventArgs e)
    {
       
        



    }

    protected void Button1_Click(object sender, EventArgs e)
    {
       
    }

    public static void ExportToSpreadsheet(DataTable table, string name, List<string> ColNameList, List<int> ColumnSortlist)
    {
        HttpContext context = HttpContext.Current;
        context.Response.Clear();

        string ThisRow = "";
        
        //write columns in order specified in ColumnSortedList
        int ii = 0;
        for (ii=0; ii<ColNameList.Count;ii++ )
        {  
            //ThisRow +=  table.Columns[ColumnSortlist[ii]].ColumnName + ",";
            ThisRow += ColNameList[ii]+ ",";
        }

        ThisRow = ThisRow.Substring(0, ThisRow.Length - 1) + "<br/>";
        context.Response.Write(ThisRow);


        foreach (DataRow row in table.Rows)
        {
            ThisRow = "";
            for (ii = 0; ii < ColumnSortlist.Count; ii++) //for each column
            {  
                ThisRow += row[ColumnSortlist[ii]].ToString().Replace(",", string.Empty) + ",";
            }
            ThisRow = ThisRow.Substring(0, ThisRow.Length - 1) + "<br/>";
            context.Response.Write(ThisRow);
        }
        
        context.Response.ContentType = "application/ms-excel";
        context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + name + ".csv");
        context.Response.End();
    }



    protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
    {
      
    }


    protected void CheckBoxShowByAns_CheckedChanged(object sender, EventArgs e)
    {
       
    }
    protected void CheckBoxShowByIgw_CheckedChanged(object sender, EventArgs e)
    {
       
    
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
        dateInitialize();
    }

    public void dateInitialize()
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
        
       
    }
    protected void ButtonCreateJob_Click(object sender, EventArgs e)
    {
        //validation
        string JobName = TextBoxJobName.Text;
        if (JobName == "")
        {
            lblJobStatus.Text = " Job name can't be empty!";
            return;
        }
        //duplicate job name...
        string s = System.Configuration.ConfigurationManager.ConnectionStrings["partnerEntities"].ConnectionString.Replace("database=telcobrightmediation", "database=telcobrightbatch");
        //System.Data.EntityClient.EntityConnectionStringBuilder ec = new System.Data.EntityClient.EntityConnectionStringBuilder(s);
        string ProviderConnectionString = "";// ec.ProviderConnectionString;
        using (PartnerEntities contextbatch = new PartnerEntities())
        {
            if (contextbatch.jobs.Where(c => c.JobName.StartsWith(JobName)).Any())
            {
                lblJobStatus.Text = "Duplicate job name!";
                return;
            }
        }

        int BatchSize = 0;
        int.TryParse(TextBoxBatchSize.Text, out BatchSize);
        if (BatchSize <= 0)
        {
            lblJobStatus.Text = "Invalid batch size";
            return;
        }

        DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
        DateTime denddate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
        DateTime comparedate = dstartdate;
        string startdate = txtDate.Text;
        string enddate = txtDate1.Text;
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
            lblJobStatus.Text = "Invalid Date!";
            return;
        }

        int? CustomerId = null;
        int? SupplierId = null;

      

        string SourceTable = "";
        int JobType = -1;
        int.TryParse(DropDownListJobType.SelectedValue, out JobType);

        if (JobType <= 0)
        {
            lblJobStatus.Text = "Invalid job type!";
            return;
        }

        switch (DropDownListJobType.SelectedValue)
        {
            case "2": //Error Process
                SourceTable = "cdrerror";
                break;
            case "5"://re rate
            case "3"://re process
                SourceTable = "cdrloaded";
                break;
            case "4"://re process file
                break;
        }

        //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
        //from Partner

        string ThisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

        MySqlConnection connection = new MySqlConnection(ThisConectionString);
        string database = connection.Database.ToString();
        List<ne> lstSwitch;
        using (PartnerEntities Context = new PartnerEntities())
        {
            telcobrightpartner ThisCustomer = (from c in Context.telcobrightpartners
                                                            where c.databasename == database
                                                            select c).First();
            int ThisOperatorId = ThisCustomer.idCustomer;
            lstSwitch = Context.nes.Where(c => c.idCustomer == ThisOperatorId).ToList() ;

            if (lstSwitch.Count <= 0)
            {
                lblJobStatus.Text = "No Switch found for customer: " + ThisCustomer.CustomerName;
                return;
            }

        }

        string ConStrSqlTelcoBright = ConfigurationManager.ConnectionStrings["telcobrightmediationSql"].ConnectionString;
        string ConStrPartner = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

        CreateBatchJob(JobName,JobType, BatchSize, SourceTable, database, ref lstSwitch, null, null, dstartdate, denddate, CustomerId, SupplierId,ref ConStrSqlTelcoBright,ref ConStrPartner);

    }

    public int CreateBatchJob(string JobName,int JobType,int BatchSize, string SourceTable, string database, ref List<ne> lstSwitch,
          long? FileSerialStart, long? FileSerialEnd, DateTime? StartingPeriod, DateTime? EndingPeriod,
        int? CustomerId, int? SupplierId,ref string ConStrSqlTelcoBright,ref string ConStrPartner)
    {
        string StrSuccess = "";
        bool NoRecordAtAll=true;
        long GrandTotalRecCount = 0;
        int JobCount = 0;
        try
        {
            List<SqlSingleWhereClause> lstWhereParams = new List<SqlSingleWhereClause>();
            SqlSingleWhereClause NewParam = new SqlSingleWhereClause();
            if (StartingPeriod != null)
            {
                //make sure both parameters are set if one of these is mentioned
                if (EndingPeriod == null || EndingPeriod <= StartingPeriod)
                {

                    lblJobStatus.Text = @"Both Starting Period and Ending Period must be mentioned and End Period must be greater than starting period!";
                    return 0;
                }
                NewParam = new SqlSingleWhereClause();
                NewParam.Expression = "starttime>=";
                NewParam.ParamType = SqlWhereParamType.Datetime;
                NewParam.ParamValue = Convert.ToDateTime(StartingPeriod).ToString("yyyy-MM-dd HH:mm:ss");
                lstWhereParams.Add(NewParam);

                NewParam = new SqlSingleWhereClause();
                NewParam.Expression = "starttime<=";
                NewParam.ParamType = SqlWhereParamType.datetime;
                NewParam.ParamValue = Convert.ToDateTime(EndingPeriod).ToString("yyyy-MM-dd HH:mm:ss");
                lstWhereParams.Add(NewParam);
            }

            
            if (DropDownListError.SelectedValue != "-1")
            {
                NewParam = new SqlSingleWhereClause();
                NewParam.Expression = "instr(field4," + DropDownListError.SelectedValue + ") > ";
                NewParam.ParamType = SqlWhereParamType.numeric;
                NewParam.ParamValue = "0";
                lstWhereParams.Add(NewParam);
            }

            if (DropDownListInRoute.SelectedValue != "-1")
            {
                //retrieve routename and switchid
                using (PartnerEntities Context = new PartnerEntities())
                {
                    int idRoute = Convert.ToInt32(DropDownListInRoute.SelectedValue);
                    route ThisRoute = Context.routes.Where(c => c.idroute == idRoute).ToList().First();

                    NewParam = new SqlSingleWhereClause();
                    NewParam.Expression = "incomingroute=";
                    NewParam.ParamType = SqlWhereParamType.text;
                    NewParam.ParamValue = ThisRoute.RouteName;
                    lstWhereParams.Add(NewParam);

                    NewParam = new SqlSingleWhereClause();
                    NewParam.Expression = "switchid=";
                    NewParam.ParamType = SqlWhereParamType.numeric;
                    NewParam.ParamValue = ThisRoute.SwitchId.ToString();
                    lstWhereParams.Add(NewParam);
                }
                
                
            }

            BatchSqlJobParamJson BaseJobParam = new BatchSqlJobParamJson
            {
                TableName = SourceTable,
                BatchSize = BatchSize,
                lstWhereParamsSingle=lstWhereParams
            };


            foreach (ne ThisSwitch in lstSwitch)//create a job for each selected ne
            {
                BatchSqlJobParamJson ThisJobParam = BaseJobParam.GetCopy();
                //create a switchid param for this job
                NewParam = new SqlSingleWhereClause();
                NewParam.Expression = "switchid=";
                NewParam.ParamType = SqlWhereParamType.numeric;
                NewParam.ParamValue = ThisSwitch.idSwitch.ToString();
                ThisJobParam.lstWhereParamsSingle.Add(NewParam);

                using (PartnerEntities context = new PartnerEntities())
                {

                    job newjob = new job();
                    newjob.Progress = 0;
                    newjob.idjobdefinition = JobType;
                    newjob.Status = 6;//created
                    newjob.JobName = JobName;
                    newjob.idjobdefinition = JobType;
                    newjob.CreationTime = DateTime.Now;
                    newjob.idNE = ThisSwitch.idSwitch;
                    newjob.JobParameter = JsonConvert.SerializeObject(ThisJobParam);
                    newjob.priority = 5;
                    context.jobs.Add(newjob);
                    context.SaveChanges();
                    JobCount++;
                }
                

            }//for each switch
            lblJobStatus.Text = JobCount + " job(s) created.";
          
            
            return 1;

        }
        catch (Exception e1)
        {
            lblJobStatus.Text = JobCount + " job(s) created." +"<br/>" +
                "Error: " + e1.Message + "<br/>" + e1.InnerException + "<br/>"
                + "Operation completed partially, try running the batch create task again."
                + "<br/>" + " TOTAL " + GrandTotalRecCount + " RECORDS SELECTED FOR THE BATCH JOB."; ;
            return 0;
        }
    }
    
}//class
