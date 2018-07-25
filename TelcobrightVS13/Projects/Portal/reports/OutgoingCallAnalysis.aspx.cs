using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text;
using System.Web.Configuration;
using System.Web.UI.WebControls;

namespace PortalApp.reports.Accounts
{
    public partial class CountryWiseAnsInvoice : System.Web.UI.Page
    {
        int _isoId;
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        // Gridview data show
        private void CountryWiseAnsInvoiceBind()
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


                   // Check the starttime end time and Rate is null or not
                    if (this.txtStartDate.Text != "" && this.txtEndDate.Text != "" && this.txtRate.Text != "")
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
                                string query = "select DATE_FORMAT(Date,'%d-%b-%y') as `Date`"
                                            +" ,case "
                                           +" when cr.partnername is null "
                                           +" Then 'Roaming' "
                                           +" ELSE "
                                           +" cr.PartnerName "
                                           +" END as 'ANS Name'"
                                           +",cricx.partnername as IOS"
                                           +" ,cr1.PartnerName as 'Egress Carrier'"
                                           +" ,cc.Name as Country"
                                           +",concat(matchedprefixy,' (',ic.Description,')') as Destination"
                                           +" ,MatchedPrefixY as Prefix"
                                           +" , CallCount as 'No. Of Calls'"
                                           +" ,round(ActualDuration,2) as 'Actual Minute'"
                                           +" ,round(RoundedDuration,2) as 'Billing Minute'"
                                           +" ,rate as 'Carrier Rate ($)'"
                                           +" , YRate as 'Y Rate ($)'"
                                           + " ,round((YRate-rate),4) as 'Rate difference'"
                                           +" ,round(.15*(x-'"+ this.txtRate.Text+"'*y),2) as 'DBL Protion (BDT)'"
                                           +" , round((Y*'"+ this.txtRate.Text+"')+.15*(x-'"+ this.txtRate.Text+"'*y),2) as 'Invoice Amount' "
                                           +" from"
                                           +" (select date(starttime) as `Date`"
                                           +" ,ansidorig"
                                           +" ,Count(*) as CallCount"
                                           +" ,matchedprefixy"
                                           +" ,sum(Duration3)/60 ActualDuration"
                                           +" ,sum(roundedduration)/60 RoundedDuration"
                                           +" , sum(XAmount) as X"
                                           +" , sum(YAmount) as Y"
                                           +" ,CustomerRate as 'YRate'"
                                           +" ,customerid"
                                           +" ,OutgoingRoute"
                                           +" ,SupplierRate as rate"
                                           +" ,SupplierID"
                                           +"  from cdr c" 
                                           +" where ServiceGroup=5" 
                                           +" and chargingstatus=1 "
                                           +" and customerid='"+ this._isoId+"'"
                                           +" and starttime>='"+ this.txtStartDate.Text+" 00:00:00'" 
                                           +" and starttime<='"+ this.txtEndDate.Text+" 23:59:59'"
                                           +" group by ansidorig,matchedprefixy"
                                           +" ,customerid"
                                           +" ,date(starttime)"
                                           +" ) x" 
                                           +" left join partner cr"
                                           +" on x.ansidorig=cr.idpartner" 
                                           +" left join partner cr1"
                                           +" on x.SupplierID=cr1.idpartner" 
                                           +" left join partner cricx"
                                           +" on x.customerid=cricx.idpartner"
                                           +" left join xyzprefix ic "
                                           +" on x.matchedprefixy=ic.Prefix" 
                                           +" left join CountryCode cc"
                                           +" on ic.CountryCode=cc.Code"
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
                        else if (this.dpdIsoName.SelectedValue == "All IOS" || this.dpdIsoName.SelectedValue == "Select IOS Name")        // Show data for all IOS
                        {
                            //Connection to mysql
                            MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["Reader"].ToString());

                            con.Open();

                            using (MySqlCommand cmd = con.CreateCommand())
                            {                              
                                string query = "select DATE_FORMAT(Date,'%d-%b-%y') as `Date`"
                                            + " ,case "
                                           + " when cr.partnername is null "
                                           + " Then 'Roaming' "
                                           + " ELSE "
                                           + " cr.PartnerName "
                                           + " END as 'ANS Name'"
                                           + ",cricx.partnername as IOS"
                                           + " ,cr1.PartnerName as 'Egress Carrier'"
                                           + " ,cc.Name as Country"
                                           + ",concat(matchedprefixy,' (',ic.Description,')') as Destination"
                                           + " ,MatchedPrefixY as Prefix"
                                           + " , CallCount as 'No. Of Calls'"
                                           + " ,round(ActualDuration,2) as 'Actual Minute'"
                                           + " ,round(RoundedDuration,2) as 'Billing Minute'"
                                           + " ,rate as 'Carrier Rate ($)'"
                                           + " , YRate as 'Y Rate ($)'"
                                           + " ,round((YRate-rate),4) as 'Rate difference'"
                                           + " ,round(.15*(x-'" + this.txtRate.Text + "'*y),2) as 'DBL Protion (BDT)'"
                                           + " , round((Y*'" + this.txtRate.Text + "')+.15*(x-'" + this.txtRate.Text + "'*y),2) as 'Invoice Amount' "
                                           + " from"
                                           + " (select date(starttime) as `Date`"
                                           + " ,ansidorig"
                                           + " ,Count(*) as CallCount"
                                           + " ,matchedprefixy"
                                           + " ,sum(Duration3)/60 ActualDuration"
                                           + " ,sum(roundedduration)/60 RoundedDuration"
                                           + " , sum(XAmount) as X"
                                           + " , sum(YAmount) as Y"
                                           + " ,CustomerRate as 'YRate'"
                                           + " ,customerid"
                                           + " ,OutgoingRoute"
                                           + " ,SupplierRate as rate"
                                           + " ,SupplierID"
                                           + "  from cdr c"
                                           + " where ServiceGroup=5"
                                           + " and chargingstatus=1 "
                                           + " and starttime>='"+ this.txtStartDate.Text+" 00:00:00'"
                                           + " and starttime<='"+ this.txtEndDate.Text+" 23:59:59'"
                                           + " group by ansidorig,matchedprefixy"
                                           + " ,customerid"
                                           + " ,date(starttime)"
                                           + " ) x"
                                           + " left join partner cr"
                                           + " on x.ansidorig=cr.idpartner"
                                           + " left join partner cr1"
                                           + " on x.SupplierID=cr1.idpartner"
                                           + " left join partner cricx"
                                           + " on x.customerid=cricx.idpartner"
                                           + " left join xyzprefix ic "
                                           + " on x.matchedprefixy=ic.Prefix"
                                           + " left join CountryCode cc"
                                           + " on ic.CountryCode=cc.Code"
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



        // Show data on Gridview
        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            CountryWiseAnsInvoiceBind();
        }


        // Gridview Pagging
        protected void PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            CountryWiseAnsInvoiceBind();
        }


        // Export to CSV
        protected void btnExporttoCSV_Click(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.Buffer = true;
            this.Response.AddHeader("content-disposition", "attachment;filename=gvtocsv.csv");
            this.Response.Charset = "";
            this.Response.ContentType = "application/text";

            this.GridView.AllowPaging = false;
            CountryWiseAnsInvoiceBind();

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
    }
}