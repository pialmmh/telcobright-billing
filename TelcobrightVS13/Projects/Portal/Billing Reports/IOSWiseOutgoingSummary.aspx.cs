using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PortalApp.reports.Accounts
{
    public partial class IosWiseOutgoingSummary : Page
    {
        int _isoId;
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        public void ShowSummaryReport()
        {
            try
            {
                this.lblErrMsg.Text = "";


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
                else if (this.dpdIsoName.SelectedValue == "All IOS")
                {
                    this._isoId = 0;
                }


                // Check the starttime end time and Rate is null or not
                if (this.txtStartDate.Text != "" && this.txtEndDate.Text != "" && this.txtRate.Text != "" && this.dpdIsoName.SelectedItem.Text != "Select IOS Name")
                {
                    this.GridView.DataSource = null;
                    this.GridView.DataBind();


                    // IF Select IOS 
                    if (this._isoId == 1 || this._isoId == 2 || this._isoId == 3 || this._isoId == 4 || this._isoId == 5 || this._isoId == 6 || this._isoId == 7)
                    {
                        //Connection to mysql
                        MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                        con.Open();

                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "select case when cr.partnername is null Then 'Roaming' ELSE cr.PartnerName END as ANS,cricx.partnername as IOS,"
                                            + " sum(CallCount) as 'No. Of Calls', round(sum(ActualDuration),2) as 'Actual Minute',round(sum(RoundedDuration),2) as 'Billing Minute', round(sum(X),2) as 'X Value', round(sum(Y*" + this.txtRate.Text + "),2) as 'Y Value', round(sum(x-" + this.txtRate.Text + "*y),2) as 'Z Value',"
                                            + " round(sum(.15*(x-" + this.txtRate.Text + "*y)),2) as '15% of Z', round(sum((Y*" + this.txtRate.Text + ")+.15*(x-" + this.txtRate.Text + "*y)),2) as 'Invoice Amount'"
                                            + " from"
                                            + " ("
                                            + " select ansidorig,Count(*) as CallCount,matchedprefixy,sum(Duration3)/60 ActualDuration,"
                                            + " sum(roundedduration)/60 RoundedDuration, sum(XAmount) as X, sum(YAmount) as Y,customerid"
                                            + " from cdr c"
                                            + " where ServiceGroup=5"
                                            + " and chargingstatus=1"
                                            + " and CustomerID=" + this._isoId + ""
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
                                            + " group by cr.partnername"
                                            + " order by cr.partnername";

                            MySqlCommand com = new MySqlCommand(query, con);
                            MySqlDataAdapter sda = new MySqlDataAdapter(com);

                            DataSet ds = new DataSet();
                            sda.Fill(ds);

                            this.GridView.DataSource = ds;
                            this.GridView.DataBind();
                        }
                        con.Close();
                    }
                    else if (this._isoId == 0) {
                        //Connection to mysql
                        MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                        con.Open();

                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "select case when cr.partnername is null Then 'Roaming' ELSE cr.PartnerName END as ANS,cricx.partnername as IOS,"
                                            + " sum(CallCount) as 'No. Of Calls', round(sum(ActualDuration),2) as 'Actual Minute',round(sum(RoundedDuration),2) as 'Billing Minute', round(sum(X),2) as 'X Value', round(sum(Y*" + this.txtRate.Text + "),2) as 'Y Value', round(sum(x-" + this.txtRate.Text + "*y),2) as 'Z Value',"
                                            + " round(sum(.15*(x-" + this.txtRate.Text + "*y)),2) as '15% of Z', round(sum((Y*" + this.txtRate.Text + ")+.15*(x-" + this.txtRate.Text + "*y)),2) as 'Invoice Amount'"
                                            + " from"
                                            + " ("
                                            + " select ansidorig,Count(*) as CallCount,matchedprefixy,sum(Duration3)/60 ActualDuration,"
                                            + " sum(roundedduration)/60 RoundedDuration, sum(XAmount) as X, sum(YAmount) as Y,customerid"
                                            + " from cdrloaded c"
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
                                            + " group by cr.partnername"
                                            + " order by cr.partnername";

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
                else
                {
                    this.lblErrMsg.Text = "Please Check Your Selection!";
                }
            }
            catch (Exception ex)
            {
                this.lblErrMsg.Text = ex.Message;
            }
        }


        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            ShowSummaryReport();
        }


        // Gridview Pagging
        protected void PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            ShowSummaryReport();
        }

        protected void btnExportCSVDetails_Click(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=IOSWiseOutgoingSummary.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView.AllowPaging = false;
            ShowSummaryReport();       

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
    }
}