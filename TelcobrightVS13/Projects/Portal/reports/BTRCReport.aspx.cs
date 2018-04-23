using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PortalApp.reports.BTRC
{
    public partial class BtrcReport : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }



        // Show BTRC Report portion
        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            BtrcReportOnGridview();
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

                        string query = "select"
                                        + " DATE_FORMAT(CallDate,'%d-%b-%y') as 'Call Date'"
                                        +" ,IncomingCall as 'Incoming Call No.'"
                                        + " ,round(IncomingDuration,2) as 'Incoming Duration (Min)'"
                                        +" ,ifnull(OutgoingCall,0) as 'Outgoing Call No.'"
                                        + " ,ifnull(OutgoingDuration,0) as 'Outgoing Duration'"
                                        +" from"
                                        +"("
                                        +" select date(starttime) as CallDate"
                                        +" ,sum(sequencenumber) as `IncomingCall`"
                                        +" ,sum(roundedduration)/60 as `IncomingDuration`"
                                        +" from cdrsummary"
                                        +" where starttime>='"+ this.txtStartDate.Text+" 00:00:00'" 
                                        +" and starttime<='"+ this.txtEndDate.Text+" 23:59:59'"
                                        +" and ServiceGroup=4"
                                        +" and partialnextidcall is null"
                                        +" group by date(starttime)"
                                        +" )x"
                                        +" left join "
                                        +"("
                                        +" select date(starttime) as CallDate1"
                                        +" ,sum(sequencenumber) as `OutgoingCall`"
                                        + " ,sum(duration3)/60 as `OutgoingDuration`"
                                        +" from cdrsummary"
                                        +" where starttime>='"+ this.txtStartDate.Text+" 00:00:00'"
                                        +" and starttime<='"+ this.txtEndDate.Text+" 23:59:59'"
                                        +" and ServiceGroup=5"
                                        + " and partialnextidcall is null"
                                        +" group by date(starttime)"
                                        +")y"
                                        +" on x.CallDate=y.CallDate1"
                                        + " order by CallDate"; 




                        MySqlCommand com = new MySqlCommand(query, con);
                        MySqlDataAdapter sda = new MySqlDataAdapter(com);

                        DataTable ds = new DataTable();
                        sda.Fill(ds);

                        this.GridView.DataSource = ds;
                        this.GridView.DataBind();


                        //Calculate Sum and display in Footer Row
                        decimal intotalCall = ds.AsEnumerable().Sum(row => row.Field<decimal>("Incoming Call No."));
                        double intotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Incoming Duration (Min)"));
                        decimal outtotalCall = ds.AsEnumerable().Sum(row => row.Field<decimal>("Outgoing Call No."));
                        double outtotalDuration = ds.AsEnumerable().Sum(row => row.Field<double>("Outgoing Duration"));


                    // Find column datatype
                    //var type = ds.Columns["Outgoing Duration"].DataType;
                    //txtStartDate.Text = type.ToString();

                    if (this.GridView.FooterRow != null)
                    {
                        this.GridView.FooterRow.Cells[0].Text = "Total";
                        this.GridView.FooterRow.Cells[1].Text = intotalCall.ToString("N2");
                        this.GridView.FooterRow.Cells[2].Text = intotalDuration.ToString("N2");
                        this.GridView.FooterRow.Cells[3].Text = outtotalCall.ToString("N2");
                        this.GridView.FooterRow.Cells[4].Text = outtotalDuration.ToString("N2");


                        // Footer design
                        this.GridView.FooterRow.Cells[0].HorizontalAlign = HorizontalAlign.Center;
                        this.GridView.FooterRow.Cells[0].ForeColor = Color.White;
                        this.GridView.FooterRow.Cells[1].HorizontalAlign = HorizontalAlign.Center;
                        this.GridView.FooterRow.Cells[1].ForeColor = Color.White;
                        this.GridView.FooterRow.Cells[2].HorizontalAlign = HorizontalAlign.Center;
                        this.GridView.FooterRow.Cells[2].ForeColor = Color.White;
                        this.GridView.FooterRow.Cells[3].HorizontalAlign = HorizontalAlign.Center;
                        this.GridView.FooterRow.Cells[3].ForeColor = Color.White;
                        this.GridView.FooterRow.Cells[4].HorizontalAlign = HorizontalAlign.Center;
                        this.GridView.FooterRow.Cells[4].ForeColor = Color.White;
                    }

                        
                        
                    }
                    con.Close();
                              
            }
            catch (Exception ex)
            {
                this.lblErrMsg.Text = ex.Message;
            }
        }


        // Gridview Pagging
        protected void PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            BtrcReportOnGridview();
        }


        // Call Date Header hide that normally created
        protected void grdViewProducts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Invisibling the first three columns of second row header (normally created on binding)
            if (e.Row.RowType == DataControlRowType.Header)
            {
                e.Row.Cells[0].Visible = false; // Invisibiling Year Header Cell              
            }         
        }


        protected void grdViewProducts_RowCreated(object sender, GridViewRowEventArgs e)
        {
            // Adding a column manually once the header created
            if (e.Row.RowType == DataControlRowType.Header) // If header created
            {
                GridView productGrid = (GridView)sender;

                // Creating a Row
                GridViewRow headerRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Insert);

                //Adding Year Column
                TableCell headerCell = new TableCell();

                // Date Header
                headerCell.Text = "Call Date";
                headerCell.HorizontalAlign = HorizontalAlign.Center;
                headerCell.RowSpan = 2;
                headerCell.CssClass = "HeaderStyle";
                headerRow.Cells.Add(headerCell);

                //Adding Incoming Column
                headerCell = new TableCell();
                headerCell.Text = "Incoming";
                headerCell.HorizontalAlign = HorizontalAlign.Center;
                headerCell.ColumnSpan = 2; // For merging three columns (Direct, Referral, Total)
                headerCell.CssClass = "HeaderStyle";
                headerRow.Cells.Add(headerCell);
                           
                
                //Adding Outgoing Column
                headerCell = new TableCell();
                headerCell.Text = "Outgoing";
                headerCell.HorizontalAlign = HorizontalAlign.Center;
                headerCell.ColumnSpan = 2; // For merging three columns (Direct, Referral, Total)
                headerCell.CssClass = "HeaderStyle";
                headerRow.Cells.Add(headerCell);
              

                //Adding the Row at the 0th position (first row) in the Grid
                productGrid.Controls[0].Controls.AddAt(0, headerRow);
            }
        }

        // Export to Excel
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=BTRCReport.xls");
            this.Response.Charset = "";
            this.Response.ContentType = "application/vnd.ms-excel";
            this.GridView.AllowPaging = false;
            BtrcReportOnGridview();
            using (System.IO.StringWriter sw = new System.IO.StringWriter())
            {
                HtmlTextWriter hw = new HtmlTextWriter(sw);

                this.GridView.HeaderRow.BackColor = Color.White;
                this.GridView.FooterRow.BackColor = Color.White;
                foreach (TableCell cell in this.GridView.HeaderRow.Cells)
                {
                    cell.BackColor = this.GridView.HeaderStyle.BackColor;
                }
                foreach (TableCell cell in this.GridView.FooterRow.Cells)
                {
                    cell.BackColor = this.GridView.FooterStyle.BackColor;
                }
                foreach (GridViewRow row in this.GridView.Rows)
                {
                    row.BackColor = Color.White;
                    foreach (TableCell cell in row.Cells)
                    {
                        if (row.RowIndex % 2 == 0)
                        {
                            cell.BackColor = this.GridView.AlternatingRowStyle.BackColor;
                        }
                        else
                        {
                            cell.BackColor = this.GridView.RowStyle.BackColor;
                        }
                        cell.CssClass = "textmode";
                    }
                }

                this.GridView.RenderControl(hw);

                string style = @"<style> .textmode { } </style>";
                this.Response.Write(style);
                this.Response.Output.Write(sw.ToString());
                this.Response.Flush();
                this.Response.End();
            }

            //string attachment = "attachment; filename=Export.xls";
            //Response.ClearContent();
            //Response.AddHeader("content-disposition", attachment);
            //Response.ContentType = "application/ms-excel";
            //StringWriter sw = new StringWriter();
            //HtmlTextWriter htw = new HtmlTextWriter(sw);
            //// Create a form to contain the grid
            //HtmlForm frm = new HtmlForm();
            //GridView.Parent.Controls.Add(frm);
            //frm.Attributes["runat"] = "server";
            //frm.Controls.Add(GridView);
            //frm.RenderControl(htw);

            ////GridView1.RenderControl(htw);
            //Response.Write(sw.ToString());
            //Response.End();
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            //
        }
    }
}