using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ExportToExcel;
using LibraryExtensions;
using reports;
using MediationModel;
using PortalApp;
using PortalApp.ReportHelper;

public partial class DefaultRptIntlOutIcx : System.Web.UI.Page
{
    DataTable _dt; bool _timerflag = false;
    private string GetQuery()
    {

        string StartDate = txtDate.Text;
        string EndtDate = (txtDate1.Text.ConvertToDateTimeFromMySqlFormat()).AddSeconds(1).ToMySqlStyleDateTimeStrWithoutQuote();
        string tableName = DropDownListReportSource.SelectedValue+"02";

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

        string constructedSQL = new SqlHelperIntOutIcx
                        (StartDate,
                         EndtDate,
                         groupInterval,
                         tableName,
                         new List<string>()
                            {
                                getInterval(groupInterval), 
                                CheckBoxShowByCountry.Checked==true?"tup_countryorareacode":string.Empty,
                                CheckBoxShowByDestination.Checked==true?"tup_matchedprefixcustomer":string.Empty,
                                CheckBoxIntlPartner.Checked==true?"tup_outpartnerid":string.Empty,
                                CheckBoxShowByAns.Checked==true?"tup_sourceID":string.Empty,
                                CheckBoxShowByIgw.Checked==true?"tup_inpartnerid":string.Empty,
                                CheckBoxShowByCustomerRate.Checked==true?"tup_customerrate":string.Empty,
                            },
                      
                         new List<string>()
                            {
                                CheckBoxShowByCountry.Checked==true?DropDownListCountry.SelectedIndex>0?" tup_countryorareacode="+DropDownListCountry.SelectedValue:string.Empty:string.Empty,
                                CheckBoxShowByDestination.Checked==true?DropDownPrefix.SelectedIndex>0?"tup_matchedprefixcustomer="+DropDownPrefix.SelectedValue:string.Empty:string.Empty,
                                CheckBoxIntlPartner.Checked==true?DropDownListIntlCarier.SelectedIndex>0?" tup_outpartnerid="+DropDownListIntlCarier.SelectedValue:string.Empty:string.Empty,
                                CheckBoxShowByAns.Checked==true?DropDownListAns.SelectedIndex>0?" tup_sourceID="+DropDownListAns.SelectedValue:string.Empty:string.Empty,
                                CheckBoxShowByIgw.Checked==true?DropDownListIgw.SelectedIndex>0?" tup_inpartnerid="+DropDownListIgw.SelectedValue:string.Empty:string.Empty
                            }).getSQLString();

        //File.WriteAllText("c:" + Path.DirectorySeparatorChar + "temp" + Path.DirectorySeparatorChar + "testQuery.txt", constructedSQL);
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
        //view by country/prefix logic has been changed later, adding this new flag
        bool notViewingByCountry = CheckBoxShowByDestination.Checked | (!CheckBoxShowByCountry.Checked);//not viewing by country if view by desination is checked

        
        //undo the effect of hiding some grid by the summary button first******************
        GridView1.Columns[0].Visible = true;
        if (CheckBoxDailySummary.Checked)
        {
            GridView1.Columns[1].Visible = false;
            GridView1.Columns[2].Visible = false;
            GridView1.Columns[3].Visible = false;
            GridView1.Columns[5].Visible = false;
        }
        //GridView1.Columns[1].Visible = true;
        //GridView1.Columns[2].Visible = false;
        //GridView1.Columns[3].Visible = false;
        GridView1.Columns[4].Visible = false;
        //*****************************

        if (CheckBoxShowByCountry.Checked == true)
        {
            GridView1.Columns[1].Visible = true;
        }
        else GridView1.Columns[1].Visible = false;

        if (CheckBoxShowByDestination.Checked == true)
        {
            GridView1.Columns[2].Visible = true;
        }
        else GridView1.Columns[2].Visible = false;

        if (CheckBoxShowByIgw.Checked == true)
        {
            GridView1.Columns[3].Visible = true;
        }
        else GridView1.Columns[3].Visible = false;

        if (CheckBoxIntlPartner.Checked == true)
        {
            GridView1.Columns[5].Visible = true;
        }
        else GridView1.Columns[5].Visible = false;

        
        if (CheckBoxShowPerformance.Checked == true)
        {
            GridView1.Columns[14].Visible = true;
        }
        else
        {
            GridView1.Columns[14].Visible = false;
        }

        if (CheckBoxShowCost.Checked == true)
        {
            GridView1.Columns[19].Visible = true;
            GridView1.Columns[20].Visible = true;
            GridView1.Columns[21].Visible = true;
            GridView1.Columns[22].Visible = true;
            GridView1.Columns[23].Visible = true;
            GridView1.Columns[24].Visible = true;
            GridView1.Columns[25].Visible = true;
            GridView1.Columns[26].Visible = true;
        }
        else
        {
            GridView1.Columns[19].Visible = false;
            GridView1.Columns[20].Visible = false;
            GridView1.Columns[21].Visible = false;
            GridView1.Columns[22].Visible = false;
            GridView1.Columns[23].Visible = false;
            GridView1.Columns[24].Visible = false;
            GridView1.Columns[25].Visible = false;
            GridView1.Columns[26].Visible = false;
        }
        

        using (MySqlConnection connection = new MySqlConnection())
        {

            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            connection.Open();

            MySqlCommand cmd = new MySqlCommand(GetQuery(),connection);
            cmd.Connection = connection;

            //All Possible Report Combinations are here:

            //^^^^^^^^^^^*********&&&&&&&&&&&&&&&& if checkbox view by destination is checked

            if (CheckBoxShowByDestination.Checked == true || CheckBoxShowByCountry.Checked == true)
            {

                GridView1.Columns[1].Visible = true;//country
                if (CheckBoxShowByDestination.Checked)
                    GridView1.Columns[2].Visible = true;//destination
                else
                    GridView1.Columns[2].Visible = false;
            }
          
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

            _dt = dataset.Tables[0];
            Session["InternationalOut.aspx.csdt17"] = dataset; //THIS MUST BE CHANGED FOR EACH PAGE

            GridView1.DataSource = dataset;
            bool hasRows = dataset.Tables.Cast<DataTable>()
                               .Any(table => table.Rows.Count != 0);

            if (hasRows == true)
            {
                Label1.Text = "";
                Button1.Visible = true; //show export
                GridView1.ShowFooter = true;//set it here before setting footer text, setting this to true clears already set footer text
                Label1.Text = "";
                Button1.Visible = true; //show export
                //Summary calculation for grid view*************************
                TrafficReportDatasetBased tr = new TrafficReportDatasetBased(dataset);
                tr.Ds = dataset;
                List<NoOfCallsVsPdd> callVsPdd = new List<NoOfCallsVsPdd>();
                foreach (DataRow dr in tr.Ds.Tables[0].Rows)
                {
                    tr.CallStat.TotalCalls += tr.ForceConvertToLong(dr["CallsCount"]);
                    tr.CallStat.ConnectedCalls += tr.ForceConvertToLong(dr["ConnectedCount"]);
                    tr.CallStat.ConnectedCallsbyCauseCodes += tr.ForceConvertToLong(dr["ConectbyCC"]);
                    tr.CallStat.SuccessfullCalls += tr.ForceConvertToLong(dr["No of Calls (Outgoing International)"]);
                    tr.CallStat.TotalActualDuration += tr.ForceConvertToDouble(dr["Paid Minutes (Outgoing Internaitonal)"]);
                    tr.CallStat.TotalRoundedDuration += tr.ForceConvertToDouble(dr["RoundedDuration"]);
                    tr.CallStat.TotalDuration3 += tr.ForceConvertToDouble(dr["hmsduration"]);
                    tr.CallStat.TotalDuration2 += tr.ForceConvertToDouble(dr["supplierduration"]);
                    tr.CallStat.XAmount += tr.ForceConvertToDouble(dr["X (BDT)"]);
                    tr.CallStat.YAmount += tr.ForceConvertToDouble(dr["Y (USD)"]);
                    tr.CallStat.ZAmount += tr.ForceConvertToDouble(dr["Z (BDT)"]);
                    tr.CallStat.IgwRevenue += tr.ForceConvertToDouble(dr["revenueigwout"]);
                    tr.CallStat.BtrcRevShare += tr.ForceConvertToDouble(dr["tax1"]);

                    NoOfCallsVsPdd cpdd = new NoOfCallsVsPdd(tr.ForceConvertToLong(dr["No of Calls (Outgoing International)"]), tr.ForceConvertToDouble(dr["PDD"]));
                    callVsPdd.Add(cpdd);
                }
                tr.CallStat.TotalActualDuration = Math.Round(tr.CallStat.TotalActualDuration, 2);
                tr.CallStat.TotalDuration1 = Math.Round(tr.CallStat.TotalDuration1, 2);
                tr.CallStat.TotalDuration2 = Math.Round(tr.CallStat.TotalDuration2, 2);
                tr.CallStat.TotalDuration3 = Math.Round(tr.CallStat.TotalDuration3, 2);
                tr.CallStat.TotalDuration4 = Math.Round(tr.CallStat.TotalDuration4, 2);
                tr.CallStat.TotalRoundedDuration = Math.Round(tr.CallStat.TotalRoundedDuration, 2);
                tr.CallStat.XAmount = Math.Round(tr.CallStat.XAmount, 2);
                tr.CallStat.YAmount = Math.Round(tr.CallStat.YAmount, 2);
                tr.CallStat.ZAmount = Math.Round(tr.CallStat.ZAmount, 2);
                tr.CallStat.IgwRevenue = Math.Round(tr.CallStat.IgwRevenue, 2);
                tr.CallStat.CalculateAsr(2);
                tr.CallStat.CalculateAcd(2);
                tr.CallStat.CalculateAveragePdd(callVsPdd, 2);
                tr.CallStat.CalculateCcr(2);
                tr.CallStat.CalculateCcRbyCauseCode(2);
                tr.CallStat.CalculateProfitPerMinute(2);
                //SUMMARY CALCULATION FOR GRIDVIEW COMPLETE


                //display summary information in the footer
                Dictionary<string, dynamic> fieldSummaries = new Dictionary<string, dynamic>();//key=colname,val=colindex in grid
                //all keys have to be lowercase, because db fields are lower case at times
                fieldSummaries.Add("callscount", tr.CallStat.TotalCalls);
                fieldSummaries.Add("connectedcount", tr.CallStat.ConnectedCalls);
                fieldSummaries.Add("connectbycc", tr.CallStat.ConnectedCallsbyCauseCodes);
                fieldSummaries.Add("no of calls (outgoing international)", tr.CallStat.SuccessfullCalls);
                fieldSummaries.Add("paid minutes (outgoing internaitonal)", tr.CallStat.TotalActualDuration);
                fieldSummaries.Add("roundedduration", tr.CallStat.TotalRoundedDuration);
                fieldSummaries.Add("hmsduration", tr.CallStat.TotalDuration3);
                fieldSummaries.Add("supplierduration", tr.CallStat.TotalDuration2);
                fieldSummaries.Add("asr", tr.CallStat.Asr);
                fieldSummaries.Add("acd", tr.CallStat.Acd);
                fieldSummaries.Add("pdd", tr.CallStat.Pdd);
                fieldSummaries.Add("ccr", tr.CallStat.Ccr);
                fieldSummaries.Add("ccrbycc", tr.CallStat.CcRbyCauseCode);
                fieldSummaries.Add("x (bdt)", tr.CallStat.XAmount);
                fieldSummaries.Add("y (usd)", tr.CallStat.YAmount);
                fieldSummaries.Add("z (bdt)", tr.CallStat.ZAmount);
                fieldSummaries.Add("revenueigwout", tr.CallStat.IgwRevenue);
                fieldSummaries.Add("tax1", tr.CallStat.BtrcRevShare);

                tr.FieldSummaries = fieldSummaries;

                Session["IntlOut"] = tr;//save to session

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


            }//if has rows=true
            else
            {
                GridView1.DataBind();
                Label1.Text = "No Data!";
                Button1.Visible = false; //hide export
            }





        }//using mysql connection

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        TelcobrightConfig tbc = PageUtil.GetTelcobrightConfig();
        PageUtil.ApplyPageSettings(this,false, tbc);
        //load Country KPI

        Dictionary<string, countrycode> dicKpiCountry = new Dictionary<string, countrycode>();
        using (PartnerEntities context = new PartnerEntities())
        {
            foreach (countrycode c in context.countrycodes.ToList())
            {
                dicKpiCountry.Add(c.Code, c);
            }
            Session["dicKpiCountry"] = dicKpiCountry;
        }

        //load Destination KPI
        Dictionary<string, xyzprefix> dicKpiDest = new Dictionary<string, xyzprefix>();
        using (PartnerEntities context = new PartnerEntities())
        {
            foreach (xyzprefix c in context.xyzprefixes.ToList())
            {
                dicKpiDest.Add(c.Prefix, c);
            }
            Session["dicKpiDest"] = dicKpiDest;
        }

        //common code for report pages
        //view state of ParamBorder div
        string tempText = hidValueFilter.Value;
        bool lastVisible = hidValueFilter.Value == "invisible" ? false : true;
        if (hidValueSubmitClickFlag.Value == "false")
        {
            if (lastVisible)
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
            //set summary as report source default
            DropDownListReportSource.SelectedIndex = 0;
            //get latest usd rate
            Single usdExchangeRate = 0;
            var connectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            using (PartnerEntities contex = new PartnerEntities())
            {
                var CountryList = contex.countrycodes.ToList();

                DropDownListCountry.Items.Clear();
                DropDownListCountry.Items.Add(new ListItem(" [All]", "-1"));
                foreach (countrycode c in CountryList)
                {
                    DropDownListCountry.Items.Add(new ListItem(c.Name, c.Code.ToString()));
                }
                var PrifexList = contex.xyzprefixes.ToList();
                DropDownPrefix.Items.Clear();
                DropDownPrefix.Items.Add(new ListItem(" [All]", "-1"));
                foreach (xyzprefix  p in PrifexList)
                {
                    DropDownPrefix.Items.Add(new ListItem(p.Description , p.Prefix.ToString()));
                }
                DropDownPrefix.Enabled = CheckBoxShowByDestination.Checked;
                var ANSList = contex.partners.Where(p => p.PartnerType == 1).ToList();
                DropDownListAns.Items.Clear();
                DropDownListAns.Items.Add(new ListItem(" [All]", "-1"));
                foreach (partner p in ANSList)
                {
                    DropDownListAns.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                }
                var IOSList = contex.partners.Where(p => p.PartnerType == 2).ToList();
                DropDownListIgw.Items.Clear();
                DropDownListIgw.Items.Add(new ListItem(" [All]", "-1"));
                foreach (partner p in IOSList)
                {
                    DropDownListIgw.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                }
                var PartnerList = contex.partners.Where(p => p.PartnerType == 3).ToList();
                DropDownListIntlCarier.Items.Clear();
                DropDownListIntlCarier.Items.Add(new ListItem(" [All]", "-1"));
                foreach (partner p in PartnerList)
                {
                    DropDownListIntlCarier.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                }

            }
            #region Comment protion for table name change
            //using (MySqlConnection connection = new MySqlConnection(connectionString))
            //{
            //    connection.Open();
            //    string sql;
            //    sql = " select RateAmount from usdexchangerateagainstbdt where field1=2 order by Field2 desc limit 0,1;";
            //    using (MySqlCommand command = new MySqlCommand(sql, connection))
            //    {

            //        command.CommandType = CommandType.Text;

            //        MySqlDataReader myReader = command.ExecuteReader();
            //        try
            //        {
            //            while (myReader.Read())
            //            {

            //                if (Single.Parse(myReader["RateAmount"].ToString()) > 0)
            //                {

            //                    usdExchangeRate = Single.Parse(myReader["RateAmount"].ToString());

            //                }

            //            }
            //        }
            //        catch (Exception e1)
            //        {
            //            //Console.WriteLine("{0} Exception caught.", e);
            //            usdExchangeRate = 0;

            //        }

            //        finally
            //        {

            //            myReader.Close();
            //            connection.Close();
            //        }

            //        TextBoxUsdRate.Text = usdExchangeRate.ToString();

            //    }//using mysql command
            //}//using mysql connection	
            #endregion



            TextBoxYear.Text = System.DateTime.Now.ToString("yyyy");
            TextBoxYear1.Text = System.DateTime.Now.ToString("yyyy");
            DropDownListMonth.SelectedIndex = int.Parse(System.DateTime.Now.ToString("MM")) - 1;
            DropDownListMonth1.SelectedIndex = int.Parse(System.DateTime.Now.ToString("MM")) - 1;
            //txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
            //txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
            txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
            txtDate1.Text = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");


            //set controls if page is called for a template
            TreeView masterTree = (TreeView)Master.FindControl("TreeView1");
            NameValueCollection n = Request.QueryString;
            CommonCode commonCodes = new CommonCode();
            if (n.HasKeys())
            {
                string templateName = "";
                var items = n.AllKeys.SelectMany(n.GetValues, (k, v) => new { key = k, value = v });
                foreach (var thisParam in items)
                {
                    if (thisParam.key == "templ")
                    {
                        templateName = thisParam.value;
                        break;
                    }
                }
                if (templateName != "")
                {
                    //set controls here ...
                    string retVal = commonCodes.SetTemplateControls(this, templateName);
                    if (retVal != "success")
                    {
                        string script = "alert('Error occured while loading template: " + templateName
                            + "! " + Environment.NewLine + retVal + "');";
                        ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                        return;
                    }
                }
                //Load Report Templates in TreeView dynically from database.
                CommonCode commonCode = new CommonCode();
                commonCode.LoadReportTemplatesTree(ref masterTree);
            }
            //Retrieve Path from TreeView for displaying in the master page caption label
            string localPath = Request.Url.LocalPath;
            int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
            string rootFolder = localPath.Substring(1, pos2NdSlash);
            int endOfRootFolder = Request.Url.AbsoluteUri.IndexOf(rootFolder);
            string urlWithQueryString = ("~" +"/"+rootFolder + Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
            TreeNodeCollection cNodes = masterTree.Nodes;
            TreeNode matchedNode = null;
            foreach (TreeNode N in cNodes)//for each nodes at root level, loop through children
            {
                matchedNode = commonCodes.RetrieveNodes(N, urlWithQueryString);
                if (matchedNode != null)
                {
                    break;
                }
            }
            //set screentile/caption in the master page...
            Label lblScreenTitle = (Label)Master.FindControl("lblScreenTitle");
            lblScreenTitle.Text = "";
            if (matchedNode != null)
            {
                lblScreenTitle.Text = matchedNode.ValuePath;
            }
            else
            {
                lblScreenTitle.Text = "";
            }
            if (lblScreenTitle.Text == "")
            {
                lblScreenTitle.Text = "Reports/Intl. Outgoing/Traffic";
            }

            //End of Site Map Part *******************************************************************

        }

        hidValueSubmitClickFlag.Value = "false";

        //load prefix dropdown combo
        string prefixFilter = "-1";
        if (DropDownListCountry.SelectedValue != "")//avoid executing during initial page load when selected value is not set
        {
            prefixFilter = DropDownListCountry.SelectedValue;
        }

        if (!IsPostBack)
        {
            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["reader"].ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    cmd.CommandText = "CALL OutgoingPrefix(@p_CountryCode)";
                    cmd.Parameters.AddWithValue("p_CountryCode", prefixFilter);

                    MySqlDataReader dr = cmd.ExecuteReader();
                    DropDownPrefix.Items.Clear();
                    while (dr.Read())
                    {
                        DropDownPrefix.Items.Add(new ListItem(dr[1].ToString(), dr[0].ToString()));
                    }
                }
            }
        }

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

    protected void DropDownListMonth_SelectedIndexChanged(object sender, EventArgs e)
    {
        //select 15th of month to find out first and last day of a month as it exists in all months.
        DateTime anyDayOfMonth = new DateTime(int.Parse(TextBoxYear.Text), int.Parse(DropDownListMonth.SelectedValue), 15);
        txtDate.Text = FirstDayOfMonthFromDateTime(anyDayOfMonth).ToString("yyyy-MM-dd 00:00:00");
    }
    protected void DropDownListMonth1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //select 15th of month to find out first and last day of a month as it exists in all months.
        DateTime anyDayOfMonth = new DateTime(int.Parse(TextBoxYear1.Text), int.Parse(DropDownListMonth1.SelectedValue), 15);
        txtDate1.Text = LastDayOfMonthFromDateTime(anyDayOfMonth).ToString("yyyy-MM-dd 23:59:59");
    }
    protected void ButtonTemplate_Click(object sender, EventArgs e)
    {
        //exit if cancel clicked in javascript...
        if (hidValueTemplate.Value == null || hidValueTemplate.Value == "")
        {
            return;
        }

        //check for duplicate templatename and alert the client...
        string templateName = hidValueTemplate.Value;
        if (templateName == "")
        {
            string script = "alert('Templatename cannot be empty!');";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
            return;
        }
        else if (templateName.IndexOf('=') >= 0 || templateName.IndexOf(':') >= 0 ||
            templateName.IndexOf(',') >= 0 || templateName.IndexOf('?') >= 0)
        {
            string script = "alert('Templatename cannot contain characters =:,?');";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
            return;
        }
        using (PartnerEntities context = new PartnerEntities())
        {
            if (context.reporttemplates.Any(c => c.Templatename == templateName))
            {
                string script = "alert('Templatename: " + templateName + " exists, try a different name.');";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                return;
            }
        }
        string localPath = Request.Url.LocalPath;
        int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
        string rootFolder = localPath.Substring(1, pos2NdSlash);
        int endOfRootFolder = Request.Url.AbsoluteUri.IndexOf(rootFolder);
        string urlWithQueryString = "~" + Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length));
        int posQMark = urlWithQueryString.IndexOf("?");
        string urlWithoutQs = (posQMark < 0 ? urlWithQueryString : urlWithQueryString.Substring(0, posQMark)).Replace("~","~/reports");
        CommonCode commonCode = new CommonCode();
        string retVal = commonCode.SaveTemplateControlsByPage(this, templateName, urlWithoutQs);
        
        TreeView masterTree = (TreeView)Page.Master.FindControl("Treeview1");
        commonCode.LoadReportTemplatesTree(ref masterTree);

        //Retrieve Path from TreeView for displaying in the master page caption label
        TreeNodeCollection cNodes = masterTree.Nodes;
        TreeNode matchedNode = null;
        foreach (TreeNode n in cNodes)//for each nodes at root level, loop through children
        {
            matchedNode = commonCode.RetrieveNodes(n, urlWithoutQs + "?templ=" + templateName);
            if (matchedNode != null)
            {
                break;
            }
        }
        //set screentile/caption in the master page...
        Label lblScreenTitle = (Label)Master.FindControl("lblScreenTitle");
        if (matchedNode != null)
        {
            lblScreenTitle.Text = matchedNode.ValuePath;
        }
        else
        {
            lblScreenTitle.Text = "";
        }

        if (retVal == "success")
        {
            string scrSuccess = "alert('Template created successfully');";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", scrSuccess, true);
        }

    }
         
   

    protected void Button1_Click(object sender, EventArgs e)
    {
        if (Session["IntlOut"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            TrafficReportDatasetBased tr = (TrafficReportDatasetBased)Session["IntlOut"];
            DataSetWithGridView dsG = new DataSetWithGridView(tr, GridView1);//invisible columns are removed in constructor
            CreateExcelFileAspNet.CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(tr.Ds, "IntlOutgoing_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + ".xlsx", Response);
        }

    }

    protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //load prefix dropdown combo
        string prefixFilter = "-1";
        if (DropDownListCountry.SelectedValue != "")//avoid executing during initial page load when selected value is not set
        {
            prefixFilter = DropDownListCountry.SelectedValue;
        }
        
        using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["reader"].ConnectionString))
        {
            con.Open();
            using (MySqlCommand cmd = new MySqlCommand("", con))
            {
                cmd.CommandText = "CALL OutgoingPrefix(@p_CountryCode)";
                cmd.Parameters.AddWithValue("p_CountryCode", prefixFilter);

                MySqlDataReader dr = cmd.ExecuteReader();
                DropDownPrefix.Items.Clear();
                while (dr.Read())
                {
                    DropDownPrefix.Items.Add(new ListItem(dr[1].ToString(), dr[0].ToString()));
                }
            }
        }
    }


    protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxIntlPartner.Checked == true)
        {
            DropDownListIntlCarier.Enabled = true;
            //GridView1.Columns[3].Visible = true;
        }
        else
        {
            DropDownListIntlCarier.Enabled = false;
            DropDownListIntlCarier.SelectedIndex = 0;
            //GridView1.Columns[3].Visible = false;
        }
    }


    protected void CheckBoxShowByAns_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxShowByAns.Checked == true)
        {
            DropDownListAns.Enabled = true;
            //GridView1.Columns[3].Visible = true;
        }
        else
        {
            DropDownListAns.Enabled = false;
            DropDownListAns.SelectedIndex = 0;
            //GridView1.Columns[3].Visible = false;
        }
    }
    protected void CheckBoxShowByIgw_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxShowByIgw.Checked == true)
        {
            DropDownListIgw.Enabled = true;
            //GridView1.Columns[4].Visible = true;
        }
        else
        {
            DropDownListIgw.Enabled = false;
            DropDownListIgw.SelectedIndex = 0;
            //GridView1.Columns[4].Visible = false;
        }
    }
    protected void CheckBoxShowByCountry_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxShowByCountry.Checked == true)
        {
            DropDownPrefix.SelectedIndex = 0;
            //DropDownPrefix.Enabled = false;

            DropDownListCountry.SelectedIndex = 0;
            DropDownListCountry.Enabled = true;
            // GridView1.Columns[2].Visible = false; //prefix

            CheckBoxShowByDestination.Checked = false;

        }
        else
        {
            //DropDownPrefix.Enabled = true;
            //GridView1.Columns[2].Visible = true;
            DropDownListCountry.SelectedIndex = 0;
            DropDownListCountry.Enabled = false;

            //load prefix dropdown combo
            string prefixFilter = "-1";
            if (DropDownListCountry.SelectedValue != "")//avoid executing during initial page load when selected value is not set
            {
                prefixFilter = DropDownListCountry.SelectedValue;
            }

            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["reader"].ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    cmd.CommandText = "CALL OutgoingPrefix(@p_CountryCode)";
                    cmd.Parameters.AddWithValue("p_CountryCode", prefixFilter);

                    MySqlDataReader dr = cmd.ExecuteReader();
                    DropDownPrefix.Items.Clear();
                    while (dr.Read())
                    {
                        DropDownPrefix.Items.Add(new ListItem(dr[1].ToString(), dr[0].ToString()));
                    }
                }

            }
        }



    }

    protected void CheckBoxShowByDestination_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxShowByDestination.Checked == true)
        {
            DropDownPrefix.SelectedIndex = 0;
        }
        DropDownPrefix.Enabled = CheckBoxShowByDestination.Checked;
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


    private void DateInitialize()
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
            txtDate1.Text = endtime.ToString("yyyy-MM-dd HH:mm:ss");
            txtDate.Text = starttime.ToString("yyyy-MM-dd HH:mm:ss");
            //return true;
        }
        else
        {
            txtDate.Text = System.DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
            txtDate1.Text = System.DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
        }
    }

    protected void CheckBoxRealTimeUpdate_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBoxRealTimeUpdate.Checked)
        {
            //Disable DailySummary,Destination, Dates& Months

            CheckBoxDailySummary.Checked = false;
            CheckBoxDailySummary.Enabled = false;

            CheckBoxShowByDestination.Checked = false;
            //CheckBoxShowByDestination.Enabled = false;

            TextBoxYear.Enabled = false;
            DropDownListMonth.Enabled = false;
            txtDate.Enabled = false;

            TextBoxYear1.Enabled = false;
            DropDownListMonth1.Enabled = false;
            txtDate1.Enabled = false;

            //Enable Timers,Duration,country
            CheckBoxShowByCountry.Checked = true;
            TextBoxDuration.Enabled = true;
            //TextBoxDuration.Text = "30";
            _timerflag = true;



            //dateInitialize
        }
        else
        {
            //Enable DailySummary,Destination, Dates& Months

            CheckBoxDailySummary.Checked = true;
            CheckBoxDailySummary.Enabled = true;

            CheckBoxShowByDestination.Checked = true;
            CheckBoxShowByDestination.Enabled = true;

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
            _timerflag = false;
        }
        CheckBoxShowByCountry_CheckedChanged(sender, e);
        DateInitialize();
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
        
        if (CheckBoxShowByCountry.Checked == true && (CheckBoxShowByDestination.Checked == false))
        {
            Dictionary<string, countrycode> dicKpiCountry = null;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (Session["dicKpiCountry"] != null)
                {
                    dicKpiCountry = (Dictionary<string, countrycode>)Session["dicKpiCountry"];
                }
                else
                {
                    return;
                }
                if (dicKpiCountry == null) return;
                //Label lblCountry = (Label)e.Row.FindControl("Label3");
                string thisCountryName = DataBinder.Eval(e.Row.DataItem, "Country").ToString();
                
                string thisCountryCode = "";
                int posBracket = thisCountryName.IndexOf("(");
                int posBracketEnd = thisCountryName.IndexOf(")");
                if (posBracket > -1 && posBracketEnd > -1)
                {
                    thisCountryCode = thisCountryName.Substring(posBracket + 1, posBracketEnd-posBracket-1);
                }
                else
                {
                    return;
                }
                if (thisCountryCode == "") return;
                
                Single thisAsr = 0;
                Single thisAcd = 0;
                Single thisCcr = 0;
                Single thisPdd = 0;
                Single thisCcRbyCc = 0;
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ASR").ToString(), out thisAsr);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ACD").ToString(), out thisAcd);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCR").ToString(), out thisCcr);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "PDD").ToString(), out thisPdd);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCRByCC").ToString(), out thisCcRbyCc);
                countrycode thisCc = null;
                dicKpiCountry.TryGetValue(thisCountryCode, out thisCc);
                if (thisCc != null)
                {
                    Color redColor = ColorTranslator.FromHtml("#FF0000");
                    //ASR
                    Single refAsr = 0;
                    if (Convert.ToSingle(thisCc.refasr) > 0)
                    {
                        refAsr = Convert.ToSingle(thisCc.refasr);
                    }
                    if ((thisAsr < refAsr) || (thisAsr == 0))
                    {
                        e.Row.Cells[13].ForeColor = Color.White;
                        e.Row.Cells[13].BackColor = redColor;
                        e.Row.Cells[13].Font.Bold = true;
                    }

                    //fas detection
                    double refAsrFas = refAsr+refAsr*.5;//fas threshold= 30% of ref asr by default
                    double tempDbl = 0;
                    double.TryParse(thisCc.refasrfas.ToString(), out tempDbl);
                    if (tempDbl > 0) refAsrFas = tempDbl;
                    
                    if (thisAsr > refAsrFas && refAsrFas>0)
                    {
                        e.Row.Cells[13].ForeColor = Color.White;
                        e.Row.Cells[13].BackColor = Color.Blue;
                        e.Row.Cells[13].Font.Bold = true;
                    }

                    //ACD
                    Single refAcd = 0;
                    if (Convert.ToSingle(thisCc.refacd) > 0)
                    {
                        refAcd = Convert.ToSingle(thisCc.refacd);
                    }
                    if (thisAcd < refAcd)
                    {
                        //e.Row.Cells[12].ForeColor = RedColor;
                        e.Row.Cells[14].ForeColor = Color.White;
                        e.Row.Cells[14].BackColor = redColor;
                        e.Row.Cells[14].Font.Bold = true;
                    }

                    //PDD
                    Single refPdd = 0;
                    if (Convert.ToSingle(thisCc.refpdd) > 0)
                    {
                        refPdd = Convert.ToSingle(thisCc.refpdd);
                    }
                    if (thisPdd > refPdd)
                    {
                        //e.Row.Cells[13].ForeColor = RedColor;
                        e.Row.Cells[15].ForeColor = Color.White;
                        e.Row.Cells[15].BackColor = redColor;
                        e.Row.Cells[15].Font.Bold = true;
                    }

                    //CCR
                    Single refCcr = 0;
                    if (Convert.ToSingle(thisCc.refccr) > 0)
                    {
                        refCcr = Convert.ToSingle(thisCc.refccr);
                    }
                    if (thisCcr < refCcr)
                    {
                        //e.Row.Cells[14].ForeColor = RedColor;
                        e.Row.Cells[16].ForeColor = Color.White;
                        e.Row.Cells[16].BackColor = redColor;
                        e.Row.Cells[16].Font.Bold = true;
                    }

                    //CCRByCauseCode
                    Single refCcrCc = 0;
                    if (Convert.ToSingle(thisCc.refccrbycc) > 0)
                    {
                        refCcrCc = Convert.ToSingle(thisCc.refccrbycc);
                    }
                    if (thisCcRbyCc < refCcrCc)
                    {
                        //e.Row.Cells[16].ForeColor = RedColor;
                        e.Row.Cells[18].ForeColor = Color.White;
                        e.Row.Cells[18].BackColor = redColor;
                        e.Row.Cells[18].Font.Bold = true;
                    }
                }
            }



        }//if checkbox Country

        if (CheckBoxShowByDestination.Checked == true) //KPI by Destination
        {   
            Dictionary<string, xyzprefix> dicKpiDest = null;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (Session["dicKpiDest"] != null)
                {
                    dicKpiDest = (Dictionary<string, xyzprefix>)Session["dicKpiDest"];
                }
                else
                {
                    return;
                }
                if (dicKpiDest == null) return;
                //Label lblCountry = (Label)e.Row.FindControl("Label3");
                string thisDestName = DataBinder.Eval(e.Row.DataItem, "Destination").ToString();

                string thisDestCode = "";
                int posBracket = thisDestName.IndexOf("(");
                if (posBracket > -1)
                {
                    thisDestCode = thisDestName.Substring(0, posBracket - 1);
                }
                else
                {
                    return;
                }
                if (thisDestCode == "") return;

                Single thisAsr = 0;
                Single thisAcd = 0;
                Single thisCcr = 0;
                Single thisPdd = 0;
                Single thisCcRbyCc = 0;
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ASR").ToString(), out thisAsr);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "ACD").ToString(), out thisAcd);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCR").ToString(), out thisCcr);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "PDD").ToString(), out thisPdd);
                Single.TryParse(DataBinder.Eval(e.Row.DataItem, "CCRByCC").ToString(), out thisCcRbyCc);
                xyzprefix thisDc = null;
                dicKpiDest.TryGetValue(thisDestCode, out thisDc);
                if (thisDc != null)
                {
                    Color redColor = ColorTranslator.FromHtml("#FF0000");
                    //ASR
                    Single refAsr = 0;
                    if (Convert.ToSingle(thisDc.refasr) > 0)
                    {
                        refAsr = Convert.ToSingle(thisDc.refasr);
                    }
                    if ((thisAsr < refAsr) || (thisAsr == 0))
                    {
                        //e.Row.Cells[12].ForeColor = RedColor;
                        e.Row.Cells[13].ForeColor = Color.White;
                        e.Row.Cells[13].BackColor = redColor;
                        e.Row.Cells[13].Font.Bold = true;
                    }

                    //fas detection
                    double tempDbl = 0;
                    double refAsrFas = refAsr + refAsr * .5;//fas threshold= 30% of ref asr by default
                    double.TryParse(thisDc.refasrfas.ToString(), out tempDbl);
                    if (tempDbl > 0) refAsrFas = tempDbl;

                    if (thisAsr > refAsrFas && refAsrFas>0)
                    {
                        e.Row.Cells[13].ForeColor = Color.White;
                        e.Row.Cells[13].BackColor = Color.Blue;
                        e.Row.Cells[13].Font.Bold = true;
                    }

                    //ACD
                    Single refAcd = 0;
                    if (Convert.ToSingle(thisDc.refacd) > 0)
                    {
                        refAcd = Convert.ToSingle(thisDc.refacd);
                    }
                    if (thisAcd < refAcd)
                    {
                        //e.Row.Cells[13].ForeColor = RedColor;
                        e.Row.Cells[14].ForeColor = Color.White;
                        e.Row.Cells[14].BackColor = redColor;
                        e.Row.Cells[14].Font.Bold = true; 
                    }

                    //PDD
                    Single refPdd = 0;
                    if (Convert.ToSingle(thisDc.refpdd) > 0)
                    {
                        refPdd = Convert.ToSingle(thisDc.refpdd);
                    }
                    if (thisPdd > refPdd)
                    {
                        e.Row.Cells[15].ForeColor = Color.White;
                        e.Row.Cells[15].BackColor = redColor;
                        e.Row.Cells[15].Font.Bold = true ;
                    }

                    //CCR
                    Single refCcr = 0;
                    if (Convert.ToSingle(thisDc.refccr) > 0)
                    {
                        refCcr = Convert.ToSingle(thisDc.refccr);
                    }
                    if (thisCcr < refCcr)
                    {
                        e.Row.Cells[16].ForeColor = Color.White;
                        e.Row.Cells[16].BackColor = redColor;
                        e.Row.Cells[16].Font.Bold = true ;
                    }

                    //CCRByCauseCode
                    Single refCcrCc = 0;
                    if (Convert.ToSingle(thisDc.refccrbycc) > 0)
                    {
                        refCcrCc = Convert.ToSingle(thisDc.refccrbycc);
                    }
                    if (thisCcRbyCc < refCcrCc)
                    {
                        e.Row.Cells[18].ForeColor = Color.White;
                        e.Row.Cells[18].BackColor = redColor;
                        e.Row.Cells[18].Font.Bold = true;
                    }
                }
            }



        }//if checkbox Destination

        //0 ASR highlighting
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            double asr = 1;
            double.TryParse(e.Row.Cells[11].Text, out asr);
            Color redColor2 = ColorTranslator.FromHtml("#FF0000");
            if (asr <= 0)
            {
                e.Row.Cells[13].ForeColor = Color.White;
                e.Row.Cells[13].BackColor = redColor2;
                e.Row.Cells[13].Font.Bold = true;
            }
        }

    }
    
}
