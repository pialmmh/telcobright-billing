using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.UI.WebControls;

namespace PortalApp.reports.Accounts
{
    public partial class EgressInvoiceClearence : System.Web.UI.Page
    {
        int _isoId;
        int _ho, _sgmt = 30;
        string _enddatetime, _startdatetime;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this.btnExportCSVDetails.Visible = false;
                this.btnExporttoCSV.Visible = false;
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

        // Gridview data show
        private void EgressInvoice()
        {
            try
            {
                this.lblErrMsg.Text = "";

                // Show Detail Report
                if (this.chkdetailreport.Checked == true)
                {
                    this.GridView.Visible = false;
                    this.GridView1.Visible = true;

                    this.btnExportCSVDetails.Visible = true;
                    this.btnExporttoCSV.Visible = false;
                    if (this.txtStartDate.Text != "" && this.txtEndDate.Text != "" && this.dpdIsoName.SelectedItem.Text != "All")
                    {
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
                        //******* End of GMT Calculation ********//


                        //Connection to mysql
                        MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                        con.Open();

                        using (MySqlCommand cmd = con.CreateCommand())
                        {

                            string query = "select DATE_FORMAT(calldate,'%d-%b-%y') as Date, DATE_FORMAT(AnswerTime, '%d-%m-%Y %H:%i:%s') as 'Call Start Time',DATE_FORMAT(EndTime, '%d-%m-%Y %H:%i:%s') as 'Call End Time', p.PartnerName as `Egress Carrier`"
                                           + " ,TerminatingIP as `Outbound IP`, TerminatingCallingNumber as `A Number`, MediaIP4 as `B Number`, SUBSTRING_INDEX(SUBSTRING_INDEX(MatchedPrefixSupplier,')',1),'(',1) as `Dial Code`,"
                                           + " ROUND(duration,2) as `Duration (Min)`, SupplierRate as `Rate ($)`, round(SupplierCost,4) as `Egress Amount ($)`"
                                           + " from"
                                           + " (select date(starttime) as callDate, answertime, endtime, SupplierID, duration2/60 as duration,"
                                           + " SupplierRate, SupplierCost, MatchedPrefixSupplier,TerminatingCallingNumber, mediaip4 ,TerminatingIP"
                                           + " from cdrloaded"
                                           + " where supplierid=" + customerId + ""
                                           + " and starttime>='"+ this._startdatetime+"'"
                                           + " and starttime<='"+ this._enddatetime+"'"
                                           + " and ServiceGroup=5"
                                           + " and chargingstatus=1"
                                           + " and duration2>0"
                                           + " order by starttime"
                                           + " ) x"
                                           + " left join partner p"
                                           + " on x.supplierid=p.idpartner"
                                           + " order by AnswerTime";

                            MySqlCommand com = new MySqlCommand(query, con);
                            MySqlDataAdapter sda = new MySqlDataAdapter(com);

                            DataTable ds = new DataTable();
                            sda.Fill(ds);

                            this.GridView1.DataSource = ds;
                            this.GridView1.DataBind();                       

                        }
                        con.Close();
                    }
                    else
                    {
                        this.lblErrMsg.Text = "Please Check your selection!";
                    }
                }
                // Show Summary Report
                else
                {
                    this.GridView.Visible = true;
                    this.GridView1.Visible = false;

                    this.btnExportCSVDetails.Visible = false;
                    this.btnExporttoCSV.Visible = true;
                    // Check the starttime end time and Rate is null or not
                    if (this.txtStartDate.Text != "" && this.txtEndDate.Text != "")
                    {
                        this.GridView.DataSource = null;
                        this.GridView.DataBind();


                        // IF Select IOS 
                        if (this.dpdIsoName.SelectedItem.Text != "All")
                        {
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
                            //******* End of GMT Calculation ********//


                            //Connection to mysql
                            MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                            con.Open();

                            using (MySqlCommand cmd = con.CreateCommand())
                            {

                                string query = "select DATE_FORMAT(calldate,'%d-%b-%y') as Date, p.PartnerName as `Egress Carrier`,pd.Prefix as `Dial Code`,MatchedPrefixSupplier as Destination,"
                                                + " TotalCalls as `Call Qty.`,round(duration1,3) as `Duration (Min)`,supplierrate as `Rate ($)`, round(EgressAmount,4) as `Bill Amount ($)`"
                                                + " from"
                                                + " (select date(starttime) as CallDate,count(*) as TotalCalls,supplierid,sum(duration2)/60 as duration1,supplierrate,sum(suppliercost) as EgressAmount,"
                                                + " matchedprefixsupplier,StartTime,idcall"
                                                + " from cdrloaded"
                                                + " where SupplierID=" + customerId + ""
                                                + " and starttime>='" + this._startdatetime+"'"
                                                + " and starttime<='"+ this._enddatetime +"'"
                                                + " and ServiceGroup=5"
                                                + " and chargingstatus=1"
                                                + " and duration2>0"
                                                + " group by date(starttime),supplierid,supplierrate,MatchedPrefixsupplier"
                                                + " ) x"
                                                + " left join partner p"
                                                + " on x.supplierid=p.idpartner"
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
                               // double TotalCall = ds.AsEnumerable().Sum(row => row.Field<double>("Call Qty."));
                                double totalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Duration (Min)"));
                                double totalAmount = ds.AsEnumerable().Sum(row => row.Field<double>("Bill Amount ($)"));
                                //  double OuttotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Outgoing Duration"));


                              //  GridView.FooterRow.Cells[4].Text = TotalCall.ToString("N2");
                                this.GridView.FooterRow.Cells[5].Text = totalDuration.ToString("N2");
                                this.GridView.FooterRow.Cells[7].Text = totalAmount.ToString("N2");

                            }
                            con.Close();
                        }
                        else if (this.dpdIsoName.SelectedItem.Text == "All")        // Show data for all IOS
                        {


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
                            //******* End of GMT Calculation ********//


                            //Connection to mysql
                            MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                            con.Open();

                            using (MySqlCommand cmd = con.CreateCommand())
                            {
                                // + " and customerid="+ ISOId+""
                                string query = "select DATE_FORMAT(calldate,'%d-%b-%y') as Date, p.PartnerName as `Egress Carrier`,pd.Prefix as Destination,MatchedPrefixSupplier as `Dial Code`,"
                                                + " TotalCalls as `Call Qty.`,round(duration1,3) as `Duration (Min)`,supplierrate as `Rate ($)`, round(EgressAmount,4) as `Bill Amount ($)`"
                                                + " from"
                                                + " (select date(starttime) as CallDate,count(*) as TotalCalls,supplierid,sum(duration2)/60 as duration1,supplierrate,sum(suppliercost) as EgressAmount,"
                                                + " matchedprefixsupplier,StartTime,idcall"
                                                + " from cdr where"
                                                + " starttime>='" + this._startdatetime+"'"
                                                + " and starttime<='" + this._enddatetime+"'"
                                                + " and ServiceGroup=5"
                                                + " and chargingstatus=1"
                                                + " and duration2>0"
                                                + " group by date(starttime),supplierid,supplierrate,MatchedPrefixsupplier"
                                                + " ) x"
                                                + " left join partner p"
                                                + " on x.supplierid=p.idpartner"
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
                              //  double TotalCall = ds.AsEnumerable().Sum(row => row.Field<double>("Call Qty."));
                                double totalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Duration (Min)"));
                                double totalAmount = ds.AsEnumerable().Sum(row => row.Field<double>("Bill Amount ($)"));
                                //  double OuttotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Outgoing Duration"));


                               // GridView.FooterRow.Cells[4].Text = TotalCall.ToString("N2");
                                this.GridView.FooterRow.Cells[5].Text = totalDuration.ToString("N2");
                                this.GridView.FooterRow.Cells[7].Text = totalAmount.ToString("N2");
                            }
                            con.Close();
                        }

                    }

                    else
                    {
                        this.lblErrMsg.Text = "Please Check Your Selection!";
                    }
                }

            }
            catch (Exception ex)
            {
                this.lblErrMsg.Text = ex.Message;
            }
        }

        // Action data show on gridview
        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            EgressInvoice();
        }


        // Gridview Pagging
        protected void PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            EgressInvoice();
        }



        // Gridview1 Pagging
        protected void PageIndexChangingDetail(object sender, GridViewPageEventArgs e)
        {
            this.GridView1.PageIndex = e.NewPageIndex;
            EgressInvoice();
        }


        // Export to CSV from Gridview (Summary Report)
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            this.btnExportCSVDetails.Visible = false;
            this.btnExporttoCSV.Visible = true;
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=EgressInvoiceClearance.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView.AllowPaging = false;
            EgressInvoice();

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
                    // if (GridView.Rows[i].Cells[k].Text=="")
                    //     sBuilder.Append("Null");
                    // else
                    sBuilder.Append(this.GridView.Rows[i].Cells[k].Text.Replace(",", "") + ",");
                }
                sBuilder.Append("\r\n");
            }
            this.Response.Output.Write(sBuilder.ToString());
            this.Response.Flush();
            this.Response.End();     
        }


                                                        
        // Export to CSV from Gridview1 (details Report)
        protected void btnExportCSVDetails_Click(object sender, EventArgs e)
        {
            this.btnExportCSVDetails.Visible = true;
            this.btnExporttoCSV.Visible = false;
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=EgressInvoiceClearance.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView1.AllowPaging = false;
            EgressInvoice();

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
                    // if (GridView.Rows[i].Cells[k].Text=="")
                    //     sBuilder.Append("Null");
                    // else
                    sBuilder.Append(this.GridView1.Rows[i].Cells[k].Text.Replace(",", "") + ",");
                }
                sBuilder.Append("\r\n");
            }
            this.Response.Output.Write(sBuilder.ToString());
            this.Response.Flush();
            this.Response.End();
        }
    }
}