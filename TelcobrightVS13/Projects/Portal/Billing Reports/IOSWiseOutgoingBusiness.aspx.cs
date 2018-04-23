using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PortalApp.reports.Accounts
{
    public partial class DemoReport : Page
    {
        int _isoId = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            //load Country KPI           
            this.GridView.Visible = false;
            this.GridView1.Visible = false;

            this.btnExportCSVDetails.Visible = false;
            this.btnExporttoCSV.Visible = false;
        }



        // Gridview data show
        private void ShowDataOnGridview()
        {
           
            try
            {                           
                // IOS Select from dropdown 
                if (this.dpdIsoName.SelectedValue == "Novotel Limited")
                {
                    this._isoId = 1;
                }
                else if (this.dpdIsoName.SelectedValue == "Mir Telecom Ltd")
                {
                    this._isoId = 2;
                }
                else if (this.dpdIsoName.SelectedValue == "Bangla Trac Communications Limited")
                {
                    this._isoId = 3;
                }
                else if (this.dpdIsoName.SelectedValue == "Global Voice Telecom Limited")
                {
                    this._isoId = 4;
                }
                else if (this.dpdIsoName.SelectedValue == "Digicon Telecommunication Ltd")
                {
                    this._isoId = 5;
                }
                else if (this.dpdIsoName.SelectedValue == "Roots Communication Ltd")
                {
                    this._isoId = 6;
                }
                else if (this.dpdIsoName.SelectedValue == "Unique Infoway Limited")
                {
                    this._isoId = 7;
                }


                if (this.txtRate.Text != "" && this.txtStartDate.Text != "" && this.txtEndDate.Text != "" && (this._isoId == 1 || this._isoId == 2 || this._isoId == 3 || this._isoId == 4 || this._isoId == 5 || this._isoId == 6 || this._isoId == 7))               
                {
                    this.lblErrMsg.Text = "";

                    // Details Report
                    if (this.chkdetailreport.Checked == true)
                    {
                        this.GridView.Visible = false;
                        this.GridView1.Visible = true;

                        this.btnExportCSVDetails.Visible = true;
                        this.btnExporttoCSV.Visible = false;
                        //Connection to mysql
                        MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                        con.Open();

                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "select DATE_FORMAT(Date, '%d-%b-%y') as `Call Date`,case when cr.partnername is null Then 'Roaming' ELSE cr.PartnerName END as 'ANS Name',cricx.partnername as `IOS Name`, OriginatingIP as `IOS IP`, OriginatingCallingNumber as `A Number`"
                                            + " , MediaIP4 as `B Number`,concat(matchedprefixy,' (',ic.Description,')') as `Destination Name`, "
                                            + " MatchedPrefixY as `Dial Code`, round(ActualDuration,2) as `Actual Duration (Sec)`,round(RoundedDuration,2) as `Billed Duration (Sec)`, round(X,2) as 'X Value', round(Y*" + this.txtRate.Text + ",2) as 'Y Value', round(x-" + this.txtRate.Text + "*y,2) as `Z Value`,"
                                            +" round(.15*(x-"+ this.txtRate.Text+"*y),2) as '15% of Z', round((Y*"+ this.txtRate.Text+")+.15*(x-"+ this.txtRate.Text+"*y),2) as 'Invoice Amount'"
                                            +" from"
                                            +" ("
                                            +" select date(starttime) as `Date`,ansidorig,Count(*) as CallCount,matchedprefixy,sum(Duration3) as ActualDuration,originatingip,"
                                            +" sum(roundedduration) as RoundedDuration, sum(SubscriberChargeXOut) as X, sum(carrierCostYigwout) as Y,customerid"
                                            +" ,OriginatingCallingNumber,MediaIP4"
                                            +" from cdrloaded c"
                                            +" where ServiceGroup=5"
                                            +" and chargingstatus=1"
                                            +" and starttime>='"+ this.txtStartDate.Text+" 00:00:00'"
                                            +" and starttime<='"+ this.txtEndDate.Text+" 23:59:59'"
                                            +" and CustomerID="+ this._isoId+""
                                            +" group by ansidorig,matchedprefixy,customerid,date(starttime),OriginatingCallingNumber,MediaIP4"
                                            +" ) x"
                                            +" left join partner cr"
                                            +" on x.ansidorig=cr.idpartner"
                                            +" left join xyzprefix ic"
                                            +" on x.matchedprefixy=ic.Prefix"
                                            +" left join CountryCode cc"
                                            +" on ic.CountryCode=cc.Code"
                                            +" left join partner cricx"
                                            +" on x.customerid=cricx.idpartner"
                                            + " order by Date";

                            MySqlCommand com = new MySqlCommand(query, con);
                            MySqlDataAdapter sda = new MySqlDataAdapter(com);

                            DataSet ds = new DataSet();
                            sda.Fill(ds);

                            this.GridView1.DataSource = ds;
                            this.GridView1.DataBind();
                        }
                        con.Close();
                    }
                    else
                    {
                        this.GridView.Visible = true;
                        this.GridView1.Visible = false;

                        this.btnExportCSVDetails.Visible = false;
                        this.btnExporttoCSV.Visible = true;
                        //Connection to mysql
                        MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                        con.Open();

                        using (MySqlCommand cmd = con.CreateCommand())
                        {

                            string query = "select  DATE_FORMAT(Date, '%d-%b-%y') as Date,case when cr.partnername is null Then 'Roaming' ELSE cr.PartnerName END as ANS,cricx.partnername as IOS,concat(matchedprefixy,' (',ic.Description,')') as Destination, concat(cc.Code,' (',cc.Name,')' )as Country,"
                            + " MatchedPrefixY as Prefix, CallCount as 'No. Of Calls', round(ActualDuration,2) as 'Actual Minute',round(RoundedDuration,2) as 'Billing Minute', round(X,2) as 'X (BDT)', round(Y,6) as 'Y (USD)',(select " + this.txtRate.Text + ") as 'USD Rate', round(Y*" + this.txtRate.Text + ",2) as 'Y (BDT)', round(x-" + this.txtRate.Text + "*y,2) as 'Z (BDT)',"
                            + " round(.15*(x-" + this.txtRate.Text + "*y),2) as '15% of Z', round((Y*" + this.txtRate.Text + ")+.15*(x-" + this.txtRate.Text + "*y),2) as 'Invoice Amount'"
                            + " from"
                            + "("
                            + " select date(starttime) as `Date`,ansidorig,Count(*) as CallCount,matchedprefixy,sum(Duration3)/60 ActualDuration,"
                            + " sum(roundedduration)/60 RoundedDuration, sum(SubscriberChargeXOut) as X, sum(carrierCostYigwout) as Y,customerid"
                            + " from cdr c"
                            + " where customerid=" + this._isoId + ""
                            + " and ServiceGroup=5"
                            + " and chargingstatus=1"
                            + " and starttime>='" + this.txtStartDate.Text + " 00:00:00'"
                            + " and starttime<='" + this.txtEndDate.Text + " 23:59:59'"
                            + " group by ansidorig,matchedprefixy,customerid,date(starttime)"
                            + " ) x"
                            + " left join partner cr"
                            + " on x.ansidorig=cr.idpartner"
                            + " left join xyzprefix ic"
                            + " on x.matchedprefixy=ic.Prefix"
                            + " left join CountryCode cc"
                            + " on ic.CountryCode=cc.Code"
                            + " left join partner cricx"
                            + " on x.customerid=cricx.idpartner"
                            + " order by Date";

                            MySqlCommand com = new MySqlCommand(query, con);
                            MySqlDataAdapter sda = new MySqlDataAdapter(com);

                            DataSet ds = new DataSet();
                            sda.Fill(ds);

                            this.GridView.DataSource = ds;
                            this.GridView.DataBind();
                        }
                        con.Close();
                    }                  
                
                }
                else if (this.txtRate.Text != "" && this.txtStartDate.Text != "" && this.txtEndDate.Text != "" && (this.dpdIsoName.SelectedValue == "Select IOS Name" || this.dpdIsoName.SelectedValue == "All IOS"))
                {
                    this.lblErrMsg.Text = "";

                    this.GridView.Visible = true;
                    this.GridView1.Visible = false;

                    this.btnExportCSVDetails.Visible = false;
                    this.btnExporttoCSV.Visible = true;
                    //Connection to mysql
                    MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                    con.Open();

                    using (MySqlCommand cmd = con.CreateCommand())
                    {

                        string query = "select  DATE_FORMAT(Date, '%d-%b-%y') as Date,case when cr.partnername is null Then 'Roaming' ELSE cr.PartnerName END as ANS,cricx.partnername as IOS,concat(matchedprefixy,' (',ic.Description,')') as Destination, concat(cc.Code,' (',cc.Name,')' )as Country,"
                        + " MatchedPrefixY as Prefix, CallCount as 'No. Of Calls', round(ActualDuration,2) as 'Actual Minute',round(RoundedDuration,2) as 'Billing Minute', round(X,2) as 'X (BDT)', round(Y,6) as 'Y (USD)',(select " + this.txtRate.Text + ") as 'USD Rate', round(Y*" + this.txtRate.Text + ",2) as 'Y (BDT)', round(x-" + this.txtRate.Text + "*y,2) as 'Z (BDT)',"
                        + " round(.15*(x-" + this.txtRate.Text + "*y),2) as '15% of Z', round((Y*" + this.txtRate.Text + ")+.15*(x-" + this.txtRate.Text + "*y),2) as 'Invoice Amount'"
                        + " from"
                        + "("
                        + " select date(starttime) as `Date`,ansidorig,Count(*) as CallCount,matchedprefixy,sum(Duration3)/60 ActualDuration,"
                        + " sum(roundedduration)/60 RoundedDuration, sum(SubscriberChargeXOut) as X, sum(carrierCostYigwout) as Y,customerid"
                        + " from cdr c"
                        + " where ServiceGroup=5"
                        + " and chargingstatus=1"
                        + " and starttime>='" + this.txtStartDate.Text + " 00:00:00'"
                        + " and starttime<='" + this.txtEndDate.Text + " 23:59:59'"
                        + " group by ansidorig,matchedprefixy,customerid,date(starttime)"
                        + " ) x"
                        + " left join partner cr"
                        + " on x.ansidorig=cr.idpartner"
                        + " left join xyzprefix ic"
                        + " on x.matchedprefixy=ic.Prefix"
                        + " left join CountryCode cc"
                        + " on ic.CountryCode=cc.Code"
                        + " left join partner cricx"
                        + " on x.customerid=cricx.idpartner"
                        + " order by Date";

                        MySqlCommand com = new MySqlCommand(query, con);
                        MySqlDataAdapter sda = new MySqlDataAdapter(com);

                        DataSet ds = new DataSet();
                        sda.Fill(ds);

                        this.GridView.DataSource = ds;
                        this.GridView.DataBind();
                    }
                    con.Close();
                }
                else
                {
                    this.lblErrMsg.Text = "Please Check your selection!";
                    this.GridView.DataSource = null;
                    this.GridView.DataBind();
                }
            }
            catch (Exception ex)
            {
                this.lblErrMsg.Text = ex.Message;
            }
        }



        // Show Report on Gridview
        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            ShowDataOnGridview();
        }



        // Gridview Pagging
        protected void PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            ShowDataOnGridview();
        }


        // Gridview1 Pagging
        protected void PageIndexChangingGridview1(object sender, GridViewPageEventArgs e)
        {
            this.GridView1.PageIndex = e.NewPageIndex;
            ShowDataOnGridview();
        }


        // Gridview Export to CSV summary report
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            this.btnExportCSVDetails.Visible = false;
            this.btnExporttoCSV.Visible = true;
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=IOSWiseOutgoingBusiness.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView.AllowPaging = false;
             ShowDataOnGridview();

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
            this.Response.Output.Write(sBuilder.ToString());
            this.Response.Flush();
            this.Response.End();     
           
        }


        public override void VerifyRenderingInServerForm(Control control)
        {
            //
        }


        //Export to CSV details report
        protected void btnExportCSVDetails_Click(object sender, EventArgs e)
        {
            this.btnExportCSVDetails.Visible = true;
            this.btnExporttoCSV.Visible = false;
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=IOSWiseOutgoingBusiness.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView1.AllowPaging = false;
            ShowDataOnGridview();

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
            this.Response.Output.Write(sBuilder.ToString());
            this.Response.Flush();
            this.Response.End();     
        }
        
       
    }
}