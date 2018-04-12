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


public partial class DefaultCauseIntlOut : Page
{
    DataTable _dt;
    bool _timerflag = false;
    protected void submit_Click(object sender, EventArgs e)
    {
        //view by country/prefix logic has been changed later, adding this new flag
        bool notViewingByCountry = this.CheckBoxShowByDestination.Checked | (!this.CheckBoxShowByCountry.Checked);//not viewing by country if view by desination is checked
        
        //undo the effect of hiding some grid by the summary button first******************
        this.GridView1.Columns[0].Visible = true;
        this.GridView1.Columns[1].Visible = true;
        //GridView1.Columns[2].Visible = false;
        //GridView1.Columns[3].Visible = false;
        //GridView1.Columns[4].Visible = false;
        //*****************************

        if (this.CheckBoxShowByCountry.Checked == true)
        {
            this.GridView1.Columns[2].Visible = false;
        }
        else this.GridView1.Columns[2].Visible = true;

        if (this.CheckBoxShowByAns.Checked == true)
        {
            this.GridView1.Columns[3].Visible = true;
        }
        else this.GridView1.Columns[3].Visible = false;

        if (this.CheckBoxShowByIgw.Checked == true)
        {
            this.GridView1.Columns[4].Visible = true;
        }
        else this.GridView1.Columns[4].Visible = false;

        if (this.CheckBoxIntlPartner.Checked == true)
        {
            this.GridView1.Columns[5].Visible = true;
        }
        else this.GridView1.Columns[5].Visible = false;

        if (this.CheckBoxDailySummary.Checked == true)
        {
            this.GridView1.Columns[11].Visible = false;//cause code percentage
            this.GridView1.Columns[12].Visible = false;//view calls
        }
        else
        {
            this.GridView1.Columns[11].Visible = true;//cause code percentage
            this.GridView1.Columns[12].Visible = true;//view calls
        }


        using (MySqlConnection connection = new MySqlConnection())
        {

            connection.ConnectionString = ConfigurationManager.ConnectionStrings["reader"].ConnectionString;

            connection.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;

            //All Possible Report Combinations are here:

            //*********&&&&&&&&&&&&&&&& if checkbox view by destination is checked

            if (this.CheckBoxShowByDestination.Checked == true || this.CheckBoxShowByCountry.Checked == true)
            {
                this.GridView1.Columns[1].Visible = true;//country
                if (this.CheckBoxShowByDestination.Checked)
                    this.GridView1.Columns[2].Visible = true;//destination
                else
                    this.GridView1.Columns[2].Visible = false;


                //IF GROUP BY INTERNATIONAL PARTNER=FALSE
                if (this.CheckBoxIntlPartner.Checked == false)
                {
                    //By Country Only, GROUP BY COUNTRY=ALL
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:1

                                cmd.CommandText = "CALL CauseOutIntCase1 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:2

                                cmd.CommandText = "CALL CauseOutIntCase2 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:3

                                cmd.CommandText = "CALL CauseOutIntCase3 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:4

                                cmd.CommandText = "CALL CauseOutIntCase4 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:5

                                cmd.CommandText = "CALL CauseOutIntCase5 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";

                                //cmd.CommandText = "CALL CauseSummaryOutIntCase5 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:6

                                cmd.CommandText = "CALL CauseOutIntCase6 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:7

                                cmd.CommandText = "CALL CauseOutIntCase7 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:8

                                cmd.CommandText = "CALL CauseOutIntCase8 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:9

                                cmd.CommandText = "CALL CauseOutIntCase9 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ALL

                    //*************************************************************************************
                    //By Country Only, GROUP BY COUNTRY=ONE
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:10

                                cmd.CommandText = "CALL CauseOutIntCase10 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);

                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:11

                                cmd.CommandText = "CALL CauseOutIntCase11 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:12

                                cmd.CommandText = "CALL CauseOutIntCase12 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:13

                                cmd.CommandText = "CALL CauseOutIntCase13 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:14

                                cmd.CommandText = "CALL CauseOutIntCase14 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:15

                                cmd.CommandText = "CALL CauseOutIntCase15 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:16

                                cmd.CommandText = "CALL CauseOutIntCase16 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:17

                                cmd.CommandText = "CALL CauseOutIntCase17 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:18

                                cmd.CommandText = "CALL CauseOutIntCase18 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ONE

                    //********prefix

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutIntCase19 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutIntCase20 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutIntCase21 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutIntCase22 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutIntCase23 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutIntCase24 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutIntCase25 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutIntCase26 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutIntCase27 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutIntCase28 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutIntCase29 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutIntCase30 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutIntCase31 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutIntCase32 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutIntCase33 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutIntCase34 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutIntCase35 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutIntCase36 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }
                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************



                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ONE***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase19 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase20 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase21 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase22 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase23 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase24 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase25 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase26 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase27 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase28 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase29 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase30 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase31 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase32 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase33 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase34 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase35 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutIntOneCountryCase36 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }


                }//IF GROUP BY INTERNATIONAL PARTNER=FALSE


                //IF GROUP BY INTERNATIONAL PARTNER=TRUE AND BY PARTNER ALL
                if ((this.CheckBoxIntlPartner.Checked == true) && (this.DropDownListIntlCarier.SelectedValue == "-1"))
                {
                    //By Country Only, GROUP BY COUNTRY=ALL
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:1

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase1 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:2

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase2 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:3

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase3 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:4

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase4 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:5

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase5 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";

                                cmd.CommandText = "CALL CauseSummaryOutIntPartnerAllCase5 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:6

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase6 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:7

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase7 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:8

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase8 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:9

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase9 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ALL

                    //*************************************************************************************
                    //By Country Only, GROUP BY COUNTRY=ONE
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:10

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase10 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);

                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:11

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase11 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:12

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase12 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:13

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase13 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:14

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase14 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:15

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase15 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:16

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase16 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:17

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase17 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:18

                                cmd.CommandText = "CALL CauseOutIntPartnerAllCase18 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ONE

                    //********prefix

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase19 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase20 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase21 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase22 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase23 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase24 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase25 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase26 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase27 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase28 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase29 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase30 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase31 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase32 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase33 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase34 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase35 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllCase36 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }
                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************



                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ONE***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase19 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase20 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase21 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase22 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase23 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase24 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase25 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase26 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase27 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase28 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase29 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase30 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase31 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase32 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase33 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase34 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase35 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutIntPartnerAllOneCountryCase36 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }


                }//IF GROUP BY INTERNATIONAL PARTNER=TRUE AND BY PARTNER ALL


                //IF GROUP BY INTERNATIONAL PARTNER=TRUE AND BY PARTNER One
                if ((this.CheckBoxIntlPartner.Checked == true) && (this.DropDownListIntlCarier.SelectedValue != "-1"))
                {
                    //By Country Only, GROUP BY COUNTRY=ALL
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:1

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase1 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));
                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:2

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase2 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";



                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));
                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:3

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase3 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));
                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:4

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase4 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:5

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase5 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:6

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase6 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:7

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase7 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:8

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase8 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:9

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase9 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ALL

                    //*************************************************************************************
                    //By Country Only, GROUP BY COUNTRY=ONE
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:10

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase10 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";

                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));
                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);

                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:11

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase11 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:12

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase12 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:13

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase13 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:14

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase14 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:15

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase15 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:16

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase16 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:17

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase17 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:18

                                cmd.CommandText = "CALL CauseOutIntPartnerOneCase18 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ONE

                    //********prefix

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase19 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase20 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase21 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase22 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase23 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase24 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase25 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase26 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase27 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase28 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase29 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase30 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase31 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase32 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase33 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase34 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase35 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneCase36 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }
                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************



                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ONE***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase19 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase20 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase21 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase22 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase23 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase24 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase25 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase26 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase27 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase28 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase29 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase30 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase31 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase32 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase33 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase34 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase35 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutIntPartnerOneOneCountryCase36 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }


                }//IF GROUP BY INTERNATIONAL PARTNER=TRUE AND BY PARTNER One

            }//*********&&&&&&&&&&&&&&&& if checkbox view by destination is checked

            else //no prefix or country wise group by
            {
                this.GridView1.Columns[1].Visible = false;
                this.GridView1.Columns[2].Visible = false;

                //IF GROUP BY INTERNATIONAL PARTNER=FALSE
                if (this.CheckBoxIntlPartner.Checked == false)
                {
                    //By Country Only, GROUP BY COUNTRY=ALL
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:1

                                cmd.CommandText = "CALL CauseOutNoDestIntCase1 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:2

                                cmd.CommandText = "CALL CauseOutNoDestIntCase2 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:3

                                cmd.CommandText = "CALL CauseOutNoDestIntCase3 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:4

                                cmd.CommandText = "CALL CauseOutNoDestIntCase4 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:5

                                cmd.CommandText = "CALL CauseOutNoDestIntCase5 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";

                                cmd.CommandText = "CALL CauseSummaryOutNoDestIntCase5 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:6

                                cmd.CommandText = "CALL CauseOutNoDestIntCase6 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:7

                                cmd.CommandText = "CALL CauseOutNoDestIntCase7 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:8

                                cmd.CommandText = "CALL CauseOutNoDestIntCase8 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:9

                                cmd.CommandText = "CALL CauseOutNoDestIntCase9 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ALL

                    //*************************************************************************************
                    //By Country Only, GROUP BY COUNTRY=ONE
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:10

                                cmd.CommandText = "CALL CauseOutNoDestIntCase10 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);

                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:11

                                cmd.CommandText = "CALL CauseOutNoDestIntCase11 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:12

                                cmd.CommandText = "CALL CauseOutNoDestIntCase12 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:13

                                cmd.CommandText = "CALL CauseOutNoDestIntCase13 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:14

                                cmd.CommandText = "CALL CauseOutNoDestIntCase14 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:15

                                cmd.CommandText = "CALL CauseOutNoDestIntCase15 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:16

                                cmd.CommandText = "CALL CauseOutNoDestIntCase16 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:17

                                cmd.CommandText = "CALL CauseOutNoDestIntCase17 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:18

                                cmd.CommandText = "CALL CauseOutNoDestIntCase18 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ONE

                    //********prefix

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase19 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase20 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase21 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase22 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase23 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase24 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase25 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase26 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase27 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase28 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase29 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase30 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase31 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase32 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase33 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase34 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase35 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutNoDestIntCase36 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }
                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************



                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ONE***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase19 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase20 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase21 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase22 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase23 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase24 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase25 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase26 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase27 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase28 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase29 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase30 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase31 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase32 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase33 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase34 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase35 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutNoDestIntOneCountryCase36 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }


                }//IF GROUP BY INTERNATIONAL PARTNER=FALSE


                //IF GROUP BY INTERNATIONAL PARTNER=TRUE AND BY PARTNER ALL
                if ((this.CheckBoxIntlPartner.Checked == true) && (this.DropDownListIntlCarier.SelectedValue == "-1"))
                {
                    //By Country Only, GROUP BY COUNTRY=ALL
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:1

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase1 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:2

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase2 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:3

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase3 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:4

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase4 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:5

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase5 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";

                                cmd.CommandText = "CALL CauseSummaryOutNoDestIntPartnerAllCase5 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:6

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase6 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";



                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:7

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase7 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:8

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase8 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:9

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase9 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ALL

                    //*************************************************************************************
                    //By Country Only, GROUP BY COUNTRY=ONE
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:10

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase10 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);

                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:11

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase11 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:12

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase12 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:13

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase13 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:14

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase14 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:15

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase15 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:16

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase16 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:17

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase17 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:18

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase18 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ONE

                    //********prefix

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase19 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase20 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase21 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase22 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase23 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase24 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase25 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase26 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase27 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase28 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase29 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase30 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase31 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase32 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase33 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase34 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase35 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllCase36 (@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }
                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************



                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ONE***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase19 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase20 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase21 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase22 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase23 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase24 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase25 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase26 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase27 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase28 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase29 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase30 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase31 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase32 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase33 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase34 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase35 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerAllOneCountryCase36 (@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }


                }//IF GROUP BY INTERNATIONAL PARTNER=TRUE AND BY PARTNER ALL


                //IF GROUP BY INTERNATIONAL PARTNER=TRUE AND BY PARTNER One
                if ((this.CheckBoxIntlPartner.Checked == true) && (this.DropDownListIntlCarier.SelectedValue != "-1"))
                {
                    //By Country Only, GROUP BY COUNTRY=ALL
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:1

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase1 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));
                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:2

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase2 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";



                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));
                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:3

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase3 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));
                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:4

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase4 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:5

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase5 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:6

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase6 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:7

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase7 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:8

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase8 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:9

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase9 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ALL

                    //*************************************************************************************
                    //By Country Only, GROUP BY COUNTRY=ONE
                    if ((this.CheckBoxShowByCountry.Checked == true) && (this.CheckBoxShowByDestination.Checked == false) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //Group by IGW=false
                        if (this.CheckBoxShowByIgw.Checked == false)
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:10

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase10 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);

                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:11

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase11 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:12

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase12 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ALL
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:13

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase13 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:14

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase14 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:15

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase15 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                        //Group by IGW=true, BY IGW ONE
                        if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                        {
                            //Group by Ans=False
                            if (this.CheckBoxShowByAns.Checked == false)
                            {
                                //CASE:16

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase16 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ALL
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                            {
                                //CASE:17

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase17 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                            //Group by Ans=True, BY ANS ONE
                            if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                            {
                                //CASE:18

                                cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase18 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_CountryCode)";


                                cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                            }
                        }
                    }//if BY COUNtRY ONLY, GROUP BY COUNTRY=ONE

                    //********prefix

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue == "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase19 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase20 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase21 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase22 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase23 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase24 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase25 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase26 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase27 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase28 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase29 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase30 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase31 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase32 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase33 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase34 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase35 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneCase36 (@p_IntlPartner,@p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }
                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************



                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ONE***************************
                    if ((notViewingByCountry == true) && (this.DropDownListCountry.SelectedValue != "-1"))
                    {
                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************

                        //By Prefix, GROUP BY PREFIX=ALL
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue == "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:19

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase19 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:20

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase20 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:21

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase21 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:22

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase22 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:23

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase23 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:24

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase24 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:25

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase25 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:26

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase26 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:27

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase27 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ALL

                        //By Prefix, GROUP BY PREFIX=ONE
                        if ((notViewingByCountry == true) && (this.DropDownPrefix.SelectedValue != "-1"))
                        {
                            //Group by IGW=false
                            if (this.CheckBoxShowByIgw.Checked == false)
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:28

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase28 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:29

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase29 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);

                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:30
                                    //CASE:30

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase30 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ALL
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue == "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:31
                                    //CASE:31

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase31 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:32
                                    //CASE:32

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase32 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:33
                                    //CASE:33

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase33 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                            //Group by IGW=true, BY IGW ONE
                            if ((this.CheckBoxShowByIgw.Checked == true) && (this.DropDownListIgw.SelectedValue != "-1"))
                            {
                                //Group by Ans=False
                                if (this.CheckBoxShowByAns.Checked == false)
                                {
                                    //CASE:34
                                    //CASE:34

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase34 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ALL
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue == "-1"))
                                {
                                    //CASE:35
                                    //CASE:35

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase35 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                                //Group by Ans=True, BY ANS ONE
                                if ((this.CheckBoxShowByAns.Checked == true) && (this.DropDownListAns.SelectedValue != "-1"))
                                {
                                    //CASE:36
                                    //CASE:36

                                    cmd.CommandText = "CALL CauseOutNoDestIntPartnerOneOneCountryCase36 (@p_IntlPartner,@p_CountryCode, @p_UsdRateY,@p_StartDateTime,@p_EndDateTime,@p_AnsId,@p_IgwId,@p_MatchedPrefixY)";


                                    cmd.Parameters.AddWithValue("p_IntlPartner", int.Parse(this.DropDownListIntlCarier.SelectedValue));

                                    cmd.Parameters.AddWithValue("p_CountryCode", this.DropDownListCountry.SelectedValue);
                                    cmd.Parameters.AddWithValue("p_UsdRateY", Single.Parse(this.TextBoxUsdRate.Text));
                                    cmd.Parameters.AddWithValue("p_StartDateTime", this.txtDate.Text);
                                    cmd.Parameters.AddWithValue("p_EndDateTime", this.txtDate1.Text);
                                    cmd.Parameters.AddWithValue("p_AnsId", Int32.Parse(this.DropDownListAns.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_IgwId", int.Parse(this.DropDownListIgw.SelectedValue));
                                    cmd.Parameters.AddWithValue("p_MatchedPrefixY", this.DropDownPrefix.SelectedValue);
                                }
                            }
                        }//if BY prefix, GROUP BY PREFIX=ONE

                        //&&&&&&&& NOT GROUP BY COUNTRY BUT PREFIX FOR COUNTRY= ALL***************************
                    }


                }//IF GROUP BY INTERNATIONAL PARTNER=TRUE AND BY PARTNER One

            }




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

            //source for report... cdr or summary data
            switch (this.DropDownListReportSource.SelectedValue)
            {
                case "1"://CDR
                    break;
                case "2"://Summary Data
                    cmd.CommandText = cmd.CommandText.Replace("CALL ", "CALL SD");
                    break;
                case "3"://Cdr Error
                    cmd.CommandText = cmd.CommandText.Replace("CALL ", "CALL Err");
                    break;
            }


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            da.Fill(dataset);

            List<int> columnSortList = new List<int>();
            List<string> colNamelist = new List<string>();

            this._dt = dataset.Tables[0];
            this.Session["CauseInternationalIn.aspx.csdt16"] = this._dt; //THIS MUST BE CHANGED FOR EACH PAGE

            //suppress hidden columns of the gridview
            int cnt = 0;
            int thisColumnIndexDataGrid = -1;
            colNamelist = new List<string>();
            for (cnt = 0; cnt < this.GridView1.Columns.Count; cnt++)
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


            this.Session["CauseInternationalOut.aspx.csdt26"] = colNamelist;//THIS MUST BE CHANGED FOR EACH PAGE
            //now add sort order to override the column list in the dataset and to match gridview setting


            this.Session["CauseInternationalOut.aspx.csdt36"] = columnSortList;

            this._dt = dataset.Tables[0];
            this.Session["CauseInternationalOut.aspx.csdt17"] = this._dt; //THIS MUST BE CHANGED FOR EACH PAGE

            this.GridView1.DataSource = dataset;
            bool hasRows = dataset.Tables.Cast<DataTable>()
                               .Any(table => table.Rows.Count != 0);

            //cause code count
            string fieldNameForCount = "CauseCodeCount";//change
            int noOfRowsSuccess = 0;
            int noOfRowsFailed = 0;
            foreach (DataRow row in this._dt.Rows)
            {
                if (row["CallStatus"].ToString() == "1")
                {
                    noOfRowsSuccess += int.Parse(row[fieldNameForCount].ToString());
                }
                else
                {
                    noOfRowsFailed += int.Parse(row[fieldNameForCount].ToString());
                }
            }
            this.Session["CauseInternationalOut.aspx.NoOfRowsSuccess"] = noOfRowsSuccess.ToString();//change
            this.Session["CauseInternationalOut.aspx.NoOfRowsFailed"] = noOfRowsFailed.ToString();//change

            this.GridView1.DataBind();
            if (hasRows == true)
            {
                this.Label1.Text = "";
                this.Button1.Visible = true; //show export

                //common code for report pages
                //hide filters...
                this.Page.ClientScript.RegisterStartupScript(GetType(), "MyKey", "HideParamBorderDivSubmit();", true);
                this.hidValueSubmitClickFlag.Value = "false";




            }//if has rows=true
            else
            {
                this.GridView1.DataBind();
                this.Label1.Text = "No Data!";
                this.Button1.Visible = false; //hide export
            }





        }//using mysql connection

    }

    


    private void DateInitialize()
    {
        if (this.CheckBoxRealTimeUpdate.Checked)
        {
            long a;
            if (!long.TryParse(this.TextBoxDuration.Text, out a))
            {
                // If Not Integer Clear Textbox text or you can also Undo() Last Operation :)

                this.TextBoxDuration.Text = "30";
                a = 30;
            }

            DateTime endtime = DateTime.Now;
            DateTime starttime = endtime.AddMinutes(a * (-1));
            this.txtDate1.Text = endtime.ToString("dd/MM/yyyy HH:mm:ss");
            this.txtDate.Text = starttime.ToString("dd/MM/yyyy HH:mm:ss");
            //return true;
        }
        else
        {
            this.txtDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.txtDate1.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }
    }

    protected void CheckBoxRealTimeUpdate_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxRealTimeUpdate.Checked)
        {
            //Disable DailySummary,Destination, Dates& Months

            this.CheckBoxDailySummary.Checked = false;
            this.CheckBoxDailySummary.Enabled = false;

            this.CheckBoxShowByDestination.Checked = false;
            //CheckBoxShowByDestination.Enabled = false;

            this.TextBoxYear.Enabled = false;
            this.DropDownListMonth.Enabled = false;
            this.txtDate.Enabled = false;

            this.TextBoxYear1.Enabled = false;
            this.DropDownListMonth1.Enabled = false;
            this.txtDate1.Enabled = false;

            //Enable Timers,Duration,country
            this.CheckBoxShowByCountry.Checked = true;
            this.TextBoxDuration.Enabled = true;
            //TextBoxDuration.Text = "30";
            this._timerflag = true;



            //dateInitialize
        }
        else
        {
            //Enable DailySummary,Destination, Dates& Months

            this.CheckBoxDailySummary.Checked = true;
            this.CheckBoxDailySummary.Enabled = true;

            this.CheckBoxShowByDestination.Checked = true;
            this.CheckBoxShowByDestination.Enabled = true;

            this.TextBoxYear.Enabled = true;
            this.DropDownListMonth.Enabled = true;
            this.txtDate.Enabled = true;

            this.TextBoxYear1.Enabled = true;
            this.DropDownListMonth1.Enabled = true;
            this.txtDate1.Enabled = true;

            //Disable Timers,Duration,
            this.CheckBoxShowByCountry.Checked = false;
            this.TextBoxDuration.Enabled = false;
            //TextBoxDuration.Text = "30";
            this._timerflag = false;
        }
        CheckBoxShowByCountry_CheckedChanged(sender, e);
        DateInitialize();
    }

    protected void TextBoxDuration_TextChanged(object sender, EventArgs e)
    {
        long a;
        if (!long.TryParse(this.TextBoxDuration.Text, out a))
        {
            // If Not Integer Clear Textbox text or you can also Undo() Last Operation :)

            this.TextBoxDuration.Text = "30";
        }
    }
    protected void Timer1_Tick(object sender, EventArgs e)
    {
        if (this.CheckBoxRealTimeUpdate.Checked)
        {
            submit_Click(sender, e);
        }
    }


    protected void Button1_Click(object sender, EventArgs e)
    {
        //DataTable dt2;
        //if (Session["CauseInternationalOut.aspx.csdt17"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        //{
        //    dt2 = (DataTable)Session["CauseInternationalOut.aspx.csdt17"];//THIS MUST BE CHANGED IN EACH PAGE
        //    ExportToSpreadsheet(dt2, "International Outgoing",ColNameList,ColumnSortlist); //THIS MUST BE CHANGED IN EACH PAGE
        //    Session.Abandon();
        //}

        DataTable dt2;
        List<string> colNameList = new List<string>();
        List<int> columnSortList = new List<int>();
        if (this.Session["CauseInternationalOut.aspx.csdt17"] != null) //THIS MUST BE CHANGED IN EACH PAGE
        {
            dt2 = (DataTable) this.Session["CauseInternationalOut.aspx.csdt17"];//THIS MUST BE CHANGED IN EACH PAGE
            if (this.Session["CauseInternationalOut.aspx.csdt26"] != null) //THIS MUST BE CHANGED IN EACH PAGE
            {
                colNameList = (List<string>) this.Session["CauseInternationalOut.aspx.csdt26"];//THIS MUST BE CHANGED IN EACH PAGE
            }

            if (this.Session["CauseInternationalOut.aspx.csdt36"] != null) //THIS MUST BE CHANGED IN EACH PAGE
            {
                columnSortList = (List<int>) this.Session["CauseInternationalOut.aspx.csdt36"];//THIS MUST BE CHANGED IN EACH PAGE
            }
            ExportToSpreadsheet(dt2, "Cause Code_International Outgoing", colNameList, columnSortList); //THIS MUST BE CHANGED IN EACH PAGE
            this.Session.Abandon();
        }

    }

    protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //load prefix dropdown combo
        string prefixFilter = "-1";
        if (this.DropDownListCountry.SelectedValue != "")//avoid executing during initial page load when selected value is not set
        {
            prefixFilter = this.DropDownListCountry.SelectedValue;
        }


        using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["reader"].ConnectionString))
        {
            con.Open();
            using (MySqlCommand cmd = new MySqlCommand("", con))
            {
                cmd.CommandText = "CALL OutgoingPrefix(@p_CountryCode)";
                cmd.Parameters.AddWithValue("p_CountryCode", prefixFilter);

                MySqlDataReader dr = cmd.ExecuteReader();
                this.DropDownPrefix.Items.Clear();
                while (dr.Read())
                {
                    this.DropDownPrefix.Items.Add(new ListItem(dr[1].ToString(), dr[0].ToString()));
                }
            }
        }

    }


    protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxIntlPartner.Checked == true)
        {
            this.DropDownListIntlCarier.Enabled = true;
            //GridView1.Columns[3].Visible = true;
        }
        else
        {
            this.DropDownListIntlCarier.Enabled = false;
            this.DropDownListIntlCarier.SelectedIndex = 0;
            //GridView1.Columns[3].Visible = false;
        }
    }


    protected void CheckBoxShowByAns_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxShowByAns.Checked == true)
        {
            this.DropDownListAns.Enabled = true;
            //GridView1.Columns[3].Visible = true;
        }
        else
        {
            this.DropDownListAns.Enabled = false;
            this.DropDownListAns.SelectedIndex = 0;
            //GridView1.Columns[3].Visible = false;
        }
    }
    protected void CheckBoxShowByIgw_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxShowByIgw.Checked == true)
        {
            this.DropDownListIgw.Enabled = true;
            //GridView1.Columns[4].Visible = true;
        }
        else
        {
            this.DropDownListIgw.Enabled = false;
            this.DropDownListIgw.SelectedIndex = 0;
            //GridView1.Columns[4].Visible = false;
        }
    }
    protected void CheckBoxShowByCountry_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxShowByCountry.Checked == true)
        {
            this.DropDownPrefix.SelectedIndex = 0;
            this.DropDownPrefix.Enabled = false;

            this.DropDownListCountry.SelectedIndex = 0;
            this.DropDownListCountry.Enabled = true;
            //GridView1.Columns[2].Visible = false; //prefix

            this.CheckBoxShowByDestination.Checked = false;

        }
        else
        {
            //DropDownPrefix.Enabled = true;
            //GridView1.Columns[2].Visible = true;
            this.DropDownListCountry.Enabled = false;

        }



    }

    protected void CheckBoxShowByDestination_CheckedChanged(object sender, EventArgs e)
    {
        if (this.CheckBoxShowByDestination.Checked == true)
        {
            this.CheckBoxShowByCountry.Checked = false;

            this.DropDownPrefix.SelectedIndex = 0;
            this.DropDownPrefix.Enabled = true;

            this.DropDownListCountry.SelectedIndex = 0;
            this.DropDownListCountry.Enabled = false;

            // GridView1.Columns[1].Visible = false;//country
            //  GridView1.Columns[2].Visible = true;//destination
        }
        else
        {
            //DropDownPrefix.Enabled = false;
            //GridView1.Columns[2].Visible = false;
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
        //write columns in order specified in ColumnSortedList
        int ii = 0;
        for (ii = 0; ii < colNameList.Count; ii++)
        {
            //ThisRow +=  table.Columns[ColumnSortlist[ii]].ColumnName + ",";
            thisRow += colNameList[ii] + ",";
        }
        //and cause code desc and %
        thisRow += "Descriptin, Status Group %" + ",";
        thisRow = thisRow.Substring(0, thisRow.Length - 1) + Environment.NewLine;
        context.Response.Write(thisRow);
        
        //int RowIndex = 0;
        //foreach (DataRow row in table.Rows)
        //{
        //    ThisRow = "";
        //    for (int i = 0; i < table.Columns.Count; i++)
        //    {

        //        ThisRow += row[i].ToString().Replace(",", string.Empty) + ",";
        //        //context.Response.Write(row[i].ToString());
        //        //context.Response.Write(row[i]);
        //    }
        //    //and cause code desc and %
        //    ThisRow += ((Label)GridView1.Rows[RowIndex].FindControl("lblDescription")).Text + "," +
        //        ((Label)GridView1.Rows[RowIndex].FindControl("lblPercentage")).Text.Replace("%", "") + ",";
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
                ((Label) this.GridView1.Rows[rowIndex].FindControl("lblPercentage")).Text.Replace("%", "") + ",";
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


    protected void LinkButtonViewCalls_Click(object sender, EventArgs e)
    {
        //String cmdarg=sende
        int a = 0;
    }
    protected void Sendu()
    {
        if (this.GridView1.SelectedRow != null)
        {
            int rowindex = this.GridView1.SelectedIndex;
            int fieldcount = this.GridView1.Columns.Count;
        }
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

            this.Session["CauseInternationalOut.aspx.csgridcountry"] = row.Cells[1].Text;
            this.Session["CauseInternationalOut.aspx.csgriddestination"] = row.Cells[2].Text;
            this.Session["CauseInternationalOut.aspx.csgridans"] = row.Cells[3].Text;
            this.Session["CauseInternationalOut.aspx.csgridicx"] = row.Cells[4].Text;
            this.Session["CauseInternationalOut.aspx.csgridpartner"] = row.Cells[5].Text;
            this.Session["CauseInternationalOut.aspx.csgridcallsstatus"] = row.Cells[6].Text;
            this.Session["CauseInternationalOut.aspx.csgridcause"] = row.Cells[7].Text;
            this.Session["CauseInternationalOut.aspx.csgridcausecodecount"] = row.Cells[8].Text;
            this.Session["CauseInternationalOut.aspx.csstartdate"] = this.txtDate.Text;
            this.Session["CauseInternationalOut.aspx.csenddate"] = this.txtDate1.Text;
            if (row.Cells[2].Text != "")
            {
                this.Session["CauseInternationalOut.aspx.csgridcountry"] = "";
            }
            /*
            string url = "CauseInternationalOutCallView.aspx";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<script type = 'text/javascript'>");
            //sb.Append("document.forms[0].target = '_blank';");
            sb.Append("window.open('");
            sb.Append(url);
            sb.Append("','_blank');");
            //sb.Append("document.forms[0].target = '_self';");
            sb.Append("</script>");
            ClientScript.RegisterStartupScript(this.GetType(),
                    "script", sb.ToString());
            */
            this.Server.Transfer("CauseInternationalOutCallView.aspx");


        }
        else
        {
            this.ClientScript.RegisterStartupScript(GetType(), "alert", "alert('Please select a row.')", true);
        }
    }


    protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //GridViewRow row = GridView1.SelectedRow;
        //string arr0 = row.Cells[0].Text;
        //string arr1 = row.Cells[1].Text;
        //string arr2 = row.Cells[2].Text;
        //string arr3 = row.Cells[3].Text;
        //string arr4 = row.Cells[4].Text;
        //string arr5 = row.Cells[5].Text;
        //int count = row.Cells.Count;
        //ArrayList arr = new ArrayList();
        //for (int i = 0; i < count; i++)
        //{
        //    arr.Add(row.Cells[i].Text);
        //}
        // Send(sender, e);
    }
    protected void GridView1_SelectedIndexChanging(object sender, GridViewSelectEventArgs e)
    {
        //GridViewRow row = GridView1.Rows[e.NewSelectedIndex];
        //int count = row.Cells.Count;
        //ArrayList arr=new ArrayList();
        //for(int i=0;i<count;i++)
        //{
        //    arr.Add(row.Cells[i].Text);
        //}
        //string arr0 = row.Cells[0].Text;
        //string arr1 = row.Cells[1].Text;
        //string arr2 = row.Cells[2].Text;
        //string arr3 = row.Cells[3].Text;
        //string arr4 = row.Cells[4].Text;
        //string arr5 = row.Cells[5].Text;
    }
    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {

            ////disable view calls for now when time based summary enabled...
            //if (CheckBoxDailySummary.Checked == true)
            //{
            //    GridView1.Columns[19].Visible = false;
            //}
            //else
            //{
                //on client click for call view
                LinkButton lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonViewCalls");
                string queryString = "gridans=" + (e.Row.Cells[3].Text != "" ? e.Row.Cells[3].Text : "null") + "&" +
                    "gridicx=" + (e.Row.Cells[4].Text != "" ? e.Row.Cells[4].Text : "null") + "&" +
                    "gridpartner=" + (e.Row.Cells[5].Text != "" ? e.Row.Cells[5].Text : "null") + "&" +
                    "gridcallsstatus=" + (e.Row.Cells[6].Text != "" ? e.Row.Cells[6].Text : "null") + "&" +
                    "gridcause=" + (e.Row.Cells[7].Text != "" ? e.Row.Cells[7].Text : "null") + "&" +
                    "gridcausecodecount=" + (e.Row.Cells[9].Text != "" ? e.Row.Cells[9].Text : "null") + "&" +
                    "gridcountry=" + (e.Row.Cells[1].Text != "" ? e.Row.Cells[1].Text : "null") + "&" +
                    "griddestination=" + (e.Row.Cells[2].Text != "" ? e.Row.Cells[2].Text : "null") + "&" +
                    "startdate=" + this.txtDate.Text + "&" +
                    "enddate=" + this.txtDate1.Text;
                lnkBtn.OnClientClick = "changecolor(this.id);window.open('CauseInternationalOutCallView.aspx?" + queryString + "');return false;";

            //}

            //cause code description
            Label lblDescription = (Label)e.Row.FindControl("lblDescription");
            long thisCauseCode = -1;
            long thisSwitchId = -1;
            long.TryParse(DataBinder.Eval(e.Row.DataItem, "CauseCode").ToString(), out thisCauseCode);
            long.TryParse(DataBinder.Eval(e.Row.DataItem, "SwitchId").ToString(), out thisSwitchId);
            if (thisCauseCode != -1 && thisSwitchId != -1)
            {
                string thisDescription = "";
                Dictionary<string, string> dicCauseCode = (Dictionary<string, string>) this.Session["dicCauseCode"];
                dicCauseCode.TryGetValue(thisSwitchId.ToString() + "-" + thisCauseCode.ToString(), out thisDescription);
                lblDescription.Text = thisDescription;
            }

            //cause code percentage
            int callStatus = -1;
            int.TryParse(DataBinder.Eval(e.Row.DataItem, "CallStatus").ToString(), out callStatus);
            Label lblPercentage = (Label)e.Row.FindControl("lblPercentage");
            long thisCodeCount = 0;
            long.TryParse(DataBinder.Eval(e.Row.DataItem, "CauseCodeCount").ToString(), out thisCodeCount);

            if (callStatus == 1)
            {
                double totalCountSuccess = double.Parse(this.Session["CauseInternationalOut.aspx.NoOfRowsSuccess"].ToString());//change
                lblPercentage.Text = string.Format("{0:0.##}", Math.Round( (100 * thisCodeCount / totalCountSuccess),2).ToString()) + " %";
            }
            else
            {
                double totalCountFailed = double.Parse(this.Session["CauseInternationalOut.aspx.NoOfRowsFailed"].ToString());//change
                lblPercentage.Text = string.Format("{0:0.##}", Math.Round((100 * thisCodeCount / totalCountFailed), 2).ToString()) + " %";
            }
            
        }//if datarow
    }
}
