using TelcobrightMediation;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using LibraryExtensions;

//using System.Reflection;

using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;
//using DocumentFormat.OpenXml.Spreadsheet;
//using DocumentFormat.OpenXml.Packaging;
using PortalApp;
using Process = System.Diagnostics.Process;
using LibraryExtensions;
using PortalApp._portalHelper;
using TelcobrightMediation.Accounting;
using Wintellect.PowerCollections;

[System.Runtime.InteropServices.Guid("5FC3FD70-56C8-4DAA-9C00-472988E7CACD")]
public partial class ConfigRateTask : Page
{
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
        public RatePositions ThisPosition;
        public OverlapTypes ThisOverLapType;
        public rate ThisRate;

        public rate PresentRate;
        public rate PrevRate;
        public rate NextRate;
        public bool Overlap;
        public bool ParameterConflict;
        public bool AutoAdjust;
        public DateTime FutureDate = new DateTime(9999, 12, 31, 23, 59, 59);


        public RatePositioning(rate pPresentRate, rate pPrevRate, rate pNextRate, rate pThisRate, bool pAutoAdjust)
        {
            try
            {
                this.PresentRate = pPresentRate;
                this.PrevRate = pPrevRate;
                this.NextRate = pNextRate;
                this.ThisRate = pThisRate;
                this.ThisRate.Status = 0;//initialize
                this.AutoAdjust = pAutoAdjust;
                this.ThisPosition = RatePositions.NotSet;//initialize.
                this.ThisOverLapType = OverlapTypes.None;
                //replace NULL for enddate field if any of these rates with the future date e.g. '9999-12-31' for simplicity of
                //comparison

                if (this.PresentRate != null)
                {
                    if (this.PresentRate.enddate == null || (this.PresentRate.enddate == new DateTime(1, 1, 1, 0, 0, 0)))
                    {
                        this.PresentRate.enddate = this.FutureDate;
                    }
                }
                if (this.PrevRate != null)
                {
                    if (this.PrevRate.enddate == null || (this.PrevRate.enddate == new DateTime(1, 1, 1, 0, 0, 0)))
                    {
                        this.PrevRate.enddate = this.FutureDate;
                    }
                }
                if (this.NextRate != null)
                {
                    if (this.NextRate.enddate == null || (this.NextRate.enddate == new DateTime(1, 1, 1, 0, 0, 0)))
                    {
                        this.NextRate.enddate = this.FutureDate;
                    }
                }
                if (this.ThisRate != null)
                {
                    if (this.ThisRate.enddate == null || (this.ThisRate.enddate == new DateTime(1, 1, 1, 0, 0, 0)))//c# set null date as 1-1-1 00:00:00
                    {
                        this.ThisRate.enddate = this.FutureDate;
                    }
                }

                //check for code delete
                if (this.ThisRate.rateamount == -1)
                {
                    this.ThisRate.Status = 1;//code delete
                    return;
                }


                //check for each rate position one at a time
                if (Existing() == false)//Parameter conflict flag is also set within this
                    if (Coincide() == false)
                        if (Latest() == false)
                            if (FirstEver() == false)
                                if (BeforeAll() == false)
                                    InBetween();


                this.Overlap = GetOverLaps();
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
                if (this.ThisRate.rateamount == -1)
                {
                    this.ThisRate.Status = 1;//code delete
                    return true;
                }
                else if (this.Overlap == true && this.AutoAdjust == false)
                {
                    this.ThisRate.Status = 9;//overlap
                    return true;
                }
                else if (this.Overlap == true && this.AutoAdjust == true)
                {
                    this.ThisRate.Status = 10;//overlap adjusted
                    return true;
                }
                else // no overlap or rate conflict
                {
                    switch (this.ThisPosition)
                    {
                        case RatePositions.NotSet:
                            this.ThisRate.Status = 12;//rate position not found
                            break;
                        case RatePositions.FirstEver:
                        case RatePositions.BeforeAll:
                            this.ThisRate.Status = 2;//new
                            break;
                        case RatePositions.Coincide:
                            if (this.AutoAdjust == true)
                            {
                                this.ThisRate.Status = 10;//overlap adjusted
                            }
                            else
                            {
                                this.ThisRate.Status = 9;//overlap
                            }
                            break;
                        case RatePositions.Existing:
                            this.ThisRate.Status = 13;//existing
                            break;
                        case RatePositions.InBetween:
                        case RatePositions.Latest:
                            if (this.ThisRate.enddate == this.FutureDate && this.NextRate != null)
                            {
                                this.ThisRate.enddate = this.NextRate.startdate;
                            }
                            if ((this.ThisRate.startdate == this.PrevRate.enddate)
                                || (this.PrevRate.enddate == new DateTime(9999, 12, 31, 23, 59, 59)))//this rate is continuous with previous rate without any pause
                            {
                                if (this.ThisRate.rateamount == this.PrevRate.rateamount)
                                {
                                    this.ThisRate.Status = 5;//unchanged
                                }
                                else if (this.ThisRate.rateamount > this.PrevRate.rateamount)
                                {
                                    this.ThisRate.Status = 3;//increase
                                }
                                else if (this.ThisRate.rateamount < this.PrevRate.rateamount)
                                {
                                    this.ThisRate.Status = 4;//decrease
                                }
                            }
                            else
                            {
                                this.ThisRate.Status = 2;//new
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
            bool overlap = false;
            try
            {
                List<string> lstOverlaps = new List<string>();
                if (this.NextRate != null)
                {
                    if ((this.ThisRate.enddate > this.NextRate.startdate) && (this.ThisRate.enddate < this.FutureDate)) //First check if overlaps next rate
                    {
                        //ThisRate.OverlappingRates = NextRate.id.ToString();
                        lstOverlaps.Add(this.NextRate.id.ToString());
                        this.ThisOverLapType = OverlapTypes.OverlappingNext;
                        overlap = true;
                    }
                }
                if (this.ThisPosition == RatePositions.Coincide)//if coincide
                {

                    lstOverlaps.Add(this.PrevRate.id.ToString());
                    if (this.ThisOverLapType == OverlapTypes.OverlappingNext)
                    {
                        //already overlapping next, now found that it also coincides
                        this.ThisOverLapType = OverlapTypes.OverlappingBoth;
                    }
                    else if (this.ThisOverLapType == OverlapTypes.None)
                    {
                        this.ThisOverLapType = OverlapTypes.OverlapByCoincide;
                    }
                    overlap = true;
                }
                this.ThisRate.OverlappingRates = string.Join(",", lstOverlaps.ToArray());

                return overlap;
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
                if (this.ParameterConflict == true)
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
                if (this.PrevRate != null)
                {
                    if (this.ThisRate.startdate < this.PrevRate.enddate) //if overlaps by coinciding previous rate
                    {
                        if (this.PrevRate.enddate < this.FutureDate)//overlap only if prevrate's end date is < futuredate or not null e.g. has really an end date
                        {
                            this.ThisPosition = RatePositions.Coincide;
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
                if (this.PresentRate != null)
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
                    this.ThisPosition = RatePositions.Existing;
                    return true;
                }
                this.ParameterConflict = false;//if there is no present rate, no param conflict ***NOT USED CURRENTLY***
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
                if (this.PrevRate == null && this.NextRate == null)//first ever
                {
                    this.ThisPosition = RatePositions.FirstEver;
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
                if (this.PrevRate == null && this.NextRate != null)//before all,but there is a next rate
                {
                    this.ThisPosition = RatePositions.BeforeAll;
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
                if (this.PrevRate != null)
                {
                    if ((this.ThisRate.startdate < this.PrevRate.enddate) && (this.PrevRate.enddate == this.FutureDate)
                        && this.NextRate == null)//if prev rate is open
                    {
                        //prevrate's end date is == futuredate == null means this rate is after all rates
                        this.ThisPosition = RatePositions.Latest;
                        return true;
                    }
                    else if ((this.ThisRate.startdate >= this.PrevRate.enddate) &&
                        (this.PrevRate.enddate < this.FutureDate) && this.NextRate == null)//prev rate has end date
                    {
                        //prevrate's end date is != futuredate != null, has a valid end date
                        //thisrate's start has to be >= prevrate.enddate
                        this.ThisPosition = RatePositions.Latest;
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
                if (this.PrevRate != null && this.NextRate != null)
                {
                    if (this.ThisRate.startdate >= this.PrevRate.enddate && this.ThisRate.startdate <= this.NextRate.startdate)
                    {
                        //in between
                        this.ThisPosition = RatePositions.InBetween;
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

        int SetStatus(rate prevRate, rate thisRate)
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
                if (thisRate.rateamount == -1)
                {
                    return -1;//code delete
                }
                if (thisRate.rateamount == prevRate.rateamount)
                {
                    return 5;//unchanged
                }
                else if (thisRate.rateamount > prevRate.rateamount)
                {
                    return 3;//increase
                }
                else if (thisRate.rateamount < prevRate.rateamount)
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
        List<CodeUpdate> _lstCodeUpdate = new List<CodeUpdate>();
    }

    public bool MyIsNumeric(string str)
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
        if (this.hidValueRefName.Value == null || this.hidValueRefName.Value == "")
        {
            return;
        }

        //check for duplicate templatename and alert the client...
        string description = this.hidValueRefName.Value;
        if (description == "")
        {
            string script = "alert('Name cannot be empty!');";
            this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
            return;
        }
        else if (description.IndexOf('=') >= 0 || description.IndexOf(':') >= 0 ||
            description.IndexOf(',') >= 0 || description.IndexOf('?') >= 0)
        {
            string script = "alert('Name cannot contain characters =:,?');";
            this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
            return;
        }
        using (PartnerEntities context = new PartnerEntities())
        {
            int idRatePlan = int.Parse(this.ViewState["task.sesidRatePlan"].ToString());
            if (context.ratetaskreferences.Any(c => c.Description == description && c.idRatePlan == idRatePlan))
            {
                string script = "alert('name: " + description + " exists, try a different name.');";
                this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
                return;
            }
        }

        //rename here
        int idRatePlanThis = int.Parse(this.ViewState["task.sesidRatePlan"].ToString());
        int selectedRefsId = int.Parse(this.DropDownListTaskRef.SelectedValue);
        int selectedRefsIndex = this.DropDownListTaskRef.SelectedIndex;

        using (PartnerEntities context = new PartnerEntities())
        {
            ratetaskreference thisRef = context.ratetaskreferences.Where(c => c.id == selectedRefsId).FirstOrDefault();
            if (thisRef != null)
            {
                thisRef.Description = description;
                context.SaveChanges();
            }
            List<ratetaskreference> lstTaskRef = new List<ratetaskreference>();
            lstTaskRef = context.ratetaskreferences.Where(c => c.idRatePlan == idRatePlanThis).OrderByDescending(c => c.id).ToList();
            this.DropDownListTaskRef.Items.Clear();
            foreach (ratetaskreference tr in lstTaskRef)
            {
                this.DropDownListTaskRef.Items.Add(new ListItem(tr.Description, tr.id.ToString()));
            }
            this.DropDownListTaskRef.SelectedIndex = selectedRefsIndex;//only if not post back
            MyGridViewDataBind();
        }


    }


    protected void NewTaskRefName_Click(object sender, EventArgs e)
    {
        //exit if cancel clicked in javascript...
        if (this.hidValueRefName.Value == null || this.hidValueRefName.Value == "")
        {
            return;
        }

        //check for duplicate templatename and alert the client...
        string description = this.hidValueRefName.Value;
        if (description == "")
        {
            string script = "alert('Name cannot be empty!');";
            this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
            return;
        }
        else if (description.IndexOf('=') >= 0 || description.IndexOf(':') >= 0 ||
            description.IndexOf(',') >= 0 || description.IndexOf('?') >= 0)
        {
            string script = "alert('Name cannot contain characters =:,?');";
            this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
            return;
        }
        using (PartnerEntities context = new PartnerEntities())
        {
            int idRatePlan = int.Parse(this.ViewState["task.sesidRatePlan"].ToString());
            if (context.ratetaskreferences.Any(c => c.Description == description && c.idRatePlan == idRatePlan))
            {
                string script = "alert('name: " + description + " exists, try a different name.');";
                this.ClientScript.RegisterClientScriptBlock(GetType(), "Alert", script, true);
                return;
            }
        }

        //create new ratetaskref here...
        int idRatePlanThis = int.Parse(this.ViewState["task.sesidRatePlan"].ToString());
        //int SelectedRefsId = int.Parse(DropDownListTaskRef.SelectedValue);
        //int SelectedRefsIndex = DropDownListTaskRef.SelectedIndex;

        using (PartnerEntities context = new PartnerEntities())
        {
            ratetaskreference thisRef = new ratetaskreference();

            thisRef.Description = description;
            thisRef.idRatePlan = idRatePlanThis;
            context.ratetaskreferences.Add(thisRef);
            context.SaveChanges();

            List<ratetaskreference> lstTaskRef = new List<ratetaskreference>();
            lstTaskRef = context.ratetaskreferences.Where(c => c.idRatePlan == idRatePlanThis).OrderByDescending(c => c.id).ToList();
            this.DropDownListTaskRef.Items.Clear();
            foreach (ratetaskreference tr in lstTaskRef)
            {
                this.DropDownListTaskRef.Items.Add(new ListItem(tr.Description, tr.id.ToString()));
            }
            this.DropDownListTaskRef.SelectedIndex = 0;//just added, latest
            MyGridViewDataBind();
        }


    }

    //[System.Web.Services.WebMethod]
    public string CodeDeleteExists()
    {
        int rateTaskRefId = int.Parse(HttpContext.Current.Session["task.vsRateTaskId"].ToString());
        using (PartnerEntities context = new PartnerEntities())
        {
            if (context.ratetasks.Any(c => (c.idrateplan == rateTaskRefId) && (c.rateamount == "-1")))
            {
                return "1";
            }
        }
        return "0";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (this.IsPostBack)
            {
                
                //required for script manager, checking code delete existence
                this.Session["task.vsRateTaskId"] = this.DropDownListTaskRef.SelectedValue;//view state didn't work, have to live with session
                this.ViewState["vsQueryString"] = this.Request.Url.Query;
            }
            else  //only post back
            {

                //Retrieve Path from TreeView for displaying in the master page caption label

                TreeView masterTree = (TreeView) this.Master.FindControl("TreeView1");
                CommonCode commonCodes = new CommonCode();
                commonCodes.LoadReportTemplatesTree(ref masterTree);

                string localPath = this.Request.Url.LocalPath;
                int pos2NdSlash = localPath.Substring(1, localPath.Length - 1).IndexOf("/");
                string rootFolder = localPath.Substring(1, pos2NdSlash);
                int endOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(rootFolder);
                string urlWithQueryString = ("~" + "/" + rootFolder + this.Request.Url.AbsoluteUri.Substring((endOfRootFolder + rootFolder.Length), this.Request.Url.AbsoluteUri.Length - (endOfRootFolder + rootFolder.Length))).Replace("%20", " ");
                //for some reason url was not including .aspx
                if (urlWithQueryString.EndsWith(".aspx==") == false)
                {
                    urlWithQueryString += ".aspx";
                }
                //TreeNodeCollection cNodes = MasterTree.Nodes;
                //TreeNode MatchedNode = null;
                //foreach (TreeNode N in cNodes)//for each nodes at root level, loop through children
                //{
                //    MatchedNode = CommonCodes.RetrieveNodes(N, UrlWithQueryString);
                //    if (MatchedNode != null)
                //    {
                //        break;
                //    }
                //}
                //set screentile/caption in the master page...
                Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
                //****CAPTION FOR THIS PAGE IS SET BELOW AFTER FINDING RATEPLAN NAME
                //End of Site Map Part *******************************************************************

                this.TextBoxDefaultDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                //TextBoxDefaultDeleteDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                //temporary code
                this.DropDownListFormat.SelectedIndex = 1;
                //end of temp code

                HttpRequest q = this.Request;
                NameValueCollection n = q.QueryString;
                int v = -1;
                if (n.HasKeys())
                {
                    string k = n.GetKey(0);
                    if (k == "idRatePlan")
                    {
                        v = Convert.ToInt32(n.Get(0));
                        this.LinkButtonRate.OnClientClick = "window.open('rates.aspx?idRatePlan=" + v.ToString() + "'); return false;";
                    }
                }

                this.ViewState["task.sesidRatePlan"] = v;

                //SqlDataTaskStatus.SelectParameters["idRatePlan"].DefaultValue = v.ToString();
                timezone tzRatePlan = null;
                int ratePlanType = 0;
                using (PartnerEntities conigw = new PartnerEntities())
                {
                    //load product dictionary
                    Dictionary<string, Productext> dicProducts = new Dictionary<string, Productext>();
                    foreach (product prd in conigw.products.ToList())
                    {
                        Productext c = new Productext();
                        c.Product.Prefix = prd.Prefix;
                        c.Product.Category = prd.Category;
                        c.Product.SubCategory = prd.SubCategory;
                        c.Product.ServiceFamily = prd.ServiceFamily;
                        c.Product.id = prd.id;
                        c.Saved = true;
                        dicProducts.Add(c.Product.Prefix + "-" + c.Product.Category + "-" + c.Product.SubCategory + "-" + c.Product.ServiceFamily, c);
                    }
                    this.Session["dicProducts"] = dicProducts;
                    rateplan thisPlan = conigw.rateplans.Where(c => c.id == v).First();
                    tzRatePlan = conigw.timezones.Where(c => c.id == thisPlan.TimeZone).First();
                    this.lblTimeZone.Text = tzRatePlan.zone.country.country_name + " " + tzRatePlan.offsetdesc + " [" + tzRatePlan.zone.zone_name + "]";
                    ratePlanType = thisPlan.Type;
                    this.Session["task.sesRatePlanType"] = ratePlanType;

                    this.lblRatePlan.Text = thisPlan.RatePlanName;
                    lblScreenTitle.Text = "Configuration/Rateplan:" + thisPlan.RatePlanName + "/Rate Task";
                    this.ViewState["vsRatePlan"] = thisPlan;
                    this.ViewState["ThisServiceFamily"] = thisPlan.Type;


                }




                //find if any rate Task reference exists
                //if not create one default instance

                using (PartnerEntities context = new PartnerEntities())
                {
                    List<ratetaskreference> lstTaskRef = context.ratetaskreferences.Where(c => c.idRatePlan == v).OrderByDescending(c => c.id).ToList();
                    if (lstTaskRef.Count == 0)
                    {
                        //add default instance
                        ratetaskreference newref = new ratetaskreference();
                        newref.Description = "Default";
                        newref.idRatePlan = v;
                        context.ratetaskreferences.Add(newref);
                        context.SaveChanges();
                        lstTaskRef = context.ratetaskreferences.Where(c => c.idRatePlan == v).OrderByDescending(c => c.id).ToList();
                    }
                    this.DropDownListTaskRef.Items.Clear();
                    foreach (ratetaskreference tr in lstTaskRef)
                    {
                        this.DropDownListTaskRef.Items.Add(new ListItem(tr.Description, tr.id.ToString()));
                    }
                    this.DropDownListTaskRef.SelectedIndex = 0;//only if not post back
                    this.SqlDataTaskStatus.SelectParameters["idRatePlan"].DefaultValue = this.DropDownListTaskRef.SelectedValue;
                }
                //required for script manager, checking code delete existence
                this.Session["task.vsRateTaskId"] = this.DropDownListTaskRef.SelectedValue;//view state didn't work, have to live with session


                //get own telcobrightcustomreid from telcobrightmediation database by matching databaes name
                //from Partner

                string thisConectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;

                MySqlConnection connection = new MySqlConnection(thisConectionString);
                string database = connection.Database.ToString();

                using (PartnerEntities context = new PartnerEntities())
                {
                    telcobrightpartner thisCustomer = (from c in context.telcobrightpartners
                                                       where c.databasename == database
                                                       select c).First();
                    int thisOperatorId = thisCustomer.idCustomer;
                    int idOperatorType = Convert.ToInt32(thisCustomer.idOperatorType);

                    this.Session["task.sesidOperator"] = thisOperatorId;
                    timezone tzNative = new timezone();
                    using (PartnerEntities conPartner = new PartnerEntities())
                    {
                        tzNative = conPartner.timezones.Where(c => c.id == thisCustomer.NativeTimeZone).FirstOrDefault();
                    }

                    long timeZoneDifference = tzNative.gmt_offset - tzRatePlan.gmt_offset;
                    this.ViewState["vsTimeZoneDifference"] = timeZoneDifference;

                    this.Session["task.sesidOperatorType"] = idOperatorType;

                }

                using (PartnerEntities context = new PartnerEntities())
                {
                    int idRatePlan = v;
                    List<ratetask> sesSupplierRates = (from c in context.ratetasks
                                                       where c.idrateplan == v
                                                       select c).ToList();

                    this.Session["task.sesSupplierRates"] = sesSupplierRates;


                    List<countrycode> sesCountryCodes = context.countrycodes.ToList();
                    this.Session["task.sesCountryCodes"] = sesCountryCodes;

                    MyGridViewDataBind();
                }
            }//!postback
        }
        catch (Exception e1)
        {
            this.StatusLabel.ForeColor = Color.Red;
            this.StatusLabel.Text = e1.Message + "<br/>" + e1.InnerException;
        }
    }


    protected void MyGridViewDataBind()
    {
        Exception e = null;
        try
        {
            List<long> lstIds = null;
            List<ratetask> lstTasks = GetRateTasksForGridAndDelete(ref e, ref lstIds);
            if (e == null)
            {
                this.GridViewSupplierRates.DataSourceID = "";
                this.GridViewSupplierRates.DataSource = lstTasks;
                this.GridViewSupplierRates.DataBind();
                this.ListView1.DataBind();
                this.LinkButtonSaveAll.Visible = true;

            }
            else
            {
                this.StatusLabel.Text = e.Message + "<br/>" + e.InnerException;
            }
        }
        catch (Exception e1)
        {
            this.StatusLabel.Text = e1.Message + "<br/>" + e1.InnerException;
        }
    }

    protected List<ratetask> GetRateTasksForGridAndDelete(ref Exception e1, ref List<long> lstIds)
    {
        List<ratetask> lstTask = new List<ratetask>();
        try
        {
            //idrateplan column in ratetask is to be used as idRateTaskReference

            if (this.DropDownListTaskRef.SelectedIndex < 0) this.DropDownListTaskRef.SelectedIndex = 0;
            int taskRefId = int.Parse(this.DropDownListTaskRef.SelectedValue);
            this.SqlDataTaskStatus.SelectParameters["idRatePlan"].DefaultValue = this.DropDownListTaskRef.SelectedValue;

            string strPrefix = this.TextBoxFindByPrefix.Text.Trim();
            string strDescription = this.TextBoxFindByDescription.Text.Trim().ToLower();
            string selectedValueMoreFilter = this.DropDownListMoreFilters.SelectedValue;
            StringBuilder sbSql = new StringBuilder().Append(" select * from ratetask where id>0 ")
                                                     .Append(" and idrateplan= " + taskRefId);


            if (strPrefix.Trim() != "")
            {
                sbSql.Append(" and prefix like '" + (strPrefix.Trim().EndsWith("*")?strPrefix.Trim().Replace("*","")+"%":strPrefix.Trim()) + "' ");
            }
            if (strDescription != "")//can't be else if
            {
                sbSql.Append(" and lower(description) like '%" + strDescription + "%' ");
            }
            if (int.Parse(selectedValueMoreFilter) >= 0)//can't be else if
            {

                switch (Convert.ToInt32(selectedValueMoreFilter))
                {
                    case 0://error, field2>0
                        sbSql.Append(" and (cast(field2 as unsigned))>0 ");
                        break;
                    case 1://code deletes
                        sbSql.Append(" and (cast(rateamount as signed))=-1 ");
                        break;
                    case 2://new codes
                        sbSql.Append(" and status=2 and changecommitted=1 ");
                        break;
                    case 3://increase
                        sbSql.Append(" and status=3 and changecommitted=1 ");
                        break;
                    case 4://decrease
                        sbSql.Append(" and status=4 and changecommitted=1 ");
                        break;
                    case 5://unchanged
                        sbSql.Append(" and status=5  and changecommitted=1 ");
                        break;
                    case 7://complete
                        sbSql.Append(" and changecommitted=1 ");
                        break;
                    case 8://change uncommitted
                        sbSql.Append(" and changecommitted!=1 ");
                        break;
                    case 9://overlap
                        sbSql.Append(" and status=9 and changecommitted!=1 ");
                        break;
                    case 10://overlap adjusted
                        sbSql.Append(" and status=10 and changecommitted=1 ");
                        break;
                    case 11://rate param conflict
                        sbSql.Append(" and status=11 and changecommitted!=1 ");
                        break;
                    case 12://rate position not found
                        sbSql.Append(" and status=12 and changecommitted!=1 ");
                        break;
                    case 13://existing
                        sbSql.Append(" and status=13 and changecommitted !=1 ");
                        break;
                }//switch
            }//if more filters are selected in the dropdown
             //use two list to order by code deletes at the top of the list 

            List<ratetask> lstNewCodes = new List<ratetask>();
            using (var context = new PartnerEntities())
            {
                lstTask = context.ratetasks.SqlQuery(sbSql.ToString()+" and rateamount=-1 ", typeof(ratetask)).ToList();
                lstNewCodes = context.ratetasks.SqlQuery(sbSql.ToString()+" and rateamount!=-1 ", typeof(ratetask)).ToList();
                lstTask.AddRange(lstNewCodes); //context.ratetasks.SqlQuery(sbSql.ToString(), typeof(ratetask)).ToList();
                lstIds = lstTask.Select(c => c.id).ToList();
                return lstTask;
            }
        }//try
        catch (Exception e)
        {
            e1 = e;
            return lstTask;
        }
    }




    protected void EntityDataSupplierRates_QueryCreated(object sender, QueryCreatedEventArgs e)
    {

    }
    protected void EntityDataSupplierRates_Inserting(object sender, EntityDataSourceChangingEventArgs e)
    {
        rate newElement = e.Entity as rate;
        System.Web.UI.WebControls.Calendar thisCalendar = (System.Web.UI.WebControls.Calendar) this.GridViewSupplierRates.FindControl("CalendarStartDate");
        newElement.startdate = thisCalendar.SelectedDate;
    }

    bool GetBitInteger(Int32 x, int bitPosition)
    {

        int shiftvariable = 0;
        shiftvariable = bitPosition - 1;

        long shiftResult = x >> shiftvariable;
        long andResult = shiftResult & 1;
        if (andResult == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    Int32 SetBitInteger(Int32 x, int bitPosition)
    {

        UInt32 orvariable = 0;
        orvariable = Convert.ToUInt32(Math.Pow(2, (bitPosition - 1)).ToString());//1;

        long z = (orvariable | x);
        return (int)z;
    }

    private int GetIntFromBitArray(BitArray bitArray)
    {

        if (bitArray.Length > 32)
            throw new ArgumentException("Argument length shall be at most 32 bits.");

        int[] array = new int[1];
        bitArray.CopyTo(array, 0);
        return array[0];

    }

    private string[] LineToFields(string thisLine)
    {
        List<string> lStr = new List<string>();
        StringBuilder sb = new StringBuilder();

        int oddEvenCounter = 0;
        foreach (char c in thisLine)
        {
            if (c == '`')
            {
                oddEvenCounter++;
                continue;
            }

            if (oddEvenCounter % 2 == 0)
            {
                lStr.Add(sb.ToString());
                sb = new StringBuilder();
                oddEvenCounter = 0;
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
        if (e.Row.RowType == DataControlRowType.DataRow)
        {

            if ((e.Row.RowState & DataControlRowState.Edit) > 0)
            {
                //
                this.lblEditPrefix.Text = "";

                //set default values of start/end date controls to their default values before editing

                string thisdate = this.lblRateGlobal.Text;
                string[] allDates = thisdate.Split('#');

                AjaxControlToolkit.CalendarExtender calDate = (AjaxControlToolkit.CalendarExtender)e.Row.FindControl("CalendarStartDate");
                TextBox txtTime = (TextBox)e.Row.FindControl("TextBoxStartDateTimePicker");

                string strCalDate = allDates[0];
                string format = "yyyy-MM-dd";
                DateTime dateTime;
                if (DateTime.TryParseExact(strCalDate, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTime))
                {
                    calDate.SelectedDate = dateTime;
                    //CalDate.vi = dateTime;
                    txtTime.Text = allDates[1];
                }
                else
                {
                    calDate.SelectedDate = DateTime.Now;
                    //CalDate.VisibleDate = DateTime.Now;
                    txtTime.Text = "00:00:00";
                }


                AjaxControlToolkit.CalendarExtender calDateEnd = (AjaxControlToolkit.CalendarExtender)e.Row.FindControl("CalendarEndDate");
                TextBox txtTimeEnd = (TextBox)e.Row.FindControl("TextBoxEndDateTimePicker");

                string strCalDateEnd = allDates[2];
                DateTime dateTimeEnd;
                if (DateTime.TryParseExact(strCalDateEnd, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTimeEnd))
                {
                    calDateEnd.SelectedDate = dateTimeEnd;

                    txtTime.Text = allDates[3];
                }
                else
                {
                    //CalDateEnd.SelectedDate = DateTime.Today;

                    txtTimeEnd.Text = "00:00:00";
                }
            }//edit mode data binding   
            else
            {//not edit mode, binding during normal gridview mode

                CheckBox checkBoxSelected = (CheckBox)e.Row.FindControl("CheckBoxSelected");
                if (this.HiddenFieldSelect.Value == "1")
                {
                    checkBoxSelected.Checked = true;
                }
                else
                {
                    checkBoxSelected.Checked = false;
                }


                //Label lblResolution = (Label)e.Row.FindControl("lblResolution");
                //lblResolution.Text = DataBinder.Eval(e.Row.DataItem, "Resolution").ToString();

                CheckBox chkbox = (CheckBox)e.Row.FindControl("CheckBox1");
                //set command argument for link button edit to be retrieved in rowcommand event,seperated by #
                LinkButton lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonEdit");

                //for change completed rate tasks, disable the edit button
                bool changeCommitted = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "changecommitted"));
                if (changeCommitted == true)
                {
                    lnkBtn.Enabled = false;
                    chkbox.Checked = true;
                }
                else
                {
                    lnkBtn.Enabled = true;
                    chkbox.Checked = false;
                }

                ////save ratetask id in LinkButtonDelete command argument for deleting
                //LnkBtn = (LinkButton)e.Row.FindControl("LinkButtonDelete");
                //LnkBtn.CommandArgument = DataBinder.Eval(e.Row.DataItem, "id").ToString();

                string effDate = ";";
                string effTime = ";";
                string endDate = ";";
                string endTime = ";";
                if (DataBinder.Eval(e.Row.DataItem, "startdate") != null)
                {
                    try//exception may occur due to invalid date string
                    {
                        DateTime tempDate = new DateTime(1, 1, 1);
                        DateTime tempTime = new DateTime(1, 1, 1);
                        if (DateTime.TryParseExact(DataBinder.Eval(e.Row.DataItem, "startdate").ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                        {
                            effDate = tempDate.ToString("yyyy-MM-dd");
                        }
                        if (DateTime.TryParseExact(DataBinder.Eval(e.Row.DataItem, "startdate").ToString(), "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempTime))
                        {
                            effTime = tempTime.ToString("yyyy-MM-dd");
                        }
                    }
                    catch (Exception e1)
                    {
                        //do nothing, jsut allow the program to continue
                    }
                }

                if (DataBinder.Eval(e.Row.DataItem, "enddate") != null && DataBinder.Eval(e.Row.DataItem, "enddate") != "")
                {
                    try
                    {
                        //EndDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "enddate")).ToString("yyyy-MM-dd");
                        //EndTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "enddate")).ToString("HH:mm:ss");
                        DateTime dEndDate = new DateTime();
                        if (DateTime.TryParseExact(DataBinder.Eval(e.Row.DataItem, "enddate").ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dEndDate)==true)
                        {
                            endDate = dEndDate.ToString("yyyy-MM-dd");
                            endTime = dEndDate.ToString("HH:mm:ss");
                        }
                        else
                        {
                            endDate = "";
                            endTime = "";
                        }
                        
                    }
                    catch (Exception e1)
                    {
                        //do nothing, jsut allow the program to continue
                    }
                }
                //LnkBtn.CommandArgument = e.Row.RowIndex.ToString();
                lnkBtn.CommandArgument = effDate + "#" + effTime + "#" + endDate + "#" + endTime;

                //set country here....
                //dropdown ThisLabel = (Label)e.Row.FindControl("lblRateAmount");

                //Country
                Label thisLabel = (Label)e.Row.FindControl("lblCountry");
                if (DataBinder.Eval(e.Row.DataItem, "CountryCode") != null && DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString() != "")
                {
                    string thisCountryCode = DataBinder.Eval(e.Row.DataItem, "CountryCode").ToString();
                    if (thisLabel != null)
                    {
                        if (this.Session["task.sesCountryCodes"] != null)
                        {

                            List<countrycode> countryCodes = (List<countrycode>) this.Session["task.sesCountryCodes"];
                            if ((countryCodes.Any(c => c.Code == thisCountryCode)) == true)
                            {
                                thisLabel.Text = (from c in countryCodes
                                                  where c.Code == thisCountryCode
                                                  select c.Name + " (" + c.Code + ")").First();
                            }
                        }
                    }
                }


                //lblRateAmount
                thisLabel = (Label)e.Row.FindControl("lblRateAmount");
                if (DataBinder.Eval(e.Row.DataItem, "RateAmount") != null)
                {
                    decimal thisRateAmount = 0;
                    decimal.TryParse(DataBinder.Eval(e.Row.DataItem, "RateAmount").ToString(), out thisRateAmount);
                    if (thisLabel != null)
                    {
                        //if(ThisRateAmount
                        thisLabel.Text = thisRateAmount.ToString("0.#00000");
                    }
                }
                else //null rateamount
                {
                    if (thisLabel != null)
                    {
                        //if(ThisRateAmount
                        thisLabel.Text = "";
                    }
                }

                //ThisLabel
                //set change types e.g. new, delete, increase, decrease etc.
                thisLabel = (Label)e.Row.FindControl("lblRateChangeType");
                if (DataBinder.Eval(e.Row.DataItem, "Status") != null)
                {
                    int changeType = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Status"));
                    switch (changeType)
                    {
                        case 0:
                        case 7:
                        case 8:
                            thisLabel.Text = "";//committed & uncommitteds are handled by changecommitted flag
                            //errors are handled by field2
                            break;
                        case 1:
                            thisLabel.Text = "Code End";
                            break;
                        case 2:
                            thisLabel.Text = "New";
                            break;
                        case 3:
                            thisLabel.Text = "Increase";
                            break;
                        case 4:
                            thisLabel.Text = "Decrease";
                            break;
                        case 5:
                            thisLabel.Text = "Unchanged";
                            break;
                        case 9:
                            thisLabel.Text = "Overlap";
                            break;
                        case 10:
                            thisLabel.Text = "Overlap Adjusted";
                            break;
                        case 11:
                            thisLabel.Text = "Rate Param Conflict";
                            break;
                        case 12:
                            thisLabel.Text = "Rate Position not found";
                            break;
                        case 13:
                            thisLabel.Text = "Existing";
                            break;
                    }

                }
                else
                {
                    thisLabel.Text = "Unknown";
                }

                //set error types
                thisLabel = (Label)e.Row.FindControl("lblRateErrors");
                if (DataBinder.Eval(e.Row.DataItem, "Field2") != null && int.Parse(DataBinder.Eval(e.Row.DataItem, "Field2").ToString()) != 0)
                {
                    var color = ColorTranslator.FromHtml("#FA0509");
                    e.Row.ForeColor = color;

                    int errorInt = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Field2"));
                    string errorString = "";
                    //check bit by bit for each field

                    if (GetBitInteger(errorInt, 1) == true)//bit 1=prefix
                    {
                        errorString += "No or Invalid Prefix." + ", ";
                    }

                    if (GetBitInteger(errorInt, 2) == true)//2=rate
                    {
                        errorString += "No or Invalid Rate or SurchargeAmount" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblRateAmount");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 3) == true)//3=pulse
                    {
                        errorString += "No or Invalid Pulse" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label pulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 4) == true)//4=effective since
                    {
                        errorString += "No or Invalid Effective DateTime." + ", ";
                        //if effective since flag is found, then set the text to ""
                        Label dateLabel = (Label)e.Row.FindControl("lblStartDate");

                        //dateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 5) == true)//5=Invalid Type
                    {
                        errorString += "No or Invalid Rate Type!" + ", ";

                    }

                    if (GetBitInteger(errorInt, 6) == true)//6=Invalid BTRC % In
                    {
                        errorString += "No or Invalid BTRC % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount1");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 7) == true)//7=invalid icx % in Intl In
                    {
                        errorString += "No or Invalid ICX % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount2");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 8) == true)//7=invalid ans % in Intl In
                    {
                        errorString += "No or Invalid ANS % Intl. In" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount3");
                        //rateLabel.Text = "";
                    }


                    if (GetBitInteger(errorInt, 9) == true)//9=Invalid X rate
                    {
                        errorString += "No or Invalid X-rate Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount4");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 10) == true)//10=Invalid Y rate
                    {
                        errorString += "No or Invalid Y-rate Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount5");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 11) == true)//11=Invalid ans % Z
                    {
                        errorString += "No or Invalid ANS % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount6");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 12) == true)//12=Invalid icx % Z
                    {
                        errorString += "No or Invalid ICX % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount7");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 13) == true)//13=Invalid igw % Z
                    {
                        errorString += "No or Invalid IGW % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount8");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 14) == true)//14=Invalid BTRC % Z
                    {
                        errorString += "No or Invalid BTRC % of Z Intl. Out" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount9");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 15) == true)//15=Invalid ICX revenue Share
                    {
                        errorString += "No or Invalid ICX Rev. % Share" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblOtherAmount10");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 16) == true)//16=Invalid Currency
                    {
                        errorString += "No or Invalid Currency" + ", ";

                        //even with invalid rate, rateamount field kept on showing 0
                        //so, if rate error flag is found, then set the text to ""
                        Label rateLabel = (Label)e.Row.FindControl("lblCurrency");
                        //rateLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 17) == true)//3=Minduration
                    {
                        errorString += "No or Invalid Mininum Duration" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label pulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }
                    if (GetBitInteger(errorInt, 18) == true)//3=pulse
                    {
                        errorString += "No or Invalid Fixed Charge Time" + ", ";
                        //if pulse flag is found, then set the text to ""
                        Label pulseLabel = (Label)e.Row.FindControl("lblPulse");
                        //PulseLabel.Text = "";
                    }

                    if (GetBitInteger(errorInt, 19) == true)//19=end datetime
                    {
                        errorString += "No or Invalid End DateTime. Or, End time less than start time." + ", ";
                        //if effective since flag is found, then set the text to ""
                        Label dateLabel = (Label)e.Row.FindControl("lblStartDate");

                        //dateLabel.Text = "";
                    }
                    //remove last new line char
                    int posLastDot = errorString.LastIndexOf(".");
                    if (posLastDot > 0)
                    {
                        errorString = errorString.Substring(0, posLastDot + 1);
                    }
                    //show the error in labelcontrol
                    thisLabel.Text = errorString;
                }

            }//else not edit mode, binding during normal gridview mode
        }// if data row
        else if (e.Row.RowType == DataControlRowType.Footer)
        {
            //keep the flag whether code delete items exist for this task here
            this.hidvalueCodeDelete.Value = CodeDeleteExists();
        }
    }
    protected void GridViewSupplierRates_RowEditing(object sender, GridViewEditEventArgs e)
    {
        this.GridViewSupplierRates.EditIndex = e.NewEditIndex;
        MyGridViewDataBind();
        //GridViewSupplierRates.DataBind();    
        //LinkButtonCancelAll.Visible = false;
        this.LinkButtonSaveAll.Visible = false;

    }
    protected void GridViewSupplierRates_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        //GridViewSupplierRates.DataBind();
        if (e.CommandName == "Edit")
        {
            string effEndDate = e.CommandArgument.ToString();
            this.lblRateGlobal.Text = effEndDate;

            //int rowIndex = ((GridViewRow)((LinkButton)e.CommandSource).NamingContainer).RowIndex;
            //GridViewSupplierRates.EditIndex = rowIndex;
            //myGridViewDataBind();

            //lblIdPartnerGlobal.Text = idPartner;
        }

        if (e.CommandName == "Update")
        {
            //myGridViewDataBind();
            //GridViewSupplierRates.EditIndex = -1;
            //myGridViewDataBind();

        }

        if (e.CommandName == "myCancel")
        {
            this.GridViewSupplierRates.EditIndex = -1;
            MyGridViewDataBind();
        }

    }
    protected void GridViewSupplierRates_PreRender(object sender, EventArgs e)
    {
        //Label lblDescription = GridViewSupplierRates.Rows[0].FindControl("lblDescription") as Label;

        //TextBox txtDescription = GridViewSupplierRates.Rows[0].FindControl("txtDescription") as TextBox;
        //string Description = GridViewSupplierRates.Rows[0].Cells[3].Text;
    }

    //private rate CreateNewRate(
    //                                    int CurrentIdRatePlan,
    //                                    string newId,//will be -1 for insert, will be >0 for update
    //                                    string newPrefix,
    //                                    string newDesc,
    //                                    string newRateAmount,
    //                                    string newResolution,
    //                                    string newMinDurationSec,
    //                                    string newCountry,
    //                                    string newStartDateAndTime,
    //                                    string newEndDateAndTime,
    //                                    string newInactive,
    //                                    string newRouteDisabled,
    //                                    string newWeekDayStart,
    //                                    string newWeekDayEnd,
    //                                    string newStartTime,
    //                                    string newEndTime,
    //                                    string newSurchargeTime,
    //                                    string newSurchargeAmount,
    //                                    string newType,
    //                                    string newOtherAmount1,
    //                                    string newOtherAmount2,
    //                                    string newOtherAmount3,
    //                                    string newOtherAmount4,
    //                                    string newOtherAmount5,
    //                                    string newOtherAmount6,
    //                                    string newOtherAmount7,
    //                                    string newOtherAmount8,
    //                                    string newOtherAmount9,
    //                                    string newOtherAmount10,
    //                                    string newCurrency,
    //                                    int idOperatorType,
    //                                    double TimeZoneOffsetSec,
    //                                    string newMinSurchargeTime,
    //                                    string newMinSurchargeAmount
    //                                    )//field3=idpartner, de-normalization for performance...
    //{
    //    rate ThisRate = new rate();

    //    //validate current rate plan id first...
    //    if (CurrentIdRatePlan == null || CurrentIdRatePlan <= 0)
    //    {
    //        throw new Exception("Fatal error: No or invalid current RatePlanId!");
    //    }
    //    else
    //    {
    //        ThisRate.idrateplan = CurrentIdRatePlan;    
    //    }

    //    //validate field3=idpartner
    //    int RatePlanType = -1;
    //    if (Session["task.sesRatePlanType"]!=null )
    //    {
    //        RatePlanType = (int)Session["task.sesRatePlanType"];
    //    }



    //    if (TimeZoneOffsetSec == -360000)
    //    {
    //        //invalid timezone
    //        throw new Exception("Fatal error: Invalid Timezone Offset (-360000)!");
    //    }
    //    else
    //    {
    //        ThisRate.TimeZoneOffsetSec = TimeZoneOffsetSec;
    //    }

    //    //initialize Field1 and Field2 as 0, they will keep certain flags
    //    ThisRate.Status = 0;
    //    ThisRate.field2 = 0;

    //    //add fields one by one and also validate them

    //    int newIdInt = -1;
    //    if (int.TryParse(newId,out newIdInt))
    //    {
    //        ThisRate.id = newIdInt;
    //    }


    //    //error or validation codes: bits are set in field2 for each rate
    //    //bit 1=prefix
    //    //2=rate
    //    //3=pulse
    //    //4=effective since

    //    double myNum = 0; //prefix isnumeric
    //    if (double.TryParse(newPrefix, out myNum))
    //    {
    //        ThisRate.Prefix = newPrefix;
    //    }
    //    else //invalid prefix
    //    {
    //        int NewFlag = SetBitInteger(Convert.ToInt32( ThisRate.field2), 1);
    //        ThisRate.field2 = NewFlag;
    //    }

    //    ThisRate.description = newDesc; //description, no validation

    //    double myDecimal = 0;//rate
    //    if (double.TryParse(newRateAmount, out myDecimal))
    //    {
    //        //only -1 is allowed as negative rates indicating rate deletion
    //        if (myDecimal >= 0)
    //        {
    //            ThisRate.rateamount = myDecimal;
    //        }
    //        else if (myDecimal == -1) //rate deletion
    //        {
    //            //0=Unknown, 1=delete, 2=new, 3=increase, 4=decrease
    //            ThisRate.rateamount = -1;
    //            ThisRate.Status = 1;
    //        }
    //        else //Unknown change
    //        {
    //            ThisRate.Status = 0;
    //        }
    //    }
    //    else //invalid Rate
    //    {
    //        if (RatePlanType!= 4 && RatePlanType!=3)//not intl.outgoing
    //        {
    //            int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 2);
    //            ThisRate.field2 = NewFlag;
    //        }
    //    }

    //    myDecimal = 0;//surchargeamount
    //    if (double.TryParse(newSurchargeAmount, out myDecimal))
    //    {   
    //        if (myDecimal >= 0)
    //        {
    //            ThisRate.SurchargeAmount = myDecimal;
    //        }
    //    }
    //    else //invalid Rate or surchargeamount
    //    {
    //        if (RatePlanType != 4 && RatePlanType!=3)//not intl.outgoing
    //        {
    //            int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 2);
    //            ThisRate.field2 = NewFlag;
    //        }
    //    }

    //    int myInt = 0;
    //    if (int.TryParse(newResolution, out myInt))
    //    {
    //        ThisRate.Resolution = myInt;
    //    }
    //    else //invalid resolution/pulse/minDurationSec or surchargetime
    //    {
    //        int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 3);
    //        ThisRate.field2 = NewFlag;
    //    }

    //    myInt = 0;
    //    if (int.TryParse(newMinDurationSec, out myInt))
    //    {
    //        ThisRate.MinDurationSec = myInt;
    //    }
    //    else //invalid resolution/pulse, minDurationSec or surchargetime
    //    {
    //        int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 3);
    //        ThisRate.field2 = NewFlag;
    //    }

    //    myInt = 0;
    //    if (int.TryParse(newSurchargeTime, out myInt))
    //    {
    //        ThisRate.SurchargeTime = myInt;
    //    }
    //    else //invalid resolution/pulse, minDurationSec or surchargetime
    //    {
    //        int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 3);
    //        ThisRate.field2 = NewFlag;
    //    }
    //    ThisRate.CountryCode = newCountry;//no validation

    //    string format = "yyyy-MM-dd HH:mm:ss"; //effective date
    //    DateTime dateTime;
    //    if (DateTime.TryParseExact(newStartDateAndTime, format, CultureInfo.InvariantCulture,
    //        DateTimeStyles.None, out dateTime))
    //    {
    //        ThisRate.startdate = dateTime;
    //    }
    //    else //invalid start or end date
    //    {
    //        int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 4);
    //        ThisRate.field2 = NewFlag;
    //        return ThisRate;//cannot proceed with further validation as end date's validity depends on startdate
    //    }

    //    if (newEndDateAndTime != "\\N" && newEndDateAndTime != "")
    //    {
    //        if (DateTime.TryParseExact(newEndDateAndTime, format, CultureInfo.InvariantCulture,
    //            DateTimeStyles.None, out dateTime))
    //        {
    //            ThisRate.enddate = dateTime;

    //            //end date must be >= start date
    //            if (ThisRate.enddate < ThisRate.startdate)
    //            {
    //                //invalid start or end date
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 4);
    //                ThisRate.field2 = NewFlag;
    //                var color = ColorTranslator.FromHtml("#FF0000");
    //                StatusLabel.ForeColor = color;
    //                StatusLabel.Text = "End date must be greater that start date for all rates!";
    //            }
    //        }
    //        else
    //        {
    //            //invalid start or end date
    //            int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 4);
    //            ThisRate.field2 = NewFlag;
    //        }
    //    }
    //    else
    //    {
    //        ThisRate.enddate = null;//enddate can be null
    //    }

    //    //end date must be > startdate
    //    if (ThisRate.enddate < ThisRate.startdate)
    //    {
    //        //throw new Exception("End date must be greater than start date!");
    //        //invalid start or end date
    //        int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 4);
    //        ThisRate.field2 = NewFlag;
    //        var color = ColorTranslator.FromHtml("#FF0000");
    //        StatusLabel.ForeColor = color;
    //        StatusLabel.Text = "End date must be greater that start date for all rates!";
    //    }

    //    //currency
    //    myInt = 0;
    //    if (int.TryParse(newCurrency, out myInt))
    //    {
    //        ThisRate.Currency = myInt;
    //    }
    //    else //invalid currency
    //    {
    //        int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 16);
    //        ThisRate.field2 = NewFlag;
    //    }



    //    myInt = 0;
    //    if (int.TryParse(newType, out myInt)) //type
    //    {
    //        ThisRate.Type = myInt;
    //        //type enum
    //        //1=customer,2=supplier,3=intl in,4=intl out
    //        if (ThisRate.Type == 3) //intl in
    //        {
    //            Single myFloat = 0;
    //            if (Single.TryParse(newOtherAmount1, out myFloat) == true)
    //            {
    //                ThisRate.OtherAmount1 = myFloat;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 6);//vat percentage in invalid
    //                ThisRate.field2 = NewFlag;
    //            }

    //            //ThisRate.OtherAmount2        =OtherAmount2      ;
    //            myFloat = 0;
    //            if (Single.TryParse(newOtherAmount2, out myFloat) == true)
    //            {
    //                ThisRate.OtherAmount2 = myFloat;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 7);//invalid icx percentage in
    //                ThisRate.field2 = NewFlag;
    //            }

    //            //ThisRate.OtherAmount3        =OtherAmount3      ;
    //            myFloat = 0;
    //            if (Single.TryParse(newOtherAmount3, out myFloat) == true)
    //            {
    //                ThisRate.OtherAmount3 = myFloat;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 8);//invalid ans percentage in
    //                ThisRate.field2 = NewFlag;
    //            }
    //        }
    //        else if (ThisRate.Type == 4) //intl out, xyz
    //        {
    //            Single myFloat = 0;
    //            double myDec = 0;

    //            if (double.TryParse(newOtherAmount4, out myDec) == true)
    //            {
    //                ThisRate.OtherAmount4 = myDec;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 9);//X is invalid
    //                ThisRate.field2 = NewFlag;
    //            }

    //            if (double.TryParse(newOtherAmount5, out myDec) == true)
    //            {
    //                ThisRate.OtherAmount5 = myDec;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 10);//Y is invalid
    //                ThisRate.field2 = NewFlag;
    //            }

    //            if (Single.TryParse(newOtherAmount6, out myFloat) == true)
    //            {
    //                ThisRate.OtherAmount6 = myFloat;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 11);//vat percentage in invalid
    //                ThisRate.field2 = NewFlag;
    //            }

    //            myFloat = 0;
    //            if (Single.TryParse(newOtherAmount7, out myFloat) == true)
    //            {
    //                ThisRate.OtherAmount7 = myFloat;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 12);//icx percentage in invalid
    //                ThisRate.field2 = NewFlag;
    //            }

    //            myFloat = 0;
    //            if (Single.TryParse(newOtherAmount8, out myFloat) == true)
    //            {
    //                ThisRate.OtherAmount8 = myFloat;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 13);//igw percentage in invalid
    //                ThisRate.field2 = NewFlag;
    //            }

    //            myFloat = 0;
    //            if (Single.TryParse(newOtherAmount9, out myFloat) == true)
    //            {
    //                ThisRate.OtherAmount9 = myFloat;
    //            }
    //            else
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 14);//vat commission percentage in invalid
    //                ThisRate.field2 = NewFlag;
    //            }

    //        }

    //        if (idOperatorType == 2)//for icx operators
    //        {//icxrevenuesharing
    //            float myFloat = 0;
    //            if (Single.TryParse(newCurrency, out myFloat))
    //            {
    //                ThisRate.OtherAmount10 = myFloat;
    //            }
    //            else //invalid revshare% icx
    //            {
    //                int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 15);
    //                ThisRate.field2 = NewFlag;
    //            }
    //        }


    //    }
    //    else //invalid Type
    //    {
    //        int NewFlag = SetBitInteger(Convert.ToInt32(ThisRate.field2), 5);
    //        ThisRate.field2 = NewFlag;
    //    }


    //    //ThisRate.OtherAmount4   =OtherAmount4 ;
    //    //ThisRate.OtherAmount5      =OtherAmount5    ;
    //    //ThisRate.idCustomerRatePlanICX  =idCustomerRatePlanICX;
    //    //ThisRate.OtherAmount6         =OtherAmount6       ;
    //    //ThisRate.OtherAmount7         =OtherAmount7       ;
    //    //ThisRate.OtherAmount8         =OtherAmount8       ;
    //    //ThisRate.OtherAmount9           =OtherAmount9         ;
    //    //ThisRate.OtherAmount10            =OtherAmount10          ;

    //    ThisRate.Inactive = int.Parse(newInactive);
    //    //ThisRate.RouteDisabled = int.Parse(newRouteDisabled);

    //    //find out new, increase or decrease; delete has been set along with rateamount
    //    //0=Unknown, 1=delete, 2=new, 3=increase, 4=decrease,5 =unchannged

    //    //find out new, rateamount= -1 will indicate rate deletion
    //    //change type may already been set e.g. delete=1 has been set along with rates=-1
    //    ThisRate.Status = 0; //field1 is not set yet
    //    return ThisRate;
    //}



    private ratetask CreateNewRateTask(
                                        int currentIdRatePlan,
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
                                        double timeZoneOffsetSec,
                                        string newMinSurchargeTime,
                                        string newMinSurchargeAmount,
                                        string newServiceType,
                                        string newSubServiceType,
                                        string newRoundUpDigits
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
        newRoundUpDigits = (newRoundUpDigits == null ? "" : newRoundUpDigits);
        //null removed


        //normalized datetimes in yyyy-MM-dd HH:mm:ss format
        if (newStartDateAndTime.Length == 10 && newStartDateAndTime.Length != 19)
        {
            newStartDateAndTime += " 00:00:00";
        }

        //if newstartdateandtime does not have a valid date, try default effective date
        DateTime defaultDate = new DateTime();
        DateTime tempDate = new DateTime();
        if (DateTime.TryParseExact(newStartDateAndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate) == false)
        {
            if (this.CheckBoxDefaultDate.Checked == true)
            {
                if (DateTime.TryParseExact(this.TextBoxDefaultDate.Text + " " + this.TextBoxDefaultTime.Text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out defaultDate) == true)
                {
                    newStartDateAndTime = defaultDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        }

        if (newEndDateAndTime.Length == 10 && newEndDateAndTime.Length != 19)
        {
            newEndDateAndTime += " 00:00:00";
        }
        ratetask thisTask = new ratetask();

        //validate current rate plan id first...
        if (currentIdRatePlan == null || currentIdRatePlan <= 0)
        {
            throw new Exception("Fatal error: No or invalid current RatePlanId!");
        }
        else
        {
            thisTask.idrateplan = currentIdRatePlan;
        }

        //validate field3=idpartner
        int ratePlanType = -1;
        if (this.Session["task.sesRatePlanType"] != null)
        {
            ratePlanType = (int) this.Session["task.sesRatePlanType"];
        }



        if (timeZoneOffsetSec == -360000)
        {
            //invalid timezone
            throw new Exception("Fatal error: Invalid Timezone Offset (-360000)!");
        }
        else
        {
            thisTask.TimeZoneOffsetSec = timeZoneOffsetSec.ToString();
        }

        //initialize Field1 and Field2 as 0, they will keep certain flags
        thisTask.Status = "0";
        thisTask.field2 = "0";
        double tempRate = 0;
        double.TryParse(newRateAmount, out tempRate);
        if (tempRate == -1)
        {
            thisTask.Status = "1";
        }
        //add fields one by one and also validate them

        int newIdInt = -1;
        if (int.TryParse(newId, out newIdInt))
        {
            thisTask.id = newIdInt;
        }


        //error or validation codes: bits are set in field2 for each rate
        //bit 1=prefix
        //2=rate
        //3=pulse
        //4=effective since
        string digits = "0123456789";
        double tempRate1 = 0;
        double.TryParse(newRateAmount, out tempRate1);
        if (tempRate1 == -1)
        {
            digits += "*";//code deletes are supported with wild cards
        }
        double myNum = 0; //prefix isnumeric
        bool invalidPrefix = false;
        int starCount = 0;
        foreach (char chr in newPrefix.ToCharArray())
        {
            if (digits.Contains(chr.ToString()) == false && chr.ToString()!="+")//+ allowed for phone numbers
            {
                //invalid prefix
                int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 1);
                thisTask.field2 = newFlag.ToString();
                invalidPrefix = true;
            }
            if (chr == '*') starCount++;
        }
        //also make sure * is used alone or last
        if (invalidPrefix == false)
        {
            if (starCount > 1)
            {
                //multiple * are not allowed
                //invalid prefix
                int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 1);
                thisTask.field2 = newFlag.ToString();
                invalidPrefix = true;
            }
            else if (starCount == 1)
            {
                //* allowed only at last
                if (newPrefix.IndexOf("*") != (newPrefix.Length - 1))
                {
                    //invalid prefix
                    int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 1);
                    thisTask.field2 = newFlag.ToString();
                    invalidPrefix = true;
                }
            }
        }

        //a final validation for prefix so that it doesn't allow zero length
        if (newPrefix.Trim() == "")
        {
            //invalid prefix
            int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 1);
            thisTask.field2 = newFlag.ToString();
            invalidPrefix = true;
        }

        if (invalidPrefix == false) thisTask.Prefix = newPrefix;



        thisTask.description = newDesc; //description, no validation

        double myDecimal = 0;//rate
        if (double.TryParse(newRateAmount, out myDecimal))
        {
            //only -1 is allowed as negative rates indicating rate deletion
            if (myDecimal >= 0)
            {
                thisTask.rateamount = myDecimal.ToString();
            }
            else if (myDecimal == -1) //rate deletion
            {
                //0=Unknown, 1=delete, 2=new, 3=increase, 4=decrease
                thisTask.rateamount = "-1";
                thisTask.Status = "1";
            }
            else //Unknown change
            {
                thisTask.Status = "0";
            }
        }
        else //invalid Rate
        {
            if (ratePlanType != 4)//not intl.outgoing
            {
                int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 2);
                thisTask.field2 = newFlag.ToString();
            }
        }

        myDecimal = 0;//surchargeamount
        if (double.TryParse(newSurchargeAmount, out myDecimal))
        {
            if (myDecimal >= 0)
            {
                thisTask.SurchargeAmount = myDecimal.ToString();
            }
        }
        else //invalid Rate or surchargeamount
        {
            if (ratePlanType != 4 && ratePlanType != 3)//not intl.outgoing
            {
                int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 2);
                thisTask.field2 = newFlag.ToString();
            }
        }

        int myInt = 0;
        if (int.TryParse(newResolution, out myInt))
        {
            thisTask.Resolution = myInt.ToString();
        }
        else //invalid resolution/pulse/minDurationSec or surchargetime
        {
            int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 3);
            thisTask.field2 = newFlag.ToString();
        }

        Single myFloat2 = 0;
        if (Single.TryParse(newMinDurationSec, out myFloat2))
        {
            thisTask.MinDurationSec = myFloat2.ToString();
        }
        else //invalid Minduratin sec
        {
            int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 17);
            thisTask.field2 = newFlag.ToString();
        }

        myInt = 0;
        if (int.TryParse(newSurchargeTime, out myInt))
        {
            thisTask.SurchargeTime = myInt.ToString();
        }
        else //invalid  surchargetime
        {
            int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 18);
            thisTask.field2 = newFlag.ToString();
        }
        thisTask.CountryCode = newCountry;//no validation

        string format = "yyyy-MM-dd HH:mm:ss"; //effective date

        //set if default code delete specified
        //timezone has been handled outside this function, set difference to 0 here
        DateTime dateTime;
        double delDouble = 0;
        double.TryParse(thisTask.rateamount, out delDouble);
        if (delDouble == -1)//delete task
        {
            DateTime delDate = new DateTime();
            if (this.CheckBoxAutoConvertTZ.Checked == true)
            {
                long timeZoneDifference = 0;
                if (DateTime.TryParseExact(newStartDateAndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out delDate) == true)
                {
                    thisTask.startdate = (delDate.AddSeconds(timeZoneDifference)).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 4);
                    thisTask.field2 = newFlag.ToString();
                }

            }
            else//no auto convert TZ
            {
                long timeZoneDifference = 0;
                if (DateTime.TryParseExact(newStartDateAndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out delDate) == true)
                {
                    thisTask.startdate = (delDate.AddSeconds(timeZoneDifference)).ToString(format);
                 
                }
                else
                {
                    thisTask.startdate = newStartDateAndTime;
                    int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 4);
                    thisTask.field2 = newFlag.ToString();
                }
            }

            
            
        }
        else//not delete task
        {

            thisTask.startdate = newStartDateAndTime;
            if (DateTime.TryParseExact(newStartDateAndTime, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dateTime))
            {
                if (this.CheckBoxAutoConvertTZ.Checked == false)
                {
                    thisTask.startdate = dateTime.ToString(format);
                }
                else
                {
                    long timeZoneDifference = 0;// (long)ViewState["vsTimeZoneDifference"];
                    thisTask.startdate = (dateTime.AddSeconds(timeZoneDifference)).ToString(format);
                }
            }

            else //invalid start date
            {
                //try default date
                if (this.CheckBoxDefaultDate.Checked == true &&
                    DateTime.TryParseExact((this.TextBoxDefaultDate.Text + " " + this.TextBoxDefaultTime.Text), format, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out dateTime) == true)//use default date if mentioned and enabled
                {
                    if (this.CheckBoxAutoConvertTZ.Checked == false)
                    {

                        thisTask.startdate = dateTime.ToString(format);
                    }
                    else
                    {

                        long timeZoneDifference = 0;// (long)ViewState["vsTimeZoneDifference"];
                        thisTask.startdate = (dateTime.AddSeconds(timeZoneDifference)).ToString(format);
                    }
                }
                else
                {
                    thisTask.startdate = newStartDateAndTime;
                    int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 4);
                    thisTask.field2 = newFlag.ToString();
                }
            }
        }//not delete task
        thisTask.enddate = newEndDateAndTime;
        if (newEndDateAndTime != "\\N" && newEndDateAndTime != "" && newEndDateAndTime != "null" && newEndDateAndTime != null)
        {
            if (DateTime.TryParseExact(newEndDateAndTime, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dateTime))
            {
                if (this.CheckBoxAutoConvertTZ.Checked == false)
                {
                    thisTask.enddate = dateTime.ToString(format);
                }
                else
                {
                    long timeZoneDifference = 0;// (long)ViewState["vsTimeZoneDifference"];
                    //ThisTask.enddate = dateTime.AddSeconds(TimeZoneDifference).ToString(format);
                }
                //end date must be >= start date
                DateTime tempEndDate = new DateTime(2000, 1, 1, 0, 0, 0, 0);
                DateTime tempStartDate = new DateTime(2000, 1, 1, 0, 0, 0, 0);

                DateTime.TryParseExact(thisTask.enddate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempEndDate);
                DateTime.TryParseExact(thisTask.startdate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempStartDate);

                if (tempEndDate < tempStartDate)
                {
                    //invalid start or end date
                    int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 19);
                    thisTask.field2 = newFlag.ToString();
                    var color = ColorTranslator.FromHtml("#FF0000");
                    this.StatusLabel.ForeColor = color;
                    this.StatusLabel.Text = "End date must be greater than start date for all rates!";
                }
            }
            else
            {
                //invalid end date
                int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 19);
                thisTask.field2 = newFlag.ToString();
            }
        }
        else
        {
            thisTask.enddate = null;//enddate can be null
        }



        //currency
        myInt = 0;
        if (int.TryParse(newCurrency, out myInt))
        {
            thisTask.Currency = myInt.ToString();
        }
        else //invalid currency
        {
            int newFlag = SetBitInteger(Convert.ToInt32(thisTask.field2), 16);
            thisTask.field2 = newFlag.ToString();
        }





        Single myFloat = 0;
        if (Single.TryParse(newOtherAmount1, out myFloat) == true)
        {
            thisTask.OtherAmount1 = myFloat.ToString();
        }

        //ThisRate.OtherAmount2        =OtherAmount2      ;
        myFloat = 0;
        if (Single.TryParse(newOtherAmount2, out myFloat) == true)
        {
            thisTask.OtherAmount2 = myFloat.ToString();
        }

        //ThisRate.OtherAmount3        =OtherAmount3      ;
        myFloat = 0;
        if (Single.TryParse(newOtherAmount3, out myFloat) == true)
        {
            thisTask.OtherAmount3 = myFloat.ToString();
        }



        double myDec = 0;

        if (double.TryParse(newOtherAmount4, out myDec) == true)
        {
            thisTask.OtherAmount4 = myDec.ToString();
        }

        if (double.TryParse(newOtherAmount5, out myDec) == true)
        {
            thisTask.OtherAmount5 = myDec.ToString();
        }


        if (Single.TryParse(newOtherAmount6, out myFloat) == true)
        {
            thisTask.OtherAmount6 = myFloat.ToString();
        }


        myFloat = 0;
        if (Single.TryParse(newOtherAmount7, out myFloat) == true)
        {
            thisTask.OtherAmount7 = myFloat.ToString();
        }


        myFloat = 0;
        if (Single.TryParse(newOtherAmount8, out myFloat) == true)
        {
            thisTask.OtherAmount8 = myFloat.ToString();
        }


        myFloat = 0;
        if (Single.TryParse(newOtherAmount9, out myFloat) == true)
        {
            thisTask.OtherAmount9 = myFloat.ToString();
        }



        if (Single.TryParse(newCurrency, out myFloat))
        {
            thisTask.OtherAmount10 = myFloat.ToString();
        }

        int tempInt = 0;
        if (Int32.TryParse(newRoundUpDigits, out tempInt))
        {
            thisTask.RateAmountRoundupDecimal = tempInt;
        }

        thisTask.Inactive = newInactive;
        //ThisRate.RouteDisabled = int.Parse(newRouteDisabled);

        //find out new, increase or decrease; delete has been set along with rateamount
        //0=Unknown, 1=delete, 2=new, 3=increase, 4=decrease,5 =unchannged

        //find out new, rateamount= -1 will indicate rate deletion
        //change type may already been set e.g. delete=1 has been set along with rates=-1
        if (Convert.ToDouble(thisTask.Status) != 1)//if not code delete
        {
            thisTask.Status = "0"; //field1 is not set yet
        }
        thisTask.Category = newServiceType;
        thisTask.SubCategory = newSubServiceType;

        return thisTask;
    }


    protected void GridViewSupplierRates_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {

        GridViewRow row = this.GridViewSupplierRates.Rows[e.RowIndex];
        string newId = ((Label)row.FindControl("lblId")).Text;
        string newPrefix = ((TextBox)row.FindControl("txtPrefix")).Text;
        string newDesc = ((TextBox)row.FindControl("txtDescription")).Text;
        string newRateAmount = ((TextBox)row.FindControl("txtRateAmount")).Text;
        string newResolution = ((TextBox)row.FindControl("txtResolution")).Text;
        string newMinDurationSec = ((TextBox)row.FindControl("txtMinDurationSec")).Text;
        string newCountry = ((DropDownList)row.FindControl("DropDownListCountry")).SelectedValue;

        string newStartDate = ((TextBox)row.FindControl("TextBoxStartDatePicker")).Text;
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


        string newInactive = ((DropDownList)row.FindControl("DropDownListInactive")).SelectedValue;
        string newRouteDisabled = "";// ((DropDownList)row.FindControl("DropDownListRouteDisabled")).SelectedValue;

        string newWeekDayStart = ((TextBox)row.FindControl("txtWeekDayStart")).Text;
        string newWeekDayEnd = ((TextBox)row.FindControl("txtWeekDayEnd")).Text;
        string newStartTimeOfDay = ((TextBox)row.FindControl("txtStartTime")).Text;
        string newEndTimeOfDay = ((TextBox)row.FindControl("txtEndTime")).Text;
        string newSurchargeTime = ((TextBox)row.FindControl("txtSurchargeTime")).Text;
        string newSurchargeAmount = ((TextBox)row.FindControl("txtSurchargeAmount")).Text;
        string newOtherAmount1 = ((TextBox)row.FindControl("txtOtherAmount1")).Text;
        string newOtherAmount2 = ((TextBox)row.FindControl("txtOtherAmount2")).Text;
        string newOtherAmount3 = ((TextBox)row.FindControl("txtOtherAmount3")).Text;
        string newOtherAmount4 = ((TextBox)row.FindControl("txtOtherAmount4")).Text;
        string newOtherAmount5 = ((TextBox)row.FindControl("txtOtherAmount5")).Text;
        string newOtherAmount6 = ((TextBox)row.FindControl("txtOtherAmount6")).Text;
        string newOtherAmount7 = ((TextBox)row.FindControl("txtOtherAmount7")).Text;
        string newOtherAmount8 = ((TextBox)row.FindControl("txtOtherAmount8")).Text;
        string newOtherAmount9 = ((TextBox)row.FindControl("txtOtherAmount9")).Text;
        string newOtherAmount10 = ((TextBox)row.FindControl("txtOtherAmount10")).Text;
        string newServiceType = ((DropDownList)row.FindControl("DropDownListServiceType")).SelectedValue;
        string newSubServiceType = ((DropDownList)row.FindControl("DropDownListSubServiceType")).SelectedValue;
        string newRoundUpDigits = ((TextBox)row.FindControl("txtRoundDigits")).Text;
        int thisServiceFamily = -1;
        ratetask thisRate = new ratetask();
        PartnerEntities contextTask = new PartnerEntities();
        Int32 idInt = int.Parse(newId);
        thisRate = contextTask.ratetasks.Where(c => c.id == idInt).FirstOrDefault();
        //call create new rate to validate the rate
        //get id of the last rateplan for this supplier


        int idCurrrentRatePlan = (int) this.ViewState["task.sesidRatePlan"];

        int idTaskReference = -1;
        //if (ViewState["task.sesidRatePlan"] != null)
        //{
        //    idCurrentRatePlan = (int)ViewState["task.sesidRatePlan"];
        //}
        idTaskReference = int.Parse(this.DropDownListTaskRef.SelectedValue);

        double timeZoneOffsetSec = -360000;//set to some invalid timezone offset (in this case 100 hours)
        using (PartnerEntities context = new PartnerEntities())
        {
            int idTimeZone = context.rateplans.Where(c => c.id == idCurrrentRatePlan).First().TimeZone;
            timeZoneOffsetSec = context.timezones.Where(c => c.id == idTimeZone).First().gmt_offset;
        }

        int newType = -1;
        if (this.Session["task.sesRatePlanType"] != null)
        {
            newType = (int) this.Session["task.sesRatePlanType"];
        }

        int newCurrency = -1;
        if (this.Session["task.sesCurrency"] != null)
        {
            newCurrency = (int) this.Session["task.sesCurrency"];
        }

        int idOperatorType = -1;
        if (this.Session["task.sesidOperatorType"] != null)
        {
            idOperatorType = (int) this.Session["task.sesidOperatorType"];
        }

        //before updating reset error flag
        thisRate.field2 = "0";
        thisRate.Status = "0";

        ratetask newRate = CreateNewRateTask(idTaskReference,
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
            timeZoneOffsetSec,
            newSurchargeTime,
            newSurchargeAmount, newServiceType, newSubServiceType, newRoundUpDigits);

        //now update, new rate has validation and other flags set...

        

        thisRate.Prefix = newRate.Prefix;
        thisRate.description = newRate.description;
        int ceilingRateAtFractionalPosition = Convert.ToInt32(newRate.RateAmountRoundupDecimal);
        if(ceilingRateAtFractionalPosition==0)
        {
            thisRate.rateamount = newRate.rateamount;
        }
        else if (ceilingRateAtFractionalPosition>7 || ceilingRateAtFractionalPosition<0)
        {
            this.StatusLabel.ForeColor = Color.Red;
            this.StatusLabel.Text = "RateAmountRoundupDecimal must be >=0 & <=7";
        }
        else if(ceilingRateAtFractionalPosition>=1 & ceilingRateAtFractionalPosition<=7)
        {
            FractionCeilingHelper fractionHelper =
                new FractionCeilingHelper(Convert.ToDecimal(newRate.rateamount), ceilingRateAtFractionalPosition);
            thisRate.rateamount = fractionHelper.GetPreciseDecimal().ToString(CultureInfo.InvariantCulture);
        }
        else throw new ArgumentOutOfRangeException();

        thisRate.Resolution = newRate.Resolution;
        //ThisRate.CountryCode = NewRate.CountryCode;
        thisRate.startdate = newRate.startdate;
        thisRate.enddate = newRate.enddate;
        thisRate.Inactive = newRate.Inactive;
        thisRate.RouteDisabled = newRate.RouteDisabled;
        thisRate.Status = newRate.field1;
        thisRate.field2 = newRate.field2;
        thisRate.id = newRate.id;
        thisRate.field3 = newRate.field3;
        thisRate.MinDurationSec = newMinDurationSec;
        thisRate.WeekDayStart = newRate.WeekDayStart;
        thisRate.WeekDayEnd = newRate.WeekDayEnd;
        thisRate.starttime = newRate.starttime;
        thisRate.endtime = newRate.endtime;
        thisRate.SurchargeTime = newRate.SurchargeTime;
        thisRate.SurchargeAmount = newRate.SurchargeAmount;
        thisRate.Type = newRate.Type;
        thisRate.OtherAmount1 = newRate.OtherAmount1;
        thisRate.OtherAmount2 = newRate.OtherAmount2;
        thisRate.OtherAmount3 = newRate.OtherAmount3;
        thisRate.OtherAmount4 = newRate.OtherAmount4;
        thisRate.OtherAmount5 = newRate.OtherAmount5;
        thisRate.OtherAmount6 = newRate.OtherAmount6;
        thisRate.OtherAmount7 = newRate.OtherAmount7;
        thisRate.OtherAmount8 = newRate.OtherAmount8;
        thisRate.OtherAmount9 = newRate.OtherAmount9;
        thisRate.OtherAmount10 = newRate.OtherAmount10;
        thisRate.Currency = newRate.Currency;
        contextTask.SaveChanges();


        SetExecutionOrder();//if it fails, no problem, before commiting it is called again
        this.GridViewSupplierRates.EditIndex = -1;
        MyGridViewDataBind();

        var color = ColorTranslator.FromHtml("#B1B1B3");
        this.StatusLabel.ForeColor = color;
        this.StatusLabel.Text = "Changes are not made to rate table until 'Commit Changes' clicked!";

    }
    protected void GridViewSupplierRates_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        this.GridViewSupplierRates.EditIndex = -1;
        MyGridViewDataBind();
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

    }


    protected void LinkButton1_Click(object sender, EventArgs e)
    {
        this.frmSupplierRatePlanInsert.DataBind();
        this.frmSupplierRatePlanInsert.ChangeMode(FormViewMode.Insert);
        this.frmSupplierRatePlanInsert.Visible = true;
    }

   


    protected void FormViewCancel_Click(object sender, EventArgs e)
    {
        this.frmSupplierRatePlanInsert.Visible = false;
    }

    

    protected void frmSupplierRatePlanInsert_ItemInserted(object sender, FormViewInsertedEventArgs e)
    {
        this.frmSupplierRatePlanInsert.Visible = false;

        MyGridViewDataBind();
    }

    protected void frmSupplierRatePlanInsert_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {


        }
    }

    protected void frmSupplierRatePlanInsert_ItemCreated(object sender, EventArgs e)
    {
        int idOperatorType = -1;
        if (this.Session["task.sesidOperatorType"] != null)
        {
            idOperatorType = (int) this.Session["task.sesidOperatorType"];
        }

        int ratePlanType = -1;
        if (this.Session["task.sesRatePlanType"] != null)
        {
            ratePlanType = (int) this.Session["task.sesRatePlanType"];
        }

        TextBox txtSurchargeTime = (TextBox) this.frmSupplierRatePlanInsert.FindControl("txtSurchargeTime");
        TextBox txtSurchargeAmount = (TextBox) this.frmSupplierRatePlanInsert.FindControl("txtSurchargeAmount");
        TextBox txtMinDurationSec = (TextBox) this.frmSupplierRatePlanInsert.FindControl("txtMinDurationSec");
        TextBox txtRound = (TextBox) this.frmSupplierRatePlanInsert.FindControl("TextBoxRoundDigits");

        var thisPlan = (rateplan) this.ViewState["vsRatePlan"];
        double tempDbl = 0;
        double.TryParse(thisPlan.SurchargeAmount.ToString(), out tempDbl);
        txtSurchargeAmount.Text = tempDbl.ToString();

        double.TryParse(thisPlan.SurchargeTime.ToString(), out tempDbl);
        txtSurchargeTime.Text = tempDbl.ToString();

        double.TryParse(thisPlan.mindurationsec.ToString(), out tempDbl);
        txtMinDurationSec.Text = tempDbl.ToString();

        double.TryParse(thisPlan.RateAmountRoundupDecimal.ToString(), out tempDbl);
        txtRound.Text = tempDbl.ToString();
    }

    public IEnumerable<string> ReadLines(Func<Stream> streamProvider,
                                     Encoding encoding, string removeChar)
    {
        using (var stream = streamProvider())
        using (var reader = new StreamReader(stream, encoding))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (removeChar == "") yield return line;
                else yield return line.Replace(removeChar, "");
            }
        }
    }



    protected void UploadButton_Click(object sender, EventArgs e)
    {
        if (this.FileUploadControl.HasFile)
        {
            try
            {

                if (this.FileUploadControl.PostedFile.ContentLength < 10485760)
                {

                    //<asp:ListItem Value="-1"> [Select]</asp:ListItem>
                    //<asp:ListItem Value="1"> Generic</asp:ListItem>
                    //<asp:ListItem Value="2"> Tata</asp:ListItem>
                    //<asp:ListItem Value="3"> Bharti</asp:ListItem>
                    //<asp:ListItem Value="4"> IDT</asp:ListItem>
                    //make sure format is selected for the file to be uploaded...
                    if (this.DropDownListFormat.SelectedValue == "-1")
                    {
                        var color = ColorTranslator.FromHtml("#FF0000");
                        this.StatusLabel.ForeColor = color;
                        this.StatusLabel.Text = "No Rate Plan format select for the input file!";
                    }

                    string filename = Path.GetFileName(this.FileUploadControl.FileName);

                    //delete all files in the temp directory first...
                    DirectoryInfo downloadedMessageInfo = new DirectoryInfo(this.Request.PhysicalApplicationPath + "config\\temp");

                    //delete all instance of excel, because it wasn't getting ended by the code of excel instancing
                    foreach (Process process in Process.GetProcessesByName("Excel"))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception e1)
                        {
                            continue;
                        }
                    }
                    //kill again, doesn't want to die. wait before trying again
                    Thread.Sleep(2000);
                    foreach (Process process in Process.GetProcessesByName("Excel"))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception e2)
                        {
                            continue;
                        }
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

                            foreach (Process process in Process.GetProcessesByName("RateTaskSerializer"))
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
                    using (MySqlConnection delCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                    {
                        delCon.Open();
                        using (MySqlCommand delCmd = new MySqlCommand("", delCon))
                        {
                            delCmd.CommandText = " delete from ratetask where idrateplan=" + this.DropDownListTaskRef.SelectedValue;
                            delCmd.ExecuteNonQuery();
                        }
                    }

                    //get session id to append to the filename
                    HttpSessionState ss = HttpContext.Current.Session;
                    this.FileUploadControl.SaveAs(this.Request.PhysicalApplicationPath + "\\config\\temp\\" + ss.SessionID.ToString() + "_" + filename);
                    int ratePlanFormat = int.Parse(this.DropDownListFormat.SelectedValue);

                    
                    List<ratetask> lstRateTask = new List<ratetask>();
                    rateplan thisRatePlan = new rateplan();
                    thisRatePlan = (rateplan) this.ViewState["vsRatePlan"];
                    MyExcel pExcel = new MyExcel();


                    string locn = (Directory.GetParent(HttpRuntime.BinDirectory)).Parent.FullName + Path.DirectorySeparatorChar + "RateTaskSerializer" +
                        Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar;
                    List<string> lstArgument = new List<string>();
                    
                    string fileUploadDirectory = (Directory.GetParent(HttpRuntime.BinDirectory)).Parent.FullName + Path.DirectorySeparatorChar
                        + "config" + Path.DirectorySeparatorChar + "temp";
                    string importFilename = ss.SessionID.ToString() + "_" + filename;
                    long idRatePlan = thisRatePlan.id;
                    //retrieve dateparse string for this rateplan
                    string dateParseSelector = thisRatePlan.field5 != null ? thisRatePlan.field5.Trim() : "";
                    dateParseSelector = (dateParseSelector == null || dateParseSelector == "" ? "MF" : dateParseSelector);//Month First is the default
                    string extensionDirectory = (Directory.GetParent(HttpRuntime.BinDirectory)).Parent.FullName + Path.DirectorySeparatorChar
                        + "Extensions";

                    lstArgument.Add(fileUploadDirectory.Replace(" ","`"));
                    lstArgument.Add(importFilename.Replace(" ", "`"));
                    lstArgument.Add(idRatePlan.ToString().Replace(" ", "`"));
                    lstArgument.Add(dateParseSelector.Replace(" ", "`"));
                    lstArgument.Add(extensionDirectory.Replace(" ", "`"));

                    string[] args = { string.Join("'", lstArgument) };
                    //Create a new appdoamin for execute my exe in a isolated way.
                    try
                    {
                        AppDomain.CurrentDomain.ExecuteAssembly((HttpRuntime.BinDirectory+Path.DirectorySeparatorChar+ "RateTaskSerializer.exe"), args);
                    }
                    catch (Exception ex)//Any exception that generate from executable can handle
                    {
                        var color = ColorTranslator.FromHtml("#FF0000");
                        this.StatusLabel.ForeColor = color;
                        this.StatusLabel.Text = "Error executing AppDomain Class while parsing ratesheet"+"<br/>"+ex.Message;
                        return;
                    }

                    if (File.Exists(fileUploadDirectory + Path.DirectorySeparatorChar + importFilename + ".ratetask") == true)
                    {
                        //lstRateTask = JsonConvert.DeserializeObject<List<ratetask>>(File.ReadAllText(FileUploadDirectory + Path.DirectorySeparatorChar + ImportFilename + ".ratetask"));
                        using (StreamReader file = File.OpenText(fileUploadDirectory + Path.DirectorySeparatorChar + importFilename + ".ratetask"))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            lstRateTask = (List<ratetask>)serializer.Deserialize(file, typeof(List<ratetask>));
                        }
                    }
                    else
                    {
                        var color = ColorTranslator.FromHtml("#FF0000");
                        this.StatusLabel.ForeColor = color;
                        this.StatusLabel.Text = "Could not find normalized ratetask file!";
                        return;
                    }
                    int idTaskReference = -1;
                    idTaskReference = int.Parse(this.DropDownListTaskRef.SelectedValue);
                    double timeZoneOffsetSec = -360000;//set to some invalid timezone offset (in this case 100 hours)
                    Dictionary<string, countrycode> dicCountryCode = new Dictionary<string, countrycode>();
                    using (PartnerEntities context = new PartnerEntities())
                    {
                        int idTimeZone = context.rateplans.Where(c => c.id == thisRatePlan.id).First().TimeZone;
                        timeZoneOffsetSec = context.timezones.Where(c => c.id == idTimeZone).First().gmt_offset;
                        dicCountryCode = context.countrycodes.ToDictionary(c => c.Code);
                    }


                    int newCurrency = 0;

                    int ratePlanType = -1;
                    ratePlanType = thisRatePlan.Type;

                    if (ratePlanType == -1)
                    {
                        var color = ColorTranslator.FromHtml("#FF0000");
                        this.StatusLabel.ForeColor = color;
                        this.StatusLabel.Text = "Invalid Rateplan Type!";
                    }

                    //use tmp negetive id, id field is auto increment, just requires temp id for gridview
                    long minidInt = -1;
                    int iteration = 0;
                    //int ErrorCount = 0;
                    int segmentCounter = 0;//save after each 1000 records for performance
                    PartnerEntities conInsert = new PartnerEntities();

                    conInsert.Configuration.AutoDetectChangesEnabled = false;//performance
                    conInsert.Configuration.ValidateOnSaveEnabled = false;//performance
                    List<string> lstExtInsert = new List<string>();
                    
                    for (iteration = 0; iteration <= lstRateTask.Count - 1; iteration++)
                    {

                        //string str = strLines[Iteration];
                        ratetask newtask = lstRateTask[iteration];//str.Split(',');//LineToFields(str);

                        string newId = Convert.ToString(minidInt - iteration - 1);
                        string newPrefix = newtask.Prefix != null ? newtask.Prefix : "";
                        string newDesc = newtask.description != null ? newtask.description : "";
                        string newRateAmount = newtask.rateamount != null ? newtask.rateamount : "";
                        string newResolution = newtask.Resolution != null ? newtask.Resolution : "";
                        string newMinDurationSec = newtask.MinDurationSec != null ? newtask.MinDurationSec : "";
                        string newCountry = newtask.CountryCode != null ? newtask.CountryCode : "";
                        if (this.CheckBoxAutoDetectCountry.Checked == true && newCountry=="")
                        {
                            List<string> lstPhoneNumbers = new List<string>();
                            for (int i = newPrefix.Length; i > 0; i--)
                            {
                                lstPhoneNumbers.Add(newPrefix.Substring(0, i));
                            }
                            countrycode matchedCc = null;
                            foreach (string prefix in lstPhoneNumbers)
                            {
                                dicCountryCode.TryGetValue(prefix, out matchedCc);
                                if (matchedCc == null)
                                {
                                    continue;
                                }
                                break;//rates are sorted desc, starttime. latest match will be returned immediately
                            }
                            if(matchedCc!=null)
                            {
                                newCountry = matchedCc.Code;
                            }
                        }
                        string newStartDateAndTime = newtask.startdate != null ? newtask.startdate : "";
                        string newEndDateAndTime = newtask.enddate != null ? newtask.enddate : "";
                        string newSurchargeTime = newtask.SurchargeTime != null ? newtask.SurchargeTime : "";
                        string newSurchargeAmount = newtask.SurchargeAmount != null ? newtask.SurchargeAmount : "";
                        string newInactive = newtask.Inactive != null ? newtask.Inactive : "0";
                        string newRouteDisabled = newtask.RouteDisabled != null ? newtask.RouteDisabled : "0";

                        string newOtherAmount4 = newtask.OtherAmount4 != null ? newtask.OtherAmount4 : "0";
                        string newOtherAmount5 = newtask.OtherAmount5 != null ? newtask.OtherAmount5 : "0";
                        string newOtherAmount6 = newtask.OtherAmount6 != null ? newtask.OtherAmount6 : "0";
                        string newOtherAmount7 = newtask.OtherAmount7 != null ? newtask.OtherAmount7 : "0";
                        string newOtherAmount8 = newtask.OtherAmount8 != null ? newtask.OtherAmount8 : "0";
                        string newOtherAmount9 = newtask.OtherAmount9 != null ? newtask.OtherAmount9 : "0";
                        string newOtherAmount10 = newtask.OtherAmount10 != null ? newtask.OtherAmount10 : "0";

                        string newOtherAmount1 = newtask.OtherAmount1 != null ? newtask.OtherAmount1 : "0";
                        string newOtherAmount2 = newtask.OtherAmount2 != null ? newtask.OtherAmount2 : "0";
                        string newOtherAmount3 = newtask.OtherAmount3 != null ? newtask.OtherAmount3 : "0";

                        string newServiceType = newtask.Category != null ? newtask.Category : "1";
                        string newSubServiceType = newtask.SubCategory != null ? newtask.SubCategory : "1";

                        //inherit from rateplan
                        var thisPlan = (rateplan) this.ViewState["vsRatePlan"];
                        int tempint = 0;
                        int.TryParse(thisPlan.RateAmountRoundupDecimal.ToString(), out tempint);
                        int newRoundUpDigits = newtask.RateAmountRoundupDecimal != null ? Convert.ToInt32(newtask.RateAmountRoundupDecimal) : tempint;


                        int idOperatorType = 0;
                        if (this.Session["task.sesidOperatorType"] != null)
                        {
                            idOperatorType = (int) this.Session["task.sesidOperatorType"];
                        }

                        ratetask newRate = CreateNewRateTask(idTaskReference,
                            newId, newPrefix, newDesc, newRateAmount, newResolution, newMinDurationSec,
                            newCountry, newStartDateAndTime,
                            newEndDateAndTime, newInactive, newRouteDisabled,
                            "1",
                            "7",
                            "000000",
                            "235959",
                            "0",
                            "0.0",
                            ratePlanType.ToString(),
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
                            timeZoneOffsetSec,
                            newSurchargeTime,
                            newSurchargeAmount, newServiceType, newSubServiceType, newRoundUpDigits.ToString());

                        //modify all time to native time zone before adding...
                        long timeZoneDifference = (long) this.ViewState["vsTimeZoneDifference"];
                        if (this.CheckBoxAutoConvertTZ.Checked == true) newRate.AdjustDateTimeToNativeTimeZone(timeZoneDifference);

                        lstExtInsert.Add(newRate.GetExtendedInsertSql());
                        //ConInsert.ratetasks.Add(NewRate);//don't use context, too slow, use extendedinsert
                        //SegmentCounter++;
                        //if (SegmentCounter == 10000)//save in each 1000 records
                        //{
                        //    ConInsert.SaveChanges();
                        //ConInsert.Dispose();
                        //ConInsert = new PartnerEntities();
                        //ConInsert.Configuration.AutoDetectChangesEnabled = false;//performance
                        //ConInsert.Configuration.ValidateOnSaveEnabled = false;//performance
                        //  SegmentCounter = 0;
                        //}

                    }//for each ratetask


                    //execute in segment because command object's text property exceeds string's limit for huge rate plans
                    int modResult = 1;
                    int segment = 10000;
                    int leftItem = lstExtInsert.Count;
                    int alreadySelected = 0;
                    int selectStart = alreadySelected;
                    List<string> subList = new List<string>();
                    int selectLength= 0;
                    try
                    {
                        conInsert.Database.ExecuteSqlCommand("set autocommit=0;");
                        string insertHeader = "insert into ratetask (Prefix,description,rateamount,WeekDayStart,WeekDayEnd,starttime,endtime,Resolution,MinDurationSec,SurchargeTime,SurchargeAmount,idrateplan,CountryCode,date1,field1,field2,field3,field4,field5,startdate,enddate,Inactive,RouteDisabled,Type,Currency,OtherAmount1,OtherAmount2,OtherAmount3,OtherAmount4,OtherAmount5,OtherAmount6,OtherAmount7,OtherAmount8,OtherAmount9,OtherAmount10,TimeZoneOffsetSec,RatePosition,IgwPercentageIn,ConflictingRateIds,ChangedByTaskId,ChangedOn,Status,idPreviousRate,EndPreviousRate,Category,SubCategory,changecommitted,OverlappingRates,ConflictingRates,AffectedRates,PartitionDate,Comment1,Comment2,ExecutionOrder,RateAmountRoundupDecimal) values ";
                        while (leftItem>segment)
                        {
                            selectStart = alreadySelected;
                            selectLength =(leftItem>=segment? segment : leftItem);
                            subList = lstExtInsert.GetRange(alreadySelected,selectLength);
                            conInsert.Database.ExecuteSqlCommand(insertHeader + string.Join(",", subList));
                            alreadySelected += selectLength;
                            leftItem -= selectLength;
                        }
                        selectLength = (leftItem >= segment ? segment : leftItem);
                        subList = lstExtInsert.GetRange(alreadySelected, selectLength);
                        conInsert.Database.ExecuteSqlCommand(insertHeader + string.Join(",", subList));
                        conInsert.Database.ExecuteSqlCommand("commit;");
                    }
                    catch(Exception e1)
                    {
                        conInsert.Database.ExecuteSqlCommand("rollback;");
                        throw new Exception(e1.Message,e1.InnerException);
                    }
                    
                    //ConInsert.SaveChanges();

                    SetExecutionOrder();//if it fails, no problem, before commiting it is called again




                    MyGridViewDataBind();
                    var color1 = ColorTranslator.FromHtml("#008000");
                    this.StatusLabel.ForeColor = color1;
                    this.StatusLabel.Text = (iteration).ToString() + " Rate(s) Imported Successfully!";
                    this.TextBoxFindByDescription.Text = "";
                    this.TextBoxFindByPrefix.Text = "";
                    this.DropDownListMoreFilters.SelectedIndex = 0;
                }
                else
                {
                    var color = ColorTranslator.FromHtml("#FF0000");
                    this.StatusLabel.ForeColor = color;
                    this.StatusLabel.Text = "Upload status: The file has to be less than 10 MB!";
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
                this.StatusLabel.ForeColor = color;
                this.StatusLabel.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
            }
        }
    }

    public int UpdateProductsAndGetIds(List<ratetask> lstTask, int thisServiceFamily)
    {
        //try
        {
            Dictionary<string, Productext> dicProducts = new Dictionary<string, Productext>();
            for (int i = 0; i < lstTask.Count; i++)
            {
                ratetask newTask = lstTask[i];
                //if this task has validatio error, skip...
                int invalid = 0;
                int.TryParse(newTask.field2, out invalid);
                if (invalid > 0) continue;

                //check if this product exists
                int thisCategory = Convert.ToInt32(newTask.Category);
                int thisSubCategory = Convert.ToInt32(newTask.SubCategory);
                //int ThisServiceFamily=Context.rateplans.Where(c=>c.id==NewTask.idra)
                dicProducts = (Dictionary<string, Productext>) this.Session["dicProducts"];
                Productext thisProduct = null;
                dicProducts.TryGetValue(newTask.Prefix + "-" + newTask.Category + "-" + newTask.SubCategory + "-" + thisServiceFamily, out thisProduct);
                if (thisProduct == null)
                {
                    Productext newProduct = new Productext();
                    newProduct.Product.Prefix = newTask.Prefix;
                    newProduct.Product.Category = thisCategory;
                    newProduct.Product.SubCategory = thisSubCategory;
                    newProduct.Product.ServiceFamily = thisServiceFamily;
                    newProduct.Product.Name = newTask.description;
                    newProduct.Saved = false;
                    //add them to the dictionary instead of adding to DB, for performance...
                    dicProducts.Add(newProduct.Product.Prefix + "-" + newProduct.Product.Category + "-" + newProduct.Product.SubCategory + "-" + thisServiceFamily,
                        newProduct);
                }
            }
            List<string> lstSql = new List<string>();
            foreach (Productext unsavedProduct in dicProducts.Values.Where(c => c.Saved == false).ToList())
            {
                lstSql.Add("('" + unsavedProduct.Product.Prefix + "','" + unsavedProduct.Product.Name + "',"
                    + unsavedProduct.Product.Category + ","
                    + unsavedProduct.Product.SubCategory + "," + unsavedProduct.Product.ServiceFamily + ")");
            }

            if (lstSql.Count > 0)//new prefixes to be defined
            {
                string sql = "insert into product (prefix,name,category,subcategory,servicefamily) values " +
                    string.Join(",", lstSql) + ";";
                using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                //re populate the dictionary with new product ids
                using (PartnerEntities conigw = new PartnerEntities())
                {
                    //load product dictionary
                    dicProducts = new Dictionary<string, Productext>();
                    foreach (product prd in conigw.products.ToList())
                    {
                        Productext c = new Productext();
                        c.Product.Prefix = prd.Prefix;
                        c.Product.Category = prd.Category;
                        c.Product.SubCategory = prd.SubCategory;
                        c.Product.ServiceFamily = prd.ServiceFamily;
                        c.Product.id = prd.id;
                        c.Saved = true;
                        dicProducts.Add(c.Product.Prefix + "-" + c.Product.Category + "-" + c.Product.SubCategory + "-" + c.Product.ServiceFamily, c);
                    }
                    this.Session["dicProducts"] = dicProducts;
                }
            }

            //set product id field for each rate
            foreach (ratetask rTask in lstTask)
            {
                //if this task has validatio error, skip...
                int invalid = 0;
                int.TryParse(rTask.field2, out invalid);
                if (invalid > 0) continue;

                string key = rTask.Prefix + "-" + rTask.Category + "-" + rTask.SubCategory + "-" + thisServiceFamily;
                rTask.ProductId = dicProducts[key].Product.id;
            }
            return 1;
        }
        //catch (Exception e1)
        //{
        //    StatusLabel.Text = "Error occured while updating Products! <br/>" + e1.Message + "<br/>" + e1.InnerException;
        //    StatusLabel.ForeColor = Color.Red;
        //    return 0;
        //}
    }

   

    protected void frmSupplierRatePlanInsert_ItemInserting(object sender, FormViewInsertEventArgs e)
    {
        string newDesc = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtDescription")).Text;
        string newPrefix = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtPrefix")).Text;
        string newRateAmount = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtRateAmount")).Text;
        string newResolution = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtResolution")).Text;
        string newMinDurationSec = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtMinDurationSec")).Text;
        string newCountry = ((DropDownList) this.frmSupplierRatePlanInsert.FindControl("DropDownListCountry")).SelectedValue;

        string newStartDate = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("TextBoxStartDatePickerFrm")).Text;
        string newStartTime = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("TextBoxStartDateTimePickerFrm")).Text;
        string newStartDateAndTime = "";
        if (newStartDate != "")
        {
            newStartDateAndTime = newStartDate + " " + newStartTime;
        }

        string newEndDate = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("TextBoxEndDatePickerFrm")).Text;
        string newEndTime = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("TextBoxEndDateTimePickerFrm")).Text;
        string newEndDateAndTime = "";
        if (newEndDate != "")
        {
            newEndDateAndTime = newEndDate + " " + newEndTime;
        }
        string newInactive = ((DropDownList) this.frmSupplierRatePlanInsert.FindControl("DropDownListInactive")).SelectedValue;
        string newRouteDisabled = "";// ((DropDownList)frmSupplierRatePlanInsert.FindControl("DropDownListRouteDisabled")).SelectedValue;

        string newWeekDayStart = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtWeekDayStart")).Text;
        string newWeekDayEnd = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtWeekDayEnd")).Text;
        string newStartTimeOfDay = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtStartTime")).Text;
        string newEndTimeOfDay = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtEndTime")).Text;
        string newSurchargeTime = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtSurchargeTime")).Text;
        string newSurchargeAmount = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtSurchargeAmount")).Text;
        string newOtherAmount1 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount1")).Text;
        string newOtherAmount2 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount2")).Text;
        string newOtherAmount3 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount3")).Text;
        string newOtherAmount4 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount4")).Text;
        string newOtherAmount5 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount5")).Text;
        string newOtherAmount6 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount6")).Text;
        string newOtherAmount7 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount7")).Text;
        string newOtherAmount8 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount8")).Text;
        string newOtherAmount9 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount9")).Text;
        string newOtherAmount10 = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("txtOtherAmount10")).Text;
        string newServiceType = ((DropDownList) this.frmSupplierRatePlanInsert.FindControl("DropDownListServiceType")).SelectedValue;
        string newSubServiceType = ((DropDownList) this.frmSupplierRatePlanInsert.FindControl("DropDownListSubServiceType")).SelectedValue;
        string newRoundUp = ((TextBox) this.frmSupplierRatePlanInsert.FindControl("TextBoxRoundDigits")).Text;

        string newId = "-1";

        //call create new rate to validate the rate

        int idTaskReference = -1;
        //if (ViewState["task.sesidRatePlan"] != null)
        //{
        //    idCurrentRatePlan = (int)ViewState["task.sesidRatePlan"];
        //}
        idTaskReference = int.Parse(this.DropDownListTaskRef.SelectedValue);

        double timeZoneOffsetSec = -360000;//set to some invalid timezone offset (in this case 100 hours)
        int thisServiceFamily = -1;
        using (PartnerEntities context = new PartnerEntities())
        {
            int idRatePlan = (int) this.ViewState["task.sesidRatePlan"];
            thisServiceFamily = context.rateplans.Where(c => c.id == idRatePlan).First().Type;
            int idTimeZone = context.rateplans.Where(c => c.id == idRatePlan).First().TimeZone;
            timeZoneOffsetSec = context.timezones.Where(c => c.id == idTimeZone).First().gmt_offset;
        }

        int newType = -1;
        if (this.Session["task.sesRatePlanType"] != null)
        {
            newType = (int) this.Session["task.sesRatePlanType"];
        }

        int newCurrency = -1;
        if (this.Session["task.sesCurrency"] != null)
        {
            newCurrency = (int) this.Session["task.sesCurrency"];
        }

        int idOperatorType = -1;
        if (this.Session["task.sesidOperatorType"] != null)
        {
            idOperatorType = (int) this.Session["task.sesidOperatorType"];
        }

        ratetask newRate = CreateNewRateTask(idTaskReference,
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
            timeZoneOffsetSec,
            newSurchargeTime,
            newSurchargeAmount,
            newServiceType,
            newSubServiceType, newRoundUp
            );

        long timeZoneDifference = (long) this.ViewState["vsTimeZoneDifference"];
        if (this.CheckBoxAutoConvertTZ.Checked == true) newRate.AdjustDateTimeToNativeTimeZone(timeZoneDifference);
        PartnerEntities conInsert = new PartnerEntities();
        conInsert.ratetasks.Add(newRate);
        conInsert.SaveChanges();
        SetExecutionOrder();//if it fails, no problem, before commiting it is called again

        this.frmSupplierRatePlanInsert.Visible = false;
        if (this.hidValueCommit.Value == "1")
        {
            CommitChanges();
            return;
        }
        MyGridViewDataBind();

        var color = ColorTranslator.FromHtml("#B1B1B3");
        this.StatusLabel.ForeColor = color;
        this.StatusLabel.Text = "Changes are not committed to rate table until 'Save All Changes' clicked!";
        this.Response.Redirect("ratetask.aspx" + (string) this.ViewState["vsQueryString"]);
    }



    protected void frmSupplierRatePlanInsert_ModeChanging(object sender, FormViewModeEventArgs e)
    {

    }
    protected void frmCodeDelete_ModeChanging(object sender, FormViewModeEventArgs e)
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
        this.GridViewSupplierRates.PageIndex = e.NewPageIndex;
        MyGridViewDataBind();

    }



    string GetExtInsertValuePartForRate(rate thisRate)
    {
        try
        {
            string x =
            "(  													 " +
                        "'" + thisRate.Prefix + "'," +
                        "'" + thisRate.description + "'," +
                        "'" + thisRate.rateamount + "'," +
                        "'" + thisRate.WeekDayStart + "'," +
                        "'" + thisRate.WeekDayEnd + "'," +
                        "'" + thisRate.starttime + "'," +
                        "'" + thisRate.endtime + "'," +
                        "'" + thisRate.Resolution + "'," +
                        "'" + thisRate.MinDurationSec + "'," +
                        "'" + thisRate.SurchargeTime + "'," +
                        "'" + thisRate.SurchargeAmount + "'," +
                        "'" + thisRate.idrateplan + "'," +
                        "'" + thisRate.CountryCode + "'," +
                        "'" + thisRate.Status + "'," +
                        "'" + thisRate.field2 + "'," +
                        "'" + thisRate.field3 + "'," +
                        "'" + thisRate.field4 + "'," +
                        "'" + thisRate.field5 + "'," +
                        "'" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                        (thisRate.enddate != new DateTime(9999, 12, 31, 23, 59, 59) ? "'" + Convert.ToDateTime(thisRate.enddate).ToString("yyyy-MM-dd HH:mm:ss") + "'" : "null") + "," +
                        "'" + thisRate.Inactive + "'," +
                        "'" + thisRate.RouteDisabled + "'," +
                        "'" + thisRate.Type + "'," +
                        "'" + thisRate.Currency + "'," +
                        "'" + thisRate.OtherAmount1 + "'," +
                        "'" + thisRate.OtherAmount2 + "'," +
                        "'" + thisRate.OtherAmount3 + "'," +
                        "'" + thisRate.OtherAmount4 + "'," +
                        "'" + thisRate.OtherAmount5 + "'," +
                        "'" + thisRate.OtherAmount6 + "'," +
                        "'" + thisRate.OtherAmount7 + "'," +
                        "'" + thisRate.OtherAmount8 + "'," +
                        "'" + thisRate.OtherAmount9 + "'," +
                        "'" + thisRate.OtherAmount10 + "'," +
                        "'" + thisRate.TimeZoneOffsetSec + "'," +
                        "'" + thisRate.RatePosition + "'," +
                        "'" + thisRate.IgwPercentageIn + "'," +
                        "'" + thisRate.ConflictingRateIds + "'," +
                        "'" + thisRate.id + "'," +//keep track of which rate gets changed by which task
                        "'" + DateTime.Now.ToString("yyyy-MM-dd: HH:mm:ss") + "'," +
                        "'" + thisRate.Status + "'," +
                        "'" + thisRate.idPreviousRate + "'," +
                        "'" + thisRate.EndPreviousRate + "'," +
                        "'" + thisRate.Category + "'," +
                        "'" + thisRate.SubCategory + "'," +
                        "'" + thisRate.ProductId + "'," + thisRate.RateAmountRoundupDecimal +
                        ")";

            return x;
        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FA0509");
            this.StatusLabel.ForeColor = color;
            this.StatusLabel.Text = e1.Message + "<br/>" + e1.InnerException + "<br/>" +
                thisRate.Prefix + " effective date:" + thisRate.startdate;
            return "";
        }
    }



    protected void LinkButtonDeleteAll_Click(object sender, EventArgs e)
    {
        //SaveTasksOrRates(false);
        if (int.Parse(this.DropDownListTaskRef.SelectedValue) > 0)
        {
            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    cmd.CommandText = " delete from ratetask where idrateplan=" + this.DropDownListTaskRef.SelectedValue;
                    cmd.ExecuteNonQuery();
                    MyGridViewDataBind();
                }
            }
        }
    }

    protected void LinkButtonDeleteCommitted_Click(object sender, EventArgs e)
    {
        //SaveTasksOrRates(false);
        if (int.Parse(this.DropDownListTaskRef.SelectedValue) > 0)
        {
            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    cmd.CommandText = " delete from ratetask where idrateplan=" + this.DropDownListTaskRef.SelectedValue +
                        " and changecommitted=1 ";
                    cmd.ExecuteNonQuery();
                    MyGridViewDataBind();
                }
            }
        }
    }

    protected void LinkButtonDeleteSelected_Click(object sender, EventArgs e)
    {
        //SaveTasksOrRates(false);
        if (int.Parse(this.DropDownListTaskRef.SelectedValue) > 0)
        {

            //gridview didn't return all the rows because of paging
            //foreach (GridViewRow row in GridViewSupplierRates.Rows)
            //{   
            //    if (((CheckBox)row.FindControl("CheckBoxSelected")).Checked== true)
            //    {   
            //        lstIds.Add(((Label)row.FindControl("lblId")).Text);
            //    }
            //}

            //select rows based on type
            //get selected item's ids
            List<long> lstIds = new List<long>();
            Exception e2 = null;
            GetRateTasksForGridAndDelete(ref e2, ref lstIds);

            if (lstIds != null & lstIds.Count > 0)
            {
                using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("", con))
                    {
                        cmd.CommandText = " delete from ratetask where idrateplan=" + this.DropDownListTaskRef.SelectedValue +
                            " and id in (" + string.Join(",", lstIds.ToArray()) + ")";
                        cmd.ExecuteNonQuery();
                        MyGridViewDataBind();
                    }
                }
            }
        }
    }

    protected void LinkButtonSaveAll_Click(object sender, EventArgs e)
    {
        CommitChanges();
    }



    void CommitChanges()
    {
        this.StatusLabel.Text = "";
        long totalCommitCount = 0;
        try
        {
            int idTaskReference = -1;
            int idRatePlan = -1;
            if (this.ViewState["task.sesidRatePlan"] != null)
            {
                idRatePlan = (int) this.ViewState["task.sesidRatePlan"];
                idTaskReference = int.Parse(this.DropDownListTaskRef.SelectedValue);

            }
            if (idTaskReference == -1 || idRatePlan == -1)
            {
                throw new Exception("idRatePlan not found!");
            }

            //if there is a single ratetask with execution order not set, set it for the whole task.
            List<int> lstDistinctExOrders = new List<int>();
            using (var context = new PartnerEntities())
            {
                int rateTaskRef = Convert.ToInt32(this.DropDownListTaskRef.SelectedValue);
                bool nullOrder = context.ratetasks.Any(c => c.ExecutionOrder == null && c.idrateplan == rateTaskRef);
                if (nullOrder == true)
                {
                    SetExecutionOrder();
                }
                //get distinct execution orders
                lstDistinctExOrders = context.Database.SqlQuery<int>(" select distinct executionorder from ratetask " +
                    " where idrateplan=" + this.DropDownListTaskRef.SelectedValue +
                    " and executionorder is not null ").ToList();
                //execute tasks in order of execution
                List<ratetask> lstTasks = new List<ratetask>();
                lstTasks = context.ratetasks.Where(c => c.idrateplan == idTaskReference && c.changecommitted != 1
                    ).ToList();//don't include execorder in where clause here for performance through iterations
                foreach (int order in lstDistinctExOrders)
                {
                    var execlist = lstTasks.Where(c => c.ExecutionOrder == order).ToList();
                    if (execlist.Count > 0)
                    {
                        CommitInOrder(idRatePlan, execlist
                            , ref totalCommitCount);
                    }
                }
            }

            var color4 = ColorTranslator.FromHtml("#008000");
            this.StatusLabel.ForeColor = color4;
            this.StatusLabel.Text += " " + totalCommitCount + " rate task(s) completed successfully !";

        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FA0509");
            this.StatusLabel.ForeColor = color;
            this.StatusLabel.Text += " Error occured while saving records! " + e1.Message + e1.InnerException;
        }
    }

    void CommitInOrder(int idRatePlan, List<ratetask> lstTasksWithValidationError, ref long totalCommitCount)
    {
        if (lstTasksWithValidationError.Where(c =>
            (//string field, could not compare >0 using linq
                                       c.field2.Contains("1") || c.field2.Contains("2") || c.field2.Contains("3")
                                        || c.field2.Contains("4") || c.field2.Contains("5") || c.field2.Contains("6")
                                         || c.field2.Contains("7") || c.field2.Contains("8") || c.field2.Contains("9")
                                       )
            ).Any() == true)//if tasks with error exists
        {
            if (this.CheckBoxContinueOnError.Checked == false)
            {
                var color = ColorTranslator.FromHtml("#FA0509");
                this.StatusLabel.ForeColor = color;
                this.StatusLabel.Text = "Validation error exists! Correct/remove them or Select Continue on Error to try with any next task.";

                this.DropDownListMoreFilters.SelectedValue = "0";
                ButtonFindPrefix_Click(null, null);
                return;
            }
        }
        //Actually commit only those without validation error
        List<ratetask> lstTaskWithoutError = lstTasksWithValidationError.Where(c =>
            (//string field, could not compare >0 using linq
                                       c.field2.Contains("1") == false && c.field2.Contains("2") == false && c.field2.Contains("3") == false
                                         && c.field2.Contains("4") == false && c.field2.Contains("5") == false && c.field2.Contains("6") == false
                                          && c.field2.Contains("7") == false && c.field2.Contains("8") == false && c.field2.Contains("9") == false
                                       )).ToList();
        //update productid
        int thisServiceFamily = (int) this.ViewState["ThisServiceFamily"];
        if (UpdateProductsAndGetIds(lstTaskWithoutError, thisServiceFamily) == 0)//error occured
        {
            return;
        }
        //proceed with process rate task
        totalCommitCount += ProcessRateTask(lstTaskWithoutError, idRatePlan);
    }

    void SetExecutionOrder()
    {
        //a task against a same prefix can be mentioned, both of them are yet to be committed
        //9866--->2014-09-09---.0005
        //9866--->2014-09-20---.0006
        //to handle this case, order has to be defined for duplicated tasks and execute them in order
        using (PartnerEntities context = new PartnerEntities())
        {

            //check if among uncommitted tasks, duplicate prefix exists for same service and subsrvice type
            //underthis ratetaskreference
            string sqlDup = " select Prefix,Category,SubCategory,(case when cast(rateamount as decimal(9,2)) >-1 then 'codeinsert' else 'codedelete' end)  as codetype,count(*) as cnt from ratetask " +
                            " where changecommitted !=1 " +
                            "  and field2=0 " +//exclude error flags
                            " and idrateplan=" + this.DropDownListTaskRef.SelectedValue +
                            " group by prefix,Category, SubCategory, " +
                            " (case when cast(rateamount as decimal(9,2)) >-1 then 'codeinsert' else 'codedelete' end)  " +
                            " having count(*)>1 ";
            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sqlDup, con))
                {
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    List<string> strIds = new List<string>();
                    var dicTuple = new Dictionary<string, List<ratetask>>();
                    //load tasks under this task in cache to save roundtrip
                    sqlDup = " select * from ratetask " +
                                " where idrateplan=" + this.DropDownListTaskRef.SelectedValue +
                                " and changecommitted!=1 " +
                                " and field2=0 ";
                    //" and (case when cast(rateamount as decimal(9,2)) >-1 then 'codeinsert' else 'codedelete' end)='" + CodeType + "' " +
                    List<ratetask> lstCurrentTasks = context.ratetasks.SqlQuery(sqlDup, typeof(ratetask)).ToList();
                    Dictionary<string, List<ratetask>> dicMultiTasks = new Dictionary<string, List<ratetask>>();
                    foreach (ratetask rtask in lstCurrentTasks)
                    {
                        string tup = rtask.Prefix + "/" + rtask.Category + "/" +
                            rtask.SubCategory + "/" + (Convert.ToDouble(rtask.rateamount) > -1 ? "codeinsert" : "codedelete");
                        List<ratetask> lstPerPrefix = null;
                        dicMultiTasks.TryGetValue(tup, out lstPerPrefix);
                        if (lstPerPrefix == null)
                        {
                            lstPerPrefix = new List<ratetask>();
                            dicMultiTasks.Add(tup, lstPerPrefix);
                            dicMultiTasks.TryGetValue(tup, out lstPerPrefix);
                        }
                        lstPerPrefix.Add(rtask);
                    }
                    while (rdr.Read())
                    {
                        string prefix = rdr["Prefix"].ToString();
                        int category = Convert.ToInt32(rdr["Category"].ToString());
                        int subCategory = Convert.ToInt32(rdr["SubCategory"].ToString());
                        string codeType = rdr["codetype"].ToString();//insert or delete
                        int cnt = Convert.ToInt32(rdr["cnt"].ToString());
                        //Tuple.Create(Prefix, Category, SubCategory, CodeType);
                        //get Ids for each tuple

                        //                        SqlDup = " select * from ratetask " +
                        //                                " where idrateplan=" + DropDownListTaskRef.SelectedValue +
                        //                                " and changecommitted!=1 " +
                        //                                " and Prefix='" + Prefix + "' " +
                        //                                " and Category=" + Category + " " +
                        //                                " and SubCategory=" + SubCategory + " " +
                        //" and (case when cast(rateamount as decimal(9,2)) >-1 then 'codeinsert' else 'codedelete' end)='" + CodeType + "' " +
                        //                                " order by startdate,id ";
                        string tup = prefix + "/" + category + "/" +
                            subCategory + "/" + codeType;
                        //List<ratetask> lstDupOrMultipleTasks = context.ratetasks.SqlQuery(SqlDup, typeof(ratetask)).ToList();
                        List<ratetask> lstDupOrMultipleTasks = null;
                        dicMultiTasks.TryGetValue(tup, out lstDupOrMultipleTasks);
                        dicTuple.Add(tup, lstDupOrMultipleTasks);
                    }
                    rdr.Close();
                    //loop through the tuple dic and update execution order of the ratetasks manually
                    var sbSql = new StringBuilder();
                    List<string> idExOrder1 = new List<string>();//set execorder to rate tasks other then dups
                    foreach (KeyValuePair<string, List<ratetask>> thisKVal in dicTuple)
                    {
                        var tp = thisKVal.Key;
                        int execOrder = 1;
                        foreach (ratetask thisTask in thisKVal.Value.OrderBy(c => c.startdate).OrderBy(c => c.id))
                        {
                            sbSql.Append(" update ratetask set executionorder=" + execOrder + " where id=" + thisTask.id + ";");
                            idExOrder1.Add(thisTask.id.ToString());
                            execOrder++;
                        }

                    }

                    using (MySqlCommand cmd2 = new MySqlCommand(" set autocommit=0; ", con))
                    {
                        if (sbSql.Length > 0)//duplicate uncommitted prefix exist in the rate task
                        {
                            try
                            {
                                cmd2.ExecuteNonQuery();//transaction

                                cmd2.CommandText = sbSql.ToString();
                                cmd2.ExecuteNonQuery();

                                cmd2.CommandText = " update ratetask set executionorder=1 " +
                                    " where idrateplan=" + this.DropDownListTaskRef.SelectedValue +
                                    " and changecommitted!=1 " +
                                    " and (executionorder is null or executionorder=0) " +
                                    " and id not in( " + string.Join(",", idExOrder1) + ");";
                                cmd2.ExecuteNonQuery();

                                cmd2.CommandText = " commit; ";
                                cmd2.ExecuteNonQuery();

                                cmd2.CommandText = " set autocommit=1; ";
                                cmd2.ExecuteNonQuery();

                            }
                            catch (Exception e1)
                            {
                                cmd2.CommandText = " rollback; ";
                                cmd2.ExecuteNonQuery();
                                this.StatusLabel.ForeColor = Color.Red;
                                this.StatusLabel.Text = e1.Message + "<br>" + e1.InnerException;
                            }
                        }
                        else//duplicate don't exist but need to set default exec order=1 for all uncommitted prefix
                        {
                            cmd2.CommandText = " update ratetask set executionorder=1 " +
                                    " where idrateplan=" + this.DropDownListTaskRef.SelectedValue +
                                    " and changecommitted!=1 " +
                                    " and executionorder is null ";
                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = " commit; ";
                            cmd2.ExecuteNonQuery();

                            cmd2.CommandText = " set autocommit=1; ";
                            cmd2.ExecuteNonQuery();
                        }
                    }//using
                }
            }
        }
    }



    int ProcessRateTask(List<ratetask> lstRateTask, int idRatePlan)
    {
        int totalCommitCount = 0;
        rate thisRate = null;
        try
        {
            Dictionary<string, List<rate>> dicRateCache = PopulateRateCache(idRatePlan);
            List<ratetask> lstPrefixDelAll, lstPrefixDelLike, lstPrefixDelSingle;
            PopulatePendingRateTasks(lstRateTask, out lstPrefixDelAll, out lstPrefixDelLike, out lstPrefixDelSingle);
            //all code changes will be executed, so count them as commit count
            totalCommitCount = lstPrefixDelAll.Count + lstPrefixDelLike.Count + lstPrefixDelSingle.Count;

            StringBuilder sbSqlCodeDel = new StringBuilder();
            sbSqlCodeDel.Append("set autocommit=0;");
            var rates = dicRateCache.Values.SelectMany(r => r).ToList();
            HandleCodeDeleteTasks(rates, dicRateCache, lstPrefixDelAll, lstPrefixDelLike, lstPrefixDelSingle, sbSqlCodeDel);

            //lock table to make sure I'm the only one to write the rate table

            using (MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"].ConnectionString + "; default command timeout=1800;"))
            {
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    try
                    {
                        con.Open();
                        string sql = "lock tables ratetask write; lock tables rate write;";
                        cmd.CommandText = sql;

                        //*****process tasks other than delete, each as one rate
                        lstRateTask = lstRateTask.Where(c => c.rateamount != "-1" &&
                            c.field2 == "0"//not having validation error
                            ).ToList();
                        //List<string> lstInsertSql = new List<string>();
                        //concurrent dictionary can hold large number of data, so use it with random key
                        BigList<string> lstInsertSql = new BigList<string>();
                        var prevGcLatencyMode = GCSettings.LatencyMode;
                        GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
                        foreach (ratetask thisTask in lstRateTask)
                        {
                            thisRate = RateTaskToRate(thisTask, idRatePlan);
                            //when converting from task to rate, any previous overlap and conflicts are discarded
                            //by the conversion as it is not necessary.

                            //now process this rate task
                            rate presentRate = null;
                            rate nextRate = null;
                            rate prevRate = null;
                            rate lastInstance = null;

                            if (SurroundingRates(ref dicRateCache, thisRate, idRatePlan, ref presentRate, ref nextRate, ref prevRate, ref lastInstance) == 0)
                            //error occured
                            {
                                if (this.CheckBoxContinueOnError.Checked == false) return 0;
                            }

                            RatePositioning ratePos = new RatePositioning(presentRate, prevRate, nextRate, thisRate, this.CheckBoxAutoAdjust.Checked);
                            //rate positions and any overlap/conflict status is known now

                            bool overLap = ratePos.Overlap;
                            bool rateConflict = ratePos.ParameterConflict;
                            bool autoAdjust = ratePos.AutoAdjust; ;
                            bool continueOnError = this.CheckBoxContinueOnError.Checked;

                            //handle rate conflict
                            if (rateConflict == true)
                            {
                                if (this.CheckBoxContinueOnError.Checked == false)
                                {
                                    var color = ColorTranslator.FromHtml("#FA0509");
                                    this.StatusLabel.ForeColor = color;
                                    this.StatusLabel.Text =
                                        "Rate Conflict exists! Correct/remove them or Select Continue on Error to try with any next task.";

                                    this.DropDownListMoreFilters.SelectedValue = "11";
                                    ButtonFindPrefix_Click(null, null);
                                    return 0;
                                }
                            }

                            if (overLap == true)
                            {
                                if (autoAdjust == false)
                                {
                                    if (this.CheckBoxContinueOnError.Checked == false)
                                    {
                                        var color = ColorTranslator.FromHtml("#FA0509");
                                        this.StatusLabel.ForeColor = color;
                                        this.StatusLabel.Text = "Overlap exists! Correct/remove them or Select Continue on Error to try with any next tasks";

                                        this.DropDownListMoreFilters.SelectedValue = "9";
                                        ButtonFindPrefix_Click(null, null);
                                        return 0;
                                    }
                                }
                            }

                            totalCommitCount = ProcessRateTasksBasedOnType(totalCommitCount, thisRate, lstInsertSql, nextRate, prevRate, ratePos, overLap, rateConflict);

                        }//for each rate task (not code delete)
                        GCSettings.LatencyMode = prevGcLatencyMode;

                        cmd.CommandText = sbSqlCodeDel.ToString();//code delete part is handled by this
                        cmd.ExecuteNonQuery();
                        WriteRateTasksToRateTable(cmd, lstInsertSql);

                        MyGridViewDataBind();


                    }//try
                    catch (Exception e1)
                    {
                        cmd.CommandText = " rollback;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "unlock tables;";
                        cmd.ExecuteNonQuery();

                        var color = ColorTranslator.FromHtml("#FA0509");
                        this.StatusLabel.ForeColor = color;
                        this.StatusLabel.Text = e1.Message + "<br/>" + e1.InnerException + "<br/>" +
                            thisRate.Prefix + " effective date:" + thisRate.startdate;
                        return -1;
                    }
                }//using command
            }//using mysql connection

        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FA0509");
            this.StatusLabel.ForeColor = color;
            this.StatusLabel.Text = e1.Message + "<br/>" + e1.InnerException + "<br/>" +
                thisRate.Prefix + " effective date:" + thisRate.startdate;
            return -1;
        }

        return totalCommitCount;


    }

    private static void WriteRateTasksToRateTable(MySqlCommand cmd, BigList<string> sqls)
    {
        //insert part, long sql causes exception, have to create multiple batch
        int batchsize = 10000;
        int batchcount = 0;
        List<string> lstBatchInsert = new List<string>();
        List<string> lstBatchUpdate = new List<string>();
        for (int l = 0; l < sqls.Count; l++)
        {
            string sql = sqls[l];
            if (sql.StartsWith("(")) //extended insert statement
            {
                lstBatchInsert.Add(sql);
            }
            else
            {
                lstBatchUpdate.Add(sql);
            }
            ++batchcount;
            if (batchcount == batchsize)
            {
                WriteInsertUpdateSqls(cmd, ref batchcount, ref lstBatchInsert, ref lstBatchUpdate);
            }
        }
        WriteInsertUpdateSqls(cmd, ref batchcount, ref lstBatchInsert, ref lstBatchUpdate);
        cmd.CommandText = " commit;";
        cmd.ExecuteNonQuery();
    }

    private static void WriteInsertUpdateSqls(MySqlCommand cmd, ref int batchcount, ref List<string> lstBatchInsert, ref List<string> lstBatchUpdate)
    {
        if (lstBatchInsert.Any())
        {
            cmd.CommandText = new StringBuilder(GetExtInsertHeader())
                .Append(string.Join(", ", lstBatchInsert)).ToString();
            cmd.ExecuteNonQuery();
            lstBatchInsert = new List<string>();
        }

        if (lstBatchUpdate.Any())
        {
            cmd.CommandText = string.Join(" ", lstBatchUpdate);
            cmd.ExecuteNonQuery();
            lstBatchUpdate = new List<string>();
            batchcount = 0;
        }
    }

    static string GetExtInsertHeader()
    {
        return "INSERT INTO rate " +
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
                   "`SubCategory`,`ProductId`,`RateAmountRoundupDecimal`)            " +
                   "VALUES                        ";
    }

    private int ProcessRateTasksBasedOnType(int totalCommitCount, rate thisRate, BigList<string> lstInsertSql, rate nextRate, rate prevRate, RatePositioning ratePos, bool overLap, bool rateConflict)
    {
        //process rates which are not overlap or conflict
        switch (thisRate.Status)
        {
            case 9://overlap
            case 12://rate position not found

                lstInsertSql.Add(
                " update ratetask set status =" + thisRate.Status +
                        " where id=" + thisRate.id + ";");

                break;
            case 2://new
                totalCommitCount = PrepareSqlForNewRates(totalCommitCount, thisRate, lstInsertSql, nextRate, ratePos, overLap, rateConflict);
                break;
            case 5://unchanged
            case 3://increase
            case 4://decrease
                totalCommitCount = PrePareSqlForUnchangedIncreaseDecrease(totalCommitCount, thisRate, lstInsertSql, prevRate, ratePos, overLap, rateConflict);
                break;
            case 10://overlap adjusted
                List<string> affectedOlRates = new List<string>();

                if (ratePos.ThisOverLapType == OverlapTypes.OverlappingNext)
                {
                    //have to change own end date as well so that it doesn't overlap the next
                    //AffectedOLRates.Add(ThisRate.id.ToString());--this can't be set, the taskid will be set in rate table
                    thisRate.enddate = nextRate.startdate;
                }
                else//overlappingboth or overlapbycoincide
                {
                    //end previous rate
                    if (prevRate != null)
                    {

                        lstInsertSql.Add(
                        " update rate set enddate='" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                        " ChangedByTaskId=" + thisRate.id +
                            " where id=" + prevRate.id + ";");

                        affectedOlRates.Add(prevRate.id.ToString());
                    }
                    if ((ratePos.ThisOverLapType == OverlapTypes.OverlappingBoth) ||
                        (thisRate.enddate == ratePos.FutureDate))
                    {
                        //have to change own end date as well so that it doesn't overlap the next
                        thisRate.enddate = nextRate.startdate;
                    }
                }

                lstInsertSql.Add(GetExtInsertValuePartForRate(thisRate));
                lstInsertSql.Add(
                    " update ratetask set changecommitted=1,status =" + thisRate.Status +
                    " ,affectedrates='" + string.Join(",", affectedOlRates.ToArray()) + "'" +
                    " where " +
                    " id=" + thisRate.id + ";");
                totalCommitCount++;
                break;
            case 13://existing, dont' mark as change committed
                lstInsertSql.Add(
                " update ratetask set status =" + thisRate.Status + " where " +
                        " id=" + thisRate.id + ";");

                break;
        }

        return totalCommitCount;
    }

    private static void HandleCodeDeleteTasks(List<rate> rateCache, Dictionary<string, List<rate>> dicRateCache, List<ratetask> lstPrefixDelAll, List<ratetask> lstPrefixDelLike, List<ratetask> lstPrefixDelSingle, StringBuilder sbSqlCodeDel)
    {
        //populate delete Sql first
        //delete in the order of most specific code delete instruction
        Dictionary<string, string> dicRateDeleteSql = new Dictionary<string, string>();//idRate,Sql
        foreach (ratetask deltask in lstPrefixDelSingle)
        {
            DateTime delDate = Convert.ToDateTime(deltask.startdate);
            if (dicRateCache.ContainsKey(deltask.Prefix) == false)
            {
                //update the status of the delete taks as well
                sbSqlCodeDel.Append(" update ratetask set changecommitted=1,status =1 where " +
                                    " id=" + deltask.id).Append(";");
                continue;
            }
            List<rate> lstDelRates = dicRateCache[deltask.Prefix].
                Where(c => (c.enddate == null || c.enddate > delDate) && c.startdate <= delDate).ToList();
            foreach (rate delRate in lstDelRates)
            {
                if (dicRateDeleteSql.ContainsKey(delRate.id.ToString())) continue;
                dicRateDeleteSql.Add(delRate.id.ToString(), " update rate set enddate='" + deltask.startdate + "', " +
                                        " ChangedByTaskId=" + deltask.ChangedByTaskId +//could have used id, but kept part of previous code with new {}
                                        " where id=" + delRate.id.ToString());
            }

            //update the status of the delete taks as well
            sbSqlCodeDel.Append(" update ratetask set changecommitted=1,status =1 where " +
                                        " id=" + deltask.id).Append(";");
        }
        foreach (ratetask deltask in lstPrefixDelLike)
        {

            DateTime delDate = Convert.ToDateTime(deltask.startdate);
            //dictionary is useless here
            List<rate> lstDelRates = rateCache.Where(c => c.Prefix.StartsWith(deltask.Prefix) && (c.enddate == null || c.enddate > delDate) && c.startdate <= delDate).ToList();
            foreach (rate delRate in lstDelRates)
            {
                if (dicRateDeleteSql.ContainsKey(delRate.id.ToString())) continue;
                dicRateDeleteSql.Add(delRate.id.ToString(), " update rate set enddate='" + deltask.startdate + "', " +
                                        " ChangedByTaskId=" + deltask.ChangedByTaskId +//could have used id, but kept part of previous code with new {}
                                        " where id=" + delRate.id.ToString());
            }

            //update the status of the delete taks as well
            sbSqlCodeDel.Append(" update ratetask set changecommitted=1,status =1 where " +
                                        " id=" + deltask.id).Append(";");
        }
        foreach (ratetask deltask in lstPrefixDelAll)
        {

            DateTime delDate = Convert.ToDateTime(deltask.startdate);
            //dictionary is useless here
            List<rate> lstDelRates = rateCache.Where(c => (c.enddate == null || c.enddate > delDate) && c.startdate <= delDate).ToList();
            foreach (rate delRate in lstDelRates)
            {
                if (dicRateDeleteSql.ContainsKey(delRate.id.ToString())) continue;
                dicRateDeleteSql.Add(delRate.id.ToString(), " update rate set enddate='" + deltask.startdate + "', " +
                                        " ChangedByTaskId=" + deltask.ChangedByTaskId +//could have used id, but kept part of previous code with new {}
                                        " where id=" + delRate.id.ToString());
            }

            //update the status of the delete taks as well
            sbSqlCodeDel.Append(" update ratetask set changecommitted=1,status =1 where " +
                                        " id=" + deltask.ChangedByTaskId).Append(";");
        }

        //write back code delete sqls to sbsql
        if (dicRateDeleteSql.Count > 0)
        {
            sbSqlCodeDel.Append(string.Join(";", dicRateDeleteSql.Values.ToList())).Append(";");
        }
    }

    private static void PopulatePendingRateTasks(List<ratetask> lstRateTask, out List<ratetask> lstPrefixDelAll, out List<ratetask> lstPrefixDelLike, out List<ratetask> lstPrefixDelSingle)
    {
        List<ratetask> lstDeleteTasks = lstRateTask.Where(c => c.rateamount == "-1"
                                    && c.field2 == "0"//not having validation error
                                    ).ToList();
        lstPrefixDelAll = lstDeleteTasks.Where(c => c.Prefix == "*").Select(c => new ratetask
        {
            Prefix = "*",
            startdate = c.startdate,
            ChangedByTaskId = c.id.ToString(),
            Category = c.Category,
            SubCategory = c.SubCategory,
            id=c.id
        }).ToList();
        lstPrefixDelLike = lstDeleteTasks.Where(c => c.Prefix.EndsWith("*") && c.Prefix.Length > 1)
            .Select(c => new ratetask
            {
                Prefix = c.Prefix.Split('*')[0],
                startdate = c.startdate,
                ChangedByTaskId = c.id.ToString(),
                Category = c.Category,
                SubCategory = c.SubCategory,
                id = c.id
            }).ToList();
        lstPrefixDelSingle = lstDeleteTasks.Where(c => c.Prefix.Contains("*") == false).Select(c => new ratetask
        {
            Prefix = c.Prefix,
            startdate = c.startdate,
            ChangedByTaskId = c.id.ToString(),
            Category = c.Category,
            SubCategory = c.SubCategory,
            id = c.id
        }).ToList();
    }
    private static List<rate> GetRatesByRatePlan(int idRatePlan, int segmentSize)
    {
        List<rate> rates = new List<rate>();
        using (PartnerEntities context = new PartnerEntities())
        {
            int startLimit = 0;
            bool rateExists = true;
            while (rateExists)
            {
                //GC.TryStartNoGCRegion(250 * 1000 * 1000);
                var prevGcLatencyMode = GCSettings.LatencyMode;
                GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
                var sql = $@"select * from rate where idrateplan={idRatePlan}
                         order by id limit {startLimit},{segmentSize}";
                var rateSegment = context.Database.SqlQuery<rate>
                    (sql).ToList();
                if (rateSegment.Any())
                {
                    rates.AddRange(rateSegment);
                    startLimit += segmentSize;
                }
                else
                {
                    rateExists = false;
                }
                //GC.EndNoGCRegion();
                GCSettings.LatencyMode = prevGcLatencyMode;
            }
        }
        return rates;
    }
    private static Dictionary<string, List<rate>> PopulateRateCache(int idRatePlan)
    {
        //this should be to load all rates in a rateplan, byte per rate: ~100, 1 GB for 10 Million rates
        var rateCache = GetRatesByRatePlan(idRatePlan,10000);
        Dictionary<string, List<rate>> dicRateCache = new Dictionary<string, List<rate>>();
        foreach (rate r in rateCache)
        {
            List<rate> thislist = null;
            dicRateCache.TryGetValue(r.Prefix, out thislist);
            if (thislist == null)
            {
                //prefix not in the dictionary yet, create dictionary item first.
                dicRateCache.Add(r.Prefix, new List<rate>());
                dicRateCache.TryGetValue(r.Prefix, out thislist);
            }
            thislist.Add(r);
        }
        return dicRateCache;
    }

    private int PrepareSqlForNewRates(int totalCommitCount, rate thisRate, BigList<string> lstInsertSql, rate nextRate, RatePositioning ratePos, bool overLap, bool rateConflict)
    {
        if (overLap == false && rateConflict == false)
        {
            if (ratePos.ThisPosition == RatePositions.BeforeAll)
            {
                thisRate.enddate = nextRate.startdate;
            }
            lstInsertSql.Add(GetExtInsertValuePartForRate(thisRate));

            lstInsertSql.Add(
            " update ratetask set changecommitted=1,status =" + thisRate.Status + " where " +
                " id=" + thisRate.id + ";");
            totalCommitCount++;
        }

        return totalCommitCount;
    }

    private int PrePareSqlForUnchangedIncreaseDecrease(int totalCommitCount, rate thisRate, BigList<string> lstInsertSql, rate prevRate, RatePositioning ratePos, bool overLap, bool rateConflict)
    {
        //applicabl for in between and latest rates.    
        if (overLap == false && rateConflict == false)
        {
            //end previous rate
            if (prevRate != null)//if a previous continued rate exists
            {
                if (prevRate.enddate == ratePos.FutureDate)//null, end previous rate with no end date
                {
                    lstInsertSql.Add(
                    " update rate set enddate='" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    " ChangedByTaskId=" + thisRate.id +
                    " where id=" + prevRate.id + ";");
                }
                //insert the new rate
                lstInsertSql.Add(GetExtInsertValuePartForRate(thisRate));

                lstInsertSql.Add(
                " update ratetask set changecommitted=1,status =" + thisRate.Status + " where " +
                    " id=" + thisRate.id + ";");
                totalCommitCount++;
            }
            else//no previous contniued rate 
            {
                lstInsertSql.Add(GetExtInsertValuePartForRate(thisRate));
                lstInsertSql.Add(
                " update ratetask set changecommitted=1,status =" + thisRate.Status +
                    " ,affectedrates='" + prevRate.id + "'" +
                    " where " +
                    " id=" + thisRate.id + ";");
                totalCommitCount++;
            }
        }

        return totalCommitCount;
    }

    rate RateTaskToRate(ratetask thisTask, int idRatePlan)
    {

        try
        {
            rate thisRate = new rate();
            thisRate.id = thisTask.id;
            thisRate.Prefix = thisTask.Prefix;
            thisRate.description = thisTask.description;
            thisRate.rateamount = Convert.ToDecimal(thisTask.rateamount != null && thisTask.rateamount != "" ? thisTask.rateamount : "0");
            thisRate.WeekDayStart = Convert.ToInt32(thisTask.WeekDayStart != null && thisTask.WeekDayStart != "" ? thisTask.WeekDayStart : "0");
            thisRate.WeekDayEnd = Convert.ToInt32(thisTask.WeekDayEnd != null && thisTask.WeekDayEnd != "" ? thisTask.WeekDayEnd : "0");
            thisRate.starttime = thisTask.starttime;
            thisRate.endtime = thisTask.endtime;
            thisRate.Resolution = Convert.ToInt32(thisTask.Resolution);
            thisRate.MinDurationSec = Convert.ToSingle(thisTask.MinDurationSec);
            thisRate.SurchargeTime = Convert.ToInt32(thisTask.SurchargeTime);
            thisRate.SurchargeAmount = Convert.ToDecimal(thisTask.SurchargeAmount);
            thisRate.idrateplan = idRatePlan;//ThisTask.idrateplan;
            thisRate.CountryCode = thisTask.CountryCode;

            DateTime tempDate = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime.TryParseExact(thisTask.date1, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate);
            thisRate.date1 = tempDate;

            thisRate.Status = Convert.ToInt32(thisTask.Status != null && thisTask.Status != "" ? thisTask.Status : "0");
            thisRate.field2 = Convert.ToInt32(thisTask.field2 != null && thisTask.field2 != "" ? thisTask.field2 : "0");
            thisRate.field3 = Convert.ToInt32(thisTask.field3 != null && thisTask.field3 != "" ? thisTask.field3 : "0");
            thisRate.field4 = thisTask.field4;
            thisRate.field5 = thisTask.field5;

            DateTime tempDatest = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime.TryParseExact(thisTask.startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDatest);
            thisRate.startdate = tempDatest;

            DateTime tempDate1 = new DateTime(2000, 1, 1, 0, 0, 0);
            if (DateTime.TryParseExact(thisTask.enddate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate1))
            {
                thisRate.enddate = tempDate1;
            }
            else
            {
                thisRate.enddate = null;
            }


            thisRate.Inactive = Convert.ToInt32(thisTask.Inactive != null ? thisRate.Inactive : 0);
            thisRate.RouteDisabled = Convert.ToInt32(thisTask.RouteDisabled != null && thisTask.RouteDisabled != "" ? thisTask.RouteDisabled : "0");
            thisRate.Type = Convert.ToInt32(thisTask.Type != null && thisTask.Type != "" ? thisTask.Type : "0");
            thisRate.Currency = Convert.ToInt32(thisTask.Currency != null && thisTask.Currency != "" ? thisTask.Currency : "0");
            thisRate.OtherAmount1 = Convert.ToDecimal(thisTask.OtherAmount1 != null && thisTask.OtherAmount1 != "" ? thisTask.OtherAmount1 : "0");
            thisRate.OtherAmount2 = Convert.ToDecimal(thisTask.OtherAmount2 != null && thisTask.OtherAmount2 != "" ? thisTask.OtherAmount2 : "0");
            thisRate.OtherAmount3 = Convert.ToDecimal(thisTask.OtherAmount3 != null && thisTask.OtherAmount3 != "" ? thisTask.OtherAmount3 : "0");
            thisRate.OtherAmount4 = Convert.ToDecimal(thisTask.OtherAmount4 != null && thisTask.OtherAmount4 != "" ? thisTask.OtherAmount4 : "0");
            thisRate.OtherAmount5 = Convert.ToDecimal(thisTask.OtherAmount5 != null && thisTask.OtherAmount5 != "" ? thisTask.OtherAmount5 : "0");
            thisRate.OtherAmount6 = Convert.ToDecimal(thisTask.OtherAmount6 != null && thisTask.OtherAmount6 != "" ? thisTask.OtherAmount6 : "0");
            thisRate.OtherAmount7 = Convert.ToSingle(thisTask.OtherAmount7 != null && thisTask.OtherAmount7 != "" ? thisTask.OtherAmount7 : "0");
            thisRate.OtherAmount8 = Convert.ToSingle(thisTask.OtherAmount8 != null && thisTask.OtherAmount8 != "" ? thisTask.OtherAmount8 : "0");
            thisRate.OtherAmount9 = Convert.ToSingle(thisTask.OtherAmount9 != null && thisTask.OtherAmount9 != "" ? thisTask.OtherAmount9 : "0");
            thisRate.OtherAmount10 = Convert.ToSingle(thisTask.OtherAmount10 != null && thisTask.OtherAmount10 != "" ? thisTask.OtherAmount10 : "0");
            thisRate.TimeZoneOffsetSec = Convert.ToDecimal(thisTask.TimeZoneOffsetSec != null && thisTask.TimeZoneOffsetSec != "" ? thisTask.TimeZoneOffsetSec : "0");
            thisRate.RatePosition = Convert.ToInt32(thisTask.RatePosition != null && thisTask.RatePosition != "" ? thisTask.RatePosition : "0");
            thisRate.IgwPercentageIn = Convert.ToSingle(thisTask.IgwPercentageIn != null && thisTask.IgwPercentageIn != "" ? thisTask.IgwPercentageIn : "0");
            thisRate.ConflictingRateIds = thisTask.ConflictingRateIds;
            thisRate.ChangedByTaskId = Convert.ToInt32(thisTask.ChangedByTaskId != null && thisTask.ChangedByTaskId != "" ? thisTask.ChangedByTaskId : "0");
            thisRate.ProductId = Convert.ToInt32(thisTask.ProductId != null ? thisTask.ProductId : 0);

            DateTime tempDateco = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime.TryParseExact(thisTask.date1, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDateco);
            thisRate.ChangedOn = tempDateco;
            thisRate.Status = Convert.ToInt32(thisTask.Status != null && thisTask.Status != "" ? thisTask.Status : "0");
            thisRate.idPreviousRate = Convert.ToInt32(thisTask.idPreviousRate != null && thisTask.idPreviousRate != "" ? thisTask.idPreviousRate : "0");
            thisRate.EndPreviousRate = Convert.ToSByte(thisTask.EndPreviousRate != null && thisTask.EndPreviousRate != "" ? thisTask.EndPreviousRate : "0");
            thisRate.Category = Convert.ToSByte(thisTask.Category != null && thisTask.Category != "" ? thisTask.Category : "0");
            thisRate.SubCategory = Convert.ToSByte(thisTask.SubCategory != null && thisTask.SubCategory != "" ? thisTask.SubCategory : "0");
            //ThisRate.ChangeCommitted = ThisTask.ChangeCommitted;
            thisRate.RateAmountRoundupDecimal = Convert.ToInt32(thisTask.RateAmountRoundupDecimal != null ? thisTask.RateAmountRoundupDecimal : 0);
            return thisRate;
        }
        catch (Exception e1)
        {
            this.StatusLabel.ForeColor = Color.Red;
            this.StatusLabel.Text = "Error in conversion from Rate Task to Rate<br/>" + e1.Message + "<br/>" + e1.InnerException;
            return null;
        }
    }

    int SurroundingRates(ref Dictionary<string, List<rate>> dicRateCache, rate thisRate, int idRatePlan, ref rate presentRate, ref rate nextRate, ref rate prevRate, ref rate lastInstance)
    {
        try
        {
            List<rate> rateCache = null;
            dicRateCache.TryGetValue(thisRate.Prefix, out rateCache);

            if (rateCache == null || rateCache.Count == 0) return 1;

            if (thisRate.rateamount == -1)//code delete
            {
                lastInstance = rateCache.Where(c => c.idrateplan == idRatePlan && c.Prefix == thisRate.Prefix
                && c.Category == thisRate.Category
                && c.SubCategory == thisRate.SubCategory
                && c.startdate <= thisRate.startdate).OrderByDescending(c => c.startdate).Take(1).ToList().FirstOrDefault();
            }
            else if (thisRate.rateamount != -1)//other than code delete
            {
                prevRate = rateCache.Where(c => c.idrateplan == idRatePlan && c.Prefix == thisRate.Prefix
                                                                && c.startdate < thisRate.startdate
                                                                && c.Category == thisRate.Category
                                                               && c.SubCategory == thisRate.SubCategory).OrderByDescending(c => c.startdate).Take(1).ToList().FirstOrDefault();


                presentRate = rateCache.Where(c => c.idrateplan == idRatePlan && c.Prefix == thisRate.Prefix
                                                                   && c.startdate == thisRate.startdate
                                                                   && c.Category == thisRate.Category
                                                                   && c.SubCategory == thisRate.SubCategory).OrderByDescending(c => c.startdate).Take(1).ToList().FirstOrDefault();



                nextRate = rateCache.Where(c => c.idrateplan == idRatePlan && c.Prefix == thisRate.Prefix
                                                                    && c.startdate > thisRate.startdate
                                                                    && c.Category == thisRate.Category
                                                                   && c.SubCategory == thisRate.SubCategory).OrderBy(c => c.startdate).Take(1).ToList().FirstOrDefault();
            }

            return 1;

        }
        catch (Exception e1)
        {
            var color = ColorTranslator.FromHtml("#FF0000");
            this.StatusLabel.ForeColor = color;
            this.StatusLabel.Text = "Error finding surrounding rates for rate " + thisRate.Prefix + " and Start Time=" + thisRate.startdate.ToString("yyyy-MM-dd HH:mm:ss");
            return 0;
        }
    }



    protected void ButtonFindPrefix_Click(object sender, EventArgs e)
    {
        this.HiddenFieldSelect.Value = "0";
        MyGridViewDataBind();
    }

    protected void ButtonFindPrefixSelect_Click(object sender, EventArgs e)
    {
        //before clicking on click has set the hidden value for filter flag to 1 already
        MyGridViewDataBind();
    }


    //rate import functions&&&&&&&&&&&ENDs&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&


    public string[] RateNormalizer(string filename, int ratePlanFormat)
    {

        String[] output = null;
        try
        {
            if (ratePlanFormat == 2) //delimited
            {
                output = File2String(filename);
            }
        }
        catch (Exception exp)
        {
            output = null;
        }
        return output;
    }

    public string[] File2String(string strFileName)
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
    public bool String2File(string strFilePath, string[] strArray)
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
    //public string[] CreateRateSheetArray(DataTable srcdt, int ratePlanFormat, int mode)
    //{

    //    int srccolumnCount = srcdt.Columns.Count;
    //    int srcrowCount = srcdt.Rows.Count;

    //    List<string> strOut = new List<string>();

    //    try
    //    {



    //        foreach (DataRow dr in srcdt.Rows)
    //        {
    //            string effectDate = "";
    //            if (ratePlanFormat == 4)
    //                effectDate = DateTime.Now.ToString();
    //            else if (ratePlanFormat == 2)
    //            {
    //                if (mode == 1)
    //                {
    //                    effectDate = dr[6].ToString().Trim();
    //                }
    //                else
    //                {
    //                    effectDate = dr[5].ToString().Trim();
    //                }

    //            }
    //            else
    //            {
    //                effectDate = dr[6].ToString().Trim();
    //            }
    //            DateTime efctDate;
    //            string dateformat1 = "";
    //            if (effectDate.Length > 0)
    //            {
    //                efctDate = Convert.ToDateTime(effectDate);
    //                effectDate = efctDate.ToString("yyyy-MM-dd HH:mm:ss");
    //                dateformat1 = efctDate.ToString("u");
    //            }

    //            string strRow = "";//Destination	USD / Min	Country Code	Area Code	Complete Code	Change	Effective Date

    //            if (effectDate == "")
    //                effectDate = @"\N";

    //            if (ratePlanFormat == 2)
    //            {
    //                if (mode == 1)
    //                {
    //                    strRow = dr[4].ToString().Trim() + "`" + dr[1].ToString().Trim() + "`" + dr[5].ToString().Trim() + "`1`" + dr[2].ToString().Trim() + "`" + effectDate.ToString().Trim() + @"`\N";
    //                }
    //                else if (mode == 2)
    //                {
    //                    if (dr[6].ToString().Trim() == "Pending Code Removal")
    //                        strRow = dr[4].ToString().Trim() + "`" + dr[1].ToString().Trim() + "`-1`1`" + dr[2].ToString().Trim() + "`" + effectDate.ToString().Trim() + @"`\N";
    //                    //else
    //                    // strRow = dr[4].ToString().Trim() + "`" + dr[1].ToString().Trim() + "`-0`1`" + dr[2].ToString().Trim() + "`" + effectDate.ToString().Trim() + @"`\N";
    //                }
    //            }
    //            else if (ratePlanFormat == 3)
    //                strRow = dr[4].ToString().Trim() + "`" + dr[0].ToString().Trim() + "`" + dr[1].ToString().Trim() + "`1`" + dr[2].ToString().Trim() + "`" + effectDate.ToString().Trim() + @"`\N";
    //            else if (ratePlanFormat == 4)//Location	Code	banglatel	Gold	Silver	Bronze
    //                strRow = dr[1].ToString().Trim() + "`" + dr[0].ToString().Trim() + "`" + dr[3].ToString().Trim() + @"`1`\N`" + effectDate.ToString().Trim() + @"`\N";

    //            //bool check = append2String(strRow.Trim());
    //            if (strRow != "")
    //                strOut.Add(strRow.Trim());
    //            //sw.WriteLine(strRow.Trim());

    //        }
    //        //sw.Close();
    //        //MessageBox.Show("ok", "RateSheet");
    //    }
    //    catch (Exception ex)
    //    {
    //        return null;
    //    }
    //    return strOut.ToArray();
    //}
    //public DataTable process(string filename, int ratePlanFormat, int currentsheet)
    //{
    //    string colHeader = "0ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    //    string FindStart = "", FindEnd = "";
    //    if (ratePlanFormat == 2)
    //    {
    //        FindStart = "Country";
    //        FindEnd = "Prime Assurance";
    //    }
    //    else if (ratePlanFormat == 3)
    //    {
    //        FindStart = "Destination";
    //    }
    //    else if (ratePlanFormat == 4)
    //    {
    //        FindStart = "Location";
    //    }
    //    else
    //    {
    //        FindStart = "Prefix";
    //    }

    //    DataTable dt = new DataTable();

    //    Excel.Workbook newWorkBook = null;
    //    Excel.Application excelApp = new Excel.Application(); //Create new App
    //    excelApp.Visible = true;

    //    try
    //    {
    //        int ro = 0; int co = 0; String startingRow = ""; String endingRow = "";
    //        //XLFileOpen:        
    //        newWorkBook = excelApp.Workbooks.Open(filename, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);//Add(System.Reflection.Missing.Value);
    //        Excel.Sheets excelSheets = newWorkBook.Worksheets;
    //        Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelSheets.get_Item(currentsheet);
    //        Excel.Range excelcell = (Excel.Range)excelWorksheet.UsedRange;

    //        int a = excelcell.Rows.Count;
    //        int b = excelcell.Columns.Count;

    //        Excel.Range rfind = null;
    //        Excel.Range nfind = null;
    //        rfind = excelcell.Find(FindStart, Missing.Value, Excel.XlFindLookIn.xlValues, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByColumns, Excel.XlSearchDirection.xlNext, true, false, false);

    //        int c = rfind.Row;
    //        int d = rfind.Column;

    //        if (ratePlanFormat == 2)
    //        {
    //            nfind = excelcell.Find(FindEnd, Missing.Value, Excel.XlFindLookIn.xlValues, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByColumns, Excel.XlSearchDirection.xlNext, true, false, false);
    //            //MessageBox.Show("Find:" + nfind.Row.ToString() + "==" + nfind.Column.ToString());
    //            Excel.Range s;
    //            ro = nfind.Row;
    //            co = nfind.Column;
    //            s = (Excel.Range)excelWorksheet.Cells[ro, co];
    //            //string khu = null;
    //            String khu = s.Value2.ToString(); //((Excel.Range)s).Value2.ToString();
    //            if (khu != "")
    //            {
    //                ro--;
    //                s = (Excel.Range)excelWorksheet.Cells[ro, co];
    //            }
    //            while (s.Value2 == null || s.Value2.ToString() == "")
    //            {
    //                ro--;
    //                s = (Excel.Range)excelWorksheet.Cells[ro, co];
    //            }
    //            //MessageBox.Show("TotalRow=" + a.ToString() + "Total Coloumn" + b.ToString() + "StartingRow=" + c.ToString() + "EndingRow=" + ro.ToString());
    //            startingRow = colHeader[co] + c.ToString();
    //            endingRow = colHeader[co] + ro.ToString();

    //        }
    //        else if (ratePlanFormat == 3 || ratePlanFormat == 4)
    //        {
    //            nfind = excelcell.Find(FindEnd, Missing.Value, Excel.XlFindLookIn.xlValues, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByColumns, Excel.XlSearchDirection.xlNext, true, false, false);
    //            //MessageBox.Show("Find:" + nfind.Row.ToString() + "==" + nfind.Column.ToString());
    //            Excel.Range s;
    //            ro = rfind.Row;
    //            co = rfind.Column;
    //            s = (Excel.Range)excelWorksheet.Cells[ro, co];
    //            //string khu = null;
    //            String khu = s.Value2.ToString(); //((Excel.Range)s).Value2.ToString();
    //            if (khu != "")
    //            {
    //                ro++; ro = a;
    //                s = (Excel.Range)excelWorksheet.Cells[ro, co];
    //            }
    //            while (s.Value2 != null)
    //            {
    //                ro++;
    //                s = (Excel.Range)excelWorksheet.Cells[ro, co];
    //            }
    //            ro--;

    //            startingRow = colHeader[co] + c.ToString();
    //            endingRow = colHeader[co] + ro.ToString();
    //        }
    //        else
    //        {
    //        }

    //        int tokenFlag = 0; int count = 0, splitcount = 0;
    //        int totalcount = 0;

    //        #region Coloumn_Header
    //        if (ratePlanFormat == 2)
    //        {
    //            if (currentsheet == 1)
    //            {
    //                dt.Columns.Add("Country");//0
    //                dt.Columns.Add("Destination");//1
    //                dt.Columns.Add("CountryCode");//2
    //                dt.Columns.Add("CityCode");//3
    //                dt.Columns.Add("Prefix");//4
    //                dt.Columns.Add("Price");//5
    //                dt.Columns.Add("EffectiveDate");//6
    //                dt.Columns.Add("PrimeAssurance");//7
    //                dt.Columns.Add("Comments");//8
    //            }
    //            else if (currentsheet == 2)
    //            {
    //                //Destination	Effective Date	Country Code(s)	Previous Code(s)	New Code(s)	Modification	Comments
    //                dt.Columns.Add("Country");//0
    //                dt.Columns.Add("Destination");//1
    //                dt.Columns.Add("CountryCode");//3
    //                dt.Columns.Add("Modification");//6
    //                dt.Columns.Add("Prefix");//4
    //                dt.Columns.Add("EffectiveDate");//2
    //                dt.Columns.Add("Comments");//7
    //            }
    //        }
    //        else if (ratePlanFormat == 3)//Bharti/////////////
    //        {
    //            dt.Columns.Add("Destination");//0
    //            dt.Columns.Add("USD / Min");//1
    //            dt.Columns.Add("Country Code");//2	
    //            dt.Columns.Add("Area Code");//3
    //            dt.Columns.Add("Complete Code");//4
    //            dt.Columns.Add("Change");//5
    //            dt.Columns.Add("Effective Date");//6
    //        }
    //        else if (ratePlanFormat == 4)//IDT/////////////
    //        {
    //            //Location	Code	banglatel	Gold	Silver	Bronze
    //            dt.Columns.Add("Location");//0
    //            dt.Columns.Add("Code");//1
    //            dt.Columns.Add("banglatel");//2	
    //            dt.Columns.Add("Gold");//3
    //            dt.Columns.Add("Silver");//4
    //            dt.Columns.Add("Bronze");//5
    //            //dt.Columns.Add("Effective Date");//6
    //        }
    //        #endregion

    //        #region Dictionary
    //        if (ratePlanFormat == 3)
    //        {
    //            #region Sheet1
    //            for (int row = c + 1; row <= ro; row++)
    //            {
    //                DataRow dr = dt.NewRow();
    //                string[] tempwords = null;
    //                int drCol = 0;
    //                count++;

    //                for (int col = co; col <= (b + 1); col++, drCol++)
    //                {
    //                    if (((Excel.Range)excelWorksheet.Cells[row, col]).Value2 != null)
    //                    {
    //                        //((Excel.Range)excelWorksheet.Cells[row, col]).Value2;
    //                        String tempstr = ((Excel.Range)excelWorksheet.Cells[row, col]).Value2.ToString();

    //                        if (col == 8)//col starts with 2 and end with 8
    //                        {
    //                            if (tempstr != "")
    //                            {
    //                                double dttim = Convert.ToDouble(tempstr);
    //                                DateTime dddt = DateTime.FromOADate(dttim);
    //                                tempstr = dddt.ToString("yyyy-MM-dd HH:mm:ss");
    //                            }
    //                        }

    //                        dr[drCol] = tempstr.Trim();

    //                    }
    //                    else
    //                    {
    //                        if (drCol == 5)
    //                        {
    //                            dr[drCol] = "No Change";
    //                        }
    //                        else if (drCol == 6)
    //                        {
    //                            dr[drCol] = DateTime.Now.ToString().Trim();
    //                        }
    //                        else
    //                        {
    //                            dr[drCol] = " ";
    //                        }
    //                    }

    //                }
    //                if (tokenFlag == 0)
    //                {
    //                    dt.Rows.Add(dr);
    //                    dt.AcceptChanges();
    //                    totalcount++;
    //                }
    //                else
    //                {
    //                    string col1 = dr[0].ToString();
    //                    string col2 = dr[1].ToString();
    //                    string col3 = dr[2].ToString();
    //                    string col4 = dr[3].ToString();
    //                    string col5 = dr[4].ToString();
    //                    string col6 = dr[5].ToString();
    //                    string col7 = dr[6].ToString();

    //                    tokenFlag = 0;
    //                    tempwords = null;

    //                }
    //            }
    //            #endregion
    //        }
    //        ////////////////IDT/////////////////
    //        if (ratePlanFormat == 4)
    //        {
    //            #region Sheet1
    //            for (int row = c + 1; row <= ro; row++)
    //            {
    //                DataRow dr = dt.NewRow();
    //                //string[] tempwords = null;
    //                int drCol = 0;
    //                count++;

    //                for (int col = co; col <= b; col++, drCol++)
    //                {
    //                    if (((Excel.Range)excelWorksheet.Cells[row, col]).Value2 != null)
    //                    {
    //                        //((Excel.Range)excelWorksheet.Cells[row, col]).Value2;
    //                        String tempstr = ((Excel.Range)excelWorksheet.Cells[row, col]).Value2.ToString();
    //                        dr[drCol] = tempstr.Trim();

    //                    }
    //                    else
    //                    {
    //                        dr[drCol] = " ";
    //                    }

    //                }
    //                if (tokenFlag == 0)
    //                {
    //                    dt.Rows.Add(dr);
    //                    dt.AcceptChanges();
    //                    totalcount++;
    //                }
    //                else
    //                {
    //                    string col1 = dr[0].ToString();
    //                    string col2 = dr[1].ToString();
    //                    string col3 = dr[2].ToString();
    //                    string col4 = dr[3].ToString();
    //                    string col5 = dr[4].ToString();
    //                    //string col6 = dr[5].ToString();
    //                    //string col7 = dr[6].ToString();

    //                    tokenFlag = 0;
    //                    //tempwords = null;

    //                }
    //            }
    //            #endregion
    //        }

    //        /////////////////TATA////////////////
    //        if (ratePlanFormat == 2)
    //        {
    //            if (currentsheet == 2)
    //            {
    //                #region Sheet2
    //                for (int row = c + 1; row <= ro; row++)
    //                {


    //                    if (((Excel.Range)excelWorksheet.Cells[row, 7]).Value2 != null)
    //                    {

    //                        string[] tempwords = null;
    //                        int drCol = 0;
    //                        count++;

    //                        String tempstr = ((Excel.Range)excelWorksheet.Cells[row, 7]).Value2.ToString();
    //                        string[] words = tempstr.Split(',');
    //                        splitcount = splitcount + words.Count();
    //                        for (int trow = 0; trow < words.Count(); trow++)
    //                        {
    //                            DataRow dr = dt.NewRow();
    //                            dr[0] = ((Excel.Range)excelWorksheet.Cells[row, 1]).Value2.ToString();
    //                            dr[1] = ((Excel.Range)excelWorksheet.Cells[row, 2]).Value2.ToString();
    //                            dr[2] = ((Excel.Range)excelWorksheet.Cells[row, 4]).Value2.ToString();
    //                            dr[3] = words[trow].ToString().Trim();
    //                            string tstr = dr[2].ToString().Trim() + dr[3].ToString().Trim();
    //                            dr[4] = tstr.ToString();
    //                            dr[5] = ((Excel.Range)excelWorksheet.Cells[row, 3]).Value2.ToString();
    //                            dr[6] = ((Excel.Range)excelWorksheet.Cells[row, 8]).Value2.ToString();

    //                            dt.Rows.Add(dr);
    //                            dt.AcceptChanges();
    //                            totalcount++;

    //                        }
    //                    }

    //                }
    //                #endregion
    //            }
    //            else if (currentsheet == 1)
    //            {
    //                #region Sheet1
    //                for (int row = c + 1; row <= ro; row++)
    //                {
    //                    DataRow dr = dt.NewRow();
    //                    string[] tempwords = null;
    //                    int drCol = 0;
    //                    count++;
    //                    for (int col = co; col <= b; col++, drCol++)
    //                    {
    //                        if (((Excel.Range)excelWorksheet.Cells[row, col]).Value2 != null)
    //                        {
    //                            //((Excel.Range)excelWorksheet.Cells[row, col]).Value2;
    //                            String tempstr = ((Excel.Range)excelWorksheet.Cells[row, col]).Value2.ToString();
    //                            string[] words = tempstr.Split(',');
    //                            splitcount = splitcount + words.Count();
    //                            if (words.Count() < 2)
    //                            {
    //                                if (drCol == 4)
    //                                {
    //                                    dr[drCol++] = dr[2].ToString().Trim() + dr[3].ToString().Trim();
    //                                }
    //                                dr[drCol] = tempstr.Trim();
    //                            }
    //                            else
    //                            {
    //                                tokenFlag = words.Count();
    //                                tempwords = words;
    //                            }
    //                        }

    //                    }
    //                    if (tokenFlag == 0)
    //                    {
    //                        dt.Rows.Add(dr);
    //                        dt.AcceptChanges();
    //                        totalcount++;
    //                    }
    //                    else
    //                    {
    //                        string col1 = dr[0].ToString();
    //                        string col2 = dr[1].ToString();
    //                        string col3 = dr[2].ToString();
    //                        string col4 = dr[3].ToString();
    //                        string col5 = dr[4].ToString();
    //                        string col6 = dr[5].ToString();
    //                        string col7 = dr[6].ToString();
    //                        string col8 = dr[7].ToString();
    //                        string col9 = dr[8].ToString();

    //                        if (col4 == "")
    //                        {
    //                            for (int i = 0; i < tempwords.Count(); i++)
    //                            {
    //                                dr[0] = col1;
    //                                dr[1] = col2;
    //                                dr[2] = col3.Trim();
    //                                dr[3] = tempwords[i].Trim();
    //                                dr[4] = dr[2].ToString() + dr[3].ToString();
    //                                dr[5] = col6;
    //                                dr[6] = col7;
    //                                dr[7] = col8;
    //                                dr[8] = col9;
    //                                dt.Rows.Add(dr);
    //                                dt.AcceptChanges();
    //                                dr = dt.NewRow();
    //                                totalcount++;
    //                            }
    //                        }
    //                        else if (col2 == "")
    //                        {
    //                            string strr = "";
    //                            for (int i = 0; i < tempwords.Count(); i++)
    //                            {

    //                                strr = strr + tempwords[i].Trim();

    //                            }
    //                            dr[0] = col1;
    //                            dr[1] = strr;
    //                            dr[2] = col3.Trim();
    //                            dr[3] = col4.Trim();
    //                            dr[4] = dr[2].ToString() + dr[3].ToString();
    //                            dr[5] = col6;
    //                            dr[6] = col7;
    //                            dr[7] = col8;
    //                            dr[8] = col9;
    //                            dt.Rows.Add(dr);
    //                            dt.AcceptChanges();
    //                            dr = dt.NewRow();
    //                            totalcount++;
    //                        }
    //                        tokenFlag = 0;
    //                        tempwords = null;

    //                    }
    //                }
    //                #endregion
    //            }
    //        }

    //        #endregion


    //        //MessageBox.Show("ok" + dt.Rows.Count.ToString() + " Count=" + count.ToString() + " total=" + totalcount.ToString() + " StringCount=" + splitcount.ToString());

    //    }
    //    catch (Exception exep)
    //    {
    //        // MessageBox.Show(exep.Message);
    //        dt = null;
    //    }
    //    finally
    //    {
    //        if (newWorkBook != null)
    //            newWorkBook.Close(false, Missing.Value, Missing.Value);
    //        if (excelApp != null)
    //            excelApp.Quit();
    //    }

    //    return dt;
    //}




    //rate import functions&&&&&&&&&&&ENDs&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&






    

    protected void EntityDataRateTask_QueryCreated(object sender, QueryCreatedEventArgs e)
    {

    }//query created

    protected void ListView1_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //select (select -1) as status ,(select 'Total') as Description,count(*) as cnt from ratetask where idrateplan=1 union all
        //select (select 100) as status,(select 'Error') as Description,count(*) as cnt from ratetask where idrateplan=1 and field2>0 union all
        //select (select 0) as status  ,(select 'Uncommitted') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=0 union all
        //select (select 1) as status  ,(select 'Code Delete') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=1 union all
        //select (select 2) as status  ,(select 'New') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=2 union all
        //select (select 3) as status  ,(select 'Increase') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=3 union all
        //select (select 4) as status  ,(select 'Decrease') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=4 union all
        //select (select 5) as status  ,(select 'Unchanged') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=5 union all
        //select (select 7) as status  ,(select 'Committed') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=7 union all
        //select (select 9) as status  ,(select 'Overlap') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=8 union all
        //select (select 10) as status  ,(select 'Overlap Adjusted') as Description,count(*) as cnt from ratetask where idrateplan=1 and field1=8;

        switch (int.Parse(e.CommandArgument.ToString()))
        {
            case 0://uncommitted
                this.DropDownListMoreFilters.SelectedValue = "8";
                break;
            case 100://error
                this.DropDownListMoreFilters.SelectedValue = "0";
                break;
            default:
                this.DropDownListMoreFilters.SelectedValue = e.CommandArgument.ToString();
                break;
        }
        ButtonFindPrefix_Click(sender, e);
    }


    protected void DropDownListTaskRef_SelectedIndexChanged(object sender, EventArgs e)
    {
        MyGridViewDataBind();
    }

    protected void Supersede_CheckedExceptChange(object sender, EventArgs e)
    {
        CheckBox superSedeExcept = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDeleteExcept");
        CheckBox superSedeAll = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDefaultDeleteDate");
        CheckBox countryWise = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWise");
        CheckBox countryWiseMin = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWiseMinDate");
        if (superSedeExcept.Checked == true)
        {
            countryWise.Checked = false;
            countryWiseMin.Checked = false;
            superSedeAll.Checked = false;
        }

    }


    protected void Supersede_CheckedChange(object sender, EventArgs e)
    {
        CheckBox superSedeExcept = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDeleteExcept");
        CheckBox superSedeAll = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDefaultDeleteDate");
        CheckBox countryWise = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWise");
        CheckBox countryWiseMin = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWiseMinDate");
        if (superSedeAll.Checked == true)
        {
            superSedeExcept.Checked = false;
            countryWise.Checked = false;
            countryWiseMin.Checked = false;
        }

    }

        protected void CountryWise_CheckedChange(object sender, EventArgs e)
    {
        CheckBox superSedeExcept = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDeleteExcept");
        CheckBox superSedeAll = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDefaultDeleteDate");
        CheckBox countryWise = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWise");
        CheckBox countryWiseMin = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWiseMinDate");
        if (countryWise.Checked == true)
        {
            superSedeAll.Checked = false;
            superSedeExcept.Checked = false;
            countryWiseMin.Checked = false;
        }
    }

    protected void CountryWiseMin_CheckedChange(object sender, EventArgs e)
    {
        CheckBox superSedeExcept = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDeleteExcept");
        CheckBox superSedeAll = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDefaultDeleteDate");
        CheckBox countryWise = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWise");
        CheckBox countryWiseMin = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWiseMinDate");
        if (countryWiseMin.Checked == true)
        {
            superSedeAll.Checked = false;
            superSedeExcept.Checked = false;
            countryWise.Checked = false;
        }
    }
    

    protected class CountryWiseDelDate
    {
        public string CountryCode { get; set; }
        public string StartDate { get; set; }
        public override string ToString()
        {
            return "Delete:" + this.CountryCode;
        }
    }
    protected void frmCodeDelete_ItemInserting(object sender, FormViewInsertEventArgs e)
    {
        Label lblStatus = (Label) this.FormViewCodeDelete.FindControl("lblCodeDeleteStatus");
        lblStatus.Text = "";
        CheckBox superSedeExcept = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDeleteExcept");
        CheckBox superSedeAll = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDefaultDeleteDate");
        CheckBox countryWise = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWise");
        CheckBox countryWiseMin = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWiseMinDate");
        TextBox txtStartDate= (TextBox) this.FormViewCodeDelete.FindControl("TextBoxDefaultDeleteDate");
        TextBox txtStartTime = (TextBox) this.FormViewCodeDelete.FindControl("TextBoxDefaultDeleteTime");
        string strStartDate = txtStartDate.Text + " " + txtStartTime.Text;
        int idTaskReference = int.Parse(this.DropDownListTaskRef.SelectedValue);
        long timeZoneDifference = (long) this.ViewState["vsTimeZoneDifference"];
        if (superSedeAll.Checked == false && countryWise.Checked == false && superSedeExcept.Checked==false&&countryWiseMin.Checked==false)
        {
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = "Choose one option";
            e.Cancel = true;
            return;
        }
        else if (superSedeAll.Checked == true)
        {
            DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
            if (DateTime.TryParseExact(strStartDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate) == false)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "Invalid Effective Date/Time for Code Deletion!";
                e.Cancel = true;
                return;
            }
            ratetask delTask = CreateNewRateTask(idTaskReference, "", "*", "End All Codes", "-1", "1", "1", "", dstartdate.ToString("yyyy-MM-dd HH:mm:ss"),"", "0", "0", "", "", "", "", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", -1, 0, "0", "0", "1", "1", "0");
            List<string> lstMultipleInsertExt = new List<string>();
            if(this.CheckBoxAutoConvertTZ.Checked==true) delTask.AdjustDateTimeToNativeTimeZone(timeZoneDifference);
            lstMultipleInsertExt.Add(delTask.GetExtendedInsertSql());
            string insertHeader = "insert into ratetask (Prefix,description,rateamount,WeekDayStart,WeekDayEnd,starttime,endtime,Resolution,MinDurationSec,SurchargeTime,SurchargeAmount,idrateplan,CountryCode,date1,field1,field2,field3,field4,field5,startdate,enddate,Inactive,RouteDisabled,Type,Currency,OtherAmount1,OtherAmount2,OtherAmount3,OtherAmount4,OtherAmount5,OtherAmount6,OtherAmount7,OtherAmount8,OtherAmount9,OtherAmount10,TimeZoneOffsetSec,RatePosition,IgwPercentageIn,ConflictingRateIds,ChangedByTaskId,ChangedOn,Status,idPreviousRate,EndPreviousRate,Category,SubCategory,changecommitted,OverlappingRates,ConflictingRates,AffectedRates,PartitionDate,Comment1,Comment2,ExecutionOrder,RateAmountRoundupDecimal) values ";
            using (PartnerEntities context = new PartnerEntities())
            {
                context.Database.ExecuteSqlCommand(insertHeader + string.Join(",", lstMultipleInsertExt));
                //Context.ratetasks.Add(DelTask);
                //Context.SaveChanges();
            }
            //myGridViewDataBind();
            //ResetFormViewCodeDelete(sender, e);
            this.Response.Redirect("ratetask.aspx" + (string) this.ViewState["vsQueryString"]);//mygrid viewbind wasn't refreshig grid
        }
        else if(superSedeExcept.Checked==true)
        {
            DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
            if (DateTime.TryParseExact(strStartDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate) == false)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "Invalid Effective Date/Time for Code Deletion!";
                e.Cancel = true;
                return;
            }


            long idRatePlan = Convert.ToInt64(this.ViewState["task.sesidRatePlan"]);
            using (PartnerEntities context = new PartnerEntities())
            {
                List<string> lstCurrentPrefixes = context.ratetasks.Where(c => c.idrateplan == idTaskReference && c.rateamount != "-1" && c.changecommitted != 1).Select(c => c.Prefix).ToList();
                if(lstCurrentPrefixes==null || lstCurrentPrefixes.Count==0)
                {
                    lblStatus.ForeColor = Color.Red;
                    lblStatus.Text = "At least one Ratetask is required in current task other than Code Delete Type!";
                    e.Cancel = true;
                    return;
                }
                List<string> lstOpenPrefixes = context.rates.Where(c => c.idrateplan == idRatePlan && c.enddate == null).Select(c => c.Prefix).ToList();
                List<string> lstMultipleInsertExt = new List<string>();
                foreach (string prefix in lstOpenPrefixes)
                {
                    if (lstCurrentPrefixes.Contains(prefix) == false)
                    {
                        ratetask delTask = CreateNewRateTask(idTaskReference, "", prefix, "End One Code", "-1", "1", "1", "", dstartdate.ToString("yyyy-MM-dd HH:mm:ss"), "", "0", "0", "", "", "", "", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", -1, 0, "0", "0", "1", "1", "0");
                        if (this.CheckBoxAutoConvertTZ.Checked == true) delTask.AdjustDateTimeToNativeTimeZone(timeZoneDifference);
                        lstMultipleInsertExt.Add(delTask.GetExtendedInsertSql());
                    }
                }

                if (lstMultipleInsertExt.Count > 0)
                {
                    string insertHeader = "insert into ratetask (Prefix,description,rateamount,WeekDayStart,WeekDayEnd,starttime,endtime,Resolution,MinDurationSec,SurchargeTime,SurchargeAmount,idrateplan,CountryCode,date1,field1,field2,field3,field4,field5,startdate,enddate,Inactive,RouteDisabled,Type,Currency,OtherAmount1,OtherAmount2,OtherAmount3,OtherAmount4,OtherAmount5,OtherAmount6,OtherAmount7,OtherAmount8,OtherAmount9,OtherAmount10,TimeZoneOffsetSec,RatePosition,IgwPercentageIn,ConflictingRateIds,ChangedByTaskId,ChangedOn,Status,idPreviousRate,EndPreviousRate,Category,SubCategory,changecommitted,OverlappingRates,ConflictingRates,AffectedRates,PartitionDate,Comment1,Comment2,ExecutionOrder,RateAmountRoundupDecimal) values ";
                    context.Database.ExecuteSqlCommand(insertHeader + string.Join(",", lstMultipleInsertExt));
                }
                
            }
            //myGridViewDataBind();
            //ResetFormViewCodeDelete(sender, e);
            this.Response.Redirect("ratetask.aspx" + (string) this.ViewState["vsQueryString"]);//mygrid viewbind wasn't refreshig grid
        }
        else if (countryWise.Checked == true || countryWiseMin.Checked == true)
        {
            DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
            if (DateTime.TryParseExact(strStartDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate) == false)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "Invalid Effective Date/Time for Code Deletion!";
                e.Cancel = true;
                return;
            }
            using (PartnerEntities context = new PartnerEntities())
            {
                string sql = "select countrycode as CountryCode, min(startdate) StartDate from ratetask where idrateplan=" + idTaskReference.ToString() +
                    " and countrycode is not null and countrycode!='' and changecommitted!=1 group by countrycode ";
                List<CountryWiseDelDate> lstDelCountry = context.Database.SqlQuery<CountryWiseDelDate>(sql).ToList();
                if(lstDelCountry.Count==0)
                {
                    lblStatus.ForeColor = Color.Red;
                    lblStatus.Text = "No uncommitted ratetask found with Country Code!";
                    e.Cancel = true;
                    return;
                }
                List<string> lstMultipleInsertExt = new List<string>();
                
                foreach (CountryWiseDelDate cw in lstDelCountry)
                {
                    string strDelDate = countryWise.Checked == true ? dstartdate.ToString("yyyy-MM-dd HH:mm:ss") : cw.StartDate;
                    ratetask delTask = CreateNewRateTask(idTaskReference, "",(cw.CountryCode+ "*"), "End "+ (cw.CountryCode + "*"), "-1", "1", "1", "", strDelDate, "", "0", "0", "", "", "", "", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", -1, 0, "0", "0", "1", "1", "0");
                    //**no need to adjust tz for countrywise min, as the tasks are already saved in db with tz adjusted
                    if(countryWiseMin.Checked==false)
                    {

                        if (this.CheckBoxAutoConvertTZ.Checked == true) delTask.AdjustDateTimeToNativeTimeZone(timeZoneDifference);
                    }
                        
                    lstMultipleInsertExt.Add(delTask.GetExtendedInsertSql());
                }
                if (lstMultipleInsertExt.Count > 0)
                {
                    string insertHeader = "insert into ratetask (Prefix,description,rateamount,WeekDayStart,WeekDayEnd,starttime,endtime,Resolution,MinDurationSec,SurchargeTime,SurchargeAmount,idrateplan,CountryCode,date1,field1,field2,field3,field4,field5,startdate,enddate,Inactive,RouteDisabled,Type,Currency,OtherAmount1,OtherAmount2,OtherAmount3,OtherAmount4,OtherAmount5,OtherAmount6,OtherAmount7,OtherAmount8,OtherAmount9,OtherAmount10,TimeZoneOffsetSec,RatePosition,IgwPercentageIn,ConflictingRateIds,ChangedByTaskId,ChangedOn,Status,idPreviousRate,EndPreviousRate,Category,SubCategory,changecommitted,OverlappingRates,ConflictingRates,AffectedRates,PartitionDate,Comment1,Comment2,ExecutionOrder,RateAmountRoundupDecimal) values ";
                    context.Database.ExecuteSqlCommand(insertHeader + string.Join(",", lstMultipleInsertExt));
                }
                    
            }
            //myGridViewDataBind();
            //ResetFormViewCodeDelete(sender, e);
            this.Response.Redirect("ratetask.aspx" + (string) this.ViewState["vsQueryString"]);//mygrid viewbind wasn't refreshig grid
        }
    }

    private void ResetFormViewCodeDelete(object sender, EventArgs e)
    {
        CheckBox superSedeExcept = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDeleteExcept");
        CheckBox superSedeAll = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDefaultDeleteDate");
        CheckBox countryWise = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWise");
        TextBox txtDefaultDeleteDate = (TextBox) this.FormViewCodeDelete.FindControl("TextBoxDefaultDeleteDate");
        TextBox txtDefaultDeleteTime = (TextBox) this.FormViewCodeDelete.FindControl("TextBoxDefaultDeleteTime");
        txtDefaultDeleteDate.Text = this.TextBoxDefaultDate.Text;
        txtDefaultDeleteTime.Text = this.TextBoxDefaultTime.Text;
        superSedeExcept.Checked = true;
        superSedeAll.Checked = false;
        countryWise.Checked = false;
        FormViewCodeDeleteCancel_Click(sender, e);
    }

    protected void FormViewCodeDeleteCancel_Click(object sender, EventArgs e)
    {
        Label lblStatus = (Label) this.FormViewCodeDelete.FindControl("lblCodeDeleteStatus");
        lblStatus.Text = "";
        CheckBox superSedeAll = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxDefaultDeleteDate");
        CheckBox countryWise = (CheckBox) this.FormViewCodeDelete.FindControl("CheckBoxCountryWise");

        this.FormViewCodeDelete.Visible = false;
    }


    protected void GridViewSupplierRates_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {

        long id = (long) this.GridViewSupplierRates.DataKeys[e.RowIndex].Value;
        using (PartnerEntities context = new PartnerEntities())
        {
            context.Database.ExecuteSqlCommand("delete from ratetask where id=" + id.ToString());
        }
        this.Response.Redirect("ratetask.aspx" + (string) this.ViewState["vsQueryString"]);//mygrid viewbind wasn't refreshig grid
    }
    protected void LinkButtonCodeDelete_Click(object sender, EventArgs e)
    {
        this.FormViewCodeDelete.DataBind();
        this.FormViewCodeDelete.ChangeMode(FormViewMode.Insert);
        this.FormViewCodeDelete.Visible = true;
        TextBox txtDefaultDeleteDate = (TextBox) this.FormViewCodeDelete.FindControl("TextBoxDefaultDeleteDate");
        TextBox txtDefaultDeleteTime = (TextBox) this.FormViewCodeDelete.FindControl("TextBoxDefaultDeleteTime");
        txtDefaultDeleteDate.Text = this.TextBoxDefaultDate.Text;
        txtDefaultDeleteTime.Text = this.TextBoxDefaultTime.Text;
    }
    protected void ButtonExport_Click(object sender, EventArgs e)
    {
        
        int idTaskReference = int.Parse(this.DropDownListTaskRef.SelectedValue);
        int idRatePlan = (int) this.ViewState["task.sesidRatePlan"];
        string ratePlanName = "Not Found";
        List<ratetask> lstRateTask = new List<ratetask>();
        List<ratetask.DisplayClass> lstRtDisplay = new List<ratetask.DisplayClass>();
        //get DisplayClassFirst
        string extensionDirectory = (Directory.GetParent(HttpRuntime.BinDirectory)).Parent.FullName + Path.DirectorySeparatorChar
                        + "Extensions";
        RouteDisplayClassData dcMefData = new RouteDisplayClassData();
        dcMefData.Composer.Compose(extensionDirectory);
        foreach (IDisplayClass ext in dcMefData.Composer.DisplayClasses)
        {
            dcMefData.DicExtensions.Add(ext.Id.ToString(), ext);
        }
        IDisplayClass thisDisplayClass = dcMefData.DicExtensions["2"];
        using (PartnerEntities context = new PartnerEntities())
        {
            ratePlanName = context.rateplans.Where(c => c.id == idRatePlan).First().RatePlanName;
            lstRateTask = context.ratetasks.Where(c => c.idrateplan == idTaskReference).ToList();
        }
        foreach(ratetask rt in lstRateTask)
        {
            lstRtDisplay.Add((ratetask.DisplayClass)thisDisplayClass.GetDisplayClass(rt));
        }
        lstRateTask = null;
        GC.Collect();
        CreateExcelFileAspNet.CreateExcelDocumentAndWriteBrowser(lstRtDisplay,ratePlanName+ ".xlsx", this.Response);

    }
}