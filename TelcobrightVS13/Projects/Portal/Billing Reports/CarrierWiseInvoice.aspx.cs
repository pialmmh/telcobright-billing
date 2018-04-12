using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.UI.WebControls;

namespace PortalApp.reports.Accounts
{
    public partial class CarrierWiseInvoice : System.Web.UI.Page
    {
        string _enddatetime,_startdatetime;
        int _ho, _sgmt=30;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this.btnExporttoCSV.Visible = false;
                this.btnExporttoCSVGridview1.Visible = false;
                Load();
            }
        }

      

        // Load Egress Carrier Name
        private void Load()
        {
            MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

            using (MySqlCommand cmd = new MySqlCommand("select idpartner,PartnerName from partner where PartnerType=3"))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    con.Open();
                    this.dpdIsoName.DataSource = cmd.ExecuteReader();
                    this.dpdIsoName.DataTextField = "PartnerName";
                    this.dpdIsoName.DataValueField = "idpartner";
                    this.dpdIsoName.DataBind();
                    con.Close();
                }
            this.dpdIsoName.Items.Insert(0, new ListItem("All", "0"));
        }



        // BD Incoming Business Show on gridview
        private void CarrierWiseInvoiceGet()
        {            
            try
            {
                // if checkbox checked then show details report 
                if (this.chkdetailreport.Checked == true)
                {
                    if (this.dpdIsoName.SelectedItem.Text != "All" && this.txtEndDate.Text != "" && this.txtStartDate.Text != "")
                    {
                        this.GridView1.Visible = true;
                        this.GridView.Visible = false;
                        string customerId = this.dpdIsoName.SelectedValue;


                        // ******** Start GMT Calculation **********//
                        string input = this.dpdgmt.SelectedValue;
                        string gmtvalue = input.Substring(4);
                        // FOR Positive gmt value
                        if (gmtvalue == "0" || gmtvalue == "1" || gmtvalue == "2" || gmtvalue == "3" || gmtvalue == "4" || gmtvalue == "5" || gmtvalue == "6" || gmtvalue == "7" || gmtvalue == "8" || gmtvalue == "9" || gmtvalue == "10" || gmtvalue == "11" || gmtvalue == "12")
                        {
                            this._ho = Convert.ToInt32(gmtvalue);
                            this._sgmt = 6 - this._ho;
                        }
                        // For Negative gmt value
                        else if (gmtvalue == "-1" || gmtvalue == "-2" || gmtvalue == "-3" || gmtvalue == "-4" || gmtvalue == "-5" || gmtvalue == "-6" || gmtvalue == "-7" || gmtvalue == "-8" || gmtvalue == "-9" || gmtvalue == "-10" || gmtvalue == "-11" || gmtvalue == "-12")
                        {
                            this._ho = Convert.ToInt32(gmtvalue);
                            this._sgmt = 6 - this._ho;
                        }
                        // Calculated gmt is grater then zero or equal to zero
                        if (this._sgmt >= 0 && this._sgmt < 15)
                        {
                            // For GMT+
                            int mh = this._sgmt;
                            string time = "" + mh + ":00:00";
                            string startdate = this.txtStartDate.Text;
                            DateTime dt = Convert.ToDateTime(startdate);
                            // Decrease 1 day
                            string modifystartdate = dt.AddDays(0).ToString("yyyy-MM-dd");
                            this._startdatetime = modifystartdate + " " + time;

                            // FOR GMT-
                            int mhm;
                            if (this._sgmt == 0)
                            {
                              //  mhm = Sgmt;
                                mhm = 23;
                                string mtime = "" + mhm + ":59:59";
                                string enddate = this.txtEndDate.Text;
                                DateTime endd = Convert.ToDateTime(enddate);
                                //Increase 1 day
                                string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                                this._enddatetime = modifyenddate + " " + mtime;
                            }
                            else
                            {
                                mhm = this._sgmt - 1;
                                string mtime = "" + mhm + ":59:59";
                                string enddate = this.txtEndDate.Text;
                                DateTime endd = Convert.ToDateTime(enddate);
                                //Increase 1 day
                                string modifyenddate = endd.AddDays(1).ToString("yyyy-MM-dd");
                                this._enddatetime = modifyenddate + " " + mtime;
                            }                           
                        }
                        // Calculated gmt is Smaller then zero
                        else if (this._sgmt < 0)
                        {
                            // For GMT+
                            int mh = 24+ this._sgmt;
                            string time = "" + mh + ":00:00";
                            string startdate = this.txtStartDate.Text;
                            DateTime dt = Convert.ToDateTime(startdate);
                            // Decrease 1 day
                            string modifystartdate = dt.AddDays(-1).ToString("yyyy-MM-dd");
                            this._startdatetime = modifystartdate + " " + time;                           

                            // FOR GMT-
                            int mhm = 23+ this._sgmt;
                            string mtime = "" + mhm + ":59:59";
                            string enddate = this.txtEndDate.Text;
                            DateTime endd = Convert.ToDateTime(enddate);
                            //Increase 1 day
                            string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                            this._enddatetime = modifyenddate + " " + mtime;
                        }
                        else
                        {
                            string time = "00:00:00";
                            string startdate = this.txtStartDate.Text;
                            DateTime dt = Convert.ToDateTime(startdate);
                            // Decrease 1 day
                            string modifystartdate = dt.AddDays(0).ToString("yyyy-MM-dd");
                            this._startdatetime = modifystartdate + " " + time;


                            string mtime = "23:59:59";
                            string enddate = this.txtEndDate.Text;
                            DateTime endd = Convert.ToDateTime(enddate);
                            //Increase 1 day
                            string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                            this._enddatetime = modifyenddate + " " + mtime;

                        }
                        this.lblErrMsg.Text = this._startdatetime.ToString() + " " + this._enddatetime.ToString();
                        //******* End of GMT Calculation ********//


                        // Connection to mysql
                        MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());
                        con.Open();

                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "select DATE_FORMAT(calldate,'%d-%b-%y') as `Date`,DATE_FORMAT(AnswerTime, '%d-%m-%Y %H:%i:%s') as 'Call Start Time',DATE_FORMAT(EndTime, '%d-%m-%Y %H:%i:%s') as 'Call End Time', p.PartnerName as `Ingress Carrier`"
                                           + " ,OriginatingIP as `Inbound IP`, originatingCallingNumber as `A Number`, MediaIP4 as `B Number`, left(SUBSTRING_INDEX(MatchedPrefixCustomer,')',1),5) as `Dial Code`"
                                           + " ,substring(SUBSTRING_INDEX(MatchedPrefixCustomer,')',1),8) as 'Destination',round(duration,2) as `Duration (Min)`,customerrate as `Rate ($)`, round(IngressAmount,5) as `Amount ($)`"
                                           + " from"
                                           + " (select date(starttime) as CallDate, customerid, duration1/60 as duration, customerrate, customercost as IngressAmount,"
                                           + " matchedprefixcustomer,originatingCallingNumber, mediaip4 ,OriginatingIP, answertime, endtime"
                                           + " from cdrloaded"
                                           + " where customerid=" + customerId + ""
                                           + " and starttime>='" + this._startdatetime + "'"
                                           + " and starttime<='" + this._enddatetime + "'"
                                           + " and calldirection=4"
                                           + " and chargingstatus=1"
                                           + " and duration1>0"
                                           + " order by StartTime "
                                           + " ) x"
                                           + " left join partner p"
                                           + " on x.customerid=p.idpartner"
                                           + " order by AnswerTime";

                            MySqlCommand com = new MySqlCommand(query, con);
                            MySqlDataAdapter sda = new MySqlDataAdapter(com);

                            DataTable ds = new DataTable();
                            sda.Fill(ds);

                            this.GridView1.DataSource = ds;
                            this.GridView1.DataBind();
                        }
                        con.Close();
                        this.btnExporttoCSV.Visible = false;
                        this.btnExporttoCSVGridview1.Visible = true;
                    }
                }   
                    // If check box is unchecked then show Summary Report
                else
                {
                    this.GridView1.Visible = false;
                    this.GridView.Visible = true;

                    this.btnExporttoCSV.Visible = true;
                    this.btnExporttoCSVGridview1.Visible = false;

                    // Show all Carrier Summary Report
                if (this.dpdIsoName.SelectedItem.Text == "All" && this.txtEndDate.Text!="" && this.txtStartDate.Text!="")
                {
                    this.lblErrMsg.Text = "";
                    // GMT Calculation
                    string input = this.dpdgmt.SelectedValue;
                    string gmtvalue = input.Substring(4);
                    // for positive gmt
                    if (gmtvalue == "0" || gmtvalue == "1" || gmtvalue == "2" || gmtvalue == "3" || gmtvalue == "4" || gmtvalue == "5" || gmtvalue == "6" || gmtvalue == "7" || gmtvalue == "8" || gmtvalue == "9" || gmtvalue == "10" || gmtvalue == "11" || gmtvalue == "12")
                    {
                        this._ho = Convert.ToInt32(gmtvalue);
                        this._sgmt = 6 - this._ho;
                    }
                        // for negative gmt
                    else if (gmtvalue == "-1" || gmtvalue == "-2" || gmtvalue == "-3" || gmtvalue == "-4" || gmtvalue == "-5" || gmtvalue == "-6" || gmtvalue == "-7" || gmtvalue == "-8" || gmtvalue == "-9" || gmtvalue == "-10" || gmtvalue == "-11" || gmtvalue == "-12")
                    {
                        this._ho = Convert.ToInt32(gmtvalue);
                        this._sgmt = 6 - this._ho;
                    }
                    // calculated gmt is grater then 0 or equal zero
                    if (this._sgmt >= 0 && this._sgmt < 15)
                    {
                        // For GMT+
                        int mh = this._sgmt;
                        string time = "" + mh + ":00:00";
                        string startdate = this.txtStartDate.Text;
                        DateTime dt = Convert.ToDateTime(startdate);
                        // Decrease 1 day
                        string modifystartdate = dt.AddDays(0).ToString("yyyy-MM-dd");
                        this._startdatetime = modifystartdate + " " + time;

                        // FOR GMT-
                        int mhm;
                        if (this._sgmt == 0)
                        {
                            //  mhm = Sgmt;
                            mhm = 23;
                            string mtime = "" + mhm + ":59:59";
                            string enddate = this.txtEndDate.Text;
                            DateTime endd = Convert.ToDateTime(enddate);
                            //Increase 1 day
                            string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                            this._enddatetime = modifyenddate + " " + mtime;
                        }
                        else
                        {
                            mhm = this._sgmt - 1;
                            string mtime = "" + mhm + ":59:59";
                            string enddate = this.txtEndDate.Text;
                            DateTime endd = Convert.ToDateTime(enddate);
                            //Increase 1 day
                            string modifyenddate = endd.AddDays(1).ToString("yyyy-MM-dd");
                            this._enddatetime = modifyenddate + " " + mtime;
                        }
                    }
                    // Calculated gmt is Smaller then zero
                    else if (this._sgmt < 0)
                    {
                        // For GMT+
                        int mh = 24 + this._sgmt;
                        string time = "" + mh + ":00:00";
                        string startdate = this.txtStartDate.Text;
                        DateTime dt = Convert.ToDateTime(startdate);
                        // Decrease 1 day
                        string modifystartdate = dt.AddDays(-1).ToString("yyyy-MM-dd");
                        this._startdatetime = modifystartdate + " " + time;

                        // FOR GMT-
                        int mhm = 23 + this._sgmt;
                        string mtime = "" + mhm + ":59:59";
                        string enddate = this.txtEndDate.Text;
                        DateTime endd = Convert.ToDateTime(enddate);
                        //Increase 1 day
                        string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                        this._enddatetime = modifyenddate + " " + mtime;
                    }
                    else
                    {
                        string time = "00:00:00";
                        string startdate = this.txtStartDate.Text;
                        DateTime dt = Convert.ToDateTime(startdate);
                        // Decrease 1 day
                        string modifystartdate = dt.AddDays(0).ToString("yyyy-MM-dd");
                        this._startdatetime = modifystartdate + " " + time;


                        string mtime = "23:59:59";
                        string enddate = this.txtEndDate.Text;
                        DateTime endd = Convert.ToDateTime(enddate);
                        //Increase 1 day
                        string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                        this._enddatetime = modifyenddate + " " + mtime;

                    }
                    this.lblErrMsg.Text = this._startdatetime.ToString() + " " + this._enddatetime.ToString();

                    // Connection to mysql
                    MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());
                    con.Open();

                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        string query = "select DATE_FORMAT(calldate,'%d-%b-%y') as `Date`, p.PartnerName as `Ingress Carrier`,substring(SUBSTRING_INDEX(MatchedPrefixCustomer,')',1),8) as Destination,pd.Prefix as `Dial Code`,"
                                        + " TotalCalls as `Call Qty.`,ROUND(duration1,2) as `Duration (Min)`,customerrate as `Rate ($)`, round(IngressAmount,5) as `Amount ($)`"
                                        + " from"
                                        + " (select date(starttime) as CallDate,count(*) as TotalCalls,customerid,sum(duration1)/60 as duration1,customerrate,sum(customercost) as IngressAmount,"
                                        + " matchedprefixcustomer,StartTime,idcall"
                                        + " from cdrloaded"
                                        + " where starttime>='" + this._startdatetime + "'"
                                        + " and starttime<='" + this._enddatetime + "'"
                                        + " and calldirection=4"
                                        + " and chargingstatus=1"
                                        + " and duration1>0"
                                        + " group by date(starttime),customerid,customerrate,MatchedPrefixCustomer"
                                        + " ) x"
                                        + " left join partner p"
                                        + " on x.customerid=p.idpartner"
                                        + " join billinfo pd"
                                        + " on x.idcall=pd.idcall"
                                        + " and x.starttime=pd.starttime"
                                        + " and pd.servicefamily=1"
                                        + " order by calldate";

                        MySqlCommand com = new MySqlCommand(query, con);
                        MySqlDataAdapter sda = new MySqlDataAdapter(com);

                        DataTable ds = new DataTable();
                        sda.Fill(ds);

                        this.GridView.DataSource = ds;
                        this.GridView.DataBind();

                        //Calculate Sum and display in Footer Row                                        
                        double outtotalBtrcAmount = ds.AsEnumerable().Sum(row => row.Field<double>("Duration (Min)"));
                        double outtotalDclMargin = ds.AsEnumerable().Sum(row => row.Field<double>("Amount ($)"));
                        //  double OuttotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Outgoing Duration"));

                        this.GridView.FooterRow.Cells[5].Text = outtotalBtrcAmount.ToString("N2");
                        this.GridView.FooterRow.Cells[7].Text = outtotalDclMargin.ToString("N2");
                    }
                    con.Close();
                }
                // Show only specific carrier summary report
                else if (this.dpdIsoName.SelectedItem.Text != "All" && this.txtEndDate.Text != "" && this.txtStartDate.Text != "")        // Select Individual Carrier wise Invoice
                {
                    this.lblErrMsg.Text = "";

                    string customerId = this.dpdIsoName.SelectedValue;


                    // GMT Calculation
                    string input = this.dpdgmt.SelectedValue;
                    string gmtvalue = input.Substring(4);
                    // FOR Positive gmt value
                    if (gmtvalue == "0" || gmtvalue == "1" || gmtvalue == "2" || gmtvalue == "3" || gmtvalue == "4" || gmtvalue == "5" || gmtvalue == "6" || gmtvalue == "7" || gmtvalue == "8" || gmtvalue == "9" || gmtvalue == "10" || gmtvalue == "11" || gmtvalue == "12")
                    {
                        this._ho = Convert.ToInt32(gmtvalue);
                        this._sgmt = 6 - this._ho;
                    }
                        // For Negative gmt value
                    else if (gmtvalue == "-1" || gmtvalue == "-2" || gmtvalue == "-3" || gmtvalue == "-4" || gmtvalue == "-5" || gmtvalue == "-6" || gmtvalue == "-7" || gmtvalue == "-8" || gmtvalue == "-9" || gmtvalue == "-10" || gmtvalue == "-11" || gmtvalue == "-12")
                    {
                        this._ho = Convert.ToInt32(gmtvalue);
                        this._sgmt = 6 - this._ho;
                    }
                    // Calculated gmt is grater then zero or equal to zero
                    if (this._sgmt >= 0 && this._sgmt < 15)
                    {
                        // For GMT+
                        int mh = this._sgmt;
                        string time = "" + mh + ":00:00";
                        string startdate = this.txtStartDate.Text;
                        DateTime dt = Convert.ToDateTime(startdate);
                        // Decrease 1 day
                        string modifystartdate = dt.AddDays(0).ToString("yyyy-MM-dd");
                        this._startdatetime = modifystartdate + " " + time;

                        // FOR GMT-
                        int mhm;
                        if (this._sgmt == 0)
                        {
                            //  mhm = Sgmt;
                            mhm = 23;
                            string mtime = "" + mhm + ":59:59";
                            string enddate = this.txtEndDate.Text;
                            DateTime endd = Convert.ToDateTime(enddate);
                            //Increase 1 day
                            string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                            this._enddatetime = modifyenddate + " " + mtime;
                        }
                        else
                        {
                            mhm = this._sgmt - 1;
                            string mtime = "" + mhm + ":59:59";
                            string enddate = this.txtEndDate.Text;
                            DateTime endd = Convert.ToDateTime(enddate);
                            //Increase 1 day
                            string modifyenddate = endd.AddDays(1).ToString("yyyy-MM-dd");
                            this._enddatetime = modifyenddate + " " + mtime;
                        }
                    }
                    // Calculated gmt is Smaller then zero
                    else if (this._sgmt < 0)
                    {
                        // For GMT+
                        int mh = 24 + this._sgmt;
                        string time = "" + mh + ":00:00";
                        string startdate = this.txtStartDate.Text;
                        DateTime dt = Convert.ToDateTime(startdate);
                        // Decrease 1 day
                        string modifystartdate = dt.AddDays(-1).ToString("yyyy-MM-dd");
                        this._startdatetime = modifystartdate + " " + time;

                        // FOR GMT-
                        int mhm = 23 + this._sgmt;
                        string mtime = "" + mhm + ":59:59";
                        string enddate = this.txtEndDate.Text;
                        DateTime endd = Convert.ToDateTime(enddate);
                        //Increase 1 day
                        string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                        this._enddatetime = modifyenddate + " " + mtime;
                    }
                    else
                    {
                        string time = "00:00:00";
                        string startdate = this.txtStartDate.Text;
                        DateTime dt = Convert.ToDateTime(startdate);
                        // Decrease 1 day
                        string modifystartdate = dt.AddDays(0).ToString("yyyy-MM-dd");
                        this._startdatetime = modifystartdate + " " + time;


                        string mtime = "23:59:59";
                        string enddate = this.txtEndDate.Text;
                        DateTime endd = Convert.ToDateTime(enddate);
                        //Increase 1 day
                        string modifyenddate = endd.AddDays(0).ToString("yyyy-MM-dd");
                        this._enddatetime = modifyenddate + " " + mtime;

                    }
                    this.lblErrMsg.Text = this._startdatetime.ToString() + " " + this._enddatetime.ToString();
                    // ***** End of GMT Calculation ******* //


                    // Connection to mysql
                    MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());
                    con.Open();

                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        string query = "select DATE_FORMAT(calldate,'%d-%b-%y') as `Date`, p.PartnerName as `Ingress Carrier`,substring(SUBSTRING_INDEX(MatchedPrefixCustomer,')',1),8) as Destination,pd.Prefix as `Dial Code`,"
                                        + " TotalCalls as `Call Qty.`,ROUND(duration1,2) as `Duration (Min)`,customerrate as `Rate ($)`, round(IngressAmount,5) as `Amount ($)`"
                                        + " from"
                                        + " (select date(starttime) as CallDate,count(*) as TotalCalls,customerid,sum(duration1)/60 as duration1,customerrate,sum(customercost) as IngressAmount,"
                                        + " matchedprefixcustomer,StartTime,idcall"
                                        + " from cdr"
                                        + " where starttime>='" + this._startdatetime + "'"
                                        + " and starttime<='" + this._enddatetime + "'"
                                        + " and calldirection=4"
                                        + " and chargingstatus=1"
                                        + " and duration1>0"
                                        + " and customerid=" + customerId + " "
                                        + " group by date(starttime),customerid,customerrate,MatchedPrefixCustomer"
                                        + " ) x"
                                        + " left join partner p"
                                        + " on x.customerid=p.idpartner"
                                        + " join billinfo pd"
                                        + " on x.idcall=pd.idcall"
                                        + " and x.starttime=pd.starttime"
                                        + " and pd.servicefamily=1"
                                        + " order by calldate"; 

                        MySqlCommand com = new MySqlCommand(query, con);
                        MySqlDataAdapter sda = new MySqlDataAdapter(com);

                        DataTable ds = new DataTable();
                        sda.Fill(ds);

                        this.GridView.DataSource = ds;
                        this.GridView.DataBind();

                        //Calculate Sum and display in Footer Row                                        
                        double outtotalBtrcAmount = ds.AsEnumerable().Sum(row => row.Field<double>("Duration (Min)"));
                        double outtotalDclMargin = ds.AsEnumerable().Sum(row => row.Field<double>("Amount ($)"));
                        //  double OuttotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Outgoing Duration"));

                        this.GridView.FooterRow.Cells[5].Text = outtotalBtrcAmount.ToString("N2");
                        this.GridView.FooterRow.Cells[7].Text = outtotalDclMargin.ToString("N2");
                    }
                    con.Close();
                }
                else
                {
                    this.GridView.DataSource = null;
                    this.GridView.DataBind();
                    this.lblErrMsg.Text = "Wrong selection!";
                }               
              }
            }
            catch (Exception ex)
            {
                this.lblErrMsg.Text = ex.Message;
            }
        }

        // Show Action button
        protected void btnShowReport_Click(object sender, EventArgs e)
        { 
            CarrierWiseInvoiceGet();    
        }


        // Gridview Pagging
        protected void PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            CarrierWiseInvoiceGet();
        }

        // Gridview1 Pagging
        protected void PageIndexChangeGridview1(object sender, GridViewPageEventArgs e)
        {
            this.GridView1.PageIndex = e.NewPageIndex;
            CarrierWiseInvoiceGet();
        }


        // Export to CSV Gridview
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=CarrierWiseInvoice.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView.AllowPaging = false;
            CarrierWiseInvoiceGet();

            StringBuilder sBuilder = new StringBuilder();
            for (int index = 0; index < this.GridView.Columns.Count; index++)
            {
                sBuilder.Append(this.GridView.Columns[index].HeaderText + ',');
            }
            sBuilder.Append("\r\n");
            for (int i = 0; i < this.GridView.Rows.Count; i++)
            {
                for (int k = 0; k < this.GridView.HeaderRow.Cells.Count; k++)
                {
                    sBuilder.Append(this.GridView.Rows[i].Cells[k].Text.Replace(",", "") + ",");
                }
                sBuilder.Append("\r\n");
            }
            sBuilder.Append("\r\n");
            for (int index = 0; index < this.GridView.Columns.Count; index++)
            {
                sBuilder.Append(this.GridView.Columns[index].FooterText + ',');
            }
            this.Response.Output.Write(sBuilder.ToString());
            this.Response.Flush();
            this.Response.End();     
           
        }


        // Export to CSV Griedview1
        protected void btnExporttoCSVGridview1_Click(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=gvtocsv.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView1.AllowPaging = false;
            CarrierWiseInvoiceGet();

            StringBuilder sBuilder = new StringBuilder();
            for (int index = 0; index < this.GridView1.Columns.Count; index++)
            {
                sBuilder.Append(this.GridView1.Columns[index].HeaderText + ',');
            }
            sBuilder.Append("\r\n");
            for (int i = 0; i < this.GridView1.Rows.Count; i++)
            {
                for (int k = 0; k < this.GridView1.HeaderRow.Cells.Count; k++)
                {
                    sBuilder.Append(this.GridView1.Rows[i].Cells[k].Text.Replace(",", "") + ",");
                }
                sBuilder.Append("\r\n");
            }
            sBuilder.Append("\r\n");
            for (int index = 0; index < this.GridView1.Columns.Count; index++)
            {
                sBuilder.Append(this.GridView1.Columns[index].FooterText + ',');
            }
            this.Response.Output.Write(sBuilder.ToString());
            this.Response.Flush();
            this.Response.End();     
        }
    }
}