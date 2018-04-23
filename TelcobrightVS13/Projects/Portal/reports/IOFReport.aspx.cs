using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Configuration;

namespace PortalApp.reports.Accounts
{
    public partial class IofReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }



        private void GetData()
        {
            if (this.txtStartDate.Text != "" && this.txtEndDate.Text != "" && this.dpdIsoName.SelectedItem.Text == "Incoming")
            {
                this.GridView.Visible = true;
                this.GridView1.Visible = false;
                this.lblErrMsg.Text = "";
                //Connection to mysql
                MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    string query = "select case "
                                + " when cr.PartnerName is null"
                                + " then 'N/A'"
                                + " ELSE cr.PartnerName"
                                + " end as `IOS Name`"
                                + " ,ConnectedCount as 'Call Attempts'"
                                + " ,CallsIn as 'Successful Calls',(ConnectedCount-CallsIn) as 'Failed Calls',ifnull(round(roundedduration,2),0) as 'Duration (Min)',ifnull(round(ACD,2),0) as ACD,ifnull(round(ASR,2),0) AS ASR"
                                + " from "
                                + " (select supplierid as IGW,sum(chargingstatus) as CallsIn, (sum(chargingstatus)/count(*))*100 as ASR"
                                + " ,((sum(round(durationsec,0))/60)/sum(chargingstatus)) as ACD,sum(roundedduration)/60 as roundedduration,count(connecttime) as ConnectedCount"
                                + " from cdr "
                                + " where starttime>='" + this.txtStartDate.Text + " 00:00:00'"
                                + " and starttime<='" + this.txtEndDate.Text + " 23:59:59'"
                                + " and partialnextidcall is null"
                                + " and ServiceGroup=4"
                                + " group by supplierid"
                                + " ) x"
                                + " left join partner cr"
                                + " on x.IGW=cr.idpartner"
                                + " group by cr.partnerName"
                                + " order by cr.PartnerName";

                    MySqlCommand com = new MySqlCommand(query, con);
                    MySqlDataAdapter sda = new MySqlDataAdapter(com);

                    DataTable ds = new DataTable();
                    sda.Fill(ds);

                    this.GridView.DataSource = ds;
                    this.GridView.DataBind();

                    //Calculate Sum and display in Footer Row                
                    Int64 totalDuration = ds.AsEnumerable().Sum(row => row.Field<Int64>("Call Attempts"));
                    Decimal totalIngressAmount = ds.AsEnumerable().Sum(row => row.Field<Decimal>("Successful Calls"));

                    Decimal totalIos = ds.AsEnumerable().Sum(row => row.Field<Decimal>("Failed Calls"));
                    double outtotalBtrcAmount = ds.AsEnumerable().Sum(row => row.Field<double>("Duration (Min)"));
                    
                    //  double OuttotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Outgoing Duration"));

                    this.GridView.FooterRow.Cells[0].Text = "Total";
                    this.GridView.FooterRow.Cells[1].Text = totalDuration.ToString("N2");
                    this.GridView.FooterRow.Cells[2].Text = totalIngressAmount.ToString("N2");
                    this.GridView.FooterRow.Cells[3].Text = totalIos.ToString("N2");
                    this.GridView.FooterRow.Cells[4].Text = outtotalBtrcAmount.ToString("N2");
                    

                }
                con.Close();
            }
            else if (this.txtStartDate.Text != "" && this.txtEndDate.Text != "" && this.dpdIsoName.SelectedItem.Text == "Outgoing")
            {
                this.GridView1.Visible = true;
                this.GridView.Visible = false;
                MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    string query = "select case when cr.PartnerName is null"
                                + " then 'N/A'"
                                + " ELSE cr.PartnerName"
                                + " end as `IOS Name`,ConnectedCount as 'Call Attempts'"
                                + " ,CallsIn as 'Successful Calls',(ConnectedCount-CallsIn) as 'Failed Calls',round(roundedduration,2) as 'Duration (Min)',round(Billing,2) as 'Billing (Min)',round(ACD,2) as ACD,round(ASR,2) as ASR"
                                + " from "
                                + " (select customerid as IGW,sum(chargingstatus) as CallsIn, (sum(chargingstatus)/count(*))*100 as ASR"
                                + " ,((sum(round(durationsec,0))/60)/sum(chargingstatus)) as ACD,sum(duration3)/60 as roundedduration,sum(roundedduration)/60 as Billing,count(connecttime) as ConnectedCount"
                                + " from cdrloaded "
                                + " where starttime>='" + this.txtStartDate.Text + " 00:00:00'"
                                + " and starttime<='" + this.txtEndDate.Text + " 23:59:59'"
                                + " and partialnextidcall is null"
                                + " and ServiceGroup=5"
                                + " group by customerid"
                                + " ) x"
                                + " left join partner cr"
                                + " on x.IGW=cr.idpartner"
                                + " order by cr.PartnerName";

                    MySqlCommand com = new MySqlCommand(query, con);
                    MySqlDataAdapter sda = new MySqlDataAdapter(com);

                    DataTable ds = new DataTable();
                    sda.Fill(ds);

                    this.GridView1.DataSource = ds;
                    this.GridView1.DataBind();


                    //Calculate Sum and display in Footer Row                
                    Int64 totalDuration = ds.AsEnumerable().Sum(row => row.Field<Int64>("Call Attempts"));
                    Decimal totalIngressAmount = ds.AsEnumerable().Sum(row => row.Field<Decimal>("Successful Calls"));

                    Decimal totalIos = ds.AsEnumerable().Sum(row => row.Field<Decimal>("Failed Calls"));
                    double outtotalBtrcAmount = ds.AsEnumerable().Sum(row => row.Field<double>("Duration (Min)"));
                  
                    double totalbillingDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Billing (Min)"));

                    //  double OuttotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Outgoing Duration"));

                    this.GridView1.FooterRow.Cells[0].Text = "Total";
                    this.GridView1.FooterRow.Cells[1].Text = totalDuration.ToString("N2");
                    this.GridView1.FooterRow.Cells[2].Text = totalIngressAmount.ToString("N2");
                    this.GridView1.FooterRow.Cells[3].Text = totalIos.ToString("N2");
                    this.GridView1.FooterRow.Cells[4].Text = outtotalBtrcAmount.ToString("N2");
                    this.GridView1.FooterRow.Cells[5].Text = totalbillingDuration.ToString("N2");
                }
                con.Close();

            }
            else
            {
                this.lblErrMsg.Text = "Your Selection is Wrong!";
            }
        }



        // data show button
        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            GetData();
        }


        // Export to CSV
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            if (this.dpdIsoName.SelectedItem.Text == "Incoming")
            {
                this.Response.Clear();
                this.Response.Buffer = true;
                this.Response.AddHeader("content-disposition", "attachment;filename=IOFReport.csv");
                this.Response.Charset = "";
                this.Response.ContentType = "application/text";

                this.GridView.AllowPaging = false;
                GetData();

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
            else if (this.dpdIsoName.SelectedItem.Text == "Outgoing")
            {
                this.Response.Clear();
                this.Response.Buffer = true;
                this.Response.AddHeader("content-disposition", "attachment;filename=IOFReport.csv");
                this.Response.Charset = "";
                this.Response.ContentType = "application/text";

                this.GridView1.AllowPaging = false;
                GetData();

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
}