using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using ExportToExcel;
using LibraryExtensions;
using MySql.Data.MySqlClient;
using PortalApp.ReportHelper;
using reports;

namespace PortalApp.reports
{
	public partial class PaymentHistory : System.Web.UI.Page
	{
	    protected void ShowReport_Click(object sender, EventArgs e)
	    {
	        GridView1.Columns[0].Visible = CheckBoxDailySummary.Checked;
	        GridView1.Columns[1].Visible = CheckBoxPartner.Checked;

            using (MySqlConnection connection = new MySqlConnection())
	        {
	            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;
	            connection.Open();
	            MySqlCommand cmd = new MySqlCommand(GetQuery(), connection);
	            cmd.Connection = connection;
	            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
	            DataSet dataset = new DataSet();
	            da.Fill(dataset);


	            GridView1.DataSource = dataset;
	            bool hasRows = dataset.Tables.Cast<DataTable>()
	                .Any(table => table.Rows.Count != 0);
	            if (hasRows == true)
	            {
	                GridView1.ShowFooter = true;//set it here before setting footer text, setting this to true clears already set footer text
                    Label1.Text = "";
	                Button1.Visible = true; //show export

	                TrafficReportDatasetBased tr = new TrafficReportDatasetBased(dataset);
	                tr.Ds = dataset;
	                foreach (DataRow dr in tr.Ds.Tables[0].Rows)
	                {
	                    tr.CallStat.PartnerCost += tr.ForceConvertToDouble(dr["Amount"]);
                    }
	                tr.CallStat.PartnerCost = Math.Round(tr.CallStat.PartnerCost, 2);

                    //display summary information in the footer
                    Dictionary<string, dynamic> fieldSummaries = new Dictionary<string, dynamic>();//key=colname,val=colindex in grid
	                fieldSummaries.Add("amount", tr.CallStat.PartnerCost);
	                tr.FieldSummaries = fieldSummaries;

                    Session["PaymentHistory"] = tr;//save to session

	                //populate footer
	                //clear first
	                bool captionSetForTotal = false;
	                for (int c = 0; c < GridView1.Columns.Count; c++)
	                {
	                    GridView1.Columns[c].FooterText = "";
	                }
	                for (int c = 0; c < GridView1.Columns.Count; c++)
	                {
	                    if (captionSetForTotal == false && GridView1.Columns[c].Visible == true)
	                    {
	                        GridView1.Columns[c].FooterText = "Total: ";//first visible column
	                        captionSetForTotal = true;
	                    }
	                    string key = GridView1.Columns[c].SortExpression.ToLower();
	                    if (key == "") continue;
	                    if (tr.FieldSummaries.ContainsKey(key))
	                    {
	                        GridView1.Columns[c].FooterText += (tr.GetDataColumnSummary(key)).ToString();//+ required to cocat "Total:"
	                    }
	                }
	                GridView1.DataBind();//call it here after setting footer, footer text doesn't show sometime otherwise, may be a bug
	                GridView1.ShowFooter = true;//don't set it now, set before footer text setting, weird! it clears the footer text
	                //hide filters...
                    Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDivSubmit();", true);
	                hidValueSubmitClickFlag.Value = "false";

	            }//if has rows

	            else
	            {
	                GridView1.DataBind();
	                Label1.Text = "No Data!";
	                Button1.Visible = false; //hide export
	            }

	        }//using mysql connection
	    }

        protected void Export_Click(object sender, EventArgs e)
	    {
	        if (Session["PaymentHistory"] != null) //THIS MUST BE CHANGED IN EACH PAGE
	        {
	            TrafficReportDatasetBased tr = (TrafficReportDatasetBased)Session["PaymentHistory"];
	            DataSetWithGridView dsG = new DataSetWithGridView(tr, GridView1);//invisible baseColumns are removed in constructor
	            CreateExcelFileAspNet.CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(tr.Ds, "PaymentHistory_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
	                                                                                                + ".xlsx", Response);
	        }
	    }

        protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
        {
            DropDownListPartner.Enabled = CheckBoxPartner.Checked;
        }

	    private string GetQuery()
	    {

	        string startDate = txtDate.Text;
	        string endtDate = (txtDate1.Text.ConvertToDateTimeFromMySqlFormat()).AddSeconds(1).ToMySqlFormatWithoutQuote();
	        string tableName = "acc_temp_transaction";
	        string groupInterval = GetSelectedRadioButtonText();

            string constructedSql = new SqlHelperPaymentHistory
            (startDate,
	            endtDate,
                groupInterval,
	            tableName,

	            new List<string>()
	            {
	                getInterval(groupInterval),
                    "uom", 
                    CheckBoxPartner.Checked==true?"idPartner":string.Empty,
	            },
	            new List<string>()
	            {
	                CheckBoxPartner.Checked==true?DropDownListPartner.SelectedIndex>0?" idPartner="+DropDownListPartner.SelectedValue:string.Empty:string.Empty,
	            }).getSQLString();


	        return constructedSql;
	    }

	    private string getInterval(string groupInterval)
	    {
	        switch (groupInterval)
	        {
	            case "Daily":
	                return "transactionTime";
	            case "Monthly":
	                return "concat(year(transactionTime),'-',date_format(transactionTime,'%b'))";
	            case "Yearly":
	                return "DATE_FORMAT(transactionTime,'%Y')";
	            default:
	                return string.Empty;
	        }
	    }

        public string GetSelectedRadioButtonText()
	    {
	        if (CheckBoxDailySummary.Checked)
	        {
	            string interval = "";
	            if (RadioButtonDaily.Checked)
	                return interval = "" + RadioButtonDaily.Text;
	            else if (RadioButtonMonthly.Checked)
	                return interval = "" + RadioButtonMonthly.Text;
	            else if (RadioButtonYearly.Checked)
	                return interval = "" + RadioButtonYearly.Text;
	            else
	                return "";
	        }
	        else return string.Empty;
	    }
	}
}