using MediationModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using CronExpressionDescriptor;
//using System.Reflection;
using PortalApp;

using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using LibraryExtensions;
using TelcobrightMediation.Accounting;
//using System.Configuration;
using System.Web.Script.Serialization;
using PortalApp;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.MefData.GenericAssignment;
using Newtonsoft.Json;
using Excel = Microsoft.Office.Interop.Excel;

public partial class config_SupplierRatePlanDetailRateAssign : System.Web.UI.Page
{
    Dictionary<int, billingruleassignment> dicBillRules = new Dictionary<int, billingruleassignment>();


    TelcobrightConfig Tbc = null;
    static List<BillingRule> billingRules;
    private PartnerEntities Context = new PartnerEntities();

    public void populateDropDownForBillingRule()
    {

        //  List<>
        DropDownList ruleddl = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownBillingRule");
        DropDownList cyclelistddl = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListBillingCycle");
        DropDownList ddlserviceGroup = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListServiceGroup");

        using (PartnerEntities context = new PartnerEntities())
        {
            billingRules = context.jsonbillingrules.ToList().Select(c => JsonConvert.DeserializeObject<BillingRule>(c.JsonExpression)).ToList();

        }
        ruleddl.Items.Clear();
        ruleddl.Items.Add(new ListItem("[Select]", "-1"));
        cyclelistddl.Items.Clear();
        cyclelistddl.Items.Add(new ListItem("[Select]", "-1"));
        foreach (BillingRule jsb in billingRules)
        {
            ruleddl.Items.Add(new ListItem(jsb.RuleName, jsb.Id.ToString()));
            cyclelistddl.Items.Add(new ListItem(jsb.Description, jsb.Id.ToString()));
        }

        string thisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

        MySqlConnection connection = new MySqlConnection(thisConectionString);
        string database = connection.Database.ToString();
        List<ne> lstSwitch;
        using (PartnerEntities context = new PartnerEntities())
        {
            telcobrightpartner thisCustomer = context.telcobrightpartners.First(c => c.databasename == database);
            int thisOperatorId = thisCustomer.idCustomer;
            lstSwitch = context.nes.Where(c => c.idCustomer == thisOperatorId).ToList();
            ddlserviceGroup.Items.Clear();
            ddlserviceGroup.Items.Add(new ListItem(" [Select]", "-1"));
            ServiceGroupPopulatorForDropDown.Populate(ddlserviceGroup, this.Tbc);
        }
    }



    private void PopulateDropdownlistService(ServiceGroupConfiguration serviceGroupConfig)
    {
        List<int> configuredSfIdsForThisServiceGroup = serviceGroupConfig.Ratingtrules.Select(c => c.IdServiceFamily)
            .ToList();
        using (PartnerEntities context = new PartnerEntities())
        {
            var lstrules = context.enumservicefamilies
                .Where(c => configuredSfIdsForThisServiceGroup.Contains(c.id)).ToList();
            DropDownList dropservice = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListservice");
            dropservice.Items.Clear();
            dropservice.Items.Add(new ListItem(" [Select]", "-1"));
            foreach (enumservicefamily thisRule in lstrules)
            {
                dropservice.Items.Add(new ListItem(thisRule.ServiceName, thisRule.id.ToString()));
            }
        }
    }

    protected void DropDownListServiceGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
        int serviceGroupId = Convert.ToInt32(((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListServiceGroup")).SelectedItem.Value);
        if (serviceGroupId == -1)
        {
            DropDownList dropservice = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListservice");
            dropservice.Items.Clear();
        }
        else
        {
            PopulateDropdownlistService(Tbc.CdrSetting.ServiceGroupConfigurations[serviceGroupId]);
        }

    }

    protected void DropDownBillingRule_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ruleddl = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownBillingRule");
        DropDownList cyclelistddl = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListBillingCycle");

        if (ruleddl.SelectedItem.Text == "Prepaid")
        {
            cyclelistddl.Enabled = true;
        }
        else
        {
            cyclelistddl.Enabled = false;
        }
    }


    enum OverlapTypes
    {
        None,
        OverlapByCoincide,
        OverlappingNext,
        OverlappingBoth
    }
    enum RatePositions
    {
        NotSet,
        FirstEver,
        BeforeAll,
        Coincide,
        InBetween,
        Latest,
        Existing

        // * First Ever
        // B4 All          Coincide     In between    Existing                   Last       
        //    *       |-------*------|      *           |*----------------|----------*------------|
        //          1 Jan          5 Jan              18 Jan           20 Jan                 9999-12-31

        //for all position--> check current rate does not overlap next rates start date
        // Coincide--> update previous rate's end date
        //Existing--> end date of current rate has to be <= end date of existing rate, update existing rate's end date
        // if current rate's end date < existing rate

    }




    class RatePositioning
    {

        public int idRateTask;
        public RatePositions ThisPosition;
        public OverlapTypes ThisOverLapType;
        public int RateStatus;
        public rateassign ThisRate;

        public rateassign PresentRate;
        public rateassign PrevRate;
        public rateassign NextRate;
        public bool Overlap;
        public bool ParameterConflict;
        public bool AutoAdjust;
        public DateTime FutureDate = new DateTime(9999, 12, 31, 23, 59, 59);


        public RatePositioning(rateassign pPresentRate, rateassign pPrevRate, rateassign pNextRate, rateassign pThisRate, bool pAutoAdjust)
        {
            try
            {
                PresentRate = pPresentRate;
                PrevRate = pPrevRate;
                NextRate = pNextRate;
                ThisRate = pThisRate;
                ThisRate.Status = 0;//initialize
                AutoAdjust = pAutoAdjust;
                ThisPosition = RatePositions.NotSet;//initialize.
                ThisOverLapType = OverlapTypes.None;
                //replace NULL for enddate field if any of these rates with the future date e.g. '9999-12-31' for simplicity of
                //comparison

                if (PresentRate != null)
                {
                    if (PresentRate.enddate == null || (PresentRate.enddate == new DateTime(1, 1, 1, 0, 0, 0)))
                    {
                        PresentRate.enddate = FutureDate;
                    }
                }
                if (PrevRate != null)
                {
                    if (PrevRate.enddate == null || (PrevRate.enddate == new DateTime(1, 1, 1, 0, 0, 0)))
                    {
                        PrevRate.enddate = FutureDate;
                    }
                }
                if (NextRate != null)
                {
                    if (NextRate.enddate == null || (NextRate.enddate == new DateTime(1, 1, 1, 0, 0, 0)))
                    {
                        NextRate.enddate = FutureDate;
                    }
                }
                if (ThisRate != null)
                {
                    if (ThisRate.enddate == null || (ThisRate.enddate == new DateTime(1, 1, 1, 0, 0, 0)))//c# set null date as 1-1-1 00:00:00
                    {
                        ThisRate.enddate = FutureDate;
                    }
                }

                //check for code delete
                if (ThisRate.rateamount == -1)
                {
                    ThisRate.Status = 1;//code delete
                    return;
                }


                //check for each rate position one at a time
                if (Existing() == false)//Parameter conflict flag is also set within this
                    if (Coincide() == false)
                        if (Latest() == false)
                            if (FirstEver() == false)
                                if (BeforeAll() == false)
                                    InBetween();


                Overlap = GetOverLaps();
                //ParameterConflict= GetParamConflict(); this flag is set within existing
                //check all possible status, if match Status field will be set like  new, delete, increase/decrease etc... e.g. >0
                SetRateStatus();

            }
            catch (Exception e1)
            {
                throw new Exception("Error occured during RateTask creation!");
            }


        }


        public bool SetRateStatus()
        {
            //<asp:ListItem Value="-1">All</asp:ListItem>
            //<asp:ListItem Value="0">Validation Errors</asp:ListItem>
            //<asp:ListItem Value="1">Code Deletes</asp:ListItem>
            //<asp:ListItem Value="2">New Codes</asp:ListItem>//status
            //<asp:ListItem Value="3">Increase</asp:ListItem>//status
            //<asp:ListItem Value="4">Decrease</asp:ListItem>//status
            //<asp:ListItem Value="5">Unchanged</asp:ListItem>//status
            //<%--<asp:ListItem Value="6">Errors</asp:ListItem>--%>
            //<asp:ListItem Value="7">Complete</asp:ListItem>
            //<asp:ListItem Value="8">Change Uncommitted</asp:ListItem>
            //<asp:ListItem Value="9">Overlap</asp:ListItem>//status
            //<asp:ListItem Value="10">Overlap Adjusted</asp:ListItem>//status
            //<asp:ListItem Value="11">Rate Amount Conflict</asp:ListItem>//status
            //<asp:ListItem Value="12">Rate Position Not Found</asp:ListItem> //status
            //<asp:ListItem Value="13">Existing</asp:ListItem> //status

            //status 0 will be considered as un determined status
            try
            {
                ////if (ParameterConflict == true) currently not implemented
                ////{
                ////    ThisRate.Status = 11;//rate amt conflict
                ////    return true;
                ////}
                if (ThisRate.rateamount == -1)
                {
                    ThisRate.Status = 1;//code delete
                    return true;
                }
                else if (Overlap == true && AutoAdjust == false)
                {
                    ThisRate.Status = 9;//overlap
                    return true;
                }
                else if (Overlap == true && AutoAdjust == true)
                {
                    ThisRate.Status = 10;//overlap adjusted
                    return true;
                }
                else // no overlap or rate conflict
                {
                    switch (ThisPosition)
                    {
                        case RatePositions.NotSet:
                            ThisRate.Status = 12;//rate position not found
                            break;
                        case RatePositions.FirstEver:
                        case RatePositions.BeforeAll:
                            ThisRate.Status = 2;//new
                            break;
                        case RatePositions.Coincide:
                            if (AutoAdjust == true)
                            {
                                ThisRate.Status = 10;//overlap adjusted
                            }
                            else
                            {
                                ThisRate.Status = 9;//overlap
                            }
                            break;
                        case RatePositions.Existing:
                            ThisRate.Status = 13;//existing
                            break;
                        case RatePositions.InBetween:
                        case RatePositions.Latest:
                            if (ThisRate.enddate == FutureDate && NextRate != null)
                            {
                                ThisRate.enddate = NextRate.startdate;
                            }
                            if ((ThisRate.startdate == PrevRate.enddate)
                                || (PrevRate.enddate == new DateTime(9999, 12, 31, 23, 59, 59)))//this rate is continuous with previous rate without any pause
                            {
                                if (ThisRate.rateamount == PrevRate.rateamount)
                                {
                                    ThisRate.Status = 5;//unchanged
                                }
                                else if (ThisRate.rateamount > PrevRate.rateamount)
                                {
                                    ThisRate.Status = 3;//increase
                                }
                                else if (ThisRate.rateamount < PrevRate.rateamount)
                                {
                                    ThisRate.Status = 4;//decrease
                                }
                            }
                            else
                            {
                                ThisRate.Status = 2;//new
                            }
                            break;

                    }//switch
                }
                return true;
            }
            catch (Exception e1)
            {
                return false;//conservative on exceptions
            }
        }


        bool GetOverLaps()
        {
            bool Overlap = false;
            try
            {
                List<string> lstOverlaps = new List<string>();
                if (NextRate != null)
                {
                    if ((ThisRate.enddate > NextRate.startdate) && (ThisRate.enddate < FutureDate)) //First check if overlaps next rate
                    {
                        //ThisRate.OverlappingRates = NextRate.id.ToString();
                        lstOverlaps.Add(NextRate.id.ToString());
                        ThisOverLapType = OverlapTypes.OverlappingNext;
                        Overlap = true;
                    }
                }
                if (ThisPosition == RatePositions.Coincide)//if coincide
                {

                    lstOverlaps.Add(PrevRate.id.ToString());
                    if (ThisOverLapType == OverlapTypes.OverlappingNext)
                    {
                        //already overlapping next, now found that it also coincides
                        ThisOverLapType = OverlapTypes.OverlappingBoth;
                    }
                    else if (ThisOverLapType == OverlapTypes.None)
                    {
                        ThisOverLapType = OverlapTypes.OverlapByCoincide;
                    }
                    Overlap = true;
                }
                ThisRate.OverlappingRates = string.Join(",", lstOverlaps.ToArray());

                return Overlap;
            }
            catch (Exception e1)
            {
                return true;//conservative on exceptions
            }
        }

        bool GetParamConflict()
        {
            try
            {
                //find out if this rate is existing already but has a rate conflict
                if (ParameterConflict == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e1)
            {
                return true;//conservative on exceptions
            }
        }

        bool Coincide()
        {
            try
            {
                if (PrevRate != null)
                {
                    if (ThisRate.startdate < PrevRate.enddate) //if overlaps by coinciding previous rate
                    {
                        if (PrevRate.enddate < FutureDate)//overlap only if prevrate's end date is < futuredate or not null e.g. has really an end date
                        {
                            ThisPosition = RatePositions.Coincide;
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e1)
            {
                return false;
            }
        }


        bool Existing()
        {
            try
            {
                //find out if this rate is existing already
                if (PresentRate != null)
                {
                    //bool ParamConflict = true;
                    //if (ThisRate.description == PresentRate.description)
                    //    if (ThisRate.rateamount == PresentRate.rateamount)
                    //        if (ThisRate.Resolution == PresentRate.Resolution)
                    //            if (ThisRate.MinDurationSec == PresentRate.MinDurationSec)
                    //                if (ThisRate.SurchargeTime == PresentRate.SurchargeTime)
                    //                    if (ThisRate.SurchargeAmount == PresentRate.SurchargeAmount)
                    //                        if (ThisRate.CountryCode == PresentRate.CountryCode)
                    //                            //if (ThisRate.date1 == PresentRate.date1)//date is not used in the application
                    //                                //if (ThisRate.field1 == PresentRate.field1)
                    //                                    //if (ThisRate.field2 == PresentRate.field2)
                    //                                        if (ThisRate.field3 == PresentRate.field3)
                    //                                            if (ThisRate.field4 == PresentRate.field4)
                    //                                                if (ThisRate.field5 == PresentRate.field5)
                    //                                                    if (ThisRate.startdate == PresentRate.startdate)
                    //                                                        if (ThisRate.enddate == PresentRate.enddate)
                    //                                                            if (ThisRate.Inactive == PresentRate.Inactive)
                    //                                                                if (ThisRate.Type == PresentRate.Type)
                    //                                                                    if (ThisRate.Currency == PresentRate.Currency)
                    //                                                                        if (ThisRate.OtherAmount1 == PresentRate.OtherAmount1)
                    //                                                                            if (ThisRate.OtherAmount2 == PresentRate.OtherAmount2)
                    //                                                                                if (ThisRate.OtherAmount3 == PresentRate.OtherAmount3)
                    //                                                                                    if (ThisRate.OtherAmount4 == PresentRate.OtherAmount4)
                    //                                                                                        if (ThisRate.OtherAmount5 == PresentRate.OtherAmount5)
                    //                                                                                            if (ThisRate.OtherAmount6 == PresentRate.OtherAmount6)
                    //                                                                                                if (ThisRate.OtherAmount7 == PresentRate.OtherAmount7)
                    //                                                                                                    if (ThisRate.OtherAmount8 == PresentRate.OtherAmount8)
                    //                                                                                                        if (ThisRate.OtherAmount9 == PresentRate.OtherAmount9)
                    //                                                                                                            if (ThisRate.OtherAmount10 == PresentRate.OtherAmount10)
                    //                                                                                                                if (ThisRate.IgwPercentageIn == PresentRate.IgwPercentageIn)
                    //                                                                                                                {
                    //                                                                                                                    ParamConflict = false;
                    //                                                                                                                }
                    //if (ParamConflict == true)
                    //{
                    //    ParameterConflict = true;
                    //}
                    //existing rate
                    ThisPosition = RatePositions.Existing;
                    return true;
                }
                ParameterConflict = false;//if there is no present rate, no param conflict ***NOT USED CURRENTLY***
                return false;
            }
            catch (Exception e1)
            {
                return false;
            }
        }



        bool FirstEver()
        {
            try
            {
                if (PrevRate == null && NextRate == null)//first ever
                {
                    ThisPosition = RatePositions.FirstEver;
                    return true;
                }
                return false;
            }
            catch (Exception e1)
            {
                return false;
            }
        }

        bool BeforeAll()
        {
            try
            {
                if (PrevRate == null && NextRate != null)//before all,but there is a next rate
                {
                    ThisPosition = RatePositions.BeforeAll;
                    return true;
                }
                return false;
            }
            catch (Exception e1)
            {
                return false;
            }
        }


        bool Latest()
        {
            try
            {
                if (PrevRate != null)
                {
                    if ((ThisRate.startdate < PrevRate.enddate) && (PrevRate.enddate == FutureDate)
                        && NextRate == null)//if prev rate is open
                    {
                        //prevrate's end date is == futuredate == null means this rate is after all rates
                        ThisPosition = RatePositions.Latest;
                        return true;
                    }
                    else if ((ThisRate.startdate >= PrevRate.enddate) &&
                             (PrevRate.enddate < FutureDate) && NextRate == null)//prev rate has end date
                    {
                        //prevrate's end date is != futuredate != null, has a valid end date
                        //thisrate's start has to be >= prevrate.enddate
                        ThisPosition = RatePositions.Latest;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e1)
            {
                return false;
            }
        }

        bool InBetween()
        {
            try
            {
                if (PrevRate != null && NextRate != null)
                {
                    if (ThisRate.startdate >= PrevRate.enddate && ThisRate.startdate <= NextRate.startdate)
                    {
                        //in between
                        ThisPosition = RatePositions.InBetween;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e1)
            {
                return false;
            }
        }

        int SetStatus(rateassign PrevRate, rateassign ThisRate)
        {
            try
            {
                //<asp:ListItem Value="-1">All</asp:ListItem>                      
                //<asp:ListItem Value="0">Errors</asp:ListItem>            
                //<asp:ListItem Value="1">Code Deletes</asp:ListItem>      
                //<asp:ListItem Value="2">New Codes</asp:ListItem>         
                //<asp:ListItem Value="3">Increase</asp:ListItem>          
                //<asp:ListItem Value="4">Decrease</asp:ListItem>          
                //<asp:ListItem Value="5">Unchanged</asp:ListItem>         
                //<%--<asp:ListItem Value="6">Errors</asp:ListItem>--%>    
                //<asp:ListItem Value="7">Complete</asp:ListItem>  
                //<asp:ListItem Value="8">Change Uncommitted</asp:ListItem>
                //<asp:ListItem Value="9">Overlap</asp:ListItem>           
                //<asp:ListItem Value="10">Overlap Adjusted</asp:ListItem> 
                //<asp:ListItem Value="11">Rate Position Not Found</asp:ListItem> 
                if (ThisRate.rateamount == -1)
                {
                    return -1;//code delete
                }
                if (ThisRate.rateamount == PrevRate.rateamount)
                {
                    return 5;//unchanged
                }
                else if (ThisRate.rateamount > PrevRate.rateamount)
                {
                    return 3;//increase
                }
                else if (ThisRate.rateamount < PrevRate.rateamount)
                {
                    return 4;//decrease
                }
                return 0;
            }
            catch (Exception e1)
            {
                return 0;
            }
        }

    }


    class CodeUpdate
    {
        public countrycode Country = new countrycode();
        public DateTime? CodeEndingDate;
    }

    class RateTask
    {
        List<CodeUpdate> lstCodeUpdate = new List<CodeUpdate>();
    }

    public bool myIsNumeric(string str)
    {
        double x;
        if (double.TryParse(str, out x))
        {
            return true;
        }
        return false;
    }


    protected void EditTaskRefName_Click(object sender, EventArgs e)
    {
        //exit if cancel clicked in javascript...
        if (hidValueRefName.Value == null || hidValueRefName.Value == "")
        {
            return;
        }

        //check for duplicate templatename and alert the client...
        string Description = hidValueRefName.Value;
        if (Description == "")
        {
            string script = "alert('Name cannot be empty!');";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
            return;
        }
        else if (Description.IndexOf('=') >= 0 || Description.IndexOf(':') >= 0 ||
                 Description.IndexOf(',') >= 0 || Description.IndexOf('?') >= 0)
        {
            string script = "alert('Name cannot contain characters =:,?');";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
            return;
        }
        using (PartnerEntities context = new PartnerEntities())
        {
            int idRatePlan = int.Parse(ViewState["sesidRatePlan"].ToString());
            if (context.ratetaskreferences.Any(c => c.Description == Description && c.idRatePlan == idRatePlan))
            {
                string script = "alert('name: " + Description + " exists, try a different name.');";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                return;
            }
        }

        //rename here
        int idRatePlanThis = int.Parse(ViewState["sesidRatePlan"].ToString());
        int SelectedRefsId = int.Parse(DropDownListTaskRef.SelectedValue);
        int SelectedRefsIndex = DropDownListTaskRef.SelectedIndex;

        using (PartnerEntities context = new PartnerEntities())
        {
            ratetaskreference ThisRef = context.ratetaskreferences.Where(c => c.id == SelectedRefsId).FirstOrDefault();
            if (ThisRef != null)
            {
                ThisRef.Description = Description;
                context.SaveChanges();
            }
            List<ratetaskreference> LstTaskRef = new List<ratetaskreference>();
            LstTaskRef = context.ratetaskreferences.Where(c => c.idRatePlan == idRatePlanThis).OrderByDescending(c => c.id).ToList();
            DropDownListTaskRef.Items.Clear();
            foreach (ratetaskreference TR in LstTaskRef)
            {
                DropDownListTaskRef.Items.Add(new ListItem(TR.Description, TR.id.ToString()));
            }
            DropDownListTaskRef.SelectedIndex = SelectedRefsIndex;//only if not post back
            myGridViewDataBind();
        }


    }


    protected void NewTaskRefName_Click(object sender, EventArgs e)
    {
        //exit if cancel clicked in javascript...
        if (hidValueRefName.Value == null || hidValueRefName.Value == "")
        {
            return;
        }

        //check for duplicate templatename and alert the client...
        string Description = hidValueRefName.Value;
        if (Description == "")
        {
            string script = "alert('Name cannot be empty!');";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
            return;
        }
        else if (Description.IndexOf('=') >= 0 || Description.IndexOf(':') >= 0 ||
                 Description.IndexOf(',') >= 0 || Description.IndexOf('?') >= 0)
        {
            string script = "alert('Name cannot contain characters =:,?');";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
            return;
        }
        using (PartnerEntities context = new PartnerEntities())
        {
            int idRatePlan = int.Parse(ViewState["sesidRatePlan"].ToString());
            if (context.ratetaskreferences.Any(c => c.Description == Description && c.idRatePlan == idRatePlan))
            {
                string script = "alert('name: " + Description + " exists, try a different name.');";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "Alert", script, true);
                return;
            }
        }

        //create new ratetaskref here...
        int idRatePlanThis = int.Parse(ViewState["sesidRatePlan"].ToString());
        //int SelectedRefsId = int.Parse(DropDownListTaskRef.SelectedValue);
        //int SelectedRefsIndex = DropDownListTaskRef.SelectedIndex;

        using (PartnerEntities context = new PartnerEntities())
        {
            ratetaskreference ThisRef = new ratetaskreference();

            ThisRef.Description = Description;
            ThisRef.idRatePlan = idRatePlanThis;
            context.ratetaskreferences.Add(ThisRef);
            context.SaveChanges();

            List<ratetaskreference> LstTaskRef = new List<ratetaskreference>();
            LstTaskRef = context.ratetaskreferences.Where(c => c.idRatePlan == idRatePlanThis).OrderByDescending(c => c.id).ToList();
            DropDownListTaskRef.Items.Clear();
            foreach (ratetaskreference TR in LstTaskRef)
            {
                DropDownListTaskRef.Items.Add(new ListItem(TR.Description, TR.id.ToString()));
            }
            DropDownListTaskRef.SelectedIndex = 0;//just added, latest
            myGridViewDataBind();
        }
    }

    //[System.Web.Services.WebMethod]
    public string CodeDeleteExists()
    {
        int RateTaskRefId = int.Parse(System.Web.HttpContext.Current.Session["assign.vsRateTaskId"].ToString());
        using (PartnerEntities Context = new PartnerEntities())
        {
            if (Context.ratetasks.Any(c => (c.idrateplan == RateTaskRefId) && (c.rateamount == "-1")))
            {
                return "1";
            }
        }
        return "0";
    }

    protected void FormviewPartnerDirectionSelectedIndexChanged(Object sender, EventArgs e)
    {
        //    DropDownList ddlDirection = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListAssignedDirection");
        //    DropDownList ruleddl = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownBillingRule");
        //    DropDownList paymentddl = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListPaymentMethod");
        //    if (ddlDirection.SelectedValue == "1")
        //    {
        //        ruleddl.Enabled = true;
        //        paymentddl.Enabled = true;
        //    }
        //    else
        //    {

        //        ruleddl.Enabled = false;
        //        paymentddl.Enabled = false;
        //    }
    }

    protected void FormviewPartnerSelectedIndexChanged(Object sender, EventArgs e)
    {
        DropDownList dropservice = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListservice");
        DropDownList dropRatePlan = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListRatePlan");

        dropRatePlan.Items.Clear();
        using (PartnerEntities Conmed = new PartnerEntities())
        {
            using (PartnerEntities Context = new PartnerEntities())
            {
                DropDownList dropPartner = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListPartner");
                DropDownList dropRoute = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListRoute");
                List<rateplan> lstPlan = new List<rateplan>();
                rateplan Dummy = new rateplan();
                Dummy.id = -1;
                Dummy.RatePlanName = " [Select]";
                lstPlan.Add(Dummy);
                ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListRatePlan")).DataSource = lstPlan;
                if (dropPartner.SelectedValue == "0")//none
                {
                    ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListAssignedDirection")).Enabled = false;
                    var lstRules = Conmed.enumservicefamilies.Where(c => c.PartnerAssignNotNeeded == 1).ToList();
                    dropRoute.SelectedValue = "-1";
                    dropRoute.Enabled = false;
                }
                else if (dropPartner.SelectedValue == "-1")//select
                {
                    //nothing in the dropdownlist
                    dropRoute.SelectedValue = "-1";
                    dropRoute.Enabled = false;
                }
                else//a partner has been selected
                {
                    var lstRules = Conmed.enumservicefamilies.Where(c => c.PartnerAssignNotNeeded != 1).ToList();
                    ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListAssignedDirection")).Enabled = true;
                    int idPartner = Convert.ToInt32(dropPartner.SelectedValue);
                    var LstRoute = Context.routes.Where(c => c.idPartner == idPartner).ToList();
                    dropRoute.Items.Clear();
                    dropRoute.Items.Add(new ListItem(" [All]", "-1"));
                    foreach (route r in LstRoute)
                    {
                        dropRoute.Items.Add(new ListItem(r.RouteName, r.idroute.ToString()));
                    }
                    dropRoute.Enabled = true;
                }
            }
        }

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //applicabel for postback and initial load

        Tbc = PortalApp.PageUtil.GetTelcobrightConfig();
        foreach (billingruleassignment thisRule in Context.billingruleassignments)
        {
            dicBillRules.Add(thisRule.idRatePlanAssignmentTuple, thisRule);
        }
        //Session["assign.sessdicBillRules"] = dicBillRules;
        if (IsPostBack)
        {
            //required for script manager, checking code delete existence
            Session["assign.vsRateTaskId"] =
                DropDownListTaskRef.SelectedValue; //view state didn't work, have to live with session
        }
        else
        {
            //!postback
            populateDropDownForBillingRule();
            DropDownList ddlist = (DropDownList) frmSupplierRatePlanInsert.FindControl("DropDownListPartner");
            DropDownList ddlistType = (DropDownList) frmSupplierRatePlanInsert.FindControl("DropDownListPartnerType");
            Dictionary<int, string> dicRatePlan = new Dictionary<int, string>();
            foreach (rateplan ThisPlan in Context.rateplans.ToList())
            {
                dicRatePlan.Add(ThisPlan.id, ThisPlan.RatePlanName);
            }
            var lstParters = Context.partners.OrderBy(p=>p.PartnerName).ToList();
            ddlist.Items.Clear();
            ddlist.Items.Add(new ListItem(" [Select]", "-1"));
            ddlist.Items.Add(new ListItem(" [None]", "0"));
            foreach (partner p in lstParters)
            {
                ddlist.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
            }
            //ddlistType.Width = ddlist.Width;
            Session["assign.dicRatePlan"] = dicRatePlan;
            //load all rateassigns in a dictionary to show the startdate and enddate values from this table 
            //in the ratetaskassign gridview's startdate and end date
            //Dictionary<string, rateassign> dicRateAssign = new Dictionary<string, rateassign>();

            //Retrieve Path from TreeView for displaying in the master page caption label

            //Load Report Templates in TreeView dynamically from database.
            TreeView MasterTree = (TreeView) Master.FindControl("TreeView1");
            CommonCode CommonCodes = new CommonCode();
            CommonCodes.LoadReportTemplatesTree(ref MasterTree);

            string LocalPath = Request.Url.LocalPath;
            int Pos2ndSlash = LocalPath.Substring(1, LocalPath.Length - 1).IndexOf("/");
            string Root_Folder = LocalPath.Substring(1, Pos2ndSlash);
            int EndOfRootFolder = Request.Url.AbsoluteUri.IndexOf(Root_Folder);
            string UrlWithQueryString = ("~/" + Root_Folder +
                                         Request.Url.AbsoluteUri.Substring((EndOfRootFolder + Root_Folder.Length),
                                             Request.Url.AbsoluteUri.Length - (EndOfRootFolder + Root_Folder.Length)))
                .Replace("%20", " ");
            TreeNodeCollection cNodes = MasterTree.Nodes;
            TreeNode MatchedNode = null;
            foreach (TreeNode N in cNodes) //for each nodes at root level, loop through children
            {
                MatchedNode = CommonCodes.RetrieveNodes(N, UrlWithQueryString);
                if (MatchedNode != null)
                {
                    break;
                }
            }
            //set screentile/caption in the master page...
            Label lblScreenTitle = (Label) Master.FindControl("lblScreenTitle");
            if (MatchedNode != null)
            {
                lblScreenTitle.Text = MatchedNode.ValuePath;
            }
            else
            {
                lblScreenTitle.Text = "";
            }


            //End of Site Map Part *******************************************************************


            List<enumservicefamily> Lstrules = new List<enumservicefamily>();
            DropDownList dropPartner = (DropDownList) frmSupplierRatePlanInsert.FindControl("DropDownListPartner");
            DropDownList dropservice = (DropDownList) frmSupplierRatePlanInsert.FindControl("DropDownListservice");
            if (dropPartner != null)
            {
                //foreach (rateassign ThisRate in Context.rateassigns.ToList())
                //{
                //    dicRateAssign.Add(ThisRate.Prefix + "#" + ThisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss"), ThisRate);
                //}
                //Session["assign.dicRateAssign"] = dicRateAssign;
                if (dropPartner.SelectedValue == "0") //none
                {
                    Lstrules = (from c in Context.enumservicefamilies
                        where c.id == 5 ||
                              c.id == 6 ||
                              c.id == 9 ||
                              c.id == 10 ||
                              c.id == 11 ||
                              c.id == 12
                        select c).ToList();
                }
                else if (dropPartner.SelectedValue == "-1") //select
                {
                    //nothing in the dropdownlist
                }
                else //a partner has been selected
                {
                    Lstrules = (from c in Context.enumservicefamilies
                        where c.id != 5 &&
                              c.id != 6 &&
                              c.id != 9 &&
                              c.id != 10 &&
                              c.id != 11 &&
                              c.id != 12
                        select c).ToList();
                }
                dropservice.Items.Clear();
                foreach (enumservicefamily ThisRule in Lstrules)
                {
                    dropservice.Items.Add(new ListItem(ThisRule.ServiceName, ThisRule.id.ToString()));
                }
            }

            //load rateplanassignmenttuple
            Dictionary<long, rateplanassignmenttuple> dicTuple = new Dictionary<long, rateplanassignmenttuple>();
            Dictionary<long, enumservicefamily> dicservice = new Dictionary<long, enumservicefamily>();
            var dicRoutes = new Dictionary<int, string>();
            using (PartnerEntities Conmed = new PartnerEntities())
            {
                foreach (rateplanassignmenttuple ThisTuple in Context.rateplanassignmenttuples.ToList())
                {
                    dicTuple.Add(ThisTuple.id, ThisTuple);
                }
                foreach (enumservicefamily ThisRule in Conmed.enumservicefamilies.ToList())
                {
                    dicservice.Add(ThisRule.id, ThisRule);
                }
                foreach (route r in Context.routes.ToList())
                {
                    dicRoutes.Add(r.idroute, r.RouteName);
                }
            }
            Session["assign.sessdictuple"] = dicTuple;
            Session["assign.sessdicservice"] = dicservice;
            Session["assign.sessdicroute"] = dicRoutes;

            TextBoxDefaultDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            TextBoxDefaultDeleteDate.Text = DateTime.Today.ToString("yyyy-MM-dd");

            //DropDownListFormat.SelectedIndex = 1;
            ddlTypeFind.DataBind();
            ddlTypeFind.SelectedIndex = 0;

            int v = 1;
            ViewState["sesidRatePlan"] = v;


            //SqlDataTaskStatus.SelectParameters["idRatePlan"].DefaultValue = v.ToString();
            timezone TzRatePlan = null;
            int RatePlanType = 0;
            List<ratetaskassignreference> LstTaskRef = Context.ratetaskassignreferences
                .Where(c => c.idRatePlan == v).OrderByDescending(c => c.id).ToList();
            if (LstTaskRef.Count == 0)
            {
                //add default instance
                ratetaskassignreference Newref = new ratetaskassignreference();
                Newref.Description = "Default";
                Newref.idRatePlan = v;
                Context.ratetaskassignreferences.Add(Newref);
                Context.SaveChanges();
                LstTaskRef = Context.ratetaskassignreferences.Where(c => c.idRatePlan == v)
                    .OrderByDescending(c => c.id).ToList();
            }

            DropDownListTaskRef.Items.Clear();
            DropDownListTaskRef.Items.Add(new ListItem("default", "1"));

            DropDownListTaskRef.SelectedIndex = 0; //only if not post back
            SqlDataTaskStatus.SelectParameters["idRatePlan"].DefaultValue = DropDownListTaskRef.SelectedValue;
            //required for script manager, checking code delete existence
            Session["assign.vsRateTaskId"] =
                DropDownListTaskRef.SelectedValue; //view state didn't work, have to live with session


            //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
            //from Partner

            string ThisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

            MySqlConnection connection = new MySqlConnection(ThisConectionString);
            string database = connection.Database.ToString();
            telcobrightpartner ThisCustomer = (from c in Context.telcobrightpartners
                where c.databasename == database
                select c).First();
            int ThisOperatorId = ThisCustomer.idCustomer;
            int idOperatorType = Convert.ToInt32(ThisCustomer.idOperatorType);


            Session["assign.sesidOperator"] = ThisOperatorId;
            timezone tzNative = new timezone();
            using (PartnerEntities ConPartner = new PartnerEntities())
            {
                tzNative = ConPartner.timezones.Where(c => c.id == ThisCustomer.NativeTimeZone).FirstOrDefault();
            }

            //long TimeZoneDifference = tzNative.gmt_offset - TzRatePlan.gmt_offset;
            //ViewState["vsTimeZoneDifference"] = TimeZoneDifference;

            Session["assign.sesidOperatorType"] = idOperatorType;

            //set visible/invisible for OtherAmount10 text box based on operator type
            //in FormView and Gridview
            switch (idOperatorType)
            {
                case 2: //icx
                    int i = 0;
                    //in formview is handled in formview_item created

                    //in GridView
                    for (i = 0; i < GridViewSupplierRates.Columns.Count; i++)
                    {
                        switch (GridViewSupplierRates.Columns[i].SortExpression)
                        {
                            case "OtherAmount10":
                                GridViewSupplierRates.Columns[i].Visible = true;
                                break;
                        }
                    }
                    break;
                default: //if operator type not icx
                    //in formview is handled in formview_item created

                    //in GridView
                    for (i = 0; i < GridViewSupplierRates.Columns.Count; i++)
                    {
                        switch (GridViewSupplierRates.Columns[i].SortExpression)
                        {
                            case "OtherAmount10":
                                GridViewSupplierRates.Columns[i].Visible = false;
                                break;
                        }
                    }
                    break;
            }
            int idRatePlan = v;
            List<ratetaskassign> sesSupplierRates = (from c in Context.ratetaskassigns
                where c.idrateplan == v
                select c).ToList();

            Session["assign.sesSupplierRates"] = sesSupplierRates;


            List<partner> sesCountryCodes = Context.partners.ToList();
            Session["assign.sesCountryCodes"] = sesCountryCodes;

            //set visibility of gridview controls based on supplierrateplan type
            if (RatePlanType == 3) //international incoming
            {
                int i = 0;
                //make gridview controls visible for International In
                //and invisible for Intl Out
                for (i = 0; i < GridViewSupplierRates.Columns.Count; i++)
                {
                    switch (GridViewSupplierRates.Columns[i].SortExpression)
                    {
                        case "OtherAmount1":
                        case "OtherAmount2":
                        case "OtherAmount3":
                            GridViewSupplierRates.Columns[i].Visible = true;
                            break;

                        case "OtherAmount4":
                        case "OtherAmount5":
                        case "OtherAmount6":
                        case "OtherAmount7":
                        case "OtherAmount8":
                        case "OtherAmount9":
                            GridViewSupplierRates.Columns[i].Visible = false;
                            break;
                    }
                }
                //set formview controls visible for International In
                //and invisible for Intl Out
                for (i = 0; i < frmSupplierRatePlanInsert.Controls.Count; i++)
                {
                    switch (frmSupplierRatePlanInsert.Controls[i].ID)
                    {
                        case "txtOtherAmount1":
                        case "txtOtherAmount2":
                        case "txtOtherAmount3":
                            frmSupplierRatePlanInsert.Controls[i].Visible = true;
                            break;

                        case "OtherAmount4":
                        case "OtherAmount5":
                        case "OtherAmount6":
                        case "OtherAmount7":
                        case "OtherAmount8":
                        case "OtherAmount9":
                            frmSupplierRatePlanInsert.Controls[i].Visible = false;
                            break;
                    }
                }
            }
            else if (RatePlanType == 4) //make gridview controls visible for International OUT
                //and invisible for Intl Incoming
            {
                int i = 0;
                for (i = 0; i < GridViewSupplierRates.Columns.Count; i++)
                {
                    switch (GridViewSupplierRates.Columns[i].SortExpression)
                    {
                        case "OtherAmount4":
                        case "OtherAmount5":
                        case "OtherAmount6":
                        case "OtherAmount7":
                        case "OtherAmount8":
                        case "OtherAmount9":
                            GridViewSupplierRates.Columns[i].Visible = true;
                            break;

                        case "OtherAmount1":
                        case "OtherAmount2":
                        case "OtherAmount3":
                            GridViewSupplierRates.Columns[i].Visible = false;
                            break;
                    }
                }
            }

            myGridViewDataBind();
        } //!postback
    }

    //    protected void ModifySupplierGrid(int Action,int id,string Prefix,string Description,Single rateamount,
    //int WeekDayStart,int WeekDayEnd,string StartTime,string EndTime,int Resolution,Single SurchargeTime,
    //Single SurchargeAmount,int idSupplierRatePlan,string CountryCode,string date1,int field1,
    //int field2,int field3,string field4,string field5,string startdate,string enddate,int Inactive,int RouteDisabled)

    //    {
    //        List<rate> sesSupplierRates = null;
    //        if (Session["assign.sesSupplierRates"] != null)
    //        {
    //            sesSupplierRates = (List<rate>)Session["assign.sesSupplierRates"];
    //            //GridViewSupplierRates.DataSource = sesSupplierRates;
    //            //GridViewSupplierRates.DataBind();
    //        }


    //    }

    protected void myGridViewDataBind()
    {

        //count committed, error etc.

        GridViewSupplierRates.DataBind();
        if (GridViewSupplierRates.Rows.Count == 0)
        {
            lblUnassignedPlans.Text = "";
            lnkCommitchanges.Visible = false;
        }
        else
        {
            lblUnassignedPlans.Text = "Unassigned Rateplans";
            lnkCommitchanges.Visible = true;
        }

        GridViewRateAssign.DataBind();
        if (GridViewRateAssign.Rows.Count == 0)
        {
            lblAssignedPlans.Text = "";
        }
        else
        {
            lblAssignedPlans.Text = "Assigned Rateplans";
        }

        //ListView1.DataBind();
        LinkButtonSaveAll.Visible = true;
        //LinkButtonCancelAll.Visible = true;

        //bind 2nd gridview
        string PrefixSelect = " select id from rateplanassignmenttuple where id>0 " + WhereClauseAnds();
        string SqlMain = "select * from rateassign where prefix in(" + PrefixSelect + ") ";
        if (TextBoxDateFind.Text != "")
        {
            SqlMain += " and " + " startdate <= '" + TextBoxDateFind.Text + "' and ifnull(enddate,'9999-12-31 23:59:59') >'" + TextBoxDateFind.Text + "' ";
        }

        using (var context = new PartnerEntities())
        {
            var lstAssignedPlans = context.rateassigns.SqlQuery(SqlMain, typeof(rateassign)).ToList()
                .OrderByDescending(c => c.id)
                .ThenBy(c => c.SubCategory) // assigned direction
                .ThenBy(c => c.Resolution)//priority
                .ThenByDescending(c => c.startdate).ToList();
            GridViewRateAssign.DataSource = lstAssignedPlans;
            GridViewRateAssign.DataBind();

            if (GridViewRateAssign.Rows.Count == 0)
            {
                lblAssignedPlans.Text = "";
            }
            else
            {
                lblAssignedPlans.Text = "Assigned Rateplans";
            }
        }


        //using (MySqlConnection Con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
        //{
        //    Con.Open();
        //    using (MySqlCommand cmd = new MySqlCommand(SqlMain + " order by startdate desc ",Con))
        //    {
        //        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
        //        DataSet dataset = new DataSet();
        //        da.Fill(dataset);
        //        GridViewRateAssign.DataSource = dataset;
        //        GridViewRateAssign.DataBind();

        //        if (GridViewRateAssign.Rows.Count == 0)
        //        {
        //            lblAssignedPlans.Text = "";
        //        }
        //        else
        //        {
        //            lblAssignedPlans.Text = "Assigned Rateplans";
        //        }
        //    }
        //}



        GridViewSupplierRates.DataBind();


    }

    string WhereClauseAnds()
    {
        StringBuilder AndString = new StringBuilder();
        using (PartnerEntities Context = new PartnerEntities())
        {
            var lstidpartner = Context.partners.Where(c => c.PartnerName.Contains(TextBoxPartnerFind.Text.ToLower().Trim())).ToList();

            if (TextBoxPartnerFind.Text.Trim() != "")
            {
                AndString.Append(" and idpartner in(").Append(string.Join(",", lstidpartner.Select(c => c.idPartner).ToArray())).Append(")").Append(" ");
            }
            if (ddlTypeFind.SelectedIndex > 0)
            {
                AndString.Append(" and idservice=").Append(ddlTypeFind.SelectedValue).Append(" ");
            }
            if (ddlAssignedDirectionFind.SelectedIndex > 0)
            {
                AndString.Append(" and assigndirection= ").Append(ddlAssignedDirectionFind.SelectedValue).Append(" ");
            }
        }
        return AndString.ToString();
    }

    protected void EntityDataSupplierRates_QueryCreated(object sender, QueryCreatedEventArgs e)
    {

    }
    protected void EntityDataSupplierRates_Inserting(object sender, EntityDataSourceChangingEventArgs e)
    {
        rateassign newElement = e.Entity as rateassign;
        System.Web.UI.WebControls.Calendar ThisCalendar = (System.Web.UI.WebControls.Calendar)GridViewSupplierRates.FindControl("CalendarStartDate");
        newElement.startdate = ThisCalendar.SelectedDate;
    }

    bool GetBitInteger(Int32 x, int BitPosition)
    {

        int Shiftvariable = 0;
        Shiftvariable = BitPosition - 1;

        long ShiftResult = x >> Shiftvariable;
        long AndResult = ShiftResult & 1;
        if (AndResult == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    Int32 SetBitInteger(Int32 x, int BitPosition)
    {

        UInt32 Orvariable = 0;
        Orvariable = Convert.ToUInt32(Math.Pow(2, (BitPosition - 1)).ToString());//1;

        long z = (Orvariable | x);
        return (int)z;
    }

    private int getIntFromBitArray(BitArray bitArray)
    {

        if (bitArray.Length > 32)
            throw new ArgumentException("Argument length shall be at most 32 bits.");

        int[] array = new int[1];
        bitArray.CopyTo(array, 0);
        return array[0];

    }

    private string[] LineToFields(string ThisLine)
    {
        List<string> lStr = new List<string>();
        StringBuilder sb = new StringBuilder();

        int OddEvenCounter = 0;
        foreach (char c in ThisLine)
        {
            if (c == '`')
            {
                OddEvenCounter++;
                continue;
            }

            if (OddEvenCounter % 2 == 0)
            {
                lStr.Add(sb.ToString());
                sb = new StringBuilder();
                OddEvenCounter = 0;
                continue;
            }
            else
            {
                sb.Append(c);
            }
        }
        lStr.Add(sb.ToString());
        return lStr.ToArray();

    }

    protected void GridViewSupplierRates_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        long idservice = -1;
        Label LabelAssignedDirection = null;
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //if ((e.Row.RowState != DataControlRowState.Edit))
            {
                Dictionary<long, rateplanassignmenttuple> dicTuple = (Dictionary<long, rateplanassignmenttuple>)Session["assign.sessdictuple"];
                Dictionary<long, enumservicefamily> dicservice = (Dictionary<long, enumservicefamily>)Session["assign.sessdicservice"];
                var dicroute = (Dictionary<int, string>)Session["assign.sessdicroute"];



                LabelAssignedDirection = (Label)e.Row.FindControl("lblAssignedDirection");
                Label Labelservicename = (Label)e.Row.FindControl("lblservice");
                Label LabelPulse = (Label)e.Row.FindControl("lblPulse");
                Label LabelRoute = (Label)e.Row.FindControl("lblRoute");

                string Routename = "Error!";
                if (DataBinder.Eval(e.Row.DataItem, "routedisabled") != null)
                {
                    int idRoute = -1;
                    Routename = "";
                    int.TryParse((DataBinder.Eval(e.Row.DataItem, "routedisabled").ToString()), out idRoute);
                    dicroute.TryGetValue(idRoute, out Routename);
                    LabelRoute.Text = Routename;
                }
                else
                {
                    LabelRoute.Text = "-";
                }

                int AssignedDir = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "SubCategory"));
                switch (AssignedDir)
                {
                    case 1:
                        LabelAssignedDirection.Text = "Customer";
                        break;
                    case 2:
                        LabelAssignedDirection.Text = "Supplier";
                        break;
                }
                rateplanassignmenttuple ThisTuple = null;
                long TupleId = Convert.ToInt64(DataBinder.Eval(e.Row.DataItem, "prefix"));

                if (dicTuple.TryGetValue(TupleId, out ThisTuple))
                {
                    enumservicefamily Thisservice = null;
                    dicservice.TryGetValue(ThisTuple.idService, out Thisservice);
                    Labelservicename.Text = Thisservice.ServiceName;
                    LabelPulse.Text = ThisTuple.priority.ToString();
                    idservice = ThisTuple.idService;
                }

                Dictionary<int, string> dicRatePlan = (Dictionary<int, string>)Session["assign.dicRatePlan"];
                int idRatePlan = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "inactive"));
                string PlanName = "";
                dicRatePlan.TryGetValue(idRatePlan, out PlanName);
                rateplan OneRatePlan = new rateplan();
                OneRatePlan.id = idRatePlan;
                OneRatePlan.RatePlanName = PlanName;
                List<rateplan> TmpRatePlan = new List<rateplan>();
                TmpRatePlan.Add(OneRatePlan);
                ((DropDownList)e.Row.FindControl("DropDownListRatePlan")).DataSource = TmpRatePlan;
                ((DropDownList)e.Row.FindControl("DropDownListRatePlan")).DataBind();



            }
            if ((e.Row.RowState & DataControlRowState.Edit) > 0)
            {
                //assignment order
                //(()e.Row.FindControl("txtPulse")).Text = DataBinder.Eval(e.Row.DataItem, "Resolution").ToString();


                using (PartnerEntities Context = new PartnerEntities())
                {
                    var Lst = Context.rateplans.Where(c => c.Type == idservice).ToList();
                    ((DropDownList)e.Row.FindControl("DropDownListRatePlan")).DataSource = Lst;
                    ((DropDownList)e.Row.FindControl("DropDownListRatePlan")).DataBind();
                }

                lblEditPrefix.Text = "";

                //set default values of start/end date controls to their default values before editing

                string Thisdate = lblRateGlobal.Text;
                string[] AllDates = Thisdate.Split('#');

                AjaxControlToolkit.CalendarExtender CalDate = (AjaxControlToolkit.CalendarExtender)e.Row.FindControl("CalendarStartDate");
                TextBox txtTime = (TextBox)e.Row.FindControl("TextBoxStartDateTimePicker");

                //lock start date,time if the assignment is already in effect
                TextBox txtDate = (TextBox)e.Row.FindControl("TextBoxStartDatePicker");
                int Completed = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "changecommitted"));
                if (Completed == 1)
                {
                    txtTime.Enabled = false;
                    txtDate.Enabled = false;
                }

                string strCalDate = AllDates[0];
                string format = "yyyy-MM-dd";
                DateTime dateTime;
                if (DateTime.TryParseExact(strCalDate, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTime))
                {
                    CalDate.SelectedDate = dateTime;
                    ((Label)e.Row.FindControl("lblStartDate")).Text = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    txtTime.Text = AllDates[1];
                }
                else
                {
                    CalDate.SelectedDate = DateTime.Now;
                    //CalDate.VisibleDate = DateTime.Now;
                    txtTime.Text = "00:00:00";
                }


                AjaxControlToolkit.CalendarExtender CalDateEnd = (AjaxControlToolkit.CalendarExtender)e.Row.FindControl("CalendarEndDate");
                TextBox txtTimeEnd = (TextBox)e.Row.FindControl("TextBoxEndDateTimePicker");

                string strCalDateEnd = AllDates[2];
                DateTime dateTimeEnd;
                if (DateTime.TryParseExact(strCalDateEnd, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTimeEnd))
                {
                    CalDateEnd.SelectedDate = dateTimeEnd;

                    txtTime.Text = AllDates[3];
                }
                else
                {
                    //CalDateEnd.SelectedDate = DateTime.Today;

                    txtTimeEnd.Text = "00:00:00";
                }

                //disable changing end date if enddate already has a date value
                TextBox txtEndDate = (TextBox)e.Row.FindControl("TextBoxEndDatePicker");
                TextBox txtEndTime = (TextBox)e.Row.FindControl("TextBoxEndDateTimePicker");
                if (CalDateEnd.SelectedDate != null)
                {
                    txtEndDate.Enabled = false;
                    txtEndTime.Enabled = false;
                }

            }//edit mode data binding   
            else
            {//not edit mode, binding during normal gridview mode

                CheckBox CheckBoxSelected = (CheckBox)e.Row.FindControl("CheckBoxSelected");
                if (HiddenFieldSelect.Value == "1")
                {
                    CheckBoxSelected.Checked = true;
                }
                else
                {
                    CheckBoxSelected.Checked = false;
                }


                //Label lblResolution = (Label)e.Row.FindControl("lblResolution");
                //lblResolution.Text = DataBinder.Eval(e.Row.DataItem, "Resolution").ToString();

                CheckBox chkbox = (CheckBox)e.Row.FindControl("CheckBox1");
                ////set command argument for link button delete, ID to be retrieved in deletewithtransaction
                LinkButton DelButton = (LinkButton)e.Row.FindControl("LinkButtonDelete");
                DelButton.CommandArgument = DataBinder.Eval(e.Row.DataItem, "id").ToString();
                //set command argument for link button edit to be retrieved in rowcommand event,seperated by #
                LinkButton LnkBtn = (LinkButton)e.Row.FindControl("LinkButtonEdit");

                //for change completed rate tasks, disable the edit button
                bool ChangeCommitted = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "changecommitted"));
                if (ChangeCommitted == true)
                {
                    //LnkBtn.Enabled = false;
                    chkbox.Checked = true;
                }
                else
                {
                    LnkBtn.Enabled = true;
                    chkbox.Checked = false;
                }
                string EffDate = ";";
                string EffTime = ";";
                string EndDate = ";";
                string EndTime = ";";

                if (DataBinder.Eval(e.Row.DataItem, "startdate") != null && DataBinder.Eval(e.Row.DataItem, "startdate").ToString() != "")
                {
                    try//exception may occur due to invalid date string
                    {
                        EffDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("yyyy-MM-dd");
                        EffTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("HH:mm:ss");
                    }
                    catch (Exception e1)
                    {
                        //do nothing, just allow the program to continue
                    }
                }

                if (DataBinder.Eval(e.Row.DataItem, "enddate") != null)
                {
                    try
                    {
                        EndDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "enddate")).ToString("yyyy-MM-dd");
                        EndTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "enddate")).ToString("HH:mm:ss");
                    }
                    catch (Exception e1)
                    {
                        //do nothing, jsut allow the program to continue
                    }
                }
                //LnkBtn.CommandArgument = e.Row.RowIndex.ToString();
                LnkBtn.CommandArgument = EffDate + "#" + EffTime + "#" + EndDate + "#" + EndTime;

                //set country here....
                //dropdown ThisLabel = (Label)e.Row.FindControl("lblRateAmount");

                //Country
                Label ThisLabel = (Label)e.Row.FindControl("lblCountry");
                if (DataBinder.Eval(e.Row.DataItem, "CountryCode") != null && DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString() != "")
                {
                    int ThisCountryCode = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString());

                    if (ThisCountryCode < 1)
                    {
                        LabelAssignedDirection.Text = "";
                    }

                    if (ThisLabel != null)
                    {
                        if (Session["assign.sesCountryCodes"] != null)//partners are used by copying old countrycodes based code from ratetask.aspx
                        {
                            List<partner> CountryCodes = (List<partner>)Session["assign.sesCountryCodes"];
                            if ((CountryCodes.Any(c => c.idPartner == ThisCountryCode)) == true)
                            {
                                ThisLabel.Text = (from c in CountryCodes
                                                  where c.idPartner == ThisCountryCode
                                                  select c.PartnerName + " (" + c.idPartner + ")").First();
                            }
                            else
                            {
                                ThisLabel.Text = "None";
                            }
                        }
                    }
                }


                //lblRateAmount
                ThisLabel = (Label)e.Row.FindControl("lblRateAmount");
                if (DataBinder.Eval(e.Row.DataItem, "RateAmount") != null)
                {
                    decimal ThisRateAmount = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "RateAmount"));
                    if (ThisLabel != null)
                    {
                        //if(ThisRateAmount
                        ThisLabel.Text = ThisRateAmount.ToString("0.#00000");
                    }
                }
                else //null rateamount
                {
                    if (ThisLabel != null)
                    {
                        //if(ThisRateAmount
                        ThisLabel.Text = "";
                    }
                }

                //ThisLabel
                //set change types e.g. new, delete, increase, decrease etc.
                ThisLabel = (Label)e.Row.FindControl("lblRateChangeType");
                if (DataBinder.Eval(e.Row.DataItem, "Status") != null)
                {
                    int ChangeType = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Status"));
                    switch (ChangeType)
                    {
                        case 9:
                            ThisLabel.Text = "Overlap";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 0:
                        case 7:
                        case 8:
                            ThisLabel.Text = "";//committed & uncommitteds are handled by changecommitted flag
                            e.Row.ForeColor = Color.Red;
                            //errors are handled by field2
                            break;
                        case 1:
                            ThisLabel.Text = "Ok";//"Code End";
                            break;
                        case 2:
                            ThisLabel.Text = "Ok";//"Code End";"New";
                            break;
                        case 3:
                            ThisLabel.Text = "Ok";//"Code End";"Increase";
                            break;
                        case 4:
                            ThisLabel.Text = "Ok";//"Code End";"Decrease";
                            break;
                        case 5:
                            ThisLabel.Text = "Ok";//"Code End";"Unchanged";
                            break;

                        case 10:
                            ThisLabel.Text = "Overlap Adjusted";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 11:
                            ThisLabel.Text = "Rate Param Conflict";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 12:
                            ThisLabel.Text = "Rate Position not found";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 13:
                            ThisLabel.Text = "Existing";
                            e.Row.ForeColor = Color.Red;
                            break;
                    }

                }
                else
                {
                    ThisLabel.Text = "Unknown";
                }

                //set error types
                ThisLabel = (Label)e.Row.FindControl("lblRateErrors");
                if (DataBinder.Eval(e.Row.DataItem, "Field2") != null && int.Parse(DataBinder.Eval(e.Row.DataItem, "Field2").ToString()) != 0)
                {
                    var color = ColorTranslator.FromHtml("#FA0509");
                    e.Row.ForeColor = color;

                    int ErrorInt = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Field2"));
                    string ErrorString = "";
                    //check bit by bit for each field

                    if (GetBitInteger(ErrorInt, 1) == true)//bit 1=prefix
                    {
                        ErrorString += "No or Invalid Prefix." + ", ";
                    }

                    if (GetBitInteger(ErrorInt, 2) == true)//2=rate
                    {
                        ErrorString += "No or Invalid Rate or SurchargeAmount" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblRateAmount");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 3) == true)//3=pulse
                    {
                        ErrorString += "No or Invalid Pulse" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label PulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 4) == true)//4=effective since
                    {
                        ErrorString += "No or Invalid Effective DateTime." + ", ";
                        //if effective since flag is found, then set the text to ""
                        Label dateLabel = (Label)e.Row.FindControl("lblStartDate");

                        //dateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 5) == true)//5=Invalid Type
                    {
                        ErrorString += "No or Invalid Rate Type!" + ", ";

                    }

                    if (GetBitInteger(ErrorInt, 6) == true)//6=Invalid BTRC % In
                    {
                        ErrorString += "No or Invalid BTRC % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount1");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 7) == true)//7=invalid icx % in Intl In
                    {
                        ErrorString += "No or Invalid ICX % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount2");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 8) == true)//7=invalid ans % in Intl In
                    {
                        ErrorString += "No or Invalid ANS % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount3");
                        //rateLabel.Text = "";
                    }


                    if (GetBitInteger(ErrorInt, 9) == true)//9=Invalid X rate
                    {
                        ErrorString += "No or Invalid X-rate Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount4");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 10) == true)//10=Invalid Y rate
                    {
                        ErrorString += "No or Invalid Y-rate Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount5");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 11) == true)//11=Invalid ans % Z
                    {
                        ErrorString += "No or Invalid ANS % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount6");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 12) == true)//12=Invalid icx % Z
                    {
                        ErrorString += "No or Invalid ICX % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount7");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 13) == true)//13=Invalid igw % Z
                    {
                        ErrorString += "No or Invalid IGW % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount8");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 14) == true)//14=Invalid BTRC % Z
                    {
                        ErrorString += "No or Invalid BTRC % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount9");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 15) == true)//15=Invalid ICX revenue Share
                    {
                        ErrorString += "No or Invalid ICX Rev. % Share" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount10");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 16) == true)//16=Invalid Currency
                    {
                        ErrorString += "No or Invalid Currency" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblCurrency");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 17) == true)//3=Minduration
                    {
                        ErrorString += "No or Invalid Mininum Duration" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label PulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }
                    if (GetBitInteger(ErrorInt, 18) == true)//3=pulse
                    {
                        ErrorString += "No or Invalid Fixed Charge Time" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label PulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 19) == true)//19=end datetime
                    {
                        ErrorString += "No or Invalid End DateTime. Or, End time less than start time." + ", ";
                        //if effective since flag is found, then set the text to ""
                        Label dateLabel = (Label)e.Row.FindControl("lblStartDate");

                        //dateLabel.Text = "";
                    }
                    //remove last new line char
                    int PosLastDot = ErrorString.LastIndexOf(".");
                    if (PosLastDot > 0)
                    {
                        ErrorString = ErrorString.Substring(0, PosLastDot + 1);
                    }
                    //show the error in labelcontrol

                    ThisLabel.Text = ErrorString;

                }

            }//else not edit mode, binding during normal gridview mode



        }// if data row
        else if (e.Row.RowType == DataControlRowType.Footer)
        {
            //keep the flag whether code delete items exist for this task here
            hidvalueCodeDelete.Value = CodeDeleteExists();
        }
    }
    protected void GridViewSupplierRates_RowEditing(object sender, GridViewEditEventArgs e)
    {



        GridViewSupplierRates.EditIndex = e.NewEditIndex;
        myGridViewDataBind();
        //GridViewSupplierRates.DataBind();    
        //LinkButtonCancelAll.Visible = false;
        LinkButtonSaveAll.Visible = false;

    }
    protected void GridViewSupplierRates_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        //GridViewSupplierRates.DataBind();
        if (e.CommandName == "Edit")
        {
            string EffEndDate = e.CommandArgument.ToString();
            lblRateGlobal.Text = EffEndDate;
        }

        if (e.CommandName == "myCancel")
        {
            GridViewSupplierRates.EditIndex = -1;
            myGridViewDataBind();
        }

    }

    protected void GridViewRateAssign_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        //GridViewSupplierRates.DataBind();
        if (e.CommandName == "Edit")
        {
            string EffEndDate = e.CommandArgument.ToString();
            lblRateGlobal.Text = EffEndDate;
        }

        if (e.CommandName == "myCancel")
        {
            GridViewSupplierRates.EditIndex = -1;
            myGridViewDataBind();
        }

    }

    protected void GridViewSupplierRates_PreRender(object sender, EventArgs e)
    {
        //Label lblDescription = GridViewSupplierRates.Rows[0].FindControl("lblDescription") as Label;

        //TextBox txtDescription = GridViewSupplierRates.Rows[0].FindControl("txtDescription") as TextBox;
        //string Description = GridViewSupplierRates.Rows[0].Cells[3].Text;
    }





    private ratetaskassign CreateNewRateTask(
        int CurrentIdRatePlan,
        string newId,//will be -1 for insert, will be >0 for update
        string newPrefix,
        string newDesc,
        string newRateAmount,
        string newResolution,
        string newMinDurationSec,
        string newCountry,
        string newStartDateAndTime,
        string newEndDateAndTime,
        string newInactive,
        string newRouteDisabled,
        string newWeekDayStart,
        string newWeekDayEnd,
        string newStartTime,
        string newEndTime,
        string newSurchargeTime,
        string newSurchargeAmount,
        string newType,
        string newOtherAmount1,
        string newOtherAmount2,
        string newOtherAmount3,
        string newOtherAmount4,
        string newOtherAmount5,
        string newOtherAmount6,
        string newOtherAmount7,
        string newOtherAmount8,
        string newOtherAmount9,
        string newOtherAmount10,
        string newCurrency,
        int idOperatorType,
        double TimeZoneOffsetSec,
        string newMinSurchargeTime,
        string newMinSurchargeAmount,
        string newServiceType,
        string newSubServiceType
    )//field3=idpartner, de-normalization for performance...
    {
        //remove null at first from params
        newId = (newId == null ? "" : newId);
        newPrefix = (newPrefix == null ? "" : newPrefix);
        newDesc = (newDesc == null ? "" : newDesc);
        newRateAmount = (newRateAmount == null ? "" : newRateAmount);
        newResolution = (newResolution == null ? "" : newResolution);
        newMinDurationSec = (newMinDurationSec == null ? "" : newMinDurationSec);
        newCountry = (newCountry == null ? "" : newCountry);
        newStartDateAndTime = (newStartDateAndTime == null ? "" : newStartDateAndTime);
        newEndDateAndTime = (newEndDateAndTime == null ? "" : newEndDateAndTime);
        newInactive = (newInactive == null ? "" : newInactive);
        newRouteDisabled = (newRouteDisabled == null ? "" : newRouteDisabled);
        newWeekDayStart = (newWeekDayStart == null ? "" : newWeekDayStart);
        newWeekDayEnd = (newWeekDayEnd == null ? "" : newWeekDayEnd);
        newStartTime = (newStartTime == null ? "" : newStartTime);
        newEndTime = (newEndTime == null ? "" : newEndTime);
        newSurchargeTime = (newSurchargeTime == null ? "" : newSurchargeTime);
        newSurchargeAmount = (newSurchargeAmount == null ? "" : newSurchargeAmount);
        newType = (newType == null ? "" : newType);
        newOtherAmount1 = (newOtherAmount1 == null ? "" : newOtherAmount1);
        newOtherAmount2 = (newOtherAmount2 == null ? "" : newOtherAmount2);
        newOtherAmount3 = (newOtherAmount3 == null ? "" : newOtherAmount3);
        newOtherAmount4 = (newOtherAmount4 == null ? "" : newOtherAmount4);
        newOtherAmount5 = (newOtherAmount5 == null ? "" : newOtherAmount5);
        newOtherAmount6 = (newOtherAmount6 == null ? "" : newOtherAmount6);
        newOtherAmount7 = (newOtherAmount7 == null ? "" : newOtherAmount7);
        newOtherAmount8 = (newOtherAmount8 == null ? "" : newOtherAmount8);
        newOtherAmount9 = (newOtherAmount9 == null ? "" : newOtherAmount9);
        newOtherAmount10 = (newOtherAmount10 == null ? "" : newOtherAmount10);
        newCurrency = (newCurrency == null ? "" : newCurrency);
        newMinSurchargeTime = (newMinSurchargeTime == null ? "" : newMinSurchargeTime);
        newMinSurchargeAmount = (newMinSurchargeAmount == null ? "" : newMinSurchargeAmount);
        newServiceType = (newServiceType == null ? "" : newServiceType);
        newSubServiceType = (newSubServiceType == null ? "" : newSubServiceType);
        //null removed


        //normalized datetimes in yyyy-MM-dd HH:mm:ss format
        if (newStartDateAndTime.Length == 10 && newStartDateAndTime.Length != 19)
        {
            newStartDateAndTime += " 00:00:00";
        }
        if (newEndDateAndTime.Length == 10 && newEndDateAndTime.Length != 19)
        {
            newEndDateAndTime += " 00:00:00";
        }
        ratetaskassign ThisTask = new ratetaskassign();

        //validate current rate plan id first...
        if (CurrentIdRatePlan == null || CurrentIdRatePlan <= 0)
        {
            throw new Exception("Fatal error: No or invalid current RatePlanId!");
        }
        else
        {
            ThisTask.idrateplan = CurrentIdRatePlan;
        }

        //validate field3=idpartner
        int RatePlanType = -1;
        if (Session["assign.sesRatePlanType"] != null)
        {
            RatePlanType = (int)Session["assign.sesRatePlanType"];
        }



        if (TimeZoneOffsetSec == -360000)
        {
            //invalid timezone
            throw new Exception("Fatal error: Invalid Timezone Offset (-360000)!");
        }
        else
        {
            ThisTask.TimeZoneOffsetSec = TimeZoneOffsetSec.ToString();
        }

        //initialize Field1 and Field2 as 0, they will keep certain flags
        ThisTask.Status = "0";
        ThisTask.field2 = "0";
        double TempRate = 0;
        double.TryParse(newRateAmount, out TempRate);
        if (TempRate == -1)
        {
            ThisTask.Status = "1";
        }
        //add fields one by one and also validate them

        int newIdInt = -1;
        if (int.TryParse(newId, out newIdInt))
        {
            ThisTask.id = newIdInt;
        }


        //error or validation codes: bits are set in field2 for each rate
        //bit 1=prefix
        //2=rate
        //3=pulse
        //4=effective since
        string Digits = "0123456789";
        double TempRate1 = 0;
        double.TryParse(newRateAmount, out TempRate1);
        if (TempRate1 == -1)
        {
            Digits += "*";//code deletes are supported with wild cards
        }
        double myNum = 0; //prefix isnumeric
        bool InvalidPrefix = false;
        int StarCount = 0;
        foreach (char Chr in newPrefix.ToCharArray())
        {
            if (Digits.Contains(Chr.ToString()) == false)
            {
                //invalid prefix
                int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 1);
                ThisTask.field2 = NewFlag.ToString();
                InvalidPrefix = true;
            }
            if (Chr == '*') StarCount++;
        }
        //also make sure * is used alone or last
        if (InvalidPrefix == false)
        {
            if (StarCount > 1)
            {
                //multiple * are not allowed
                //invalid prefix
                int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 1);
                ThisTask.field2 = NewFlag.ToString();
                InvalidPrefix = true;
            }
            else if (StarCount == 1)
            {
                //* allowed only at last
                if (newPrefix.IndexOf("*") != (newPrefix.Length - 1))
                {
                    //invalid prefix
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 1);
                    ThisTask.field2 = NewFlag.ToString();
                    InvalidPrefix = true;
                }
            }
        }

        //a final validation for prefix so that it doesn't allow zero length
        if (newPrefix.Trim() == "")
        {
            //invalid prefix
            int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 1);
            ThisTask.field2 = NewFlag.ToString();
            InvalidPrefix = true;
        }

        if (InvalidPrefix == false) ThisTask.Prefix = Convert.ToInt32(newPrefix);



        ThisTask.description = newDesc; //description, no validation

        double myDecimal = 0;//rate
        if (double.TryParse(newRateAmount, out myDecimal))
        {
            //only -1 is allowed as negative rates indicating rate deletion
            if (myDecimal >= 0)
            {
                ThisTask.rateamount = myDecimal.ToString();
            }
            else if (myDecimal == -1) //rate deletion
            {
                //0=Unknown, 1=delete, 2=new, 3=increase, 4=decrease
                ThisTask.rateamount = "-1";
                ThisTask.Status = "1";
            }
            else //Unknown change
            {
                ThisTask.Status = "0";
            }
        }
        else //invalid Rate
        {
            if (RatePlanType != 4)//not intl.outgoing
            {
                int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 2);
                ThisTask.field2 = NewFlag.ToString();
            }
        }

        myDecimal = 0;//surchargeamount
        if (double.TryParse(newSurchargeAmount, out myDecimal))
        {
            if (myDecimal >= 0)
            {
                ThisTask.SurchargeAmount = myDecimal.ToString();
            }
        }
        else //invalid Rate or surchargeamount
        {
            if (RatePlanType != 4 && RatePlanType != 3)//not intl.outgoing
            {
                int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 2);
                ThisTask.field2 = NewFlag.ToString();
            }
        }

        int myInt = 0;
        if (int.TryParse(newResolution, out myInt))
        {
            ThisTask.Resolution = myInt.ToString();
        }
        else //invalid resolution/pulse/minDurationSec or surchargetime
        {
            int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 3);
            ThisTask.field2 = NewFlag.ToString();
        }

        Single myFloat2 = 0;
        if (Single.TryParse(newMinDurationSec, out myFloat2))
        {
            ThisTask.MinDurationSec = myFloat2.ToString();
        }
        else //invalid Minduratin sec
        {
            int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 17);
            ThisTask.field2 = NewFlag.ToString();
        }

        myInt = 0;
        if (int.TryParse(newSurchargeTime, out myInt))
        {
            ThisTask.SurchargeTime = myInt.ToString();
        }
        else //invalid  surchargetime
        {
            int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 18);
            ThisTask.field2 = NewFlag.ToString();
        }
        ThisTask.CountryCode = Convert.ToInt32(newCountry);//idpartner

        string format = "yyyy-MM-dd HH:mm:ss"; //effective date

        //set if default code delete specified
        DateTime dateTime;
        double DelDouble = 0;
        double.TryParse(ThisTask.rateamount, out DelDouble);
        if (DelDouble == -1)//delete task
        {
            DateTime DelDate = new DateTime();
            if (CheckBoxDefaultDeleteDate.Checked == true)
            {
                if (DateTime.TryParseExact((TextBoxDefaultDeleteDate.Text + " " + TextBoxDefaultDeleteTime.Text), format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DelDate))
                {
                    if (CheckBoxAutoConvertTZ.Checked == false)
                    {
                        ThisTask.startdate = DelDate.ToString(format);
                    }
                    else
                    {
                        long TimeZoneDifference = (long)ViewState["vsTimeZoneDifference"];
                        ThisTask.startdate = DelDate.AddSeconds(TimeZoneDifference).ToString(format);
                    }
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 4);
                    ThisTask.field2 = NewFlag.ToString();
                }
            }
            else
            {//default delete period not specified, if after import delete date is not valid set error flag
                ThisTask.startdate = newStartDateAndTime;
                if (DateTime.TryParseExact(ThisTask.startdate, format, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DelDate) == false)
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 4);
                    ThisTask.field2 = NewFlag.ToString();
                }
            }
        }
        else//not delete task
        {

            ThisTask.startdate = newStartDateAndTime;
            if (DateTime.TryParseExact(newStartDateAndTime, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dateTime))
            {
                if (CheckBoxAutoConvertTZ.Checked == false)
                {
                    ThisTask.startdate = dateTime.ToString(format);
                }
                else
                {
                    long TimeZoneDifference = (long)ViewState["vsTimeZoneDifference"];
                    ThisTask.startdate = dateTime.AddSeconds(TimeZoneDifference).ToString(format);
                }
            }

            else //invalid start date
            {
                if (CheckBoxDefaultDate.Checked == true &&
                    DateTime.TryParseExact((TextBoxDefaultDate.Text + " " + TextBoxDefaultTime.Text), format, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out dateTime) == true)//use default date if mentioned and enabled
                {
                    if (CheckBoxAutoConvertTZ.Checked == false)
                    {
                        ThisTask.startdate = dateTime.ToString(format);
                    }
                    else
                    {
                        long TimeZoneDifference = (long)ViewState["vsTimeZoneDifference"];
                        ThisTask.startdate = dateTime.AddSeconds(TimeZoneDifference).ToString(format);
                    }
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 4);
                    ThisTask.field2 = NewFlag.ToString();
                }
            }
        }//not delete task
        ThisTask.enddate = newEndDateAndTime;
        if (newEndDateAndTime != "\\N" && newEndDateAndTime != "" && newEndDateAndTime != "null" && newEndDateAndTime != null)
        {
            if (DateTime.TryParseExact(newEndDateAndTime, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dateTime))
            {
                if (CheckBoxAutoConvertTZ.Checked == false)
                {
                    ThisTask.enddate = dateTime.ToString(format);
                }
                else
                {
                    long TimeZoneDifference = (long)ViewState["vsTimeZoneDifference"];
                    ThisTask.enddate = dateTime.AddSeconds(TimeZoneDifference).ToString(format);
                }
                //end date must be >= start date
                DateTime TempEndDate = new DateTime(2000, 1, 1, 0, 0, 0, 0);
                DateTime TempStartDate = new DateTime(2000, 1, 1, 0, 0, 0, 0);

                DateTime.TryParseExact(ThisTask.enddate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out TempEndDate);
                DateTime.TryParseExact(ThisTask.startdate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out TempStartDate);

                if (TempEndDate < TempStartDate)
                {
                    //invalid start or end date
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 19);
                    ThisTask.field2 = NewFlag.ToString();
                    var color = ColorTranslator.FromHtml("#FF0000");
                    StatusLabel.ForeColor = color;
                    StatusLabel.Text = "End date must be greater than start date for all rates!";
                }
            }
            else
            {
                //invalid end date
                int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 19);
                ThisTask.field2 = NewFlag.ToString();
            }
        }
        else
        {
            ThisTask.enddate = null;//enddate can be null
        }



        //currency
        myInt = 0;
        if (int.TryParse(newCurrency, out myInt))
        {
            ThisTask.Currency = myInt.ToString();
        }
        else //invalid currency
        {
            int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 16);
            ThisTask.field2 = NewFlag.ToString();
        }



        myInt = 0;
        if (int.TryParse(newType, out myInt)) //type
        {
            ThisTask.Type = myInt.ToString();
            //type enum
            //1=customer,2=supplier,3=intl in,4=intl out
            if (ThisTask.Type == "3") //intl in
            {
                Single myFloat = 0;
                if (Single.TryParse(newOtherAmount1, out myFloat) == true)
                {
                    ThisTask.OtherAmount1 = myFloat.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 6);//vat percentage in invalid
                    ThisTask.field2 = NewFlag.ToString();
                }

                //ThisRate.OtherAmount2        =OtherAmount2      ;
                myFloat = 0;
                if (Single.TryParse(newOtherAmount2, out myFloat) == true)
                {
                    ThisTask.OtherAmount2 = myFloat.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 7);//invalid icx percentage in
                    ThisTask.field2 = NewFlag.ToString();
                }

                //ThisRate.OtherAmount3        =OtherAmount3      ;
                myFloat = 0;
                if (Single.TryParse(newOtherAmount3, out myFloat) == true)
                {
                    ThisTask.OtherAmount3 = myFloat.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 8);//invalid ans percentage in
                    ThisTask.field2 = NewFlag.ToString();
                }
            }
            else if (int.Parse(ThisTask.Type) == 4) //intl out, xyz
            {
                Single myFloat = 0;
                double myDec = 0;

                if (double.TryParse(newOtherAmount4, out myDec) == true)
                {
                    ThisTask.OtherAmount4 = myDec.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 9);//X is invalid
                    ThisTask.field2 = NewFlag.ToString();
                }

                if (double.TryParse(newOtherAmount5, out myDec) == true)
                {
                    ThisTask.OtherAmount5 = myDec.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 10);//Y is invalid
                    ThisTask.field2 = NewFlag.ToString();
                }

                if (Single.TryParse(newOtherAmount6, out myFloat) == true)
                {
                    ThisTask.OtherAmount6 = myFloat.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 11);//vat percentage in invalid
                    ThisTask.field2 = NewFlag.ToString();
                }

                myFloat = 0;
                if (Single.TryParse(newOtherAmount7, out myFloat) == true)
                {
                    ThisTask.OtherAmount7 = myFloat.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 12);//icx percentage in invalid
                    ThisTask.field2 = NewFlag.ToString();
                }

                myFloat = 0;
                if (Single.TryParse(newOtherAmount8, out myFloat) == true)
                {
                    ThisTask.OtherAmount8 = myFloat.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 13);//igw percentage in invalid
                    ThisTask.field2 = NewFlag.ToString();
                }

                myFloat = 0;
                if (Single.TryParse(newOtherAmount9, out myFloat) == true)
                {
                    ThisTask.OtherAmount9 = myFloat.ToString();
                }
                else
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 14);//vat commission percentage in invalid
                    ThisTask.field2 = NewFlag.ToString();
                }

            }

            if (idOperatorType == 2)//for icx operators
            {//icxrevenuesharing
                float myFloat = 0;
                if (Single.TryParse(newCurrency, out myFloat))
                {
                    ThisTask.OtherAmount10 = myFloat.ToString();
                }
                else //invalid revshare% icx
                {
                    int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 15);
                    ThisTask.field2 = NewFlag.ToString();
                }
            }


        }
        else //invalid Type
        {
            int NewFlag = SetBitInteger(Convert.ToInt32(ThisTask.field2), 5);
            ThisTask.field2 = NewFlag.ToString();
        }


        //ThisRate.OtherAmount4   =OtherAmount4 ;
        //ThisRate.OtherAmount5      =OtherAmount5    ;
        //ThisRate.idCustomerRatePlanICX  =idCustomerRatePlanICX;
        //ThisRate.OtherAmount6         =OtherAmount6       ;
        //ThisRate.OtherAmount7         =OtherAmount7       ;
        //ThisRate.OtherAmount8         =OtherAmount8       ;
        //ThisRate.OtherAmount9           =OtherAmount9         ;
        //ThisRate.OtherAmount10            =OtherAmount10          ;

        ThisTask.Inactive = Convert.ToInt32(newInactive);
        ThisTask.RouteDisabled = newRouteDisabled;

        //find out new, increase or decrease; delete has been set along with rateamount
        //0=Unknown, 1=delete, 2=new, 3=increase, 4=decrease,5 =unchannged

        //find out new, rateamount= -1 will indicate rate deletion
        //change type may already been set e.g. delete=1 has been set along with rates=-1
        if (Convert.ToDouble(ThisTask.Status) != 1)//if not code delete
        {
            ThisTask.Status = "0"; //field1 is not set yet
        }
        ThisTask.Category = newServiceType;
        ThisTask.SubCategory = newSubServiceType;

        return ThisTask;
    }





    protected void GridViewSupplierRates_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {

        hidvaluerowcolorchange.Value = "";

        GridViewRow row = GridViewSupplierRates.Rows[e.RowIndex];
        string newId = ((Label)row.FindControl("lblId")).Text;
        string newPrefix = ((Label)row.FindControl("lblPrefix")).Text;
        string newDesc = "1";
        string newRateAmount = "100";

        string newServiceType = "";//Rating Rule
        string newResolution = ((TextBox)row.FindControl("txtPulse")).Text;//Assignment Order in ratetaskassign table
        Dictionary<long, rateplanassignmenttuple> dicTuple = (Dictionary<long, rateplanassignmenttuple>)Session["assign.sessdictuple"];
        Dictionary<long, enumservicefamily> dicservice = (Dictionary<long, enumservicefamily>)Session["assign.sessdicservice"];
        rateplanassignmenttuple ThisTuple = null;
        long TupleId = Convert.ToInt64(newPrefix);
        if (dicTuple.TryGetValue(TupleId, out ThisTuple))
        {
            enumservicefamily Thisservice = null;
            dicservice.TryGetValue(ThisTuple.idService, out Thisservice);
            newServiceType = Thisservice.id.ToString();
            newResolution = ThisTuple.priority.ToString();
        }

        string newMinDurationSec = "1";
        string newCountry = "1";

        //string newStartDate = Convert.ToDateTime(((AjaxControlToolkit.CalendarExtender)row.FindControl("CalendarStartDate")).SelectedDate).ToString("yyyy-MM-dd");
        string newStartDate = ((TextBox)row.FindControl("TextBoxStartDatePicker")).Text;
        //when a new start date is selected via calendar control, this will be the value in TextBoxStartDatePicker
        //but when updating enddate, comparison with previous startdate is required, TextBoxStartDatePicker will
        //not have the previous start date because the calendar has not been clicked
        if (newStartDate == "")
        {
            newStartDate = ((Label)row.FindControl("lblStartDate")).Text;
        }

        string newStartTime = ((TextBox)row.FindControl("TextBoxStartDateTimePicker")).Text;
        string newStartDateAndTime = "";
        if (newStartDate != "")
        {
            newStartDateAndTime = newStartDate + " " + newStartTime;
        }
        string newEndDate = ((TextBox)row.FindControl("TextBoxEndDatePicker")).Text;
        string newEndTime = ((TextBox)row.FindControl("TextBoxEndDateTimePicker")).Text;
        string newEndDateAndTime = "";
        if (newEndDate != "")
        {
            newEndDateAndTime = newEndDate + " " + newEndTime;
        }


        string newInactive = "1";
        string newRouteDisabled = "";// ((DropDownList)row.FindControl("DropDownListRouteDisabled")).SelectedValue;

        string newWeekDayStart = "1";
        string newWeekDayEnd = "1";
        string newStartTimeOfDay = "1";
        string newEndTimeOfDay = "1";
        string newSurchargeTime = "1";
        string newSurchargeAmount = "1";
        string newOtherAmount1 = "1";
        string newOtherAmount2 = "1";
        string newOtherAmount3 = "1";
        string newOtherAmount4 = "1";
        string newOtherAmount5 = "1";
        string newOtherAmount6 = "1";
        string newOtherAmount7 = "1";
        string newOtherAmount8 = "1";
        string newOtherAmount9 = "1";
        string newOtherAmount10 = "1";
        string newSubServiceType = "1";

        ratetaskassign ThisRateTask = new ratetaskassign();

        using (PartnerEntities ContextTask = new PartnerEntities())
        {
            Int32 idInt = int.Parse(newId);
            ThisRateTask = ContextTask.ratetaskassigns.Where(c => c.id == idInt).FirstOrDefault();

            //call create new rate to validate the rate
            //get id of the last rateplanassign for this supplier


            int idCurrentRatePlan = -1;
            //if (ViewState["sesidRatePlan"] != null)
            //{
            //    idCurrentRatePlan = (int)ViewState["sesidRatePlan"];
            //}
            idCurrentRatePlan = int.Parse(DropDownListTaskRef.SelectedValue);

            //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
            //from Partner

            string ThisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

            MySqlConnection connection = new MySqlConnection(ThisConectionString);
            string database = connection.Database.ToString();
            int idTimeZone = -1;
            using (PartnerEntities Context = new PartnerEntities())
            {
                telcobrightpartner ThisCustomer = (from c in Context.telcobrightpartners
                                                   where c.databasename == database
                                                   select c).First();
                int ThisOperatorId = ThisCustomer.idCustomer;
                idTimeZone = Context.telcobrightpartners.Where(c => c.idCustomer == ThisOperatorId).First().NativeTimeZone;
            }

            double TimeZoneOffsetSec = -360000;//set to some invalid timezone offset (in this case 100 hours)
            using (PartnerEntities Context = new PartnerEntities())
            {
                TimeZoneOffsetSec = Context.timezones.Where(c => c.id == idTimeZone).First().gmt_offset;
            }

            int newType = -1;
            if (Session["assign.sesRatePlanType"] != null)
            {
                newType = (int)Session["assign.sesRatePlanType"];
            }

            int newCurrency = -1;
            if (Session["assign.sesCurrency"] != null)
            {
                newCurrency = (int)Session["assign.sesCurrency"];
            }

            int idOperatorType = -1;
            if (Session["assign.sesidOperatorType"] != null)
            {
                idOperatorType = (int)Session["assign.sesidOperatorType"];
            }

            if (ThisRateTask.changecommitted == 0)
            {
                //before updating reset error flag
                ThisRateTask.field2 = "0";
                ThisRateTask.Status = "0";

                ratetaskassign NewRate = CreateNewRateTask(idCurrentRatePlan,
                    newId, newPrefix, newDesc, newRateAmount, newResolution, newMinDurationSec,
                    newCountry, newStartDateAndTime,
                    newEndDateAndTime, newInactive, newRouteDisabled,
                    newWeekDayStart,
                    newWeekDayEnd,
                    newStartTime,
                    newEndTime,
                    newSurchargeTime,
                    newSurchargeAmount,
                    newType.ToString(),
                    newOtherAmount1,
                    newOtherAmount2,
                    newOtherAmount3,
                    newOtherAmount4,
                    newOtherAmount5,
                    newOtherAmount6,
                    newOtherAmount7,
                    newOtherAmount8,
                    newOtherAmount9,
                    newOtherAmount10,
                    newCurrency.ToString(),
                    idOperatorType,
                    TimeZoneOffsetSec,
                    newSurchargeTime,
                    newSurchargeAmount, newServiceType, newSubServiceType);

                //now update, new rate has validation and other flags set...

                ThisRateTask.Prefix = NewRate.Prefix;
                ThisRateTask.description = NewRate.description;
                ThisRateTask.rateamount = NewRate.rateamount;
                ThisRateTask.Resolution = NewRate.Resolution;
                //ThisRate.CountryCode = NewRate.CountryCode;
                ThisRateTask.startdate = NewRate.startdate;
                ThisRateTask.enddate = NewRate.enddate;
                ThisRateTask.Inactive = NewRate.Inactive;
                ThisRateTask.RouteDisabled = NewRate.RouteDisabled;
                ThisRateTask.Status = NewRate.field1;
                ThisRateTask.field2 = NewRate.field2;
                ThisRateTask.id = NewRate.id;
                ThisRateTask.field3 = NewRate.field3;

                ThisRateTask.WeekDayStart = NewRate.WeekDayStart;
                ThisRateTask.WeekDayEnd = NewRate.WeekDayEnd;
                ThisRateTask.starttime = NewRate.starttime;
                ThisRateTask.endtime = NewRate.endtime;
                ThisRateTask.SurchargeTime = NewRate.SurchargeTime;
                ThisRateTask.SurchargeAmount = NewRate.SurchargeAmount;
                ThisRateTask.Type = NewRate.Type;
                ThisRateTask.OtherAmount1 = NewRate.OtherAmount1;
                ThisRateTask.OtherAmount2 = NewRate.OtherAmount2;
                ThisRateTask.OtherAmount3 = NewRate.OtherAmount3;
                ThisRateTask.OtherAmount4 = NewRate.OtherAmount4;
                ThisRateTask.OtherAmount5 = NewRate.OtherAmount5;
                ThisRateTask.OtherAmount6 = NewRate.OtherAmount6;
                ThisRateTask.OtherAmount7 = NewRate.OtherAmount7;
                ThisRateTask.OtherAmount8 = NewRate.OtherAmount8;
                ThisRateTask.OtherAmount9 = NewRate.OtherAmount9;
                ThisRateTask.OtherAmount10 = NewRate.OtherAmount10;
                ThisRateTask.Currency = NewRate.Currency;

                //set rateplan
                DropDownList ddlRatePlan = (DropDownList)(GridViewSupplierRates.Rows[e.RowIndex].FindControl("DropDownListRatePlan"));
                ThisRateTask.Inactive = Convert.ToInt32(ddlRatePlan.SelectedValue);
                ContextTask.SaveChanges();
                CommitChanges();// to make the changes permanent in the rateassign table
            }//if uncommited task
            else
            {//committed task, allow only change of end date
                //for commited task, only end date change is allowed
                string thisPrefix = ((Label)row.FindControl("lblPrefix")).Text;
                Label lblRateErrors = (Label)(GridViewSupplierRates.Rows[e.RowIndex].FindControl("lblRateErrors"));
                DateTime ChangedEndDateTime = new DateTime();
                DateTime SaveStartDateTimeThisRate = new DateTime();
                DateTime.TryParseExact(((Label)row.FindControl("lblStartDate")).Text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out SaveStartDateTimeThisRate);
                if (newEndDateAndTime != "")//new end time not empty
                {
                    bool ValidEndTime = false;
                    if (DateTime.TryParseExact(newEndDateAndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out ChangedEndDateTime))
                    {
                        ValidEndTime = true;
                        if (ChangedEndDateTime < SaveStartDateTimeThisRate)
                        {
                            //GridViewSupplierRates.Rows[e.RowIndex].Attributes.Add("style", "color:Red;"); 
                            //could not make the following work, gridview doesn't change color in the item template
                            //lblRateErrors.Text = "End datetime must be greater than start datetime";

                            //alternate
                            hidvaluerowcolorchange.Value = e.RowIndex + "," + "End datetime must be greater than start datetime";
                            ValidEndTime = false;
                            return;
                        }
                        //changedenddatetime must not overlap next startdatetime
                        rate NextRate = null;
                        using (PartnerEntities Context = new PartnerEntities())
                        {
                            NextRate = Context.rates.Where(c => c.idrateplan == 1 && c.Prefix == thisPrefix
                                                                && c.startdate > SaveStartDateTimeThisRate)
                                .OrderBy(c => c.startdate).Take(1).ToList().FirstOrDefault();
                        }
                        if (NextRate != null && ChangedEndDateTime > NextRate.startdate)
                        {
                            hidvaluerowcolorchange.Value = e.RowIndex + "," + "End datetime cannot overlap next effective date.";
                            ValidEndTime = false;
                            return;
                        }
                    }
                    else//new enddatetime is given but, format invalid
                    {
                        hidvaluerowcolorchange.Value = e.RowIndex + "," + "Invalid End Datetime.";
                        return;
                    }
                    if (ValidEndTime == true)
                    {
                        //update enddatetime for this assignment
                        //has to be transaction safe
                        //ratetaskassign and rateassign has be in synced always
                        using (PartnerEntities Context = new PartnerEntities())
                        {
                            rateassign ThisRate = Context.rateassigns.Where(c => c.idrateplan == 1 && c.Prefix == Convert.ToInt32(thisPrefix)
                                                                                 && c.startdate == SaveStartDateTimeThisRate).FirstOrDefault();

                            using (MySqlConnection Con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                            {
                                Con.Open();
                                using (MySqlCommand Cmd = new MySqlCommand("", Con))
                                {
                                    DropDownList ddlRatePlan = (DropDownList)(GridViewRateAssign.Rows[e.RowIndex].FindControl("DropDownListRatePlan"));

                                    Cmd.CommandText = " set autocommit=0;";
                                    Cmd.ExecuteNonQuery();
                                    string EndDateStr = ChangedEndDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                    //update enddate in ratetaskassign table
                                    Cmd.CommandText = " update ratetaskassign set enddate='" + EndDateStr + "' " +
                                                      " where id=" + ThisRateTask.id.ToString();
                                    Cmd.ExecuteNonQuery();
                                    //update enddate in rateassign table
                                    Cmd.CommandText = " update rateassign set enddate='" + EndDateStr + "',inactive=" + ddlRatePlan.SelectedValue +
                                                      " where id=" + ThisRate.id.ToString();
                                    Cmd.ExecuteNonQuery();
                                    Cmd.CommandText = " commit;";
                                    Cmd.ExecuteNonQuery();
                                    Cmd.CommandText = " set autocommit=1;";
                                    Cmd.ExecuteNonQuery();

                                    StatusLabel.ForeColor = Color.Green;
                                    StatusLabel.Text = "Updated Successfully";

                                }
                            }
                        }


                    }
                }
                else//enddatetime kept empty
                {
                    // do nothing....
                }
            }
        }//using context task

        GridViewSupplierRates.EditIndex = -1;
        myGridViewDataBind();

        var color = ColorTranslator.FromHtml("#B1B1B3");
        StatusLabel.ForeColor = color;







    }
    protected void GridViewSupplierRates_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        GridViewSupplierRates.EditIndex = -1;
        myGridViewDataBind();
        StatusLabel.Text = "";
    }
    protected void SupplierRateObjectDataSource_OnUpdated(object sender, ObjectDataSourceStatusEventArgs e)
    {

    }
    protected void SupplierRateObjectDataSource_OnInserted(object sender, ObjectDataSourceStatusEventArgs e)
    {

    }
    protected void SupplierRateObjectDataSource_OnDeleted(object sender, ObjectDataSourceStatusEventArgs e)
    {

    }
    protected void GridViewSupplierRates_RowUpdated(object sender, GridViewUpdatedEventArgs e)
    {
        if (hidvaluerowcolorchange.Value != "")
        {
            //couldnot change row color after validation fail in row updating
            //taking help of hiddenfield
            string rowcolorchange = "";
            rowcolorchange = hidvaluerowcolorchange.Value;
            if (rowcolorchange.Length > 1 && rowcolorchange.Contains(","))
            {
                int TargetRow = -1;
                int.TryParse(rowcolorchange.Split(',')[0], out TargetRow);
                if (TargetRow > -1)
                {
                    string Msg = rowcolorchange.Split(',')[1];
                    StatusLabel.ForeColor = Color.Red;
                    StatusLabel.Text = Msg;
                }
            }
            e.KeepInEditMode = true;
        }
    }


    protected void LinkButton1_Click(object sender, EventArgs e)
    {

        //frmSupplierRatePlanInsert.DataBind();
        frmSupplierRatePlanInsert.ChangeMode(FormViewMode.Insert);
        frmSupplierRatePlanInsert.Visible = true;
    }



    protected void FormViewCancel_Click(object sender, EventArgs e)
    {
        frmSupplierRatePlanInsert.Visible = false;
    }

    protected void frmSupplierRatePlanInsert_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {
        frmSupplierRatePlanInsert.Visible = false;
        myGridViewDataBind();
    }

    protected void frmSupplierRatePlanInsert_ItemInserting(object sender, FormViewInsertEventArgs e)
    {

        try
        {
            string newDesc = "";
            string newPrefix = "9999"; //tuple, will be found later

            //partner
            string newCountry = ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListPartner")).SelectedValue;

            //route
            string newRouteDisabled = ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListRoute")).SelectedValue;


            //rating rule
            string newServiceType = ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListservice")).SelectedValue;

            //priority
            string newResolution = ((TextBox)frmSupplierRatePlanInsert.FindControl("txtResolution")).Text;

            //assigned direction
            string newSubServiceType = ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListAssignedDirection")).SelectedValue;



            DropDownList ServiceType = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListAssignedDirection");
            DropDownList ddlistSf = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListservice");
            DropDownList ddlBillingRule = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownBillingRule");
            if (ServiceType.SelectedIndex == 2)
            {
                billingRule = "";
                paymentMode = "";
            }
            else
            {
                billingRule = ddlBillingRule.SelectedValue;
            }
            // BillingInformation bl = new BillingInformation(billingRule, paymentMode);
            // var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(bl);
            //Console.WriteLine(jsonString);
            if (ddlistSf.SelectedIndex == 0)
            {
                StatusLabel.ForeColor = Color.Red;
                StatusLabel.Text = "No Service Family Selected!";
                return;
            }
            if (ddlBillingRule.SelectedIndex == 0)
            {
                StatusLabel.ForeColor = Color.Red;
                StatusLabel.Text = "No Billing Rule Selected!";
                return;
            }
            Dictionary<string, enumservicefamily> dicServiceFamily = new Dictionary<string, enumservicefamily>();
            using (PartnerEntities Context = new PartnerEntities())
            {
                dicServiceFamily = Context.enumservicefamilies.ToDictionary(c => c.id.ToString());
            }
            enumservicefamily ThisSf = dicServiceFamily[ddlistSf.SelectedValue];
            if (ThisSf.PartnerAssignNotNeeded == 0)//partner assign required
            {
                if (Convert.ToInt32(newCountry) <= 0)
                {
                    StatusLabel.ForeColor = Color.Red;
                    StatusLabel.Text = "No Partner Selected!";
                    return;
                }
                if (Convert.ToInt32(newSubServiceType) <= 0)
                {
                    StatusLabel.ForeColor = Color.Red;
                    StatusLabel.Text = "Rateplan Assignment Direction is not selected!";
                    return;
                }
            }

            //if assignment not applicable, set it to 0
            using (PartnerEntities Conmed = new PartnerEntities())
            {
                using (PartnerEntities Context = new PartnerEntities())
                {
                    int Tempservice = Convert.ToInt32(newServiceType);
                    int CarAssignNotReq = Convert.ToInt32(Conmed.enumservicefamilies.Where(c => c.id == Tempservice).First().PartnerAssignNotNeeded);
                    if (CarAssignNotReq == 1)
                    {
                        newSubServiceType = "0";
                    }
                }
            }
            //id rate plan
            string stridRatePlan = ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListRatePlan")).SelectedValue;
            int tempint = -1;
            if (int.TryParse(stridRatePlan, out tempint) == false)
            {
                StatusLabel.ForeColor = Color.Red;
                StatusLabel.Text = "No Rate Plan Selected!";
                return;
            }



            string newInactive = tempint.ToString();

            string newMinDurationSec = "1";
            string newStartDate = ((TextBox)frmSupplierRatePlanInsert.FindControl("TextBoxStartDatePickerFrm")).Text;
            string newStartTime = ((TextBox)frmSupplierRatePlanInsert.FindControl("TextBoxStartDateTimePickerFrm")).Text;
            string newStartDateAndTime = "";
            if (newStartDate != "")
            {
                newStartDateAndTime = newStartDate + " " + newStartTime;
            }

            //validate start date time in advance
            DateTime EffectiveStartDate = new DateTime();
            if (DateTime.TryParseExact(newStartDateAndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out EffectiveStartDate) == false)
            {
                StatusLabel.Text = "No Effective Datetime!";
                return;
            }

            string newEndDate = ((TextBox)frmSupplierRatePlanInsert.FindControl("TextBoxEndDatePickerFrm")).Text;
            string newEndTime = ((TextBox)frmSupplierRatePlanInsert.FindControl("TextBoxEndDateTimePickerFrm")).Text;
            string newEndDateAndTime = "";
            if (newEndDate != "")
            {
                newEndDateAndTime = newEndDate + " " + newEndTime;
            }

            DateTime? EffectiveEndDate = null;    //end date can be null, so assign null if no valid enddate time is present
            DateTime TempEndDate = new DateTime();
            if (DateTime.TryParseExact(newEndDateAndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out TempEndDate) == true)
            {
                EffectiveEndDate = TempEndDate;
            }



            string newRateAmount = "100";
            string newWeekDayStart = "1";
            string newWeekDayEnd = "1";
            string newStartTimeOfDay = "1";
            string newEndTimeOfDay = "1";
            string newSurchargeTime = "1";
            string newSurchargeAmount = "1";
            string newOtherAmount1 = "1";
            string newOtherAmount2 = "1";
            string newOtherAmount3 = "1";
            string newOtherAmount4 = "1";
            string newOtherAmount5 = "1";
            string newOtherAmount6 = "1";
            string newOtherAmount7 = "1";
            string newOtherAmount8 = "1";
            string newOtherAmount9 = "1";
            string newOtherAmount10 = "1";



            string newId = "-1";

            //call create new rate to validate the rate

            int idCurrentRatePlan = -1;
            //if (ViewState["sesidRatePlan"] != null)
            //{
            //    idCurrentRatePlan = (int)ViewState["sesidRatePlan"];
            //}
            idCurrentRatePlan = int.Parse(DropDownListTaskRef.SelectedValue);

            //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
            //from Partner

            string ThisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

            MySqlConnection connection = new MySqlConnection(ThisConectionString);
            string database = connection.Database.ToString();
            int idTimeZone = -1;
            using (PartnerEntities Context = new PartnerEntities())
            {
                telcobrightpartner ThisCustomer = (from c in Context.telcobrightpartners
                                                   where c.databasename == database
                                                   select c).First();
                int ThisOperatorId = ThisCustomer.idCustomer;
                idTimeZone = Context.telcobrightpartners.Where(c => c.idCustomer == ThisOperatorId).First().NativeTimeZone;
            }

            double TimeZoneOffsetSec = -360000;//set to some invalid timezone offset (in this case 100 hours)
            using (PartnerEntities Context = new PartnerEntities())
            {
                TimeZoneOffsetSec = Context.timezones.Where(c => c.id == idTimeZone).First().gmt_offset;
            }

            int newType = -1;
            if (Session["assign.sesRatePlanType"] != null)
            {
                newType = (int)Session["assign.sesRatePlanType"];
            }

            int newCurrency = -1;
            if (Session["assign.sesCurrency"] != null)
            {
                newCurrency = (int)Session["assign.sesCurrency"];
            }

            int idOperatorType = -1;
            if (Session["assign.sesidOperatorType"] != null)
            {
                idOperatorType = (int)Session["assign.sesidOperatorType"];
            }



            ratetaskassign NewRate = CreateNewRateTask(idCurrentRatePlan,
                newId, newPrefix, newDesc, newRateAmount, newResolution, newMinDurationSec
                , newCountry, newStartDateAndTime,
                newEndDateAndTime, newInactive, newRouteDisabled,
                newWeekDayStart,
                newWeekDayEnd,
                newStartTimeOfDay,
                newEndTimeOfDay,
                newSurchargeTime,
                newSurchargeAmount,
                newType.ToString(),
                newOtherAmount1,
                newOtherAmount2,
                newOtherAmount3,
                newOtherAmount4,
                newOtherAmount5,
                newOtherAmount6,
                newOtherAmount7,
                newOtherAmount8,
                newOtherAmount9,
                newOtherAmount10,
                newCurrency.ToString(),
                idOperatorType,
                TimeZoneOffsetSec,
                newSurchargeTime,
                newSurchargeAmount,
                newServiceType,
                newSubServiceType
            );


            using (PartnerEntities Context = new PartnerEntities())
            {
                //first check if the rate assignment tuple exists in rateplanassignmenttuple
                int? NewidPartner = Convert.ToInt32(NewRate.CountryCode);
                int? NewRoute = null;
                int tInt = 0;
                if (int.TryParse(NewRate.RouteDisabled, out tInt))
                {
                    NewRoute = tInt;
                }
                int NewserviceId = Convert.ToInt32(NewRate.Category);
                int NewPriority = Convert.ToInt32(NewRate.Resolution);
                int NewAssignDirection = Convert.ToInt32(NewRate.SubCategory);


                rateplanassignmenttuple ExistingTuple = null;
                if (NewRoute == -1)//route not used
                {
                    if (NewidPartner == 0)
                    {
                        ExistingTuple = Context.rateplanassignmenttuples.Where(c => c.idpartner == null &&
                                                                                    c.route == null &&
                                                                                    c.idService == NewserviceId &&
                                                                                    c.priority == NewPriority &&
                                                                                    c.AssignDirection == NewAssignDirection).ToList().FirstOrDefault();
                    }
                    else
                    {
                        ExistingTuple = Context.rateplanassignmenttuples.Where(c => c.idpartner == NewidPartner &&
                                                                                    c.route == null &&
                                                                                    c.idService == NewserviceId &&
                                                                                    c.priority == NewPriority &&
                                                                                    c.AssignDirection == NewAssignDirection).ToList().FirstOrDefault();
                    }
                }
                else//route used
                {
                    if (NewidPartner == 0)
                    {
                        ExistingTuple = Context.rateplanassignmenttuples.Where(c => c.idpartner == null &&
                                                                                    c.route == NewRoute &&
                                                                                    c.idService == NewserviceId &&
                                                                                    c.priority == NewPriority &&
                                                                                    c.AssignDirection == NewAssignDirection).ToList().FirstOrDefault();
                    }
                    else
                    {
                        ExistingTuple = Context.rateplanassignmenttuples.Where(c => c.idpartner == NewidPartner &&
                                                                                    c.route == NewRoute &&
                                                                                    c.idService == NewserviceId &&
                                                                                    c.priority == NewPriority &&
                                                                                    c.AssignDirection == NewAssignDirection).ToList().FirstOrDefault();
                    }
                }
                //following is an extra validation to check overlap, if it is validated here
                //the code goes through ratetask assignment routine, gets validated again there.
                //purpose is to tyr to prevent the ratetaskassignment additional griview to show up
                if (ExistingTuple != null)
                {
                    DateRange dRange = new DateRange();
                    dRange.StartDate = EffectiveStartDate;
                    dRange.EndDate = (EffectiveEndDate == null ? new DateTime(9999, 12, 31, 23, 59, 59) : Convert.ToDateTime(EffectiveEndDate));

                    //load all the assigned rateplans for this tuple
                    List<rateassign> lstAssignments = Context.rateassigns.Where(c => c.Prefix == ExistingTuple.id).ToList();
                    //there can be only one assignment with enddate=null
                    int ExistingAssignmentCount = lstAssignments.Count;
                    int OpenAssignmentsCount = lstAssignments.Where(c => c.enddate == null).Count();
                    if (ExistingAssignmentCount > 0)
                    {
                        if (OpenAssignmentsCount > 1)
                        {
                            StatusLabel.ForeColor = Color.Red;
                            StatusLabel.Text = "There can be only one assigned rateplan open i.e. without end date!";
                            return;
                        }
                        rateassign LatestAssignment = lstAssignments.OrderByDescending(c => c.startdate).First();
                        rateassign FirstAssignment = lstAssignments.OrderByDescending(c => c.startdate).First();
                        //if this assignemnt=the latest assignment
                        if (LatestAssignment != null && dRange.StartDate == LatestAssignment.startdate)
                        {
                            StatusLabel.ForeColor = Color.Red;
                            StatusLabel.Text = "Effective Datetime overlaps with existing assignment starting at " + LatestAssignment.startdate.ToString("yyyy-MM-dd HH:mm:ss") + ". " +
                                               " To Assign multiple Rateplan for the same combination of [Service Family, Assigned Order, Assign Direction, Customer/Supplier/Route!],<br/>" +
                                               " Use a different value for Assigned Order or a different effective datetime which does not overlap any existing assignment.";
                            e.Cancel = true;
                            return;
                        }
                        if (dRange.StartDate < FirstAssignment.startdate)//before all
                        {
                            EffectiveEndDate = FirstAssignment.startdate;
                        }
                        //check overlap with all assignment except latest one
                        List<rateassign> AllExceptLatest = lstAssignments.Where(c => c.id != LatestAssignment.id).ToList();
                        foreach (rateassign ra in AllExceptLatest)
                        {
                            DateRange CompareWith = new DateRange() { StartDate = ra.startdate, EndDate = Convert.ToDateTime(ra.enddate) };
                            if (Util.DateIntersection(dRange, CompareWith) != null)
                            {
                                StatusLabel.ForeColor = Color.Red;
                                StatusLabel.Text = "Effective Datetime overlaps with existing assignment starting at " + ra.startdate.ToString("yyyy-MM-dd HH:mm:ss") + ". " +
                                                   " To Assign multiple Rateplan for the same combination of [Service Family, Assigned Order, Assign Direction, Customer/Supplier/Route!],<br/>" +
                                                   " Use a different value for Assigned Order or a different effective datetime which does not overlap any existing assignment.";
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                    //else.....first assignment of this kind


                }

                if (ExistingTuple == null)//tuple does not exist
                {
                    //todo: fix autoincrement work around currently implemented for billingRuleassignment
                    int maxIdRatePlanAssignmentTuple = 0;
                    using (PartnerEntities context = new PartnerEntities())
                    {
                        if (context.rateplanassignmenttuples.Any())
                        {
                            maxIdRatePlanAssignmentTuple = context.rateplanassignmenttuples.Max(c => c.id);
                        }
                    }
                    rateplanassignmenttuple newTuple =
                        new rateplanassignmenttuple() { id = ++maxIdRatePlanAssignmentTuple };
                    if (NewidPartner == 0)//has to be null in the database
                    {
                        newTuple.idpartner = null;
                    }
                    else
                    {
                        newTuple.idpartner = NewidPartner;
                    }
                    if (NewRoute == -1)
                    {
                        newTuple.route = null;
                    }
                    else
                    {
                        newTuple.route = NewRoute;
                    }
                    newTuple.idService = NewserviceId;
                    newTuple.priority = NewPriority;
                    newTuple.AssignDirection = NewAssignDirection;
                    Context.rateplanassignmenttuples.Add(newTuple);
                    //insert billingRule
                    ddlBillingRule = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownBillingRule");
                    DropDownList ddlserviceGroup = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListServiceGroup");
                    int selectedBillingRule = Convert.ToInt32(ddlBillingRule.SelectedValue);
                    int selectedServiceGroup = Convert.ToInt32(ddlserviceGroup.SelectedValue);
                    billingruleassignment billingruleassignment = new billingruleassignment()
                    {
                        idRatePlanAssignmentTuple = newTuple.id,
                        idServiceGroup = selectedServiceGroup,
                        idBillingRule = selectedBillingRule
                    };
                    Context.billingruleassignments.Add(billingruleassignment);
                    Context.SaveChanges();
                }

                //get the id of the tuple
                //surprise, setting nullable var=null then using it in linq doesn't work
                int tupleId = -1;

                if (NewRoute == -1)//route not used
                {
                    if (NewidPartner == 0)
                    {
                        tupleId = Context.rateplanassignmenttuples.Where(c => c.idpartner == null &&
                                                                              c.route == null &&
                                                                              c.idService == NewserviceId &&
                                                                              c.priority == NewPriority &&
                                                                              c.AssignDirection == NewAssignDirection).FirstOrDefault().id;
                    }
                    else
                    {
                        tupleId = Context.rateplanassignmenttuples.Where(c => c.idpartner == NewidPartner &&
                                                                              c.route == null &&
                                                                              c.idService == NewserviceId &&
                                                                              c.priority == NewPriority &&
                                                                              c.AssignDirection == NewAssignDirection).FirstOrDefault().id;
                    }
                }
                else//route used
                {
                    if (NewidPartner == 0)
                    {
                        tupleId = Context.rateplanassignmenttuples.Where(c => c.idpartner == null &&
                                                                              c.route == NewRoute &&
                                                                              c.idService == NewserviceId &&
                                                                              c.priority == NewPriority &&
                                                                              c.AssignDirection == NewAssignDirection).FirstOrDefault().id;
                    }
                    else
                    {
                        tupleId = Context.rateplanassignmenttuples.Where(c => c.idpartner == NewidPartner &&
                                                                              c.route == NewRoute &&
                                                                              c.idService == NewserviceId &&
                                                                              c.priority == NewPriority &&
                                                                              c.AssignDirection == NewAssignDirection).FirstOrDefault().id;
                    }
                }

                NewRate.Prefix = tupleId;//represents the tuple
                NewRate.RouteDisabled = newRouteDisabled;
                NewRate.Inactive = Convert.ToInt32(newInactive);//id rate plan

                //LCR Flag
                var ddlExcludeLCR = frmSupplierRatePlanInsert.FindControl("ddlExcludeLCR") as DropDownList;
                NewRate.field3 = ddlExcludeLCR.SelectedValue;


                if (NewRate.CountryCode == 0) NewRate.CountryCode = null;//countryCode in ratetaskassign=idpartner,0=null=no partner
                Context.ratetaskassigns.Add(NewRate);
                Context.SaveChanges();//entry  has been added to ratetaskassign table


                //call commit changes to add them in the rateassign table
                CommitChanges();
                //bad code for now, not transaction support 
                //find a way to fix it sometime
                //  string billingRuleName = ((DropDownList) frmSupplierRatePlanInsert.FindControl("DropDownBillingRule")).SelectedItem.Value;
                //genericparameterassignment g =
                //GetAdditionalBillingParamsAsGenericAssignment(NewRate.Prefix, Context);
                //try
                //{
                //  Context.genericparameterassignments.Add(additionalBillingParamsAsGenericAssignment);
                // Context.SaveChanges();
                //}
                //catch (Exception exception)
                //{

                //  throw;
                //}
            }

            frmSupplierRatePlanInsert.Visible = false;
            CreateCustomerServiceAccounts();
            myGridViewDataBind();
            Response.Redirect("rateassignment.aspx");
            //var color = ColorTranslator.FromHtml("#B1B1B3");
            //StatusLabel.ForeColor = color;
            //StatusLabel.Text = "Changes are not committed to rate table until 'Save All Changes' clicked!";
            StatusLabel.ForeColor = Color.Green;
            StatusLabel.Text = "Rateplan Successfully Assigned";
        }
        catch (Exception e1)
        {
            StatusLabel.ForeColor = Color.Red;
            StatusLabel.Text = e1.Message + "<br/>" + (e1.InnerException != null ? e1.InnerException.ToString() : "");
        }

    }

    private static void CreateCustomerServiceAccounts()
    {
        using (PartnerEntities context = new PartnerEntities())
        {
            AccountingContext accContext = new AccountingContext(context, 0,
                new AutoIncrementManagerManualInt(context), new List<DateTime>(), 10000);
            AccountCreatorFromRatePlanAssignment accountCreator =
                new AccountCreatorFromRatePlanAssignment(accContext, context);
            accountCreator.CreateAllMissingCustomerAccountsFromRatePlanAssignmentInfo();
        }
    }

    protected void frmSupplierRatePlanInsert_Load(object sender, EventArgs e)
    {
    }

    protected void frmSupplierRatePlanInsert_ItemCreated(object sender, EventArgs e)
    {
        ((TextBox)frmSupplierRatePlanInsert.FindControl("TextBoxStartDatePickerFrm")).Text = (new DateTime(2000, 1, 1)).ToString("yyyy-MM-dd");
        ((TextBox)frmSupplierRatePlanInsert.FindControl("TextBoxStartDateTimePickerFrm")).Text = "00:00:00";

        // DropDownList ruleddl = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownBillingRule");
        //  DropDownList paymentddl = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListPaymentMethod");

        //ruleddl.DataSource = _billingRules;
        //ruleddl.DataTextField = "value";
        //ruleddl.DataValueField = "key";
        //ruleddl.DataBind();

        //foreach (string s in _paymentMethods)
        //{
        //    paymentddl.Items.Add(s);
        //}






    }

    public IEnumerable<string> ReadLines(Func<Stream> streamProvider,
        Encoding encoding, string RemoveChar)
    {
        using (var stream = streamProvider())
        using (var reader = new StreamReader(stream, encoding))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (RemoveChar == "") yield return line;
                else yield return line.Replace(RemoveChar, "");
            }
        }
    }

    protected void UploadButton_Click(object sender, EventArgs e)
    {

        if (FileUploadControl.HasFile)
        {
            try
            {
                //if (FileUploadControl.PostedFile.ContentType == "text/plain")
                //{
                if (FileUploadControl.PostedFile.ContentLength < 5242880)
                {

                    //<asp:ListItem Value="-1"> [Select]</asp:ListItem>
                    //<asp:ListItem Value="1"> Generic</asp:ListItem>
                    //<asp:ListItem Value="2"> Tata</asp:ListItem>
                    //<asp:ListItem Value="3"> Bharti</asp:ListItem>
                    //<asp:ListItem Value="4"> IDT</asp:ListItem>
                    //make sure format is selected for the file to be uploaded...
                    if (DropDownListFormat.SelectedValue == "-1")
                    {
                        var color = ColorTranslator.FromHtml("#FF0000");
                        StatusLabel.ForeColor = color;
                        StatusLabel.Text = "No Rate Plan format select for the input file!";
                    }

                    string filename = Path.GetFileName(FileUploadControl.FileName);

                    //delete all files in the temp directory first...
                    System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(Request.PhysicalApplicationPath + "\\config\\temp");

                    //delete all instance of excel, because it wasn't getting ended by the code of excel instancing
                    foreach (Process process in Process.GetProcessesByName("Excel"))
                    {
                        process.Kill();
                    }
                    //kill again, doesn't want to die
                    foreach (Process process in Process.GetProcessesByName("Excel"))
                    {
                        process.Kill();
                    }


                    foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e1)
                        {
                            //kill again, doesn't want to die
                            foreach (Process process in Process.GetProcessesByName("Excel"))
                            {
                                process.Kill();
                            }
                            file.Delete();
                        }
                    }
                    foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    //delete previous tasks under current task reference
                    using (MySqlConnection DelCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                    {
                        DelCon.Open();
                        using (MySqlCommand DelCmd = new MySqlCommand("", DelCon))
                        {
                            DelCmd.CommandText = " delete from ratetaskassign where idrateplan=" + DropDownListTaskRef.SelectedValue;
                            DelCmd.ExecuteNonQuery();

                        }
                    }


                    //get session id to append to the filename
                    HttpSessionState ss = HttpContext.Current.Session;

                    FileUploadControl.SaveAs(Request.PhysicalApplicationPath + "\\config\\temp\\" + filename + "_" + ss.SessionID.ToString());
                    int RatePlanFormat = int.Parse(DropDownListFormat.SelectedValue);

                    //<asp:ListItem Value="-1"> [Select]</asp:ListItem>
                    //<asp:ListItem Value="1"> Generic (Excel-Native)</asp:ListItem>
                    //<asp:ListItem Value="2"> Generic (Text/CSV)</asp:ListItem>
                    //<asp:ListItem Value="101"> Tata</asp:ListItem>
                    //<asp:ListItem Value="102"> Bharti</asp:ListItem>
                    //<asp:ListItem Value="103"> IDT</asp:ListItem>
                    List<string[]> strLines = new List<string[]>();
                    rateplanassign ThisRatePlan = new rateplanassign();
                    ThisRatePlan = (rateplanassign)ViewState["vsRatePlan"];
                    //myExcel pExcel = new myExcel();
                    string Retval = "";//pExcel.GetRates(Request.PhysicalApplicationPath + "\\config\\temp\\" + filename + "_" + ss.SessionID.ToString(), ref strLines, ThisRatePlan,(CheckBoxDefaultDeleteDate.Checked==true?true:false));
                    if (Retval != "")
                    {
                        var color = ColorTranslator.FromHtml("#FF0000");
                        StatusLabel.ForeColor = color;
                        StatusLabel.Text = "Error occured during rate parsing." + Retval;
                        return;
                    }



                    int idCurrentRatePlan = -1;
                    idCurrentRatePlan = int.Parse(DropDownListTaskRef.SelectedValue);

                    double TimeZoneOffsetSec = -360000;//set to some invalid timezone offset (in this case 100 hours)
                    using (PartnerEntities Context = new PartnerEntities())
                    {
                        int idTimeZone = Context.ratetaskreferences.Where(c => c.id == idCurrentRatePlan).First().rateplan.TimeZone;
                        TimeZoneOffsetSec = Context.timezones.Where(c => c.id == idTimeZone).First().gmt_offset;
                    }


                    int newCurrency = 0;

                    int RatePlanType = -1;
                    RatePlanType = ThisRatePlan.Type;

                    if (RatePlanType == -1)
                    {
                        var color = ColorTranslator.FromHtml("#FF0000");
                        StatusLabel.ForeColor = color;
                        StatusLabel.Text = "Invalid Rateplan Type!";
                    }

                    //use tmp negetive id, id field is auto increment, just requires temp id for gridview
                    long MinidInt = -1;


                    int Iteration = 0;
                    //int ErrorCount = 0;
                    using (PartnerEntities Context = new PartnerEntities())
                    {
                        for (Iteration = 0; Iteration <= strLines.Count - 1; Iteration++)
                        {

                            //string str = strLines[Iteration];
                            string[] TheseFields = strLines[Iteration];//str.Split(',');//LineToFields(str);

                            string newId = Convert.ToString(MinidInt - Iteration - 1);

                            string newPrefix = "";
                            string newDesc = "";
                            string newRateAmount = "";
                            string newResolution = "";
                            string newMinDurationSec = "";
                            string newCountry = "";
                            string newStartDateAndTime = "";
                            string newEndDateAndTime = "";
                            string newSurchargeTime = "";
                            string newSurchargeAmount = "";
                            string newInactive = "0";
                            string newRouteDisabled = "0";

                            string newOtherAmount4 = "0";
                            string newOtherAmount5 = "0";
                            string newOtherAmount6 = "0";
                            string newOtherAmount7 = "0";
                            string newOtherAmount8 = "0";
                            string newOtherAmount9 = "0";
                            string newOtherAmount10 = "0";

                            string newOtherAmount1 = "0";
                            string newOtherAmount2 = "0";
                            string newOtherAmount3 = "0";

                            string newServiceType = "1";
                            string NewSubServiceType = "1";

                            switch (RatePlanType)
                            {
                                case 1://customer
                                case 2://supplier
                                    newPrefix = TheseFields[0];
                                    newDesc = TheseFields[1];
                                    newRateAmount = TheseFields[2];
                                    newResolution = TheseFields[3];
                                    newMinDurationSec = TheseFields[4];

                                    newCountry = TheseFields[5];
                                    newStartDateAndTime = TheseFields[6];
                                    newEndDateAndTime = TheseFields[7];
                                    newSurchargeTime = TheseFields[8];
                                    newSurchargeAmount = TheseFields[9];
                                    int TempInt = 1;
                                    if (int.TryParse(TheseFields[10], out TempInt) == true)//default=1
                                    {
                                        newServiceType = TempInt.ToString();
                                    }
                                    if (int.TryParse(TheseFields[11], out TempInt) == true)
                                    {
                                        NewSubServiceType = TempInt.ToString();
                                    }
                                    newInactive = "0";
                                    newRouteDisabled = "0";
                                    break;
                                case 4://international outgoing
                                    newPrefix = TheseFields[0];
                                    newDesc = TheseFields[1];
                                    //newRateAmount = TheseFields[2];
                                    newResolution = TheseFields[2];
                                    newOtherAmount4 = TheseFields[3];
                                    newOtherAmount5 = TheseFields[4];

                                    newOtherAmount6 = TheseFields[5];
                                    newOtherAmount7 = TheseFields[6];
                                    newOtherAmount8 = TheseFields[7];
                                    newOtherAmount9 = TheseFields[8];
                                    newCountry = TheseFields[9];
                                    newStartDateAndTime = TheseFields[10];
                                    if (TheseFields.GetLength(0) >= 12)
                                    {
                                        newEndDateAndTime = TheseFields[11];
                                    }
                                    if (TheseFields.GetLength(0) >= 13)
                                    {
                                        newMinDurationSec = TheseFields[12];
                                    }
                                    break;

                            }

                            int idOperatorType = 0;
                            if (Session["assign.sesidOperatorType"] != null)
                            {
                                idOperatorType = (int)Session["assign.sesidOperatorType"];
                            }

                            ratetaskassign NewRate = CreateNewRateTask(idCurrentRatePlan,
                                newId, newPrefix, newDesc, newRateAmount, newResolution, newMinDurationSec,
                                newCountry, newStartDateAndTime,
                                newEndDateAndTime, newInactive, newRouteDisabled,
                                "1",
                                "7",
                                "000000",
                                "235959",
                                "0",
                                "0.0",
                                RatePlanType.ToString(),
                                newOtherAmount1,
                                newOtherAmount2,
                                newOtherAmount3,
                                newOtherAmount4,
                                newOtherAmount5,
                                newOtherAmount6,
                                newOtherAmount7,
                                newOtherAmount8,
                                newOtherAmount9,
                                newOtherAmount10,
                                newCurrency.ToString(),
                                idOperatorType,
                                TimeZoneOffsetSec,
                                newSurchargeTime,
                                newSurchargeAmount, newServiceType, NewSubServiceType);


                            Context.ratetaskassigns.Add(NewRate);


                        }//for each line

                        Context.SaveChanges();

                    }


                    myGridViewDataBind();
                    var color1 = ColorTranslator.FromHtml("#008000");
                    StatusLabel.ForeColor = color1;
                    StatusLabel.Text = (Iteration + 1).ToString() + " Rate(s) Imported Successfully!";
                    TextBoxPartnerFind.Text = "";
                    TextBoxDateFind.Text = "";
                    ddlTypeFind.SelectedIndex = 0;
                }
                else
                {
                    var color = ColorTranslator.FromHtml("#FF0000");
                    StatusLabel.ForeColor = color;
                    StatusLabel.Text = "Upload status: The file has to be less than 5 MB!";
                }
                //}//file types
                //else
                //{
                //var color = ColorTranslator.FromHtml("#FF0000");
                //StatusLabel.ForeColor = color;
                //StatusLabel.Text = "Upload status: Only text/csv files are accepted!";
                //}
            }
            catch (Exception ex)
            {
                var color = ColorTranslator.FromHtml("#FF0000");
                StatusLabel.ForeColor = color;
                StatusLabel.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
            }
        }
    }
    Dictionary<string, string> _billingRules = new Dictionary<string, string>()
    {

        { "0 0 0 ? * SUN-MON *",ExpressionDescriptor.GetDescription("0 0 0 ? * SUN-MON *") },
        { "0 0 12 15 1/1 ? *" ,ExpressionDescriptor.GetDescription("0 0 12 15 1/1 ? *")},
        { "0 0 23  L * ?" ,ExpressionDescriptor.GetDescription("0 0 23  L * ?")}
    };
    List<string> _paymentMethods = new List<string>()
    {

        "Prepaid",
        "Postpaid"
    };
    string billingRule = null;
    string paymentMode = null;





    protected void frmSupplierRatePlanInsert_ModeChanging(object sender, FormViewModeEventArgs e)
    {

    }
    protected void frmSupplierRatePlanInsert_DataBound(object sender, EventArgs e)
    {

    }
    protected void frmSupplierRatePlanInsert_ModeChanged(object sender, EventArgs e)
    {

    }
    protected void GridViewSupplierRates_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GridViewSupplierRates.PageIndex = e.NewPageIndex;
        myGridViewDataBind();

    }



    string InsertSqlRate(rateassign ThisRate)
    {
        //     BillingInformation bl = new BillingInformation(billingRule, paymentMode);
        try
        {

            string x = "INSERT INTO rateassign " +
                       "(                             " +
                       "`Prefix`,                     " +
                       "`description`,                " +
                       "`rateamount`,                 " +
                       "`WeekDayStart`,               " +
                       "`WeekDayEnd`,                 " +
                       "`starttime`,                  " +
                       "`endtime`,                    " +
                       "`Resolution`,                 " +
                       "`MinDurationSec`,             " +
                       "`SurchargeTime`,              " +
                       "`SurchargeAmount`,            " +
                       "`idrateplan`,                 " +
                       "`CountryCode`,                " +
                       "`field1`,                     " +
                       "`field2`,                     " +
                       "`field3`,                     " +
                       "`field4`,                     " +
                       "`field5`,                     " +
                       "`startdate`,                  " +
                       "`enddate`,                    " +
                       "`Inactive`,                   " +
                       "`RouteDisabled`,              " +
                       "`Type`,                       " +
                       "`Currency`,                   " +
                       "`OtherAmount1`,            " +
                       "`OtherAmount2`,            " +
                       "`OtherAmount3`,            " +
                       "`OtherAmount4`,       " +
                       "`OtherAmount5`,          " +
                       "`OtherAmount6`,             " +
                       "`OtherAmount7`,             " +
                       "`OtherAmount8`,             " +
                       "`OtherAmount9`,               " +
                       "`OtherAmount10`,                " +
                       "`TimeZoneOffsetSec`,          " +
                       "`RatePosition`,               " +
                       "`IgwPercentageIn`,            " +
                       "`ConflictingRateIds`,         " +
                       "`ChangedByTaskId`,            " +
                       "`ChangedOn`,                  " +
                       "`Status`,                     " +
                       "`idPreviousRate`,             " +
                       "`EndPreviousRate`,            " +
                       "`Category`,                " +
                       "`SubCategory`,             " +
                       "`BillingParams`)" +
                       "VALUES                        " +

                       "(  													 " +
                       "'" + ThisRate.Prefix + "'," +
                       "'" + ThisRate.description + "'," +
                       "'" + ThisRate.rateamount + "'," +
                       "'" + ThisRate.WeekDayStart + "'," +
                       "'" + ThisRate.WeekDayEnd + "'," +
                       "'" + ThisRate.starttime + "'," +
                       "'" + ThisRate.endtime + "'," +
                       "'" + ThisRate.Resolution + "'," +
                       "'" + ThisRate.MinDurationSec + "'," +
                       "'" + ThisRate.SurchargeTime + "'," +
                       "'" + ThisRate.SurchargeAmount + "'," +
                       "'" + ThisRate.idrateplan + "'," +
                       "'" + ThisRate.CountryCode + "'," +
                       "'" + ThisRate.Status + "'," +
                       "'" + ThisRate.field2 + "'," +
                       "'" + ThisRate.field3 + "'," +
                       "'" + ThisRate.field4 + "'," +
                       "'" + ThisRate.field5 + "'," +
                       "'" + ThisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                       (ThisRate.enddate != new DateTime(9999, 12, 31, 23, 59, 59) ? "'" + Convert.ToDateTime(ThisRate.enddate).ToString("yyyy-MM-dd HH:mm:ss") + "'" : "null") + "," +
                       "'" + ThisRate.Inactive + "'," +
                       "'" + ThisRate.RouteDisabled + "'," +
                       "'" + ThisRate.Type + "'," +
                       "'" + ThisRate.Currency + "'," +
                       "'" + ThisRate.OtherAmount1 + "'," +
                       "'" + ThisRate.OtherAmount2 + "'," +
                       "'" + ThisRate.OtherAmount3 + "'," +
                       "'" + ThisRate.OtherAmount4 + "'," +
                       "'" + ThisRate.OtherAmount5 + "'," +
                       "'" + ThisRate.OtherAmount6 + "'," +
                       "'" + ThisRate.OtherAmount7 + "'," +
                       "'" + ThisRate.OtherAmount8 + "'," +
                       "'" + ThisRate.OtherAmount9 + "'," +
                       "'" + ThisRate.OtherAmount10 + "'," +
                       "'" + ThisRate.TimeZoneOffsetSec + "'," +
                       "'" + ThisRate.RatePosition + "'," +
                       "'" + ThisRate.IgwPercentageIn + "'," +
                       "'" + ThisRate.ConflictingRateIds + "'," +
                       "'" + ThisRate.id + "'," +//keep track of which rate gets changed by which task
                       "'" + DateTime.Now.ToString("yyyy-MM-dd: HH:mm:ss") + "'," +
                       "'" + ThisRate.Status + "'," +
                       "'" + ThisRate.idPreviousRate + "'," +
                       "'" + ThisRate.EndPreviousRate + "'," +
                       "'" + ThisRate.Category + "'," +
                       "'" + ThisRate.SubCategory + "'," +
                       "'" + ThisRate.Category + "'" + ")";

            return x;
        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FA0509");
            StatusLabel.ForeColor = color;
            StatusLabel.Text = e1.Message + "<br/>" + e1.InnerException + "<br/>" +
                               ThisRate.Prefix + " effective date:" + ThisRate.startdate;
            return "";
        }
    }
    int InsertSqlRateTask(rateassign ThisRate, ref List<string> lstValues)
    {

        try
        {

            lstValues.Add(
                "(  													 " +
                "'" + ThisRate.Prefix + "'," +
                "'" + ThisRate.description + "'," +
                "'" + ThisRate.rateamount + "'," +
                "'" + ThisRate.WeekDayStart + "'," +
                "'" + ThisRate.WeekDayEnd + "'," +
                "'" + ThisRate.starttime + "'," +
                "'" + ThisRate.endtime + "'," +
                "'" + ThisRate.Resolution + "'," +
                "'" + ThisRate.MinDurationSec + "'," +
                "'" + ThisRate.SurchargeTime + "'," +
                "'" + ThisRate.SurchargeAmount + "'," +
                "'" + ThisRate.idrateplan + "'," +
                "'" + ThisRate.CountryCode + "'," +
                "'" + ThisRate.Status + "'," +
                "'" + ThisRate.field2 + "'," +
                "'" + ThisRate.field3 + "'," +
                "'" + ThisRate.field4 + "'," +
                "'" + ThisRate.field5 + "'," +
                "'" + ThisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                (ThisRate.enddate != null ? "'" + Convert.ToDateTime(ThisRate.enddate).ToString("yyyy-MM-dd HH:mm:ss") + "'" : "null") + "," +
                "'" + ThisRate.Inactive + "'," +
                "'" + ThisRate.RouteDisabled + "'," +
                "'" + ThisRate.Type + "'," +
                "'" + ThisRate.Currency + "'," +
                "'" + ThisRate.OtherAmount1 + "'," +
                "'" + ThisRate.OtherAmount2 + "'," +
                "'" + ThisRate.OtherAmount3 + "'," +
                "'" + ThisRate.OtherAmount4 + "'," +
                "'" + ThisRate.OtherAmount5 + "'," +
                "'" + ThisRate.OtherAmount6 + "'," +
                "'" + ThisRate.OtherAmount7 + "'," +
                "'" + ThisRate.OtherAmount8 + "'," +
                "'" + ThisRate.OtherAmount9 + "'," +
                "'" + ThisRate.OtherAmount10 + "'," +
                "'" + ThisRate.TimeZoneOffsetSec + "'," +
                "'" + ThisRate.RatePosition + "'," +
                "'" + ThisRate.IgwPercentageIn + "'," +
                "'" + ThisRate.ConflictingRateIds + "'," +
                "'" + ThisRate.ChangedByTaskId + "'," +
                "'" + DateTime.Now.ToString("yyyy-MM-dd: HH:mm:ss") + "'," +
                "'" + ThisRate.Status + "'," +
                "'" + ThisRate.idPreviousRate + "'," +
                "'" + ThisRate.EndPreviousRate + "'," +
                "'" + ThisRate.Category + "'," +
                "'" + ThisRate.SubCategory + "'," +
                "'" + ThisRate.ChangeCommitted + "'" +
                //"'" + billingRule + paymentMode + "'" +
                ")");

            return 1;
        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FA0509");
            StatusLabel.ForeColor = color;
            StatusLabel.Text = e1.Message + "<br/>" + e1.InnerException;
            return 0;
        }
    }



    protected void LinkButtonDeleteAll_Click(object sender, EventArgs e)
    {
        //SaveTasksOrRates(false);
        if (int.Parse(DropDownListTaskRef.SelectedValue) > 0)
        {
            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    cmd.CommandText = " delete from ratetaskassign where idrateplan=" + DropDownListTaskRef.SelectedValue;
                    cmd.ExecuteNonQuery();
                    myGridViewDataBind();
                }
            }
        }
    }

    protected void LinkButtonDeleteCommitted_Click(object sender, EventArgs e)
    {
        //SaveTasksOrRates(false);
        if (int.Parse(DropDownListTaskRef.SelectedValue) > 0)
        {
            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    cmd.CommandText = " delete from ratetaskassign where idrateplan=" + DropDownListTaskRef.SelectedValue;
                    //" and changecommitted=1 ";
                    cmd.ExecuteNonQuery();
                    myGridViewDataBind();
                }
            }
        }
    }

    protected void DeleteWithTransaction(object sender, EventArgs e)
    {
        StatusLabel.Text = "";
        //get selected item's id
        int DelIdTask = Convert.ToInt32(((LinkButton)sender).CommandArgument);
        using (PartnerEntities Context = new PartnerEntities())
        {
            ratetaskassign thisRateTask = Context.ratetaskassigns.Where(c => c.id == DelIdTask).FirstOrDefault();
            DateTime startdate = new DateTime();
            DateTime.TryParseExact(thisRateTask.startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startdate);
            var prefix = Convert.ToInt32(thisRateTask.Prefix);
            rateassign thisRate = Context.rateassigns.Where(c => c.idrateplan == 1 &&
                                                                 c.Prefix == prefix &&
                                                                 c.startdate == startdate).FirstOrDefault();

            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    cmd.CommandText = " set autocommit=0;";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = " delete from ratetaskassign where id=" + DelIdTask;
                    cmd.ExecuteNonQuery();

                    if (thisRateTask.changecommitted == 1)
                    {
                        if (thisRate != null)
                        {
                            //cmd.CommandText = " delete from rateassign where id=" + ThisRate.id;
                            //cmd.ExecuteNonQuery();
                        }
                    }

                    cmd.CommandText = " commit;";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = " set autocommit=1;";
                    cmd.ExecuteNonQuery();
                    myGridViewDataBind();
                }
            }
        }
        Response.Redirect("rateassignment.aspx");
    }



    protected void LinkButtonDeleteSelected_Click(object sender, EventArgs e)
    {

    }

    protected void LinkButtonSaveAll_Click(object sender, EventArgs e)
    {
        CommitChanges();
    }

    void CommitChanges()
    {
        StatusLabel.Text = "";
        int TotalCommitCount = 0;
        try
        {
            int idTaskReference = -1;
            int idRatePlan = -1;
            if (ViewState["sesidRatePlan"] != null)
            {
                idRatePlan = (int)ViewState["sesidRatePlan"];
                idTaskReference = int.Parse(DropDownListTaskRef.SelectedValue);

            }
            if (idTaskReference == -1)
            {
                throw new Exception("idRatePlan not found!");
            }

            using (PartnerEntities context = new PartnerEntities())
            {

                List<ratetaskassign> lstTasks = new List<ratetaskassign>();
                lstTasks = context.ratetaskassigns.Where(c => c.idrateplan == idTaskReference && c.changecommitted != 1).ToList();//Incomplete

                if (lstTasks.Where(c => c.changecommitted != 1).Any() == true)
                {

                    if (lstTasks.Any(c => (//string field, could not compare >0 using linq
                                c.field2.Contains("1") || c.field2.Contains("2") || c.field2.Contains("3")
                                || c.field2.Contains("4") || c.field2.Contains("5") || c.field2.Contains("6")
                                || c.field2.Contains("7") || c.field2.Contains("8") || c.field2.Contains("9")
                            )) == true)//if tasks with error exists
                    {
                        if (CheckBoxContinueOnError.Checked == false)
                        {
                            var color = ColorTranslator.FromHtml("#FA0509");
                            StatusLabel.ForeColor = color;
                            StatusLabel.Text = "Validation error exists! Correct/remove them or Select Continue on Error to try with any next task.";

                            ddlTypeFind.SelectedValue = "0";
                            ButtonFindPrefix_Click(null, null);
                            return;
                        }
                    }

                    //proceed with process rate task
                    TotalCommitCount = ProcessRateTask(lstTasks, idRatePlan);
                    if (TotalCommitCount == -1)
                    {
                        var color2 = Color.Red;
                        StatusLabel.ForeColor = color2;
                        StatusLabel.Text += "Error encountered while committing changes ! ";
                    }
                }
                else
                {
                    var color = ColorTranslator.FromHtml("#FA0509");
                    StatusLabel.ForeColor = color;
                    StatusLabel.Text = "No Uncommitted Task!";
                }
            }

            var color4 = ColorTranslator.FromHtml("#008000");
            StatusLabel.ForeColor = color4;
            StatusLabel.Text += " " + TotalCommitCount + " rateplan assignment tasks completed successfully !";

        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FA0509");
            StatusLabel.ForeColor = color;
            StatusLabel.Text += " Error occured while saving records! " + e1.InnerException;
        }
    }



    int ProcessRateTask(List<ratetaskassign> lstRateTask, int idRatePlan)
    {
        int TotalCommitCount = 0;
        //<asp:ListItem Value="-1">All</asp:ListItem>
        //<asp:ListItem Value="0">Validation Errors</asp:ListItem>
        //<asp:ListItem Value="1">Code Deletes</asp:ListItem>
        //<asp:ListItem Value="2">New Codes</asp:ListItem>//status
        //<asp:ListItem Value="3">Increase</asp:ListItem>//status
        //<asp:ListItem Value="4">Decrease</asp:ListItem>//status
        //<asp:ListItem Value="5">Unchanged</asp:ListItem>//status
        //<%--<asp:ListItem Value="6">Errors</asp:ListItem>--%>
        //<asp:ListItem Value="7">Complete</asp:ListItem>
        //<asp:ListItem Value="8">Change Uncommitted</asp:ListItem>
        //<asp:ListItem Value="9">Overlap</asp:ListItem>//status
        //<asp:ListItem Value="10">Overlap Adjusted</asp:ListItem>//status
        //<asp:ListItem Value="11">Rate Param Conflict</asp:ListItem>//status
        //<asp:ListItem Value="12">Rate Position Not Found</asp:ListItem> //status
        //<asp:ListItem Value="13">Existing</asp:ListItem> //status

        rateassign thisRate = null;
        try
        {
            List<ratetaskassign> lstDeleteTasks = lstRateTask.Where(c => c.rateamount == "-1"
                                                            && c.field2 == "0"//not having validation error
            ).ToList();

            List<ratetaskassign> lstPrefixDelAll = lstDeleteTasks.Where(c => c.Prefix.ToString() == "*").Select(c => new ratetaskassign { Prefix = -1, startdate = c.startdate, ChangedByTaskId = c.id.ToString(), Category = c.Category, SubCategory = c.SubCategory }).ToList();
            //List<ratetaskassign> lstPrefixDelLike = lstDeleteTasks.Where(c => c.Prefix.ToString().Contains("*") && c.Prefix.ToString().Length > 1).Select(c => new ratetaskassign { Prefix = c.Prefix.Split('*')[0], startdate = c.startdate, ChangedByTaskId = c.id.ToString(), Category = c.Category, SubCategory = c.SubCategory }).ToList();
            List<ratetaskassign> lstPrefixDelLike = lstDeleteTasks.Where(c => c.Prefix.ToString().Contains("*") && c.Prefix.ToString().Length > 1).Select(c => new ratetaskassign { Prefix = -1, startdate = c.startdate, ChangedByTaskId = c.id.ToString(), Category = c.Category, SubCategory = c.SubCategory }).ToList();
            List<ratetaskassign> lstPrefixDelSingle = lstDeleteTasks.Where(c => c.Prefix.ToString().Contains("*") == false).Select(c => new ratetaskassign { Prefix = c.Prefix, startdate = c.startdate, ChangedByTaskId = c.id.ToString(), Category = c.Category, SubCategory = c.SubCategory }).ToList();

            List<rateassign> lstDelAll = new List<rateassign>();
            List<rateassign> lstDelLike = new List<rateassign>();
            List<rateassign> lstDelSingle = new List<rateassign>();

            List<rateassign> RateCache = new List<rateassign>();
            Dictionary<int, List<rateassign>> dicRateCache = new Dictionary<int, List<rateassign>>();//use prefix as index
            using (PartnerEntities Context = new PartnerEntities())
            {
                RateCache = Context.rateassigns.Where(c => c.idrateplan == idRatePlan).ToList();
            }
            foreach (rateassign R in RateCache)
            {
                List<rateassign> thislist = null;
                dicRateCache.TryGetValue(R.Prefix, out thislist);
                if (thislist == null)
                {
                    //prefix not in the dictionary yet, create dictionary item first.
                    dicRateCache.Add(R.Prefix, new List<rateassign>());
                    dicRateCache.TryGetValue(R.Prefix, out thislist);
                }
                thislist.Add(R);

                //check for matching code delete entries


                //prefix=*
                foreach (ratetaskassign rTask in lstPrefixDelAll.Where(c => c.Category == R.Category.ToString() && c.SubCategory == R.SubCategory.ToString()).ToList())
                {
                    DateTime DelDate = new DateTime(2000, 1, 1);
                    DateTime.TryParseExact(rTask.startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DelDate);
                    if (DelDate > (new DateTime(2000, 1, 1))
                        && (R.startdate <= DelDate)
                        && (R.enddate == null || DelDate < R.enddate))
                    {
                        //new instance of R is notrequired, as any change here in R will overwrite
                        //previous chanage e.g. in * [All] loop, this is a more specific match than before
                        //specific match can only be beaten if previous match (* or like 2*) has a lower code enddate
                        R.ChangedByTaskId = Convert.ToInt64(rTask.ChangedByTaskId);
                        R.enddate = DelDate;//delete/code ending in advance, later will be written in DB    
                        lstDelAll.Add(R);
                    }
                }

                //prefix like 2*, 91* etc.
                foreach (ratetaskassign rTask in lstPrefixDelLike.Where(c => c.Category == R.Category.ToString() && c.SubCategory == R.SubCategory.ToString()).ToList())
                {
                    DateTime DelDate = new DateTime(2000, 1, 1);
                    DateTime.TryParseExact(rTask.startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DelDate);
                    if (R.Prefix.ToString().StartsWith(rTask.Prefix.ToString())
                        && DelDate > (new DateTime(2000, 1, 1))
                        && (R.startdate <= DelDate)
                        && (R.enddate == null || DelDate < R.enddate))
                    {
                        //new instance of R is notrequired, as any change here in R will overwrites 
                        //previous chanage e.g. in * [All] loop, this is a more specific match than before
                        R.ChangedByTaskId = Convert.ToInt64(rTask.ChangedByTaskId);
                        R.enddate = DelDate;//delete/code ending in advance, later will be written in DB    
                        lstDelLike.Add(R);
                    }
                }

                //if R.prefix= one single prefix code delete instructions e.g. 919
                //debug
                //if (R.Prefix == "26386")
                //{
                //    int r = 1;
                //}
                foreach (ratetaskassign rTask in lstPrefixDelSingle.Where(c => c.Category == R.Category.ToString() && c.SubCategory == R.SubCategory.ToString()).ToList())
                {
                    DateTime DelDate = new DateTime(2000, 1, 1);
                    DateTime.TryParseExact(rTask.startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DelDate);
                    if (R.Prefix.ToString() == rTask.Prefix.ToString()
                        && DelDate > (new DateTime(2000, 1, 1))
                        && (R.startdate <= DelDate)
                        && (R.enddate == null || DelDate < R.enddate)
                    )
                    {
                        //new instance
                        R.ChangedByTaskId = Convert.ToInt64(rTask.ChangedByTaskId);
                        R.enddate = DelDate;//delete/code ending in advance, later will be written in DB    
                        lstDelSingle.Add(R);
                    }
                }
            }
            RateCache = null;//not required anymore

            //all code changes will be executed, so count them as commit count
            TotalCommitCount = lstPrefixDelAll.Count + lstPrefixDelLike.Count + lstPrefixDelSingle.Count;

            StringBuilder sbSQL = new StringBuilder();
            sbSQL.Append("set autocommit=0;");

            //populate delete Sql first
            foreach (rateassign DelRate in lstDelAll.Concat(lstDelLike).Concat(lstDelSingle))
            {
                DateTime DelDate = Convert.ToDateTime(DelRate.enddate);
                sbSQL.Append(
                    " update rateassign set enddate='" + DelDate.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    " ChangedByTaskId=" + DelRate.ChangedByTaskId +
                    " where id=" + DelRate.id).Append(";");
            }
            //update the status of the delete taks as well
            foreach (ratetaskassign DelTask in lstDeleteTasks)
            {
                sbSQL.Append(" update ratetaskassign set changecommitted=1,status =1 where " +
                             " id=" + DelTask.id).Append(";");
            }
            //lock table to make sure I'm the only one to write the rate table
            using (MySqlConnection Con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString + "; default command timeout=600;"))
            {
                using (MySqlCommand Cmd = new MySqlCommand("", Con))
                {
                    Con.Open();
                    string Sql = "lock tables ratetaskassign write; lock tables rateassign write;";
                    Cmd.CommandText = Sql;

                    //*****process tasks other than delete, each as one rate
                    lstRateTask = lstRateTask.Where(c => c.rateamount != "-1" &&
                                                         c.field2 == "0"//not having validation error
                    ).ToList();

                    foreach (ratetaskassign ThisTask in lstRateTask)
                    {
                        thisRate = RateTaskToRate(ThisTask, idRatePlan);
                        //when converting from task to rate, any previous overlap and conflicts are discarded
                        //by the conversion as it is not necessary.

                        //now process this rate task
                        rateassign PresentRate = null;
                        rateassign NextRate = null;
                        rateassign PrevRate = null;
                        rateassign LastInstance = null;

                        if (SurroundingRates(ref dicRateCache, thisRate, idRatePlan, ref PresentRate, ref NextRate, ref PrevRate, ref LastInstance) == 0)
                        //error occured
                        {
                            if (CheckBoxContinueOnError.Checked == false) return 0;
                        }

                        RatePositioning RatePos = new RatePositioning(PresentRate, PrevRate, NextRate, thisRate, CheckBoxAutoAdjust.Checked);
                        //rate positions and any overlap/conflict status is known now

                        bool OverLap = RatePos.Overlap;
                        bool RateConflict = RatePos.ParameterConflict;
                        bool AutoAdjust = RatePos.AutoAdjust; ;
                        bool ContinueOnError = CheckBoxContinueOnError.Checked;

                        //handle rate conflict
                        if (RateConflict == true)
                        {
                            if (CheckBoxContinueOnError.Checked == false)
                            {
                                var color = ColorTranslator.FromHtml("#FA0509");
                                StatusLabel.ForeColor = color;
                                StatusLabel.Text = "Rate Conflict exists! Correct/remove them or Select Continue on Error to try with any next task.";

                                ddlTypeFind.SelectedValue = "11";
                                ButtonFindPrefix_Click(null, null);
                                return 0;
                            }
                        }

                        if (OverLap == true)
                        {
                            if (AutoAdjust == false)
                            {
                                if (CheckBoxContinueOnError.Checked == false)
                                {
                                    var color = ColorTranslator.FromHtml("#FA0509");
                                    StatusLabel.ForeColor = color;
                                    StatusLabel.Text = "Overlap exists! Correct/remove them or Select Continue on Error to try with any next tasks";

                                    ddlTypeFind.SelectedValue = "9";
                                    ButtonFindPrefix_Click(null, null);
                                    return 0;
                                }
                            }
                        }

                        //process rates which are not overlap or conflict
                        switch (thisRate.Status)
                        {
                            case 9://overlap
                            case 12://rate position not found

                                sbSQL.Append(
                                    " update ratetaskassign set status =" + thisRate.Status +
                                    " where id=" + thisRate.id).Append(";");

                                break;
                            case 2://new
                                if (OverLap == false && RateConflict == false)
                                {
                                    if (RatePos.ThisPosition == RatePositions.BeforeAll)
                                    {
                                        thisRate.enddate = NextRate.startdate;
                                    }

                                    sbSQL.Append(
                                        InsertSqlRate(thisRate)).Append(";");

                                    sbSQL.Append(
                                        " update ratetaskassign set changecommitted=1, " +
                                        " startdate='" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                        " enddate=" +
                                        (thisRate.enddate != new DateTime(9999, 12, 31, 23, 59, 59) ?
                                            ("'" + Convert.ToDateTime(thisRate.enddate).ToString("yyyy-MM-dd HH:mm:ss") + "'") : "null")
                                        + "," +
                                        "status =" + thisRate.Status + " where " +
                                        " id=" + thisRate.id).Append(";");
                                    TotalCommitCount++;
                                }
                                break;
                            case 5://unchanged
                            case 3://increase
                            case 4://decrease
                                //applicabl for in between and latest rates.    
                                if (OverLap == false && RateConflict == false)
                                {
                                    //end previous rate
                                    if (PrevRate != null)
                                    {



                                        if (PrevRate.enddate == RatePos.FutureDate)//null, end previous rate with no end date
                                        {
                                            sbSQL.Append(
                                                " update rateassign set enddate='" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                                " ChangedByTaskId=" + thisRate.id +
                                                " where id=" + PrevRate.id).Append(";");

                                            //also make sure the corresponding previous task for this rate has the 
                                            //same startdate and end date
                                            using (PartnerEntities Context = new PartnerEntities())
                                            {
                                                string PreviousRatesDate = PrevRate.startdate.ToString("yyyy-MM-dd HH:mm:ss");
                                                ratetaskassign PrevTask = Context.ratetaskassigns
                                                    .Where(c => c.Prefix == PrevRate.Prefix && c.idrateplan == 1
                                                    && c.startdate == PreviousRatesDate).FirstOrDefault();
                                                if (PrevTask != null)
                                                {
                                                    sbSQL.Append(
                                                    " update ratetaskassign set enddate='" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                    " where id=" + PrevTask.id).Append(";");
                                                }
                                                
                                            }
                                        }
                                        //insert the new rate
                                        sbSQL.Append(
                                            InsertSqlRate(thisRate)).Append(";");

                                        sbSQL.Append(
                                            " update ratetaskassign set changecommitted=1," +
                                            " startdate='" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                            " enddate=" +
                                            (thisRate.enddate != new DateTime(9999, 12, 31, 23, 59, 59) ?
                                                ("'" + Convert.ToDateTime(thisRate.enddate).ToString("yyyy-MM-dd HH:mm:ss") + "'") : "null")
                                            + "," +
                                            " status =" + thisRate.Status + " where " +
                                            " id=" + thisRate.id).Append(";");
                                        TotalCommitCount++;
                                    }
                                    //else if previous rate is null
                                    else
                                    {
                                        sbSQL.Append(
                                            InsertSqlRate(thisRate)).Append(";");

                                        sbSQL.Append(
                                            " update ratetaskassign set changecommitted=1, " +
                                            " startdate='" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                            " enddate='" + Convert.ToDateTime(thisRate.enddate).ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                            ",status =" + thisRate.Status +
                                            " ,affectedrates='" + PrevRate.id + "'" +
                                            " where " +
                                            " id=" + thisRate.id).Append(";");
                                        TotalCommitCount++;
                                    }
                                }
                                break;
                            case 10://overlap adjusted
                                List<string> AffectedOLRates = new List<string>();

                                if (RatePos.ThisOverLapType == OverlapTypes.OverlappingNext)
                                {
                                    //have to change own end date as well so that it doesn't overlap the next
                                    //AffectedOLRates.Add(ThisRate.id.ToString());--this can't be set, the taskid will be set in rate table
                                    thisRate.enddate = NextRate.startdate;
                                }
                                else//overlappingboth or overlapbycoincide
                                {
                                    //end previous rate
                                    if (PrevRate != null)
                                    {

                                        sbSQL.Append(
                                            " update rateassign set enddate='" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                            " ChangedByTaskId=" + thisRate.id +
                                            " where id=" + PrevRate.id).Append(";");

                                        AffectedOLRates.Add(PrevRate.id.ToString());
                                    }
                                    if ((RatePos.ThisOverLapType == OverlapTypes.OverlappingBoth) ||
                                        (thisRate.enddate == RatePos.FutureDate))
                                    {
                                        //have to change own end date as well so that it doesn't overlap the next
                                        thisRate.enddate = NextRate.startdate;
                                    }
                                }

                                sbSQL.Append(
                                    InsertSqlRate(thisRate)).Append(";");
                                sbSQL.Append(
                                    " update ratetaskassign set changecommitted=1,status =" + thisRate.Status +
                                    " ,affectedrates='" + string.Join(",", AffectedOLRates.ToArray()) + "'" +
                                    " where " +
                                    " id=" + thisRate.id).Append(";");
                                TotalCommitCount++;
                                break;
                            case 13://existing, dont' mark as change committed
                                sbSQL.Append(
                                    " update ratetaskassign set status =" + thisRate.Status + " where " +
                                    " id=" + thisRate.id).Append(";");

                                break;
                        }

                    }//for each rate task (not code delete)

                    //Cmd.CommandText = "unlock tables;";

                    Cmd.CommandText = sbSQL.Append(" commit;").ToString().Replace("'9999-12-31 23:59:59'", "null");
                    Cmd.ExecuteNonQuery();

                    myGridViewDataBind();

                }//using command
            }
        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FA0509");
            StatusLabel.ForeColor = color;
            StatusLabel.Text = e1.Message + "<br/>" + e1.InnerException + "<br/>" +
                               thisRate.Prefix + " effective date:" + thisRate.startdate;
            return -1;
        }


        return TotalCommitCount;


    }

    rateassign RateTaskToRate(ratetaskassign rateTaskAssign, long idRatePlan)
    {
        try
        {
            rateassign rateAssign = new rateassign();
            rateAssign.id = rateTaskAssign.id;
            rateAssign.Prefix = Convert.ToInt32(rateTaskAssign.Prefix);
            rateAssign.description = rateTaskAssign.description;
            rateAssign.rateamount = Convert.ToDecimal(rateTaskAssign.rateamount);
            rateAssign.WeekDayStart = Convert.ToInt32(rateTaskAssign.WeekDayStart);
            rateAssign.WeekDayEnd = Convert.ToInt32(rateTaskAssign.WeekDayEnd);
            rateAssign.starttime = rateTaskAssign.starttime;
            rateAssign.endtime = rateTaskAssign.endtime;
            rateAssign.Resolution = Convert.ToInt32(rateTaskAssign.Resolution);
            rateAssign.MinDurationSec = Convert.ToSingle(rateTaskAssign.MinDurationSec);
            rateAssign.SurchargeTime = Convert.ToInt32(rateTaskAssign.SurchargeTime);
            rateAssign.SurchargeAmount = Convert.ToDecimal(rateTaskAssign.SurchargeAmount);
            rateAssign.idrateplan = idRatePlan;//ThisTask.idrateplan;
            rateAssign.CountryCode = rateTaskAssign.CountryCode.ToString();

            DateTime TempDate = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime.TryParseExact(rateTaskAssign.date1, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out TempDate);
            rateAssign.date1 = TempDate;

            rateAssign.Status = Convert.ToInt32(rateTaskAssign.Status);
            rateAssign.field2 = Convert.ToInt32(rateTaskAssign.field2);
            rateAssign.field3 = Convert.ToInt32(rateTaskAssign.field3);
            rateAssign.field4 = rateTaskAssign.field4;
            rateAssign.field5 = rateTaskAssign.field5;

            DateTime TempDatest = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime.TryParseExact(rateTaskAssign.startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out TempDatest);
            rateAssign.startdate = TempDatest;

            DateTime TempDate1 = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime.TryParseExact(rateTaskAssign.enddate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out TempDate1);
            rateAssign.enddate = TempDate1;

            rateAssign.Inactive = Convert.ToInt32(rateTaskAssign.Inactive);
            int TempRouteDisabled = 0;
            rateAssign.RouteDisabled = TempRouteDisabled;
            int.TryParse(rateTaskAssign.RouteDisabled, out TempRouteDisabled);
            rateAssign.RouteDisabled = TempRouteDisabled;
            rateAssign.Type = Convert.ToInt32(rateTaskAssign.Type);
            rateAssign.Currency = Convert.ToInt32(rateTaskAssign.Currency);
            rateAssign.OtherAmount1 = Convert.ToSingle(rateTaskAssign.OtherAmount1);
            rateAssign.OtherAmount2 = Convert.ToSingle(rateTaskAssign.OtherAmount2);
            rateAssign.OtherAmount3 = Convert.ToSingle(rateTaskAssign.OtherAmount3);
            rateAssign.OtherAmount4 = Convert.ToDecimal(rateTaskAssign.OtherAmount4);
            rateAssign.OtherAmount5 = Convert.ToDecimal(rateTaskAssign.OtherAmount5);
            rateAssign.OtherAmount6 = Convert.ToSingle(rateTaskAssign.OtherAmount6);
            rateAssign.OtherAmount7 = Convert.ToSingle(rateTaskAssign.OtherAmount7);
            rateAssign.OtherAmount8 = Convert.ToSingle(rateTaskAssign.OtherAmount8);
            rateAssign.OtherAmount9 = Convert.ToSingle(rateTaskAssign.OtherAmount9);
            rateAssign.OtherAmount10 = Convert.ToSingle(rateTaskAssign.OtherAmount10);
            rateAssign.TimeZoneOffsetSec = Convert.ToDecimal(rateTaskAssign.TimeZoneOffsetSec);
            rateAssign.RatePosition = Convert.ToInt32(rateTaskAssign.RatePosition);
            rateAssign.IgwPercentageIn = Convert.ToSingle(rateTaskAssign.IgwPercentageIn);
            rateAssign.ConflictingRateIds = rateTaskAssign.ConflictingRateIds;
            rateAssign.ChangedByTaskId = Convert.ToInt32(rateTaskAssign.ChangedByTaskId);

            DateTime TempDateco = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime.TryParseExact(rateTaskAssign.date1, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out TempDateco);
            rateAssign.ChangedOn = TempDateco;
            rateAssign.Status = Convert.ToInt32(rateTaskAssign.Status);
            rateAssign.idPreviousRate = Convert.ToInt32(rateTaskAssign.idPreviousRate);
            rateAssign.EndPreviousRate = Convert.ToSByte(rateTaskAssign.EndPreviousRate);
            rateAssign.Category = Convert.ToSByte(rateTaskAssign.Category);
            rateAssign.SubCategory = Convert.ToSByte(rateTaskAssign.SubCategory);
            //ThisRate.ChangeCommitted = ThisTask.ChangeCommitted;

            return rateAssign;
        }
        catch (Exception e1)
        {
            StatusLabel.ForeColor = Color.Red;
            StatusLabel.Text = "Error in conversion from Rate Task to Rate<br/>" + e1.Message + "<br/>" + e1.InnerException;
            return null;
        }
    }

    int SurroundingRates(ref Dictionary<int, List<rateassign>> dicRateCache, rateassign ThisRate, int idRatePlan, ref rateassign PresentRate, ref rateassign NextRate, ref rateassign PrevRate, ref rateassign LastInstance)
    {
        try
        {
            List<rateassign> RateCache = null;
            dicRateCache.TryGetValue(ThisRate.Prefix, out RateCache);

            if (RateCache == null || RateCache.Count == 0) return 1;

            if (ThisRate.rateamount == -1)//code delete
            {
                LastInstance = RateCache.Where(c => c.idrateplan == idRatePlan && c.Prefix == ThisRate.Prefix
                                                    && c.Category == ThisRate.Category
                                                    && c.SubCategory == ThisRate.SubCategory
                                                    && c.startdate <= ThisRate.startdate).OrderByDescending(c => c.startdate).Take(1).ToList().FirstOrDefault();
            }
            else if (ThisRate.rateamount != -1)//other than code delete
            {
                PrevRate = RateCache.Where(c => c.idrateplan == idRatePlan && c.Prefix == ThisRate.Prefix
                                                && c.startdate < ThisRate.startdate
                                                && c.Category == ThisRate.Category
                                                && c.SubCategory == ThisRate.SubCategory).OrderByDescending(c => c.startdate).Take(1).ToList().FirstOrDefault();


                PresentRate = RateCache.Where(c => c.idrateplan == idRatePlan && c.Prefix == ThisRate.Prefix
                                                   && c.startdate == ThisRate.startdate
                                                   && c.Category == ThisRate.Category
                                                   && c.SubCategory == ThisRate.SubCategory).OrderByDescending(c => c.startdate).Take(1).ToList().FirstOrDefault();



                NextRate = RateCache.Where(c => c.idrateplan == idRatePlan && c.Prefix == ThisRate.Prefix
                                                && c.startdate > ThisRate.startdate
                                                && c.Category == ThisRate.Category
                                                && c.SubCategory == ThisRate.SubCategory).OrderBy(c => c.startdate).Take(1).ToList().FirstOrDefault();
            }

            return 1;

        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FF0000");
            StatusLabel.ForeColor = color;
            StatusLabel.Text = "Error finding surrounding rates for rate " + ThisRate.Prefix + " and Start Time=" + ThisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss");
            return 0;
        }
    }



    protected void ButtonFindPrefix_Click(object sender, EventArgs e)
    {
        HiddenFieldSelect.Value = "0";
        myGridViewDataBind();
    }

    protected void ButtonFindPrefixSelect_Click(object sender, EventArgs e)
    {
        //before clicking on click has set the hidden value for filter flag to 1 already
        myGridViewDataBind();
    }


    //rate import functions&&&&&&&&&&&ENDs&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&


    public string[] RateNormalizer(string filename, int ratePlanFormat)
    {

        String[] output = null;
        try
        {
            if (ratePlanFormat == 2) //delimited
            {
                output = file2String(filename);
            }
        }
        catch (Exception exp)
        {
            output = null;
        }
        return output;
    }

    public string[] file2String(string strFileName)
    {
        StreamReader sw = new StreamReader(strFileName);
        List<string> strOut = new List<string>();
        strOut.Clear();

        while (!sw.EndOfStream)
        {
            string strRd = sw.ReadLine();
            if (strRd != "")
                strOut.Add(strRd);
        }

        return strOut.ToArray();
    }
    public bool string2File(string strFilePath, string[] strArray)
    {
        StreamWriter sw = null;
        bool retFlag = false;

        try
        {
            int arrLength = strArray.Length;
            sw = new StreamWriter(strFilePath, false);
            string strRow = "";//Destination	USD / Min	Country Code	Area Code	Complete Code	Change	Effective Date

            for (int i = 0; i < arrLength; i++)
            {
                strRow = strArray[i].ToString();
                sw.WriteLine(strRow.Trim());
            }

            retFlag = true;
        }
        catch (Exception exp)
        {
            retFlag = false;
            //throw exp;
        }
        finally
        {
            if (sw != null)
                sw.Close();

        }
        return retFlag;
    }
    public string[] CreateRateSheetArray(DataTable srcdt, int ratePlanFormat, int mode)
    {

        int srccolumnCount = srcdt.Columns.Count;
        int srcrowCount = srcdt.Rows.Count;

        List<string> strOut = new List<string>();

        try
        {



            foreach (DataRow dr in srcdt.Rows)
            {
                string effectDate = "";
                if (ratePlanFormat == 4)
                    effectDate = DateTime.Now.ToString();
                else if (ratePlanFormat == 2)
                {
                    if (mode == 1)
                    {
                        effectDate = dr[6].ToString().Trim();
                    }
                    else
                    {
                        effectDate = dr[5].ToString().Trim();
                    }

                }
                else
                {
                    effectDate = dr[6].ToString().Trim();
                }
                DateTime efctDate;
                string dateformat1 = "";
                if (effectDate.Length > 0)
                {
                    efctDate = Convert.ToDateTime(effectDate);
                    effectDate = efctDate.ToString("yyyy-MM-dd HH:mm:ss");
                    dateformat1 = efctDate.ToString("u");
                }

                string strRow = "";//Destination	USD / Min	Country Code	Area Code	Complete Code	Change	Effective Date

                if (effectDate == "")
                    effectDate = @"\N";

                if (ratePlanFormat == 2)
                {
                    if (mode == 1)
                    {
                        strRow = dr[4].ToString().Trim() + "`" + dr[1].ToString().Trim() + "`" + dr[5].ToString().Trim() + "`1`" + dr[2].ToString().Trim() + "`" + effectDate.ToString().Trim() + @"`\N";
                    }
                    else if (mode == 2)
                    {
                        if (dr[6].ToString().Trim() == "Pending Code Removal")
                            strRow = dr[4].ToString().Trim() + "`" + dr[1].ToString().Trim() + "`-1`1`" + dr[2].ToString().Trim() + "`" + effectDate.ToString().Trim() + @"`\N";
                        //else
                        // strRow = dr[4].ToString().Trim() + "`" + dr[1].ToString().Trim() + "`-0`1`" + dr[2].ToString().Trim() + "`" + effectDate.ToString().Trim() + @"`\N";
                    }
                }
                else if (ratePlanFormat == 3)
                    strRow = dr[4].ToString().Trim() + "`" + dr[0].ToString().Trim() + "`" + dr[1].ToString().Trim() + "`1`" + dr[2].ToString().Trim() + "`" + effectDate.ToString().Trim() + @"`\N";
                else if (ratePlanFormat == 4)//Location	Code	banglatel	Gold	Silver	Bronze
                    strRow = dr[1].ToString().Trim() + "`" + dr[0].ToString().Trim() + "`" + dr[3].ToString().Trim() + @"`1`\N`" + effectDate.ToString().Trim() + @"`\N";

                //bool check = append2String(strRow.Trim());
                if (strRow != "")
                    strOut.Add(strRow.Trim());
                //sw.WriteLine(strRow.Trim());

            }
            //sw.Close();
            //MessageBox.Show("ok", "RateSheet");
        }
        catch (Exception ex)
        {
            return null;
        }
        return strOut.ToArray();
    }
    public DataTable process(string filename, int ratePlanFormat, int currentsheet)
    {
        string colHeader = "0ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string FindStart = "", FindEnd = "";
        if (ratePlanFormat == 2)
        {
            FindStart = "Country";
            FindEnd = "Prime Assurance";
        }
        else if (ratePlanFormat == 3)
        {
            FindStart = "Destination";
        }
        else if (ratePlanFormat == 4)
        {
            FindStart = "Location";
        }
        else
        {
            FindStart = "Prefix";
        }

        DataTable dt = new DataTable();

        Excel.Workbook newWorkBook = null;
        Excel.Application excelApp = new Excel.Application(); //Create new App
        excelApp.Visible = true;

        try
        {
            int ro = 0; int co = 0; String startingRow = ""; String endingRow = "";
            //XLFileOpen:        
            newWorkBook = excelApp.Workbooks.Open(filename, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);//Add(System.Reflection.Missing.Value);
            Excel.Sheets excelSheets = newWorkBook.Worksheets;
            Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelSheets.get_Item(currentsheet);
            Excel.Range excelcell = (Excel.Range)excelWorksheet.UsedRange;

            int a = excelcell.Rows.Count;
            int b = excelcell.Columns.Count;

            Excel.Range rfind = null;
            Excel.Range nfind = null;
            rfind = excelcell.Find(FindStart, Missing.Value, Excel.XlFindLookIn.xlValues, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByColumns, Excel.XlSearchDirection.xlNext, true, false, false);

            int c = rfind.Row;
            int d = rfind.Column;

            if (ratePlanFormat == 2)
            {
                nfind = excelcell.Find(FindEnd, Missing.Value, Excel.XlFindLookIn.xlValues, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByColumns, Excel.XlSearchDirection.xlNext, true, false, false);
                //MessageBox.Show("Find:" + nfind.Row.ToString() + "==" + nfind.Column.ToString());
                Excel.Range s;
                ro = nfind.Row;
                co = nfind.Column;
                s = (Excel.Range)excelWorksheet.Cells[ro, co];
                //string khu = null;
                String khu = s.Value2.ToString(); //((Excel.Range)s).Value2.ToString();
                if (khu != "")
                {
                    ro--;
                    s = (Excel.Range)excelWorksheet.Cells[ro, co];
                }
                while (s.Value2 == null || s.Value2.ToString() == "")
                {
                    ro--;
                    s = (Excel.Range)excelWorksheet.Cells[ro, co];
                }
                //MessageBox.Show("TotalRow=" + a.ToString() + "Total Coloumn" + b.ToString() + "StartingRow=" + c.ToString() + "EndingRow=" + ro.ToString());
                startingRow = colHeader[co] + c.ToString();
                endingRow = colHeader[co] + ro.ToString();

            }
            else if (ratePlanFormat == 3 || ratePlanFormat == 4)
            {
                nfind = excelcell.Find(FindEnd, Missing.Value, Excel.XlFindLookIn.xlValues, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByColumns, Excel.XlSearchDirection.xlNext, true, false, false);
                //MessageBox.Show("Find:" + nfind.Row.ToString() + "==" + nfind.Column.ToString());
                Excel.Range s;
                ro = rfind.Row;
                co = rfind.Column;
                s = (Excel.Range)excelWorksheet.Cells[ro, co];
                //string khu = null;
                String khu = s.Value2.ToString(); //((Excel.Range)s).Value2.ToString();
                if (khu != "")
                {
                    ro++; ro = a;
                    s = (Excel.Range)excelWorksheet.Cells[ro, co];
                }
                while (s.Value2 != null)
                {
                    ro++;
                    s = (Excel.Range)excelWorksheet.Cells[ro, co];
                }
                ro--;

                startingRow = colHeader[co] + c.ToString();
                endingRow = colHeader[co] + ro.ToString();
            }
            else
            {
            }

            int tokenFlag = 0; int count = 0, splitcount = 0;
            int totalcount = 0;

            #region Coloumn_Header
            if (ratePlanFormat == 2)
            {
                if (currentsheet == 1)
                {
                    dt.Columns.Add("Country");//0
                    dt.Columns.Add("Destination");//1
                    dt.Columns.Add("CountryCode");//2
                    dt.Columns.Add("CityCode");//3
                    dt.Columns.Add("Prefix");//4
                    dt.Columns.Add("Price");//5
                    dt.Columns.Add("EffectiveDate");//6
                    dt.Columns.Add("PrimeAssurance");//7
                    dt.Columns.Add("Comments");//8
                }
                else if (currentsheet == 2)
                {
                    //Destination	Effective Date	Country Code(s)	Previous Code(s)	New Code(s)	Modification	Comments
                    dt.Columns.Add("Country");//0
                    dt.Columns.Add("Destination");//1
                    dt.Columns.Add("CountryCode");//3
                    dt.Columns.Add("Modification");//6
                    dt.Columns.Add("Prefix");//4
                    dt.Columns.Add("EffectiveDate");//2
                    dt.Columns.Add("Comments");//7
                }
            }
            else if (ratePlanFormat == 3)//Bharti/////////////
            {
                dt.Columns.Add("Destination");//0
                dt.Columns.Add("USD / Min");//1
                dt.Columns.Add("Country Code");//2	
                dt.Columns.Add("Area Code");//3
                dt.Columns.Add("Complete Code");//4
                dt.Columns.Add("Change");//5
                dt.Columns.Add("Effective Date");//6
            }
            else if (ratePlanFormat == 4)//IDT/////////////
            {
                //Location	Code	banglatel	Gold	Silver	Bronze
                dt.Columns.Add("Location");//0
                dt.Columns.Add("Code");//1
                dt.Columns.Add("banglatel");//2	
                dt.Columns.Add("Gold");//3
                dt.Columns.Add("Silver");//4
                dt.Columns.Add("Bronze");//5
                //dt.Columns.Add("Effective Date");//6
            }
            #endregion

            #region Dictionary
            if (ratePlanFormat == 3)
            {
                #region Sheet1
                for (int row = c + 1; row <= ro; row++)
                {
                    DataRow dr = dt.NewRow();
                    string[] tempwords = null;
                    int drCol = 0;
                    count++;

                    for (int col = co; col <= (b + 1); col++, drCol++)
                    {
                        if (((Excel.Range)excelWorksheet.Cells[row, col]).Value2 != null)
                        {
                            //((Excel.Range)excelWorksheet.Cells[row, col]).Value2;
                            String tempstr = ((Excel.Range)excelWorksheet.Cells[row, col]).Value2.ToString();

                            if (col == 8)//col starts with 2 and end with 8
                            {
                                if (tempstr != "")
                                {
                                    double dttim = Convert.ToDouble(tempstr);
                                    DateTime dddt = DateTime.FromOADate(dttim);
                                    tempstr = dddt.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                            }

                            dr[drCol] = tempstr.Trim();

                        }
                        else
                        {
                            if (drCol == 5)
                            {
                                dr[drCol] = "No Change";
                            }
                            else if (drCol == 6)
                            {
                                dr[drCol] = DateTime.Now.ToString().Trim();
                            }
                            else
                            {
                                dr[drCol] = " ";
                            }
                        }

                    }
                    if (tokenFlag == 0)
                    {
                        dt.Rows.Add(dr);
                        dt.AcceptChanges();
                        totalcount++;
                    }
                    else
                    {
                        string col1 = dr[0].ToString();
                        string col2 = dr[1].ToString();
                        string col3 = dr[2].ToString();
                        string col4 = dr[3].ToString();
                        string col5 = dr[4].ToString();
                        string col6 = dr[5].ToString();
                        string col7 = dr[6].ToString();

                        tokenFlag = 0;
                        tempwords = null;

                    }
                }
                #endregion
            }
            ////////////////IDT/////////////////
            if (ratePlanFormat == 4)
            {
                #region Sheet1
                for (int row = c + 1; row <= ro; row++)
                {
                    DataRow dr = dt.NewRow();
                    //string[] tempwords = null;
                    int drCol = 0;
                    count++;

                    for (int col = co; col <= b; col++, drCol++)
                    {
                        if (((Excel.Range)excelWorksheet.Cells[row, col]).Value2 != null)
                        {
                            //((Excel.Range)excelWorksheet.Cells[row, col]).Value2;
                            String tempstr = ((Excel.Range)excelWorksheet.Cells[row, col]).Value2.ToString();
                            dr[drCol] = tempstr.Trim();

                        }
                        else
                        {
                            dr[drCol] = " ";
                        }

                    }
                    if (tokenFlag == 0)
                    {
                        dt.Rows.Add(dr);
                        dt.AcceptChanges();
                        totalcount++;
                    }
                    else
                    {
                        string col1 = dr[0].ToString();
                        string col2 = dr[1].ToString();
                        string col3 = dr[2].ToString();
                        string col4 = dr[3].ToString();
                        string col5 = dr[4].ToString();
                        //string col6 = dr[5].ToString();
                        //string col7 = dr[6].ToString();

                        tokenFlag = 0;
                        //tempwords = null;

                    }
                }
                #endregion
            }

            /////////////////TATA////////////////
            if (ratePlanFormat == 2)
            {
                if (currentsheet == 2)
                {
                    #region Sheet2
                    for (int row = c + 1; row <= ro; row++)
                    {


                        if (((Excel.Range)excelWorksheet.Cells[row, 7]).Value2 != null)
                        {

                            string[] tempwords = null;
                            int drCol = 0;
                            count++;

                            String tempstr = ((Excel.Range)excelWorksheet.Cells[row, 7]).Value2.ToString();
                            string[] words = tempstr.Split(',');
                            splitcount = splitcount + words.Count();
                            for (int trow = 0; trow < words.Count(); trow++)
                            {
                                DataRow dr = dt.NewRow();
                                dr[0] = ((Excel.Range)excelWorksheet.Cells[row, 1]).Value2.ToString();
                                dr[1] = ((Excel.Range)excelWorksheet.Cells[row, 2]).Value2.ToString();
                                dr[2] = ((Excel.Range)excelWorksheet.Cells[row, 4]).Value2.ToString();
                                dr[3] = words[trow].ToString().Trim();
                                string tstr = dr[2].ToString().Trim() + dr[3].ToString().Trim();
                                dr[4] = tstr.ToString();
                                dr[5] = ((Excel.Range)excelWorksheet.Cells[row, 3]).Value2.ToString();
                                dr[6] = ((Excel.Range)excelWorksheet.Cells[row, 8]).Value2.ToString();

                                dt.Rows.Add(dr);
                                dt.AcceptChanges();
                                totalcount++;

                            }
                        }

                    }
                    #endregion
                }
                else if (currentsheet == 1)
                {
                    #region Sheet1
                    for (int row = c + 1; row <= ro; row++)
                    {
                        DataRow dr = dt.NewRow();
                        string[] tempwords = null;
                        int drCol = 0;
                        count++;
                        for (int col = co; col <= b; col++, drCol++)
                        {
                            if (((Excel.Range)excelWorksheet.Cells[row, col]).Value2 != null)
                            {
                                //((Excel.Range)excelWorksheet.Cells[row, col]).Value2;
                                String tempstr = ((Excel.Range)excelWorksheet.Cells[row, col]).Value2.ToString();
                                string[] words = tempstr.Split(',');
                                splitcount = splitcount + words.Count();
                                if (words.Count() < 2)
                                {
                                    if (drCol == 4)
                                    {
                                        dr[drCol++] = dr[2].ToString().Trim() + dr[3].ToString().Trim();
                                    }
                                    dr[drCol] = tempstr.Trim();
                                }
                                else
                                {
                                    tokenFlag = words.Count();
                                    tempwords = words;
                                }
                            }

                        }
                        if (tokenFlag == 0)
                        {
                            dt.Rows.Add(dr);
                            dt.AcceptChanges();
                            totalcount++;
                        }
                        else
                        {
                            string col1 = dr[0].ToString();
                            string col2 = dr[1].ToString();
                            string col3 = dr[2].ToString();
                            string col4 = dr[3].ToString();
                            string col5 = dr[4].ToString();
                            string col6 = dr[5].ToString();
                            string col7 = dr[6].ToString();
                            string col8 = dr[7].ToString();
                            string col9 = dr[8].ToString();

                            if (col4 == "")
                            {
                                for (int i = 0; i < tempwords.Count(); i++)
                                {
                                    dr[0] = col1;
                                    dr[1] = col2;
                                    dr[2] = col3.Trim();
                                    dr[3] = tempwords[i].Trim();
                                    dr[4] = dr[2].ToString() + dr[3].ToString();
                                    dr[5] = col6;
                                    dr[6] = col7;
                                    dr[7] = col8;
                                    dr[8] = col9;
                                    dt.Rows.Add(dr);
                                    dt.AcceptChanges();
                                    dr = dt.NewRow();
                                    totalcount++;
                                }
                            }
                            else if (col2 == "")
                            {
                                string strr = "";
                                for (int i = 0; i < tempwords.Count(); i++)
                                {

                                    strr = strr + tempwords[i].Trim();

                                }
                                dr[0] = col1;
                                dr[1] = strr;
                                dr[2] = col3.Trim();
                                dr[3] = col4.Trim();
                                dr[4] = dr[2].ToString() + dr[3].ToString();
                                dr[5] = col6;
                                dr[6] = col7;
                                dr[7] = col8;
                                dr[8] = col9;
                                dt.Rows.Add(dr);
                                dt.AcceptChanges();
                                dr = dt.NewRow();
                                totalcount++;
                            }
                            tokenFlag = 0;
                            tempwords = null;

                        }
                    }
                    #endregion
                }
            }

            #endregion


            //MessageBox.Show("ok" + dt.Rows.Count.ToString() + " Count=" + count.ToString() + " total=" + totalcount.ToString() + " StringCount=" + splitcount.ToString());

        }
        catch (Exception exep)
        {
            // MessageBox.Show(exep.Message);
            dt = null;
        }
        finally
        {
            if (newWorkBook != null)
                newWorkBook.Close(false, Missing.Value, Missing.Value);
            if (excelApp != null)
                excelApp.Quit();
        }

        return dt;
    }




    //rate import functions&&&&&&&&&&&ENDs&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&






    protected void GridViewSupplierRates_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {

    }

    protected void EntityDataRateTask_QueryCreated(object sender, QueryCreatedEventArgs e)
    {
        var AllTasks = e.Query.Cast<ratetaskassign>();
        e.Query = (from c in AllTasks
                   where c.idrateplan == 1 && c.changecommitted != 1
                   orderby c.Status, c.Prefix
                   select c).OrderByDescending(c => c.startdate);
        return;
    }//query created




    protected void ListView1_ItemCommand(object sender, ListViewCommandEventArgs e)
    {

    }

    protected void PartnerTypeSelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddlistType = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListPartnerType");
        DropDownList ddlist = (DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListPartner");
        int ptype = Convert.ToInt32(ddlistType.SelectedValue);
        using (PartnerEntities context = new PartnerEntities())
        {
            var lstParters = context.partners.Where(c => c.PartnerType == ptype).OrderBy(c => c.PartnerName).ToList();
            ddlist.Items.Clear();
            ddlist.Items.Add(new ListItem(" [Select]", "-1"));
            ddlist.Items.Add(new ListItem(" [None]", "0"));
            foreach (partner p in lstParters)
            {
                ddlist.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
            }
        }
        //ddlistType.Width = ddlist.Width;
    }


    protected void DropDownListTaskRef_SelectedIndexChanged(object sender, EventArgs e)
    {
        myGridViewDataBind();
    }

    protected void service_SelectedIndexChanged(object sender, EventArgs e)
    {


        int Category = Convert.ToInt32(((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListservice")).SelectedValue);
        using (PartnerEntities Context = new PartnerEntities())
        {
            List<rateplan> lstPlan = Context.rateplans.Where(c => c.Type == Category).ToList();
            ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListRatePlan")).DataSource = lstPlan;
            ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListRatePlan")).DataBind();
        }

    }


    protected void EntityDataservice_QueryCreated(object sender, QueryCreatedEventArgs e)
    {

    }



    protected void GridViewRateAssign_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Dictionary<long, rateplanassignmenttuple> dicTuple = (Dictionary<long, rateplanassignmenttuple>)Session["assign.sessdictuple"];
            Dictionary<long, enumservicefamily> dicservice = (Dictionary<long, enumservicefamily>)Session["assign.sessdicservice"];
            var dicroute = (Dictionary<int, string>)Session["assign.sessdicroute"];
            Label LabelRoute = (Label)e.Row.FindControl("lblRoute");
            Label Labelservicename = (Label)e.Row.FindControl("lblservice");
            Label LabelPulse = (Label)e.Row.FindControl("lblPulse");
            Label LabelAssignDir = (Label)e.Row.FindControl("lblAssignDir");

            LinkButton LnkBtnRate = (LinkButton)e.Row.FindControl("LinkButtonRate");
            LnkBtnRate = (LinkButton)e.Row.FindControl("LinkButtonRate");
            //LnkBtnRate.OnClientClick = "window.open('rates.aspx?idRatePlan=" + DataBinder.Eval(e.Row.DataItem, "inactive").ToString() + "')";

            //Label TempLabel = (Label)e.Row.FindControl("lblservice");
            rateplanassignmenttuple thisTuple = null;
            int tupleId = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "prefix"));
            long idservice = -1;
            if (dicTuple.TryGetValue(tupleId, out thisTuple))
            {
                enumservicefamily thisservice = null;
                dicservice.TryGetValue(thisTuple.idService, out thisservice);
                Labelservicename.Text = thisservice.ServiceName;
                LabelPulse.Text = thisTuple.priority.ToString();
                LabelAssignDir.Text = thisTuple.AssignDirection.ToString();

                string Routename = "Error!";
                if (thisTuple.route != null)
                {
                    dicroute.TryGetValue(Convert.ToInt32(thisTuple.route), out Routename);
                    LabelRoute.Text = Routename;
                }
                else
                {
                    LabelRoute.Text = "-";
                }

                idservice = thisTuple.idService;
                switch (thisTuple.AssignDirection)
                {
                    case 1:
                        LabelAssignDir.Text = "Customer";
                        break;
                    case 2:
                        LabelAssignDir.Text = "Supplier";
                        break;
                    default:
                        LabelAssignDir.Text = "-";
                        break;
                }

            }

            Dictionary<int, string> dicRatePlan = (Dictionary<int, string>)Session["assign.dicRatePlan"];
            int idRatePlan = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "inactive"));
            string planName = "";
            dicRatePlan.TryGetValue(idRatePlan, out planName);
            rateplan tmprateplan = new rateplan();
            tmprateplan.id = idRatePlan;
            tmprateplan.RatePlanName = planName;
            var lstPlan = new List<rateplan>();
            lstPlan.Add(tmprateplan);

            DropDownList DropDownListRatePlan = (DropDownList)e.Row.FindControl("DropDownListRatePlan");
            DropDownListRatePlan.DataSource = lstPlan;
            DropDownListRatePlan.DataBind();
            DropDownListRatePlan.SelectedValue = idRatePlan.ToString();

            DropDownList DropDownListBillingRule = ((DropDownList)e.Row.FindControl("DropDownListBillingRule"));
            foreach (BillingRule jsb in billingRules)
            {
                DropDownListBillingRule.Items.Add(new ListItem(jsb.RuleName, jsb.Id.ToString()));
            }
            billingruleassignment billingruleassignment = null;
            dicBillRules = Context.billingruleassignments.ToDictionary(b => b.idRatePlanAssignmentTuple);
            //foreach (billingruleassignment thisRule in Context.billingruleassignments)
            //{
            //    dicBillRules.Add(thisRule.idRatePlanAssignmentTuple, thisRule);
            //}
            dicBillRules.TryGetValue(tupleId, out billingruleassignment);
            DropDownListBillingRule.SelectedValue = billingruleassignment.idBillingRule.ToString();

            if ((e.Row.RowState & DataControlRowState.Edit) > 0)
            {

                DropDownList ddlExcludeLCR = (e.Row.FindControl("ddlExcludeLCR") as DropDownList);
                int exclude = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "field3"));
                ddlExcludeLCR.SelectedValue = (exclude == 1 ? "1" : "0");

                using (PartnerEntities Context = new PartnerEntities())
                {
                    var Lst = Context.rateplans.Where(c => c.Type == idservice).ToList();
                    ((DropDownList)e.Row.FindControl("DropDownListRatePlan")).DataSource = Lst;
                    ((DropDownList)e.Row.FindControl("DropDownListRatePlan")).DataBind();
                }


                //ThisLabel
                //set change types e.g. new, delete, increase, decrease etc.
                CheckBox chkDeactive = (CheckBox)e.Row.FindControl("CheckBox1");
                chkDeactive.Enabled = true;

                Label ThisLabel = (Label)e.Row.FindControl("lblRateChangeType");
                if (DataBinder.Eval(e.Row.DataItem, "Status") != null)
                {
                    int ChangeType = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Status"));
                    switch (ChangeType)
                    {
                        case 9:
                            ThisLabel.Text = "Overlap";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 0:
                        case 7:
                        case 8:
                            ThisLabel.Text = "";//committed & uncommitteds are handled by changecommitted flag
                            e.Row.ForeColor = Color.Red;
                            //errors are handled by field2
                            break;
                        case 1:
                            ThisLabel.Text = "Ok";//"Code End";
                            break;
                        case 2:
                            ThisLabel.Text = "Ok";//"Code End";"New";
                            break;
                        case 3:
                            ThisLabel.Text = "Ok";//"Code End";"Increase";
                            break;
                        case 4:
                            ThisLabel.Text = "Ok";//"Code End";"Decrease";
                            break;
                        case 5:
                            ThisLabel.Text = "Ok";//"Code End";"Unchanged";
                            break;

                        case 10:
                            ThisLabel.Text = "Overlap Adjusted";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 11:
                            ThisLabel.Text = "Rate Param Conflict";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 12:
                            ThisLabel.Text = "Rate Position not found";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 13:
                            ThisLabel.Text = "Existing";
                            e.Row.ForeColor = Color.Red;
                            break;
                    }

                }

                //Country
                ThisLabel = (Label)e.Row.FindControl("lblCountry");
                if (DataBinder.Eval(e.Row.DataItem, "CountryCode") != null && DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString() != "")
                {
                    int ThisCountryCode = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString());
                    if (ThisLabel != null)
                    {
                        if (Session["assign.sesCountryCodes"] != null)//partners are used by copying old countrycodes based code from ratetask.aspx
                        {
                            List<partner> CountryCodes = (List<partner>)Session["assign.sesCountryCodes"];
                            if ((CountryCodes.Any(c => c.idPartner == ThisCountryCode)) == true)
                            {
                                ThisLabel.Text = (from c in CountryCodes
                                                  where c.idPartner == ThisCountryCode
                                                  select c.PartnerName + " (" + c.idPartner + ")").First();
                            }
                            else
                            {
                                ThisLabel.Text = "-";
                            }
                        }
                    }
                }

                lblEditPrefix.Text = "";

                //set default values of start/end date controls to their default values before editing

                string Thisdate = lblRateGlobal.Text;
                string[] AllDates = Thisdate.Split('#');

                AjaxControlToolkit.CalendarExtender CalDate = (AjaxControlToolkit.CalendarExtender)e.Row.FindControl("CalendarStartDate");
                TextBox txtTime = (TextBox)e.Row.FindControl("TextBoxStartDateTimePicker");

                //lock start date,time if the assignment is already in effect
                TextBox txtDate = (TextBox)e.Row.FindControl("TextBoxStartDatePicker");
                int Completed = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "changecommitted"));
                if (Completed == 1)
                {
                    txtTime.Enabled = false;
                    txtDate.Enabled = false;
                }

                string strCalDate = AllDates[0];
                string format = "yyyy-MM-dd";
                DateTime dateTime;
                if (DateTime.TryParseExact(strCalDate, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTime))
                {
                    CalDate.SelectedDate = dateTime;
                    ((Label)e.Row.FindControl("lblStartDate")).Text = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    txtTime.Text = AllDates[1];
                }
                else
                {
                    CalDate.SelectedDate = DateTime.Now;
                    //CalDate.VisibleDate = DateTime.Now;
                    txtTime.Text = "00:00:00";
                }

                string StartDateEdit = ";";
                if (DataBinder.Eval(e.Row.DataItem, "startdate") != null)
                {
                    try//exception may occur due to invalid date string
                    {
                        StartDateEdit = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("yyyy-MM-dd HH:mm:ss");
                        ((Label)e.Row.FindControl("lblStartDate")).Text = StartDateEdit;
                    }
                    catch (Exception e1)
                    {
                        //do nothing, jsut allow the program to continue
                    }
                }


                AjaxControlToolkit.CalendarExtender CalDateEnd = (AjaxControlToolkit.CalendarExtender)e.Row.FindControl("CalendarEndDate");
                TextBox txtTimeEnd = (TextBox)e.Row.FindControl("TextBoxEndDateTimePicker");

                string strCalDateEnd = AllDates[2];
                DateTime dateTimeEnd;
                if (DateTime.TryParseExact(strCalDateEnd, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTimeEnd))
                {
                    CalDateEnd.SelectedDate = dateTimeEnd;

                    txtTime.Text = AllDates[3];
                }
                else
                {
                    //CalDateEnd.SelectedDate = DateTime.Today;

                    txtTimeEnd.Text = "00:00:00";
                }

                //disable changing end date if enddate already has a date value
                TextBox txtEndDate = (TextBox)e.Row.FindControl("TextBoxEndDatePicker");
                TextBox txtEndTime = (TextBox)e.Row.FindControl("TextBoxEndDateTimePicker");
                if (CalDateEnd.SelectedDate != null)
                {
                    txtEndDate.Enabled = false;
                    txtEndTime.Enabled = false;
                }

                CheckBox chkbox = (CheckBox)e.Row.FindControl("CheckBox1");
                bool ChangeCommitted = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "changecommitted"));
                if (ChangeCommitted == true)
                {
                    chkbox.Checked = true;
                }
                else
                {
                    chkbox.Checked = false;
                }



            }//edit mode data binding   
            else
            {//not edit mode, binding during normal gridview mode

                LinkButton LnkRate = (LinkButton)e.Row.FindControl("LinkButtonRate");
                LnkRate.OnClientClick = "window.open('rates.aspx?idRatePlan=" + DataBinder.Eval(e.Row.DataItem, "inactive").ToString() + "'); return false;";

                Label lblExcludeLCR = e.Row.FindControl("lblExcludeLCR") as Label;
                int exclude = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "field3"));
                lblExcludeLCR.Text = (exclude == 1 ? "Yes" : "No");
                if (DataBinder.Eval(e.Row.DataItem, "startdate") != null)
                {
                    try//exception may occur due to invalid date string
                    {
                        ((Label)e.Row.FindControl("lblStartDate")).Text = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("yyyy-MM-dd HH:mm:ss");
                        //EffTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("HH:mm:ss");
                    }
                    catch (Exception e1)
                    {
                        //do nothing, just allow the program to continue
                    }
                }


                if (DataBinder.Eval(e.Row.DataItem, "enddate") != null)
                {
                    try//exception may occur due to invalid date string
                    {
                        string ThisEndDate = "";
                        if (DataBinder.Eval(e.Row.DataItem, "enddate") != null && DataBinder.Eval(e.Row.DataItem, "enddate").ToString() != "")
                        {
                            ThisEndDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "enddate")).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        ((Label)e.Row.FindControl("lblEndDate")).Text = ThisEndDate;
                        //EffTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("HH:mm:ss");
                    }
                    catch (Exception e1)
                    {
                        //do nothing, just allow the program to continue
                    }
                }




                CheckBox chkDeactive = (CheckBox)e.Row.FindControl("CheckBox1");
                chkDeactive.Enabled = false;

                CheckBox CheckBoxSelected = (CheckBox)e.Row.FindControl("CheckBoxSelected");
                if (HiddenFieldSelect.Value == "1")
                {
                    CheckBoxSelected.Checked = true;
                }
                else
                {
                    CheckBoxSelected.Checked = false;
                }


                //Label lblResolution = (Label)e.Row.FindControl("lblResolution");
                //lblResolution.Text = DataBinder.Eval(e.Row.DataItem, "Resolution").ToString();

                CheckBox chkbox = (CheckBox)e.Row.FindControl("CheckBox1");
                ////set command argument for link button delete, ID to be retrieved in deletewithtransaction
                LinkButton DelButton = (LinkButton)e.Row.FindControl("LinkButtonDelete");
                DelButton.CommandArgument = DataBinder.Eval(e.Row.DataItem, "id").ToString();
                //set command argument for link button edit to be retrieved in rowcommand event,seperated by #
                LinkButton LnkBtn = (LinkButton)e.Row.FindControl("LinkButtonEdit");

                //for change completed rate tasks, disable the edit button
                bool ChangeCommitted = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "changecommitted"));
                if (ChangeCommitted == true)
                {
                    //LnkBtn.Enabled = false;
                    chkbox.Checked = true;
                }
                else
                {
                    LnkBtn.Enabled = true;
                    chkbox.Checked = false;
                }
                string EffDate = ";";
                string EffTime = ";";
                string EndDate = ";";
                string EndTime = ";";
                if (DataBinder.Eval(e.Row.DataItem, "startdate") != null)
                {
                    try//exception may occur due to invalid date string
                    {
                        EffDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("yyyy-MM-dd");
                        EffTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "startdate")).ToString("HH:mm:ss");
                    }
                    catch (Exception e1)
                    {
                        //do nothing, jsut allow the program to continue
                    }
                }

                if (DataBinder.Eval(e.Row.DataItem, "enddate") != null && DataBinder.Eval(e.Row.DataItem, "enddate").ToString() != "")
                {
                    try
                    {
                        EndDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "enddate")).ToString("yyyy-MM-dd");
                        EndTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "enddate")).ToString("HH:mm:ss");
                    }
                    catch (Exception e1)
                    {
                        //do nothing, jsut allow the program to continue
                    }
                }
                //LnkBtn.CommandArgument = e.Row.RowIndex.ToString();
                LnkBtn.CommandArgument = EffDate + "#" + EffTime + "#" + EndDate + "#" + EndTime;

                //set country here....
                //dropdown ThisLabel = (Label)e.Row.FindControl("lblRateAmount");

                //Country
                Label ThisLabel = (Label)e.Row.FindControl("lblCountry");
                if (DataBinder.Eval(e.Row.DataItem, "CountryCode") != null && DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString() != "")
                {
                    int ThisCountryCode = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString());
                    if (ThisLabel != null)
                    {
                        if (Session["assign.sesCountryCodes"] != null)//partners are used by copying old countrycodes based code from ratetask.aspx
                        {
                            List<partner> CountryCodes = (List<partner>)Session["assign.sesCountryCodes"];
                            if ((CountryCodes.Any(c => c.idPartner == ThisCountryCode)) == true)
                            {
                                ThisLabel.Text = (from c in CountryCodes
                                                  where c.idPartner == ThisCountryCode
                                                  select c.PartnerName + " (" + c.idPartner + ")").First();
                            }
                            else
                            {
                                ThisLabel.Text = "-";
                            }
                        }
                    }
                }


                //lblRateAmount
                ThisLabel = (Label)e.Row.FindControl("lblRateAmount");
                if (DataBinder.Eval(e.Row.DataItem, "RateAmount") != null)
                {
                    decimal ThisRateAmount = Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "RateAmount"));
                    if (ThisLabel != null)
                    {
                        //if(ThisRateAmount
                        ThisLabel.Text = ThisRateAmount.ToString("0.#00000");
                    }
                }
                else //null rateamount
                {
                    if (ThisLabel != null)
                    {
                        //if(ThisRateAmount
                        ThisLabel.Text = "";
                    }
                }

                //ThisLabel
                //set change types e.g. new, delete, increase, decrease etc.
                ThisLabel = (Label)e.Row.FindControl("lblRateChangeType");
                if (DataBinder.Eval(e.Row.DataItem, "Status") != null)
                {
                    int ChangeType = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Status"));
                    switch (ChangeType)
                    {
                        case 9:
                            ThisLabel.Text = "Overlap";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 0:
                        case 7:
                        case 8:
                            ThisLabel.Text = "";//committed & uncommitteds are handled by changecommitted flag
                            e.Row.ForeColor = Color.Red;
                            //errors are handled by field2
                            break;
                        case 1:
                            ThisLabel.Text = "Ok";//"Code End";
                            break;
                        case 2:
                            ThisLabel.Text = "Ok";//"Code End";"New";
                            break;
                        case 3:
                            ThisLabel.Text = "Ok";//"Code End";"Increase";
                            break;
                        case 4:
                            ThisLabel.Text = "Ok";//"Code End";"Decrease";
                            break;
                        case 5:
                            ThisLabel.Text = "Ok";//"Code End";"Unchanged";
                            break;

                        case 10:
                            ThisLabel.Text = "Overlap Adjusted";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 11:
                            ThisLabel.Text = "Rate Param Conflict";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 12:
                            ThisLabel.Text = "Rate Position not found";
                            e.Row.ForeColor = Color.Red;
                            break;
                        case 13:
                            ThisLabel.Text = "Existing";
                            e.Row.ForeColor = Color.Red;
                            break;
                    }

                }
                else
                {
                    ThisLabel.Text = "Unknown";
                }

                //set error types
                ThisLabel = (Label)e.Row.FindControl("lblRateErrors");
                if (DataBinder.Eval(e.Row.DataItem, "Field2") != null && int.Parse(DataBinder.Eval(e.Row.DataItem, "Field2").ToString()) != 0)
                {
                    var color = ColorTranslator.FromHtml("#FA0509");
                    e.Row.ForeColor = color;

                    int ErrorInt = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Field2"));
                    string ErrorString = "";
                    //check bit by bit for each field

                    if (GetBitInteger(ErrorInt, 1) == true)//bit 1=prefix
                    {
                        ErrorString += "No or Invalid Prefix." + ", ";
                    }

                    if (GetBitInteger(ErrorInt, 2) == true)//2=rate
                    {
                        ErrorString += "No or Invalid Rate or SurchargeAmount" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblRateAmount");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 3) == true)//3=pulse
                    {
                        ErrorString += "No or Invalid Pulse" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label PulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 4) == true)//4=effective since
                    {
                        ErrorString += "No or Invalid Effective DateTime." + ", ";
                        //if effective since flag is found, then set the text to ""
                        Label dateLabel = (Label)e.Row.FindControl("lblStartDate");

                        //dateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 5) == true)//5=Invalid Type
                    {
                        ErrorString += "No or Invalid Rate Type!" + ", ";

                    }

                    if (GetBitInteger(ErrorInt, 6) == true)//6=Invalid BTRC % In
                    {
                        ErrorString += "No or Invalid BTRC % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount1");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 7) == true)//7=invalid icx % in Intl In
                    {
                        ErrorString += "No or Invalid ICX % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount2");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 8) == true)//7=invalid ans % in Intl In
                    {
                        ErrorString += "No or Invalid ANS % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount3");
                        //rateLabel.Text = "";
                    }


                    if (GetBitInteger(ErrorInt, 9) == true)//9=Invalid X rate
                    {
                        ErrorString += "No or Invalid X-rate Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount4");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 10) == true)//10=Invalid Y rate
                    {
                        ErrorString += "No or Invalid Y-rate Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount5");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 11) == true)//11=Invalid ans % Z
                    {
                        ErrorString += "No or Invalid ANS % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount6");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 12) == true)//12=Invalid icx % Z
                    {
                        ErrorString += "No or Invalid ICX % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount7");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 13) == true)//13=Invalid igw % Z
                    {
                        ErrorString += "No or Invalid IGW % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount8");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 14) == true)//14=Invalid BTRC % Z
                    {
                        ErrorString += "No or Invalid BTRC % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount9");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 15) == true)//15=Invalid ICX revenue Share
                    {
                        ErrorString += "No or Invalid ICX Rev. % Share" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount10");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 16) == true)//16=Invalid Currency
                    {
                        ErrorString += "No or Invalid Currency" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblCurrency");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 17) == true)//3=Minduration
                    {
                        ErrorString += "No or Invalid Mininum Duration" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label PulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }
                    if (GetBitInteger(ErrorInt, 18) == true)//3=pulse
                    {
                        ErrorString += "No or Invalid Fixed Charge Time" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label PulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }

                    if (GetBitInteger(ErrorInt, 19) == true)//19=end datetime
                    {
                        ErrorString += "No or Invalid End DateTime. Or, End time less than start time." + ", ";
                        //if effective since flag is found, then set the text to ""
                        Label dateLabel = (Label)e.Row.FindControl("lblStartDate");

                        //dateLabel.Text = "";
                    }
                    //remove last new line char
                    int PosLastDot = ErrorString.LastIndexOf(".");
                    if (PosLastDot > 0)
                    {
                        ErrorString = ErrorString.Substring(0, PosLastDot + 1);
                    }
                    //show the error in labelcontrol

                    ThisLabel.Text = ErrorString;

                }

            }//else not edit mode, binding during normal gridview mode



        }// if data row
        else if (e.Row.RowType == DataControlRowType.Footer)
        {
            //keep the flag whether code delete items exist for this task here
            hidvalueCodeDelete.Value = CodeDeleteExists();
        }
    }

    protected void GridViewRateAssign_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        hidvaluerowcolorchange.Value = "";
        string newEndDateAndTime = "";
        GridViewRow row = GridViewRateAssign.Rows[e.RowIndex];
        string newEndDate = ((TextBox)row.FindControl("TextBoxEndDatePicker")).Text;
        string newEndTime = ((TextBox)row.FindControl("TextBoxEndDateTimePicker")).Text;
        if (newEndDate != "")
        {
            newEndDateAndTime = newEndDate + " " + newEndTime;
        }
        string thisPrefix = ((Label)row.FindControl("lblPrefix")).Text;
        Label lblRateErrors = (Label)(GridViewRateAssign.Rows[e.RowIndex].FindControl("lblRateErrors"));
        Label lblBillingInfo = (Label)(GridViewRateAssign.Rows[e.RowIndex].FindControl("lblBillingInfo"));
        DateTime ChangedEndDateTime = new DateTime();
        DateTime SaveStartDateTimeThisRate = new DateTime();
        DateTime.TryParseExact(((Label)row.FindControl("lblStartDate")).Text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out SaveStartDateTimeThisRate);
        if (newEndDateAndTime != "")//new end time not empty
        {
            bool ValidEndTime = false;
            if (DateTime.TryParseExact(newEndDateAndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out ChangedEndDateTime))
            {
                ValidEndTime = true;

                if (ChangedEndDateTime < SaveStartDateTimeThisRate)
                {
                    hidvaluerowcolorchange.Value = e.RowIndex + "," + "End datetime must be greater than start datetime";
                    StatusLabel.Text = "End datetime must be greater than start datetime";
                    StatusLabel.ForeColor = Color.Red;
                    ValidEndTime = false;
                    return;
                }
                //changedenddatetime must not overlap next startdatetime
                rateassign NextRate = null;
                using (PartnerEntities Context = new PartnerEntities())
                {
                    NextRate = Context.rateassigns.Where(c => c.idrateplan == 1 && c.Prefix == Convert.ToInt32(thisPrefix)
                                                              && c.startdate > SaveStartDateTimeThisRate)
                        .OrderBy(c => c.startdate).Take(1).ToList().FirstOrDefault();
                }
                if (NextRate != null && ChangedEndDateTime > NextRate.startdate)
                {
                    hidvaluerowcolorchange.Value = e.RowIndex + "," + "End datetime cannot overlap next effective date.";
                    ValidEndTime = false;
                    return;
                }
            }
            else//new enddatetime is given but, format invalid
            {
                hidvaluerowcolorchange.Value = e.RowIndex + "," + "Invalid End Datetime.";
                return;
            }
            if (ValidEndTime == true)
            {
                //update enddatetime for this assignment

                using (PartnerEntities Context = new PartnerEntities())
                {
                    rateassign ThisRate = Context.rateassigns.Where(c => c.idrateplan == 1 && c.Prefix == Convert.ToInt32(thisPrefix)
                                                                         && c.startdate == SaveStartDateTimeThisRate).FirstOrDefault();

                    DropDownList ddlRatePlan = (DropDownList)(GridViewRateAssign.Rows[e.RowIndex].FindControl("DropDownListRatePlan"));
                    DropDownList ddlBillingRule = (DropDownList)(GridViewRateAssign.Rows[e.RowIndex].FindControl("DropDownListBillingRule"));
                    int Deactive = Convert.ToInt32(((CheckBox)row.FindControl("CheckBox1")).Checked);
                    using (MySqlConnection Con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                    {
                        Con.Open();
                        using (MySqlCommand Cmd = new MySqlCommand("", Con))
                        {
                            string EndDateStr = ChangedEndDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                            //update enddate in rateassign table
                            //use changecommitted field as deactive
                            Cmd.CommandText = " update rateassign set enddate='" + EndDateStr + "', inactive=" + ddlRatePlan.SelectedValue + ", changecommitted=" + Deactive +
                                              " ,field3= " + (GridViewRateAssign.Rows[e.RowIndex].FindControl("ddlExcludeLCR") as DropDownList).SelectedValue +
                                              " where id=" + ThisRate.id.ToString();
                            Cmd.ExecuteNonQuery();

                            Cmd.CommandText =
                                "update billingruleassignment set idBillingRule = " + ddlBillingRule.SelectedValue +
                                " where idRatePlanAssignmentTuple = " + thisPrefix + "";
                            Cmd.ExecuteNonQuery();

                            StatusLabel.ForeColor = Color.Green;
                            StatusLabel.Text = "Updated Successfully";
                        }
                    }
                }
            }
        }
        else//enddatetime kept empty
        {
            // update without datetime part
            //update enddatetime for this assignment

            using (PartnerEntities Context = new PartnerEntities())
            {
                int thisPrefixAsInt = Convert.ToInt32(thisPrefix);
                rateassign thisRate = Context.rateassigns.FirstOrDefault(c => c.idrateplan == 1
                                        && c.Prefix == thisPrefixAsInt
                                        && c.startdate == SaveStartDateTimeThisRate);

                DropDownList ddlRatePlan = (DropDownList)(GridViewRateAssign.Rows[e.RowIndex].FindControl("DropDownListRatePlan"));
                DropDownList ddlBillingRule = (DropDownList)(GridViewRateAssign.Rows[e.RowIndex].FindControl("DropDownListBillingRule"));
                int Deactive = Convert.ToInt32(((CheckBox)row.FindControl("CheckBox1")).Checked);
                using (MySqlConnection Con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                {
                    Con.Open();
                    using (MySqlCommand Cmd = new MySqlCommand("", Con))
                    {
                        string EndDateStr = ChangedEndDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                        //update enddate in rateassign table
                        //use changecommitted field as deactive
                        Cmd.CommandText = " update rateassign set " + " inactive=" + ddlRatePlan.SelectedValue + ", changecommitted=" + Deactive +
                                          " ,field3= " + (GridViewRateAssign.Rows[e.RowIndex].FindControl("ddlExcludeLCR") as DropDownList).SelectedValue +
                                          " where id=" + thisRate.id.ToString();
                        Cmd.ExecuteNonQuery();

                        Cmd.CommandText =
                            "update billingruleassignment set idBillingRule = " + ddlBillingRule.SelectedValue +
                            " where idRatePlanAssignmentTuple = " + thisPrefix + "";
                        Cmd.ExecuteNonQuery();

                        StatusLabel.ForeColor = Color.Green;
                        StatusLabel.Text = "Updated Successfully";
                        GridViewSupplierRates.EditIndex = -1;
                        //myGridViewDataBind(); //row wasn't leaving edit mode
                        Response.Redirect("rateassignment.aspx");
                    }
                }
            }
        }
    }


    protected void GridViewRateAssign_RowUpdated(object sender, GridViewUpdatedEventArgs e)
    {
        if (hidvaluerowcolorchange.Value != "")
        {
            //couldnot change row color after validation fail in row updating
            //taking help of hiddenfield
            string rowcolorchange = "";
            rowcolorchange = hidvaluerowcolorchange.Value;
            if (rowcolorchange.Length > 1 && rowcolorchange.Contains(","))
            {
                int TargetRow = -1;
                int.TryParse(rowcolorchange.Split(',')[0], out TargetRow);
                if (TargetRow > -1)
                {
                    string Msg = rowcolorchange.Split(',')[1];
                    StatusLabel.ForeColor = Color.Red;
                    StatusLabel.Text = Msg;
                }
            }
            e.KeepInEditMode = true;
        }
        else //successfully updated
        {
            GridViewRateAssign.EditIndex = -1;
            myGridViewDataBind();
        }
    }


    protected void lnkCommitchanges_Click(object sender, EventArgs e)
    {
        CommitChanges();
    }
    protected void GridViewRateAssign_RowEditing(object sender, GridViewEditEventArgs e)
    {
        GridViewRateAssign.EditIndex = e.NewEditIndex;
        myGridViewDataBind();
    }
    protected void GridViewRateAssign_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        GridViewRateAssign.EditIndex = -1;
        myGridViewDataBind();
        StatusLabel.Text = "";
    }
    protected void GridViewRateAssign_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        using (PartnerEntities context = new PartnerEntities())
        {
            int id = Convert.ToInt32(((LinkButton)((GridView)sender).Rows[e.RowIndex].FindControl("LinkButtonDelete")).CommandArgument);
            context.Database.ExecuteSqlCommand("delete from rateassign where id=" + id);
        }
        myGridViewDataBind();
    }

    protected void GridViewRateAssign_RowDeleted(object sender, GridViewDeletedEventArgs e)
    {
        //code does not come here...
    }


}
