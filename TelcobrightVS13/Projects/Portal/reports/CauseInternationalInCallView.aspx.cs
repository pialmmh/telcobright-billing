using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using ExportToExcel;
using MediationModel;

public partial class DefaultCIntlInCallView1 : System.Web.UI.Page
{
    string _startdate = "";
    string _enddate = "";
    string _country = "";
    string _destination = "";
    string _ans = "";
    string _ansid = "";
    string _icx = "";
    string _icxid = "";
    string _partner = "";
    string _partnerid = "";
    string _callsstatus = "";
    string _causecode="";
    string _causecodecount = "";
    string _sqlquerystring = "";

    int _startindex = 0;
    //int endindex = startindex + 99;
    DataTable _dtpartner = null;
    DataTable _dtTable = null;
    
    //GridView Implement var
    int _totalNumRows = 0;
    int _pageRowIndex = 0;

    int _maxNUmRows = 100;//number of rows per grid
    int _totalNumberOfRows = 0;
    int _maxPageVisible = 10;//maximum number of pages visible in grid footer
    int _totalpage = 0; int _numberOfSlot = 0;

    string _placeholderString = "";
    int _totalNumRowsTemp = 0;//TotalNumRows;
    int _gridActiveIndexTemp = 0;//index;

    

    protected void Page_Load(object sender, EventArgs e)
    {
       
        //ClientScript.RegisterStartupScript(GetType(), "script", "document.form[0].target='_blank';", true);
        /*ContentPlaceHolder content = PreviousPage.Master.FindControl("mainform") as ContentPlaceHolder;
        System.Web.UI.HtmlControls.HtmlForm frmctrl = PreviousPage.Form;
        frmctrl.Target = "_blank";
        */
        if (this.ViewState["Partner"] == null)
        {
            this._dtpartner=GetPartner();
        }
        else
        {
            this._dtpartner = (DataTable) this.ViewState["Partner"];
        }
        
        string qString = this.Server.UrlDecode(this.Request.Url.Query.Split('?')[1]).Replace("&amp;","~!@").Replace("&","^").Replace("~!@","&");
        Dictionary<string, string> dicQueryString = new Dictionary<string, string>();
        
        foreach (string str in qString.Split('^'))
        {
            if (str.Contains('=') == false) continue;
            string[] thisKeyVal = str.Split('=');
            dicQueryString.Add(thisKeyVal[0].ToString(),((thisKeyVal[1].ToString()=="null")?"":thisKeyVal[1].ToString()));
        }
        

        DataInit(dicQueryString);
        NametoIdMapping();
        /*
        ArrayList tempgridval=new ArrayList();
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csgridcountry"]);//=row.Cells[1].Text;
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csgriddestination"]);// = row.Cells[2].Text;
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csgridans"]);// = row.Cells[3].Text;
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csgridicx"]);// = row.Cells[4].Text;
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csgridpartner"]);// = row.Cells[5].Text;
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csgrIdCallsstatus"]);// = row.Cells[6].Text;
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csgridcausecodecount"]);// = row.Cells[8].Text;
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csstartdate"]);// = txtDate.Text;
        tempgridval.Add(Session["CauseInternationalInCallView.aspx.csenddate"]);// = textDate1.Text;
        */
        this._sqlquerystring = Querybuilder();
        //handle export, post back triggered by export button
        if (this.IsPostBack == false)
        {
            this.ViewState["squery"] = this._sqlquerystring;
        }
        
        
        //lbl1.Text = sqlstring;
        this.lbl1.Text = "Call View#  "+((this._country == "") ? "" : "Country:" + this._country) + ((this._destination == "") ? "" : " <br /> Destination:" + this._destination) + ((this._ans == "") ? "" : " ANS:" + this._ans) + ((this._icx == "") ? "" : " ICX:" + this._icx) + ((this._partner == "") ? "" : " Partner:" + this._partner);

        this.ViewState["totalnumrows"] = this._totalNumRows;
        this.ViewState["gridactiveindex"] = this._gridActiveIndexTemp;

        #region optional/Extra/Rough
        //----Server.Response("Sample.cs)----method/////
        //////////////////////////////////////
        /*
        if (this.Page.PreviousPage != null)
        {
             
            ContentPlaceHolder content = PreviousPage.Master.FindControl("MainContent") as ContentPlaceHolder;
            System.Web.UI.HtmlControls.HtmlForm frmctrl = PreviousPage.Form;
            frmctrl.Target = "_self";
            if (content != null)
            {
                if (content.FindControl("txtDate") != null)
                {
                    TextBox startdt = (TextBox)content.FindControl("txtDate");
                    startdate = dateConvert(startdt.Text);
                    //Response.Write("Start Date: " + startdate);
                }
                else
                {
                }
                if (content.FindControl("txtDate1") != null)
                {
                    TextBox enddt = (TextBox)content.FindControl("txtDate1");
                    enddate = dateConvert(enddt.Text);
                    //Response.Write("End Date: " + enddate);
                }
                else
                {
                }
                if (content.FindControl("GridView1") != null)
                {
                    GridView GridView11 = (GridView)this.Page.PreviousPage.FindControl("GridView1");
                    GridView11=(GridView)content.FindControl("GridView1");
                }
            }
            GridView GridView1 = (GridView)this.Page.PreviousPage.Master.FindControl("GridView1");
            GridView1 = (GridView)content.FindControl("GridView1");

            GridViewRow headerRow = GridView1.HeaderRow;
            int headercount = headerRow.Cells.Count;
            System.Collections.ArrayList headarr = new System.Collections.ArrayList();
            for (int a = 0; a < headercount; a++)
            {
                headarr.Add(headerRow.Cells[a].Text);
            }


            GridViewRow selectedRow = GridView1.SelectedRow;
            int count = selectedRow.Cells.Count;
            System.Collections.ArrayList arr = new System.Collections.ArrayList();
            for (int i = 0; i < count; i++)
            {
                arr.Add(selectedRow.Cells[i].Text);
            }
            for (int z = 0; z < headercount; z++)
            {
               // Response.Write(headerRow.Cells[z].Text + ": " + selectedRow.Cells[z].Text + "<br />");

            }
            //Response.Write(+": " + selectedRow.Cells[1].Text + "<br />");
            //Response.Write("PostalCode: " + selectedRow.Cells[3].Text);
        }
        */
        #endregion

        //DataTable resultdt = GetDataSource(startindex,100, sqlstring);
        GridDataBinding2();
    }
    private DataTable GetPartner()
    {
        string strquery="Select idPartner,PartnerName from partner";
        DataTable dt = GetDataSourceAll(strquery);
        this.ViewState["Partner"] = dt;
        return dt;
    }

    private bool DataInit(Dictionary<string,string> dicParameters )
    {
        bool result = true;
        try
        {   
            dicParameters.TryGetValue("startdate", out this._startdate);
            dicParameters.TryGetValue("enddate", out this._enddate);
            dicParameters.TryGetValue("gridcountry", out this._country);
            dicParameters.TryGetValue("griddestination", out this._destination);
            dicParameters.TryGetValue("gridans", out this._ans);
            dicParameters.TryGetValue("gridicx", out this._icx);
            dicParameters.TryGetValue("gridpartner", out this._partner);
            dicParameters.TryGetValue("grIdCallsstatus", out this._callsstatus);
            dicParameters.TryGetValue("gridcause", out this._causecode);
            dicParameters.TryGetValue("gridcausecodecount", out this._causecodecount);

        }
        catch (Exception e)
        {
            result = false;
        }
        finally
        {
        }
        return result;
    }


    private bool DataInitOld()
    {
        bool result = true;
        try
        {
            if (this.Session["CauseInternationalInCallView.aspx.csstartdate"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csstartdate"].ToString() != "")
                {
                    this._startdate = DateConvert(this.Session["CauseInternationalInCallView.aspx.csstartdate"].ToString());
                    
                }
            }
            if (this.Session["CauseInternationalInCallView.aspx.csenddate"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csenddate"].ToString() != "")
                {
                    this._enddate = DateConvert(this.Session["CauseInternationalInCallView.aspx.csenddate"].ToString());
                    if (this._enddate.Length == 10)
                    {
                        this._enddate +=  " 23:59:59";
                    }
                }
            }
            if (this.Session["CauseInternationalInCallView.aspx.csgridcountry"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csgridcountry"].ToString() != "")
                    this._country = this.Session["CauseInternationalInCallView.aspx.csgridcountry"].ToString();
            }
            if (this.Session["CauseInternationalInCallView.aspx.csgriddestination"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csgriddestination"].ToString() != "")
                    this._destination = this.Session["CauseInternationalInCallView.aspx.csgriddestination"].ToString();
            }
            if (this.Session["CauseInternationalInCallView.aspx.csgridans"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csgridans"].ToString() != "")
                    this._ans = this.Session["CauseInternationalInCallView.aspx.csgridans"].ToString();
            }
            if (this.Session["CauseInternationalInCallView.aspx.csgridicx"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csgridicx"].ToString() != "")
                    this._icx = this.Session["CauseInternationalInCallView.aspx.csgridicx"].ToString();
            }
            if (this.Session["CauseInternationalInCallView.aspx.csgridpartner"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csgridpartner"].ToString() != "")
                    this._partner = this.Session["CauseInternationalInCallView.aspx.csgridpartner"].ToString();
            }
            if (this.Session["CauseInternationalInCallView.aspx.csgrIdCallsstatus"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csgrIdCallsstatus"].ToString() != "")
                    this._callsstatus = this.Session["CauseInternationalInCallView.aspx.csgrIdCallsstatus"].ToString();
            }//gridcause
            if (this.Session["CauseInternationalInCallView.aspx.csgridcause"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csgridcause"].ToString() != "")
                    this._causecode = this.Session["CauseInternationalInCallView.aspx.csgridcause"].ToString();
            }
            if (this.Session["CauseInternationalInCallView.aspx.csgridcausecodecount"] != null)
            {
                if (this.Session["CauseInternationalInCallView.aspx.csgridcausecodecount"].ToString() != "")
                    this._causecodecount = this.Session["CauseInternationalInCallView.aspx.csgridcausecodecount"].ToString();
            }
            
        }
        catch (Exception e)
        {
            result = false;
        }
        finally
        {
        }
        return result;
    }
    private void NametoIdMapping()
    {
        int numOfRows= this._dtpartner.Rows.Count;
        for (int i = 0; i < numOfRows; i++)
        {
            if (this._dtpartner.Rows[i][1].ToString() == this._ans)
            {
                this._ansid = this._dtpartner.Rows[i][0].ToString();
            }
            else if (this._dtpartner.Rows[i][1].ToString() == this._icx)
            {
                this._icxid = this._dtpartner.Rows[i][0].ToString();
            }
            else if (this._dtpartner.Rows[i][1].ToString() == this._partner)
            {
                this._partnerid = this._dtpartner.Rows[i][0].ToString();
            }
        }
    }
    private string Querybuilder()
    {
        
        //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
        //from Partner,Find causecode fieldname later...
        string causeCodeFieldName = "";
        string thisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

        MySqlConnection connection = new MySqlConnection(thisConectionString);
        string database = connection.Database.ToString();

        using (PartnerEntities context = new PartnerEntities())
        {
            int ccrCauseCodeFldNo = Convert.ToInt32( (from c in context.telcobrightpartners
                                                where c.databasename == database
                                                select c).First().nes.First().CcrCauseCodeField);
            causeCodeFieldName = context.cdrfieldlists.Where(c => c.fieldnumber ==ccrCauseCodeFldNo).First().FieldName;
        }


        string sqlstring = "";
        DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
        DateTime denddate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
        DateTime comparedate = dstartdate;
        
        if(this._startdate.Length==10)
        {
            DateTime.TryParseExact(this._startdate,"dd/MM/yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None, out dstartdate);
        }
        else if(this._startdate.Length>10)
        {
            DateTime.TryParseExact(this._startdate,"dd/MM/yyyy HH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None, out dstartdate);
        }
        
        if(this._enddate.Length==10)
        {
            DateTime.TryParseExact(this._enddate+" 23:59:59","dd/MM/yyyy HH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None, out denddate);
        }
        else if(this._enddate.Length>10)
        {
            DateTime.TryParseExact(this._enddate,"dd/MM/yyyy HH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None, out denddate);
        }

        if (dstartdate > comparedate && denddate > comparedate)
        {
            this._startdate = dstartdate.ToString("yyyy-MM-dd HH:mm:ss");
            this._enddate = denddate.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            this.lbl1.Text = "Invalid Date!";
            return "";
        }
        sqlstring = "Select OriginatingCalledNumber,OriginatingCallingNumber,TerminatingCalledNumber,TerminatingCallingNumber,IncomingRoute,OutgoingRoute,StartTime,ConnectTime,EndTime,DurationSec as ActualDuration,RoundedDuration," + causeCodeFieldName + " as Cause from cdrloaded where ServiceGroup=4"; //,CustomerID as ICX,SupplierID as Partner,AnsIDOrig as ANS
        sqlstring += " and starttime>='"+ this._startdate+"' and starttime<='"+ this._enddate+"'";
        if (this._country!="")
        {
            string resultString = System.Text.RegularExpressions.Regex.Match(this._country, @"\d+").Value;
            int start = this._country.IndexOf('(');
            int end = this._country.IndexOf(')');
            int substringlength=end-start-1;
            string resultstring1 = this._country.Substring(start+1, substringlength);
            sqlstring += " and CountryCode='" + resultString + "'";
        }
        if (this._destination!="")
        {
            string resultString = System.Text.RegularExpressions.Regex.Match(this._destination, @"\d+").Value;
            //int start = destination.IndexOf('(');
            int end = this._destination.IndexOf('(');
            int substringlength = end - 1;
            string resultstring1 = this._destination.Substring(0, substringlength).Trim();
            sqlstring += " and MatchedPrefixY='" + resultString + "'";
        }
        if (this._ans!="")
        {            
            sqlstring += " and AnsIdTerm='"+ this._ansid+"'";
        }
        if (this._icx!="")
        {
            sqlstring += " and SupplierID='"+ this._icxid+"'";
        }
        if (this._partner!="")
        {
            sqlstring += " and CustomerID='"+ this._partnerid+"'";
        }
        if (this._causecode != "")
        {
            sqlstring += " and " + causeCodeFieldName + "='" + this._causecode + "'";
        }
        if (this._callsstatus != "")
        {
            if(this._callsstatus=="failed")
            {
                 sqlstring += " and chargingStatus<>1";
            }
            else
            {
                sqlstring += " and chargingStatus=1";
            }
        }
        if (this._causecodecount != "")
        {
            this._totalNumRows= Convert.ToInt32(this._causecodecount);
            this.ViewState["totalnumrows"] = this._totalNumRows;
        }
        // sqlstring += " Limit 0,1000;";

        //set propercausecode field by switch
                    
        //remove / change these codes after passing switchid to this page
        string partnerConStr = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
        int posDatabase = partnerConStr.IndexOf("database");
        //make sure to keep databasename at the last of the connection string
        string dbName = partnerConStr.Substring(posDatabase + 9, partnerConStr.Length - posDatabase - 9);
        //find TB customerid
        string switchCause = "";
        using (PartnerEntities contextmed = new PartnerEntities())
        {
            telcobrightpartner thisCustomer = contextmed.telcobrightpartners.Where(c => c.databasename == dbName).First();
            ne sw=contextmed.nes.Where(c=>c.idCustomer==thisCustomer.idCustomer).First();
            switchCause+= " " + contextmed.cdrfieldlists.Where(c=>c.fieldnumber==sw.CcrCauseCodeField).First().FieldName + " and switchid="+ sw.idSwitch + " " ;
        }

        return sqlstring.Replace("from cdrloaded", " ,cc.description  from cdrloaded c left join causecode cc on  c.switchid=cc.idswitch and c."+ causeCodeFieldName +"=cc.cc ")
            .Replace("StartTime","date_format(StartTime,'%Y-%m-%d %T') as StartTime")
            .Replace("ConnectTime", "date_format(ConnectTime,'%Y-%m-%d %T') as ConnectTime")
            .Replace("EndTime", "date_format(EndTime,'%Y-%m-%d %T') as EndTime");
    }
    private string DateConvert(string datedata)
    {
        DateTime dtTime; String strDate = "";int flag=0;
        if (DateTime.TryParseExact(datedata, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtTime))
        {
            //Console.WriteLine(dtTime);
            strDate = dtTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else if (DateTime.TryParseExact(datedata, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtTime))
        {
            strDate = dtTime.ToString("yyyy-MM-dd");
            
        }
        DateTime cnvDate = dtTime;//Convert.ToDateTime(datedata);

        return strDate;
    }
    private DataTable GetDataSourceAll(string queryString)
    {
        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            connection.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
    private DataTable GetDataSource(int startRowIndex, int maximumRows, string queryString)
    {
        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            connection.Open();

            // string strSelectCmd = "SELECT * FROM CDRListed order by fileserialnumber desc ";
            string strSelectCmd = "Call CDRLGetRows1(@RowIndex,@MaxRows,@QueryString)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;

            //cmd.CommandText = "SELECT * FROM CDRListed";
            cmd.CommandText = strSelectCmd;

            cmd.Parameters.AddWithValue("RowIndex", startRowIndex);
            cmd.Parameters.AddWithValue("MaxRows", maximumRows);
            cmd.Parameters.AddWithValue("QueryString", queryString);
            //string strSelectCmd0 = "SELECT * FROM CDRReceived order by fileserialnumber desc ";
            //MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            // MySqlDataAdapter da0 = new MySqlDataAdapter(strSelectCmd0, connection);
            //conn.Open();
            //da.Fill(dsPerson, "Person"); 

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
    private void GridDataBinding2()
    {
        bool checknum1 = int.TryParse(this.ViewState["totalnumrows"].ToString(), out this._totalNumRows);
        if (!checknum1)
        {
            this._totalNumRows = Convert.ToInt32(this._causecodecount);//GetCount("banglatel.re_rate");
            this.ViewState["totalnumrows"] = this._totalNumRows;
        }

        int.TryParse(this.ViewState["gridactiveindex"].ToString(), out this._gridActiveIndexTemp);
        this._pageRowIndex = this._gridActiveIndexTemp;

        //String startdate = dateConvert(txtDate.Text);
        //String enddate = dateConvert(txtDate1.Text);
        //if (CheckBoxAutoReport.Checked)
        //{
        //   // DateTime tmpdt = DateTime.Now;
        //   // DateTime tmpdt1 = tmpdt.AddDays(1);
        //    startdate = DateTime.Now.ToString("yyyy-MM-dd");
        //    enddate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        //}ero

        //if (dtTable1 == null)
        {
            //string Query = "SELECT * FROM CDRReceived order by fileserialnumber desc "; idCDRDecoded,FileName,FileSerialNumber,idSwitch,DecodeTimeServer1,NoOfRecordsSrv1,StartSequenceNumberSrv1,EndSequenceNumberSrv1,LoadingMarked
            //string Query = "SELECT idCDRDecoded as id,FileName,FileSerialNumber as FileSerial,idSwitch,DecodeTimeServer1 as DecodedTime,NoOfRecordsSrv1 as `Total Record`,StartSequenceNumberSrv1 as StartSeq,EndSequenceNumberSrv1 as EndSeq,LoadingMarked FROM CDRDecoded order by fileserialnumber desc ";
            string query = "select * from re_rate order by jobID desc";
            query = this._sqlquerystring;
            this._dtTable = GetDataSource((this._pageRowIndex * this._maxNUmRows), this._maxNUmRows, query);
        }
        if (this._dtTable.Rows.Count < (this._maxNUmRows + 1))
        {
            DataRow dtExtra = this._dtTable.NewRow();
            this._dtTable.Rows.Add(dtExtra);

        }
        // int cccnnntt = dtTable2.Rows.Count;
        this.gridView.DataSource = this._dtTable;// dtSet;
        this.gridView.AllowPaging = true;
        if (this._dtTable.Rows.Count > 1)
            this.gridView.PageSize = this._dtTable.Rows.Count - 1;// maxNUmRows;
        else
            this.gridView.PageSize = 1;
        // if(dtTable2.Rows.count==maxNUmRows+1)
        this.gridView.PageIndex = 0;// pageRowIndex;
        this.gridView.DataBind();
        //init pager control
        this._placeholderString = "placeholder";
        this._totalNumRowsTemp = this._totalNumRows;
        //_gridActiveIndexTemp = gridactiveindex;
        SetPaging1(this.gridView);

    }
    private void PagingPreProcess()
    {
        //D R PV TP SL
        //totalrow=getcount();maxnumrows;maxPageVisible;Totalpage;Slot==start,end,prev,next

        bool checknum1 = int.TryParse(this.ViewState["totalnumrows"].ToString(), out this._totalNumRows);
        if (!checknum1)
        {
            this._totalNumRows = Convert.ToInt32(this._causecodecount);//GetCount("banglatel.re_rate");
            this.ViewState["totalnumrows"] = this._totalNumRows;
        }


        int.TryParse(this.ViewState["gridactiveindex"].ToString(), out this._gridActiveIndexTemp);
        //pageRowIndex = gridactiveindex3;

        this._totalNumberOfRows = this._totalNumRows;//D
        int d = this._totalNumberOfRows;
        int r = this._maxNUmRows;
        int pv = this._maxPageVisible;

        int tpResult = d / r;
        if (d % r > 0)
        {
            this._totalpage = tpResult + 1;
        }
        else
        {
            this._totalpage = tpResult;
        }

        int slotResult = this._totalpage / this._maxPageVisible;
        if (this._totalpage % this._maxPageVisible > 0)
        {
            this._numberOfSlot = slotResult + 1;
        }
        else
        {
            this._numberOfSlot = slotResult;
        }



    }
    private void SetPaging1(GridView tempGrid)
    {
        //D R PV TP SL
        //totalrow=getcount();maxnumrows;maxPageVisible;Totalpage;Slot==start,end,prev,next
        //
        if (this._gridActiveIndexTemp > tempGrid.PageIndex)
        {
            tempGrid.PageIndex = this._gridActiveIndexTemp;
        }

        bool prevflag = false; bool nextflag = false;
        GridViewRow row1 = tempGrid.BottomPagerRow;
        // int numOfPage1 = _totalNumRowsTemp / maxNUmRows;
        int start = this._gridActiveIndexTemp + 1, slot = 1;
        /* if (_gridActiveIndexTemp >= maxNUmRows)
         {
             slot = (_gridActiveIndexTemp + 1) / maxNUmRows;
             start = maxNUmRows * slot + 1;
         }

         slot = _gridActiveIndexTemp / maxNUmRows;
         start = maxNUmRows * slot + 1;

         int end = start + maxNUmRows;

         if (end > numOfPage1)
             end = numOfPage1;*/

        ////////////
        PagingPreProcess();
        int pi = this._gridActiveIndexTemp + 1;


        int currentSlot = pi / this._maxPageVisible;
        if (pi % this._maxPageVisible > 0)
        {
            currentSlot++;
        }
        if (currentSlot > this._numberOfSlot)
        {
            currentSlot = this._numberOfSlot;
        }

        start = (currentSlot - 1) * this._maxPageVisible + 1;
        int end = start + this._maxPageVisible - 1;

        if (this._numberOfSlot > 1)
        {
            if (currentSlot == 1)
            {
                prevflag = false;
                nextflag = true;
                // start = 1; end = start + maxPageVisible-1;
            }
            else if (currentSlot == this._numberOfSlot)
            {
                prevflag = true;
                nextflag = false;
                // start = currentSlot;
                end = this._totalpage;
            }
            else
            {
                prevflag = true;
                nextflag = true;
            }
        }
        else
        {
            end = this._totalpage;
        }


        ///////////////
        if (row1 != null)
        {
            if (prevflag)
            {
                LinkButton btn = new LinkButton();
                btn.CommandName = "G1Page";
                btn.CommandArgument = (start - 1).ToString();
                btn.Text = "<..";
                btn.ToolTip = "Page " + (start - 1).ToString();

                PlaceHolder place = (PlaceHolder)row1.Cells[0].FindControl(this._placeholderString);
                place.Controls.Add(btn);

                Label lbl = new Label();
                lbl.Text = "| ";
                place.Controls.Add(lbl);
            }
            for (int i = start; i <= end; i++)
            {
                if (i <= end)
                {
                    LinkButton btn = new LinkButton();
                    btn.CommandName = "G1Page";
                    btn.CommandArgument = i.ToString();

                    if (i == this._gridActiveIndexTemp + 1)
                    {
                        btn.BackColor = System.Drawing.Color.Khaki;
                    }

                    btn.Text = i.ToString();
                    btn.ToolTip = "Page " + i.ToString();

                    PlaceHolder place = (PlaceHolder)row1.Cells[0].FindControl(this._placeholderString);
                    place.Controls.Add(btn);

                    Label lbl = new Label();
                    lbl.Text = "| ";
                    place.Controls.Add(lbl);

                    place.Visible = true;

                    /*  bool a = place.Visible;
                      if (!a)
                      {
                          place.Visible = true;
                      }*/
                }

                //if (i + 1 == numOfPage1)
                //  i = end + 1;
            }
            if (nextflag)
            {
                LinkButton btn = new LinkButton();
                btn.CommandName = "G1Page";
                btn.CommandArgument = (end + 1).ToString();

                btn.Text = "..>";
                btn.ToolTip = "Page " + (end + 1).ToString();

                PlaceHolder place = (PlaceHolder)row1.Cells[0].FindControl(this._placeholderString);
                place.Controls.Add(btn);

                // Label lbl = new Label();
                // lbl.Text = "| ";
                // place.Controls.Add(lbl);
            }
        }

    }
    protected void gridView_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "G1Page")
        {
            // Retrieve the row index stored in the 
            // CommandArgument property.
            int index = Convert.ToInt32(e.CommandArgument) - 1;

            if (sender == this.gridView)
            {
                //gridactiveindex = index;
                this._placeholderString = "placeholder";
                this._totalNumRowsTemp = this._totalNumRows;
                this._gridActiveIndexTemp = index;// gridactiveindex;
                this.ViewState["gridactiveindex"] = index;
            }
            else
            {
                // GridViewRow row = gridView1.Rows[index];
            }

            GridDataBinding2();
        }

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
        long records=-1;
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
        string sql = "";
        if (noOfRecords == -1)//all records
        {
            sql = (string) this.ViewState["squery"];
        }
        else//first N
        {
            sql = ((string) this.ViewState["squery"]).Replace(";","") + " limit 0," + this.TextBoxNoOfRecords.Text +";" ;
        }
        DataTable dt = GetDataSet(ConfigurationManager.ConnectionStrings["reader"].ConnectionString, sql).Tables[0];
        ExportToSpreadsheet(dt, "callview_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    public static void ExportToSpreadsheet(DataTable table, string name)
    {
        HttpContext context = HttpContext.Current;
        context.Response.Clear();
        CreateExcelFileAspNet.CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(table, "CauseCodeWiseCallsIntlIn_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + ".xlsx",context.Response );
    }

}