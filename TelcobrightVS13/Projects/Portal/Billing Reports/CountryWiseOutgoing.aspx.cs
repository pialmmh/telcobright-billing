using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.UI.WebControls;

namespace PortalApp.reports.Accounts
{
    public partial class CountryWiseOutgoingAnalysis : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        private void BtrcReportOnGridview()
        {

            try
            {
                this.lblErrMsg.Text = "";

                //Connection to mysql
                MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {

                    string query = "select DATE_FORMAT(Date,'%d-%b-%y') as Date,cc.Name as Country"
                                    +" , sum(CallCount) as 'No. Of Calls', round(sum(ActualDuration),2) as 'Actual Minute',round(sum(RoundedDuration),2) as 'Billing Minute', round(sum(X),2) as 'X (BDT)', round(sum(Y),6) as 'Y (USD)',(select "+ this.txtRate.Text+") as 'USD Rate', round(sum(Y*"+ this.txtRate.Text+"),2) as 'Y (BDT)', round(sum(x-"+ this.txtRate.Text+"*y),2) as 'Z (BDT)',"
                                    +" round(sum(.15*(x-"+ this.txtRate.Text+"*y)),2) as '15% of Z', round(sum((Y*"+ this.txtRate.Text+")+.15*(x-"+ this.txtRate.Text+"*y)),2) as 'Invoice Amount'"
                                    +" from"
                                    +" ("
                                    +" select date(starttime) as `Date`,ansidorig,Count(*) as CallCount,matchedprefixy,sum(Duration3)/60 ActualDuration,"
                                    +" sum(roundedduration)/60 RoundedDuration, sum(SubscriberChargeXOut) as X, sum(carrierCostYigwout) as Y,customerid"
                                    +" from cdr c"
                                    +" where calldirection=5"
                                    +" and chargingstatus=1"
                                    +" and starttime>='"+ this.txtStartDate.Text+" 00:00:00'	"	
                                    +" and starttime<='"+ this.txtEndDate.Text+" 23:59:59'"
                                    +" group by ansidorig,matchedprefixy,customerid,date(starttime)"
                                    +" ) x"
                                    +" left join partner cr"
                                    +" on x.ansidorig=cr.idpartner"
                                    +" left join xyzprefix ic"
                                    +" on x.matchedprefixy=ic.Prefix"
                                    +" left join CountryCode cc"
                                    +" on ic.CountryCode=cc.Code"
                                    +" left join partner cricx"
                                    +" on x.customerid=cricx.idpartner"
                                    +" group by Date,cc.Name"
                                    + " order by Date";

                    MySqlCommand com = new MySqlCommand(query, con);
                    MySqlDataAdapter sda = new MySqlDataAdapter(com);

                    DataTable ds = new DataTable();
                    sda.Fill(ds);

                    this.GridView.DataSource = ds;
                    this.GridView.DataBind();


                    //Calculate Sum and display in Footer Row
                    double intotalCall = ds.AsEnumerable().Sum(row => row.Field<double>("Actual Minute"));
                    double intotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Billing Minute"));
                    double outtotalCall = ds.AsEnumerable().Sum(row => row.Field<double>("15% of Z"));
                    double outtotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Invoice Amount"));


                    // Find column datatype
                    //var type = ds.Columns["Outgoing Duration"].DataType;
                    //txtStartDate.Text = type.ToString();

                    this.GridView.FooterRow.Cells[0].Text = "Total";
                    this.GridView.FooterRow.Cells[3].Text = intotalCall.ToString("N2");
                    this.GridView.FooterRow.Cells[4].Text = intotalDuration.ToString("N2");
                    this.GridView.FooterRow.Cells[10].Text = outtotalCall.ToString("N2");
                    this.GridView.FooterRow.Cells[11].Text = outtotalDuration.ToString("N2");


                    // Footer design
                    this.GridView.FooterRow.Cells[0].HorizontalAlign = HorizontalAlign.Center;
                    this.GridView.FooterRow.Cells[0].ForeColor = System.Drawing.Color.White;
                    this.GridView.FooterRow.Cells[3].HorizontalAlign = HorizontalAlign.Center;
                    this.GridView.FooterRow.Cells[3].ForeColor = System.Drawing.Color.White;
                    this.GridView.FooterRow.Cells[4].HorizontalAlign = HorizontalAlign.Center;
                    this.GridView.FooterRow.Cells[4].ForeColor = System.Drawing.Color.White;
                    this.GridView.FooterRow.Cells[10].HorizontalAlign = HorizontalAlign.Center;
                    this.GridView.FooterRow.Cells[10].ForeColor = System.Drawing.Color.White;
                    this.GridView.FooterRow.Cells[11].HorizontalAlign = HorizontalAlign.Center;
                    this.GridView.FooterRow.Cells[11].ForeColor = System.Drawing.Color.White;

                }
                con.Close();

            }
            catch (Exception ex)
            {
                this.lblErrMsg.Text = ex.Message;
            }
        }



        // Country Wise Outgoing business Action button      
        protected void btnShowReport_Click1(object sender, EventArgs e)
        {
            BtrcReportOnGridview();
        }



        // Gridview Pagging
        protected void PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            BtrcReportOnGridview();
        }

        // Export to CSV
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=CountryWiseOutgoing.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView.AllowPaging = false;
            BtrcReportOnGridview();

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
    }
}