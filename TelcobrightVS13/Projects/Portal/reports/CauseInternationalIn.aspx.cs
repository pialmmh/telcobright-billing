using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class DefaultCauseIntlIn : Page
{
    
    DataTable _dt;

    protected void submit_Click(object sender, EventArgs e)
    {


        if (this.CheckBoxShowByAns.Checked == true)
        {
            this.GridView1.Columns[3].Visible = true;
        }
        else this.GridView1.Columns[3].Visible = false;

        if (this.CheckBoxShowByIgw.Checked == true)
        {
            this.GridView1.Columns[2].Visible = true;
        }
        else this.GridView1.Columns[2].Visible = false;

        if (this.CheckBoxPartner.Checked == true)
        {
            this.GridView1.Columns[1].Visible = true;
        }
        else this.GridView1.Columns[1].Visible = false;
        if (this.CheckBoxShowCost.Checked == true)
        {
            this.GridView1.Columns[9].Visible = true;
            this.GridView1.Columns[10].Visible = true;
            this.GridView1.Columns[11].Visible = true;
            this.GridView1.Columns[12].Visible = true;
            this.GridView1.Columns[13].Visible = false;
            this.GridView1.Columns[14].Visible = true;
        }
        else
        {
            this.GridView1.Columns[9].Visible = false;
            this.GridView1.Columns[10].Visible = false;
            this.GridView1.Columns[11].Visible = false;
            this.GridView1.Columns[12].Visible = false;
            this.GridView1.Columns[13].Visible = false;
            this.GridView1.Columns[14].Visible = false;
        }
        if (this.CheckBoxShowPerformance.Checked == true)
        {
            this.GridView1.Columns[15].Visible = true;
            this.GridView1.Columns[16].Visible = true;
            this.GridView1.Columns[17].Visible = true;
            //GridView1.Columns[18].Visible = true;
            
        }
        else
        {
            this.GridView1.Columns[15].Visible = false;
            this.GridView1.Columns[16].Visible = false;
            this.GridView1.Columns[17].Visible = false;
            //GridView1.Columns[18].Visible = false;
            
        }

        if (this.CheckBoxDailySummary.Checked == true)
        {
            this.GridView1.Columns[19].Visible = false;//cause code percentage
            this.GridView1.Columns[20].Visible = false;//view calls
        }
        else
        {
            this.GridView1.Columns[19].Visible = true;//cause code percentage
            this.GridView1.Columns[20].Visible = true;//view calls
        }


        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            connection.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;

            //IF GROUP BY INTERNATIONAL PARTNER=FALSE
            if (this.CheckBoxPartner.Checked == false)
            {
                //if don't group by ANS
                if (this.CheckBoxShowByAns.Checked == false)
                {
                    //if don't group by IGW : NoAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntNoAnsNoIgw (@p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);

                    }
                    //if don't group by ANS, group by IGW ALL : NoAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntNoAnsGroupIGWAll (@p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                    }
                    //if don't group by ANS, group by IGW One : NoAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntNoAnsGroupIGWOne (@p_StartDateTime,@p_EndDateTime,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if don't group by ans

                //if group by ANS ALL
                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                {
                    //if don't group by IGW : AllAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntAllAnsNoIgw (@p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);

                    }
                    //if group by ALL ANS, IGW ALL : IncIntAllAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntAllAnsGroupIGWAll (@p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                    }
                    //if group by ANS ALL,IGW One : IncIntAllAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntAllAnsGroupIGWOne (@p_StartDateTime,@p_EndDateTime,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if group by ans ALL

                //if group by ANS One
                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                {
                    //if don't group by IGW : OneAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntOneAnsNoIGW (@p_StartDateTime,@p_EndDateTime,@p_AnsId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));

                    }
                    //if group by One ANS, IGW ALL : IncIntAllAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntOneAnsGroupIGWAll (@p_StartDateTime,@p_EndDateTime,@p_AnsId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                    }
                    //if group by ANS One,IGW One : IncIntOneAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntOneAnsGroupIGWOne (@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if group by ans One


            }//IF GROUP BY INTERNATIONAL PARTNER=FALSE

            //IF GROUP BY INTERNATIONAL PARTNER ALL
            if ((this.CheckBoxPartner.Checked == true) && this.DropDownListPartner.SelectedValue == "-1")
            {
                //if don't group by ANS
                if (this.CheckBoxShowByAns.Checked == false)
                {
                    //if don't group by IGW : NoAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllNoAnsNoIgw (@p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);

                    }
                    //if don't group by ANS, group by IGW ALL : NoAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllNoAnsGroupIGWAll (@p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                    }
                    //if don't group by ANS, group by IGW One : NoAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllNoAnsGroupIGWOne (@p_StartDateTime,@p_EndDateTime,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if don't group by ans

                //if group by ANS ALL
                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                {
                    //if don't group by IGW : AllAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllAllAnsNoIgw (@p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);

                    }
                    //if group by ALL ANS, IGW ALL : IncIntPartnerAllAllAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllAllAnsGroupIGWAll (@p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                    }
                    //if group by ANS ALL,IGW One : IncIntPartnerAllAllAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllAllAnsGroupIGWOne (@p_StartDateTime,@p_EndDateTime,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if group by ans ALL

                //if group by ANS One
                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                {
                    //if don't group by IGW : OneAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllOneAnsNoIGW (@p_StartDateTime,@p_EndDateTime,@p_AnsId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));

                    }
                    //if group by One ANS, IGW ALL : IncIntPartnerAllAllAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllOneAnsGroupIGWAll (@p_StartDateTime,@p_EndDateTime,@p_AnsId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                    }
                    //if group by ANS One,IGW One : IncIntPartnerAllOneAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerAllOneAnsGroupIGWOne (@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if group by ans One


            }//IF GROUP BY INTERNATIONAL PARTNER ALL

            //IF GROUP BY INTERNATIONAL PARTNER One
            if ((this.CheckBoxPartner.Checked == true) && this.DropDownListPartner.SelectedValue != "-1")
            {
                //if don't group by ANS
                if (this.CheckBoxShowByAns.Checked == false)
                {
                    //if don't group by IGW : NoAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneNoAnsNoIgw (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);

                    }
                    //if don't group by ANS, group by IGW ALL : NoAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneNoAnsGroupIGWAll (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                    }
                    //if don't group by ANS, group by IGW One : NoAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneNoAnsGroupIGWOne (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if don't group by ans

                //if group by ANS ALL
                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                {
                    //if don't group by IGW : AllAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneAllAnsNoIgw (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);

                    }
                    //if group by ALL ANS, IGW ALL : IncIntPartnerOneAllAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneAllAnsGroupIGWAll (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                    }
                    //if group by ANS ALL,IGW One : IncIntPartnerOneAllAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneAllAnsGroupIGWOne (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if group by ans ALL

                //if group by ANS One
                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                {
                    //if don't group by IGW : OneAnsNoIgw
                    if (this.CheckBoxShowByIgw.Checked == false)
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneOneAnsNoIGW (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime,@p_AnsId)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));

                    }
                    //if group by One ANS, IGW ALL : IncIntPartnerOneAllAnsGroupIGWAll
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneOneAnsGroupIGWAll (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime,@p_AnsId)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                    }
                    //if group by ANS One,IGW One : IncIntPartnerOneOneAnsGroupIGWOne
                    else if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                    {
                        
                            cmd.CommandText = "CALL CauseIncIntPartnerOneOneAnsGroupIGWOne (@p_IntlPartner, @p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";
                        

                        cmd.Parameters.AddWithValue("p_IntlPartner", this.DropDownListPartner.SelectedValue);

                        cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                        cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                        cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                        cmd.Parameters.AddWithValue("p_IgwId", Int32.Parse(this.DropDownListIgw.SelectedValue));
                    }

                } //if group by ans One


            }//IF GROUP BY INTERNATIONAL PARTNER One


            //Common Code**************#########################################
            this.Label1.Text = "";
            //GRID VIEW SUM HOLDER, DECLARE TO HANDLE MAX 100 COLUMNS
            decimal[] sumOfGridColumns = new decimal[100];
            int i = 0;
            for (i = 0; i < 100; i++) sumOfGridColumns[i] = 0; //initialize to 0


            if (this.CheckBoxDailySummary.Checked == false)
            {
                this.GridView1.Columns[0].Visible = false;
               
            }
            else
            {
                this.GridView1.Columns[0].Visible = true;

            }

            //common code
            if (this.CheckBoxDailySummary.Checked == true)
            {
                string summaryInterval = "";
                if (this.RadioButtonHalfHourly.Checked == true)
                {
                    summaryInterval = "Halfhourly";
                    this.GridView1.Columns[0].HeaderText = "Half Hour";
                }
                else if (this.RadioButtonHourly.Checked == true)
                {
                    summaryInterval = "Hourly";
                    this.GridView1.Columns[0].HeaderText = "Hour";
                }
                else if (this.RadioButtonDaily.Checked == true)
                {
                    summaryInterval = "Daily";
                    this.GridView1.Columns[0].HeaderText = "Date";
                }
                else if (this.RadioButtonWeekly.Checked == true)
                {
                    summaryInterval = "Weekly";
                    this.GridView1.Columns[0].HeaderText = "Week";
                }
                else if (this.RadioButtonMonthly.Checked == true)
                {
                    summaryInterval = "Monthly";
                    this.GridView1.Columns[0].HeaderText = "Month";
                }
                else if (this.RadioButtonYearly.Checked == true)
                {
                    summaryInterval = "Yearly";
                    this.GridView1.Columns[0].HeaderText = "Year";
                }

                cmd.CommandText = cmd.CommandText.Replace("CALL ", "CALL " + summaryInterval + "Summary");

            }

            //source for report... cdr or summary data, not applicable for cause code
            //switch (DropDownListReportSource.SelectedValue)
            //{
            //    case "1"://CDR
            //        break;
            //    case "2"://Summary Data
            //        cmd.CommandText = cmd.CommandText.Replace("CALL ", "CALL SD");
            //        break;
            //    case "3"://Cdr Error
            //        cmd.CommandText = cmd.CommandText.Replace("CALL ", "CALL Err");
            //        break;
            //}
            
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            da.Fill(dataset);

            List<int> columnSortList = new List<int>();
            List<string> colNamelist = new List<string>();

            this._dt = dataset.Tables[0];
            this.Session["CauseInternationalIn.aspx.csdt16"] = this._dt; //THIS MUST BE CHANGED FOR EACH PAGE

            //suppress hidden baseColumns of the gridview
            int cnt=0;
            int thisColumnIndexDataGrid = -1;
            colNamelist = new List<string>();
            for(cnt=0;cnt< this.GridView1.Columns.Count;cnt++)
            {
                thisColumnIndexDataGrid = -1;
                int count = 0;
                for (count = 0; count < this._dt.Columns.Count; count++)
                {
                    if (this.GridView1.Columns[cnt].SortExpression.ToLower() == this._dt.Columns[count].ColumnName.ToLower())
                    {
                        thisColumnIndexDataGrid = count;
                        
                        if (this.GridView1.Columns[cnt].Visible == true)
                        {
                            columnSortList.Add(count);
                            colNamelist.Add(this.GridView1.Columns[cnt].HeaderText);   
                        }
                        break;
                    }
                }
            }


            this.Session["CauseInternationalIn.aspx.csdt26"] = colNamelist;//THIS MUST BE CHANGED FOR EACH PAGE
            //now add sort order to override the column list in the dataset and to match gridview setting


            this.Session["CauseInternationalIn.aspx.csdt36"] = columnSortList;

            this.GridView1.DataSource = dataset;
            bool hasRows = dataset.Tables.Cast<DataTable>()
                               .Any(table => table.Rows.Count != 0);
            //cause code count
            string fieldNameForCount = "costansin";//change
            int noOfRowsSuccess = 0;
            int noOfRowsFailed = 0;
            foreach (DataRow row in this._dt.Rows)
            {
                if (row["callstatus"].ToString() == "1")//change
                {
                    noOfRowsSuccess += int.Parse(row[fieldNameForCount].ToString());
                }
                else
                {
                    noOfRowsFailed += int.Parse(row[fieldNameForCount].ToString());
                }
            }
            this.Session["CauseInternationalIn.aspx.NoOfRowsSuccess"] = noOfRowsSuccess.ToString();//change
            this.Session["CauseInternationalIn.aspx.NoOfRowsFailed"] = noOfRowsFailed.ToString();//change

            this.GridView1.DataBind();
            if (hasRows == true)
            {
                this.Label1.Text = "";
                this.Button1.Visible = true; //show export

                //common code for report pages
                //hide filters...
                this.Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDivSubmit();", true);
                this.hidValueSubmitClickFlag.Value = "false";


            }//if has rows

            else
            {
                this.GridView1.DataBind();
                this.Label1.Text = "No Data!";
                this.Button1.Visible = false; //hide export
            }

            }//using mysql connection
        

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        DataTable dt2;
        List<string> colNameList=new List<string>();
        List<int> columnSortList=new List<int>();
        if (this.Session["CauseInternationalIn.aspx.csdt16"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            dt2 = (DataTable) this.Session["CauseInternationalIn.aspx.csdt16"];//THIS MUST BE CHANGED IN EACH PAGE
            
            if (this.Session["CauseInternationalIn.aspx.csdt26"] != null) //THIS MUST BE CHANGED IN EACH PAGE
            {
                colNameList = (List<string>) this.Session["CauseInternationalIn.aspx.csdt26"];//THIS MUST BE CHANGED IN EACH PAGE
            }

            if (this.Session["CauseInternationalIn.aspx.csdt36"] != null) //THIS MUST BE CHANGED IN EACH PAGE
            {
                columnSortList = (List<int>) this.Session["CauseInternationalIn.aspx.csdt36"];//THIS MUST BE CHANGED IN EACH PAGE
            }
            ExportToSpreadsheet(dt2, "Cause Code_International Incoming",colNameList,columnSortList); //THIS MUST BE CHANGED IN EACH PAGE
            this.Session.Abandon();
        }
    }

    public void ExportToSpreadsheet(DataTable table, string name, List<string> colNameList, List<int> columnSortlist)
    {
        HttpContext context = HttpContext.Current;
        context.Response.Clear();

        string thisRow = "";

        //foreach (DataColumn column in table.Columns)
        //{

        //    ThisRow += column.ColumnName + ",";
        //}
        //header row
        //write baseColumns in order specified in ColumnSortedList
        int ii = 0;
        for (ii=0; ii<colNameList.Count;ii++ )
        {  
            //ThisRow +=  table.Columns[ColumnSortlist[ii]].ColumnName + ",";
            thisRow += colNameList[ii]+ ",";
        }
        //and cause code desc and %
        thisRow += "Descriptin, Status Group %" + ",";
        thisRow = thisRow.Substring(0, thisRow.Length - 1) + Environment.NewLine;
        context.Response.Write(thisRow);

        //int RowIndex=0;
        //foreach (DataRow row in table.Rows)
        //{
        //    ThisRow = "";
        //    for (int i = 0; i < table.Columns.Count; i++)
        //    {
        //        ThisRow += row[i].ToString().Replace(",", string.Empty) + ",";
        //    }
            
        //    ThisRow = ThisRow.Substring(0, ThisRow.Length - 1) + Environment.NewLine;
        //    context.Response.Write(ThisRow);
        //    RowIndex++;
        //}


        int rowIndex = 0;
        foreach (DataRow row in table.Rows)
        {
            thisRow = "";
            for (ii = 0; ii < columnSortlist.Count; ii++) //for each column
            {  
                thisRow += row[columnSortlist[ii]].ToString().Replace(",", string.Empty) + ",";
            }
            //and cause code desc and %
            thisRow += ((Label) this.GridView1.Rows[rowIndex].FindControl("lblDescription")).Text + "," +
                ((Label) this.GridView1.Rows[rowIndex].FindControl("lblPercentage")).Text.Replace("%","") + ",";
            thisRow = thisRow.Substring(0, thisRow.Length - 1) + Environment.NewLine;
            context.Response.Write(thisRow);
            rowIndex++;
        }

        //context.Response.ContentType = "text/csv";
        //context.Response.ContentType = "application/vnd.ms-excel";
        context.Response.ContentType = "application/ms-excel";
        context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + name + ".csv");
        context.Response.End();
    }

    protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxPartner.Checked == true)
        {
            this.DropDownListPartner.Enabled = true;
        }
        else this.DropDownListPartner.Enabled = false;
    }


    protected void CheckBoxShowByAns_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxShowByAns.Checked == true)
        {
            this.DropDownListAns.Enabled = true;
        }
        else this.DropDownListAns.Enabled = false;
    }
    protected void CheckBoxShowByIgw_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxShowByIgw.Checked == true)
        {
            this.DropDownListIgw.Enabled = true;
        }
        else this.DropDownListIgw.Enabled = false;
    
    }
    protected void CheckBoxShowCost_CheckedChanged(object sender, EventArgs e)
    {
        //if (CheckBoxShowCost.Checked == true)
        //{
        //    DropDownListIgw.Enabled = true;
        //}
        //else DropDownListIgw.Enabled = false;

    }
    protected void CheckBoxShowPerformance_CheckedChanged(object sender, EventArgs e)
    {
        //if (CheckBoxShowByIgw.Checked == true)
        //{
        //    DropDownListIgw.Enabled = true;
        //}
        //else DropDownListIgw.Enabled = false;

    }
    protected void ButtonSummary_Click(object sender, EventArgs e)
    {

    }
    protected void Send(object sender, EventArgs e)
    {

        if (this.GridView1.SelectedRow != null)
        {
            int rowindex = this.GridView1.SelectedIndex;
            int fieldcount = this.GridView1.Columns.Count;
            //string str=GridView1.Rows[rowindex].Cells[1].Text;
            //HiddenField hdn = new HiddenField();
            //hdn.Value="

            //Header row
            GridViewRow headerRow = this.GridView1.HeaderRow;
            int headercount = headerRow.Cells.Count;
            ArrayList headarr = new ArrayList();
            for (int a = 0; a < headercount; a++)
            {
                headarr.Add(headerRow.Cells[a].Text);
            }

            //Data row
            GridViewRow row = this.GridView1.Rows[rowindex];
            string strr = row.Cells[2].Text;

            int count = row.Cells.Count;
            ArrayList arr = new ArrayList();
            for (int i = 0; i < count; i++)
            {
                arr.Add(row.Cells[i].Text);
            }
            //Server.Transfer("CauseInternationalOutCallView.aspx");
            //string strfrmtarget = Form.Target;
            //if (strfrmtarget != "_blank")
            //{
            //    Form.Target = "_blank";
            //}
           // Session["CauseInternationalIn.aspx.csgridcountry"] = row.Cells[1].Text;
           // Session["CauseInternationalIn.aspx.csgriddestination"] = row.Cells[2].Text;
            this.Session["CauseInternationalIn.aspx.csgridans"] = row.Cells[3].Text;
            this.Session["CauseInternationalIn.aspx.csgridicx"] = row.Cells[2].Text;
            this.Session["CauseInternationalIn.aspx.csgridpartner"] = row.Cells[1].Text;
            this.Session["CauseInternationalIn.aspx.csgrIdCallsstatus"] = row.Cells[5].Text;
            this.Session["CauseInternationalIn.aspx.csgridcause"] = row.Cells[6].Text;
            this.Session["CauseInternationalIn.aspx.csgridcausecodecount"] = row.Cells[8].Text;
            this.Session["CauseInternationalIn.aspx.csstartdate"] = this.txtDate.Text;
            this.Session["CauseInternationalIn.aspx.csenddate"] = this.txtDate1.Text;
            /*
            string url = "CauseInternationalInCallView.aspx";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<script type = 'text/javascript'>");
            //sb.Append("document.forms[0].target = '_blank';");
            sb.Append("window.open('");
            sb.Append(url);
            sb.Append("');");
            //sb.Append("document.forms[0].target = '_self';");
            sb.Append("</script>");
            ClientScript.RegisterStartupScript(this.GetType(),
                    "script", sb.ToString());
            */
            this.Server.Transfer("CauseInternationalInCallView.aspx");


        }
        else
        {
            this.ClientScript.RegisterStartupScript(GetType(), "alert", "alert('Please select a row.')", true);
        }
    }
    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //disable view calls for now when time based summary enabled...
            if (this.CheckBoxDailySummary.Checked == true)
            {
                this.GridView1.Columns[19].Visible = false;
            }
            else
            {
                //on client click for call view
                LinkButton lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonViewCalls");
                string queryString = "gridans=" + (e.Row.Cells[3].Text != "" ? e.Row.Cells[3].Text : "null") + "&" +
                    "gridicx=" + (e.Row.Cells[2].Text != "" ? e.Row.Cells[2].Text : "null") + "&" +
                    "gridpartner=" + (e.Row.Cells[1].Text != "" ? e.Row.Cells[1].Text : "null") + "&" +
                    "grIdCallsstatus=" + (e.Row.Cells[5].Text != "" ? e.Row.Cells[5].Text : "null") + "&" +
                    "gridcause=" + (e.Row.Cells[6].Text != "" ? e.Row.Cells[6].Text : "null") + "&" +
                    "gridcausecodecount=" + (e.Row.Cells[8].Text != "" ? e.Row.Cells[8].Text : "null") + "&" +
                    "gridcountry=null" + "&" +
                    "griddestination=null" + "&" +
                    "startdate=" + this.txtDate.Text + "&" +
                    "enddate=" + this.txtDate1.Text;
                lnkBtn.OnClientClick = "changecolor(this.id);window.open('CauseInternationalInCallView.aspx?" + queryString + "');return false;";
 
            }
            //cause code description
            Label lblDescription = (Label)e.Row.FindControl("lblDescription");
            long thisCauseCode = -1;
            long thisSwitchId = -1;
            long.TryParse(DataBinder.Eval(e.Row.DataItem,"CauseCode").ToString(),out thisCauseCode);
            long.TryParse(DataBinder.Eval(e.Row.DataItem, "SwitchId").ToString(), out thisSwitchId);
            if (thisCauseCode != -1&&thisSwitchId!=-1)
            {
                string thisDescription = "";
                Dictionary<string, string> dicCauseCode = (Dictionary<string, string>) this.Session["dicCauseCode"];
                dicCauseCode.TryGetValue(thisSwitchId.ToString() + "-" + thisCauseCode.ToString(), out thisDescription);
                lblDescription.Text = thisDescription;
            }

            //cause code percentage
            int callStatus = -1;
            int.TryParse(DataBinder.Eval(e.Row.DataItem, "callstatus").ToString(), out callStatus);
            Label lblPercentage = (Label)e.Row.FindControl("lblPercentage");
            long thisCodeCount = 0;
            long.TryParse(DataBinder.Eval(e.Row.DataItem, "costansin").ToString(), out thisCodeCount);//change

            if (callStatus == 1)
            {
                double totalCountSuccess = double.Parse(this.Session["CauseInternationalIn.aspx.NoOfRowsSuccess"].ToString());//change
                lblPercentage.Text = string.Format("{0:0.##}", Math.Round((100 * thisCodeCount / totalCountSuccess), 2).ToString()) + " %";
            }
            else
            {
                double totalCountFailed = double.Parse(this.Session["CauseInternationalIn.aspx.NoOfRowsFailed"].ToString());//change
                lblPercentage.Text = string.Format("{0:0.##}", Math.Round((100 * thisCodeCount / totalCountFailed), 2).ToString()) + " %";
            }

        }
    }//row data bound
}
