using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PortalApp.reports.Accounts
{
    public partial class BdIncomingBusiness : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.btnExporttoCSV.Visible = false;
        }




        // BD Incoming Business Show on gridview
        private void BdIncomingBusinessGet()
        {
            try
            {
                this.lblErrMsg.Text = "";

                //Connection to mysql
                MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    string query = "select DATE_FORMAT(CallDate,'%d-%b-%y') as `Call Date`,cust.partnername as `Ingress Carrier`,Callqty as 'Call qty.',round(duration,2) as `Duration (Min)`,customerrate as `Ingress Rate ($)`,"
                                    + " round(ingressAmount,2) as `Ingress Amount ($)`,sup.PartnerName as `Egress Carrier`,round(iosamount,2) as `IOS Amount ($)`,"
                                    + " round(btrcamount,2) as `BTRC Amount`,round((IngressAmount-iosAmount-btrcamount),4) as `DCL Margin ($)`"
                                    + " from"
                                    + " ("
                                    + " select date(starttime) as CallDate,sum(chargingstatus) as Callqty,customerid,sum(duration1)/60 as duration,customerrate,sum(customercost) as IngressAmount,"
                                    + " supplierid,sum(costicxin) as IoSAmount,sum(CostVATCommissionIn) as btrcamount"
                                    + " from cdr"
                                    + " where starttime>='"+ this.txtStartDate.Text+" 00:00:00'"
                                    + " and starttime<='"+ this.txtEndDate.Text+" 23:59:59'"
                                    + " and calldirection=4"
                                    + " and chargingstatus=1"
                                    + " and duration1>0"
                                    + " group by date(starttime),customerid,customerrate,supplierid"
                                    + " ) x"
                                    + " left join partner cust"
                                    + " on x.customerid=cust.idpartner"
                                    + " left join partner sup"
                                    + " on x.supplierid=sup.idpartner"
                                    + " order by CallDate";

                    MySqlCommand com = new MySqlCommand(query, con);
                    MySqlDataAdapter sda = new MySqlDataAdapter(com);

                    DataTable ds = new DataTable();
                    sda.Fill(ds);

                    this.GridView.DataSource = ds;
                    this.GridView.DataBind();


                    //Calculate Sum and display in Footer Row         
                    Decimal totalCall = ds.AsEnumerable().Sum(row => row.Field<Decimal>("Call qty."));
                    double totalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Duration (Min)"));
                    double totalIngressAmount = ds.AsEnumerable().Sum(row => row.Field<double>("Ingress Amount ($)"));

                    double totalIos = ds.AsEnumerable().Sum(row => row.Field<double>("IOS Amount ($)"));
                    double outtotalBtrcAmount = ds.AsEnumerable().Sum(row => row.Field<double>("BTRC Amount"));
                    double outtotalDclMargin = ds.AsEnumerable().Sum(row => row.Field<double>("DCL Margin ($)"));                    
                  //  double OuttotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Outgoing Duration"));


                    this.GridView.FooterRow.Cells[2].Text = totalCall.ToString();
                    this.GridView.FooterRow.Cells[3].Text = totalDuration.ToString("N2");
                    this.GridView.FooterRow.Cells[5].Text = totalIngressAmount.ToString("N2");
                    this.GridView.FooterRow.Cells[7].Text = totalIos.ToString("N2");
                    this.GridView.FooterRow.Cells[8].Text = outtotalBtrcAmount.ToString("N2");
                    this.GridView.FooterRow.Cells[9].Text = outtotalDclMargin.ToString("N2");
                   

                }
                con.Close();

            }
            catch (Exception ex)
            {
                this.lblErrMsg.Text = ex.Message;
            }
        }



        // Button to show gridview data
        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            this.btnExporttoCSV.Visible = true;
            BdIncomingBusinessGet();
        }


        // Gridview Pagging
        protected void PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            BdIncomingBusinessGet();
        }


        // Export to CSV
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=BDIncomingBusiness.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView.AllowPaging = false;
            BdIncomingBusinessGet();

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