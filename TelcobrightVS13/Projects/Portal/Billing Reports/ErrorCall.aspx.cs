using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text;
using System.Web.Configuration;

namespace PortalApp.reports.Other
{
    public partial class ErrorCall : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        //Show Error Report on Gridview
        public void ShowErrorCall()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());
                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    string query = "SELECT StartTime as 'Call Date', incomingroute as 'Ingress Trunk', outgoingroute as 'Egress Trunk', DurationSec as 'Duration (Sec)'"
                                    + " FROM cdrerror"
                                    + " where starttime>='" + this.txtStartDate.Text + " 00:00:00'"
                                    + " and starttime<='" + this.txtEndDate.Text + " 23:59:59'"
                                    + " and (chargingstatus=1 or DurationSec>0)";

                    MySqlCommand com = new MySqlCommand(query, con);
                    MySqlDataAdapter sda = new MySqlDataAdapter(com);

                    DataTable ds = new DataTable();
                    sda.Fill(ds);

                    this.GridView.DataSource = ds;
                    this.GridView.DataBind();
                }
                con.Close();
            }
            catch (Exception ex)
            {

            }
        }
        //Show Error Report on Gridview
        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            ShowErrorCall();
        }


        // Export to CSV
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=ErrorCall.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView.AllowPaging = false;
            ShowErrorCall();

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
    }
}