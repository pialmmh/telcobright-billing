using LcrJob;
using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using LibraryExtensions;
using ExportToExcel;
using System.Reflection;
using OfficeOpenXml;
using MediationModel;
using PortalApp;
using MySql.Data.MySqlClient;
using TelcobrightMediation.Config;
using Color = System.Drawing.Color;

public partial class config_lcr : Page
{

    override protected void OnInit(EventArgs e)
    {
        Load += new EventHandler(Page_Load);
        base.OnInit(e);
    }

    protected void frmSupplierRatePlanInsert_ItemInserting(object sender, FormViewInsertEventArgs e)
    {
        try
        {
            DropDownList ddlistFrmRatePlan = (DropDownList) this.frmInitLCR.FindControl("ddlistFrmRatePlan");
            string RateplanName = ddlistFrmRatePlan.SelectedItem.Text;
            int idRatePlan = -1;
            long idNewLcrRatePlan = 0;
            if (int.TryParse(ddlistFrmRatePlan.SelectedValue, out idRatePlan) == false)
            {
                throw new Exception("Invalid Rateplan.");
            }

            string newActiveDate = ((TextBox) this.frmInitLCR.FindControl("txtDateFrm")).Text;
            string newActiveTime = ((TextBox) this.frmInitLCR.FindControl("TextBoxTime")).Text;
            newActiveDate += " " + newActiveTime;
            DateTime ActivedateTime;
            if (DateTime.TryParseExact(newActiveDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out ActivedateTime) == false)
            {
                throw new Exception("Invalid Effective Datetime");
            }



            using (PartnerEntities Conpartner = new PartnerEntities())
            {
                //same rateplan cannot be initialized at the same existing time, check first
                bool Exists = Conpartner.lcrrateplans.Any(c => c.idRatePlan == idRatePlan && c.StartDate == ActivedateTime);
                if (Exists == true)
                {
                    throw new Exception("Rateplan:" + RateplanName + " has been initialized already at the specified datetime:" + ActivedateTime.ToString("yyyy-MM-dd HH:mm:ss") + ". " + Environment.NewLine +
                        "Please select a different datetime to initialize this Rateplan.");
                }
                lcrrateplan lplan = new lcrrateplan();
                lplan.idRatePlan = idRatePlan;
                lplan.StartDate = ActivedateTime;
                lplan.Description = RateplanName + "/" + ActivedateTime.ToString("yyyy-MM-dd HH:mm:ss");
                Conpartner.lcrrateplans.Add(lplan);
                Conpartner.SaveChanges();
                idNewLcrRatePlan = Conpartner.lcrrateplans.Where(c => c.Description == lplan.Description && c.JobCreated != 1).Select(c => c.id).First();
            }
            //get any ne of this telcobright partner, required by rate handling objects
            string conStrPartner = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;
            string dbNameAppConf = "";
            foreach (string param in conStrPartner.Split(';'))
            {
                if (param.ToLower().Contains("database"))
                {
                    dbNameAppConf = param.Split('=')[1].Trim('"');
                    break;
                }
            }
            ne thisNe = null;
            using (MySqlConnection conPartner= new MySqlConnection(ConfigurationManager.ConnectionStrings["Partner"].ConnectionString))
            {
                conPartner.Open();
                using (PartnerEntities context = new PartnerEntities(conPartner,false))
                {
                    thisNe = context.telcobrightpartners.First(c => c.databasename == dbNameAppConf).nes.First();
                    //tbConfig = ConTelco.telcobrightconfigs.ToList().First();
                }
                JcLcr LCRJobCreator = new JcLcr();
                string configFileName = string.Join(Path.DirectorySeparatorChar.ToString(),
                                            (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase))
                                            .Split(Path.DirectorySeparatorChar)
                                            .Where(c => c.Contains("file") == false)) +
                                        Path.DirectorySeparatorChar.ToString() + dbNameAppConf + ".conf";
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromFile(configFileName);
                string Jobname = LCRJobCreator.InitNewLcrRatePlan(thisNe, idNewLcrRatePlan,conPartner);
                this.lblStatus.ForeColor = Color.Green;
                this.lblStatus.Text = " LCR job " + Jobname + " has been created, LCR for this rate plan will be updated shortly.";
            }
        }
        catch (Exception e1)
        {
            this.lblStatus.ForeColor = Color.Red;
            this.lblStatus.Text = e1.Message + " " + (e1.InnerException != null ? e1.InnerException.ToString() : "");
        }
    }

    public DateTime FirstDayOfMonthFromDateTime(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    public DateTime LastDayOfMonthFromDateTime(DateTime dateTime)
    {
        DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
        return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
    }

    protected void DropDownListMonth_SelectedIndexChanged(object sender, EventArgs e)
    {
        //select 15th of month to find out first and last day of a month as it exists in all months.
        DateTime AnyDayOfMonth = new DateTime(int.Parse(this.TextBoxYear.Text), int.Parse(this.DropDownListMonth.SelectedValue), 15);
        this.txtDate.Text = FirstDayOfMonthFromDateTime(AnyDayOfMonth).ToString("yyyy-MM-dd");
    }
    protected void DropDownListMonth1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //select 15th of month to find out first and last day of a month as it exists in all months.
        DateTime AnyDayOfMonth = new DateTime(int.Parse(this.TextBoxYear1.Text), int.Parse(this.DropDownListMonth1.SelectedValue), 15);
        this.txtDate1.Text = LastDayOfMonthFromDateTime(AnyDayOfMonth).AddDays(1).ToString("yyyy-MM-dd");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            //Retrieve Path from TreeView for displaying in the master page caption label

            TreeView MasterTree = (TreeView) this.Master.FindControl("TreeView1");
            CommonCode CommonCodes = new CommonCode();
            CommonCodes.LoadReportTemplatesTree(ref MasterTree);

            string LocalPath = this.Request.Url.LocalPath;
            int Pos2ndSlash = LocalPath.Substring(1, LocalPath.Length - 1).IndexOf("/");
            string Root_Folder = LocalPath.Substring(1, Pos2ndSlash);
            int EndOfRootFolder = this.Request.Url.AbsoluteUri.IndexOf(Root_Folder);
            string UrlWithQueryString = ("~" + "/" + Root_Folder + this.Request.Url.AbsoluteUri.Substring((EndOfRootFolder + Root_Folder.Length), this.Request.Url.AbsoluteUri.Length - (EndOfRootFolder + Root_Folder.Length))).Replace("%20", " ");

            TreeNodeCollection cNodes = MasterTree.Nodes;
            TreeNode MatchedNode = null;
            foreach (TreeNode N in cNodes)//for each nodes at root level, loop through children
            {
                MatchedNode = CommonCodes.RetrieveNodes(N, UrlWithQueryString);
                if (MatchedNode != null)
                {
                    break;
                }
            }
            //set screentile/caption in the master page...
            Label lblScreenTitle = (Label) this.Master.FindControl("lblScreenTitle");
            if (MatchedNode != null)
            {
                lblScreenTitle.Text = MatchedNode.ValuePath;
            }
            else
            {
                lblScreenTitle.Text = "";
            }
            this.TextBoxYear.Text = DateTime.Now.Year.ToString();
            this.TextBoxYear1.Text = DateTime.Now.Year.ToString();

            //End of Site Map Part *******************************************************************


            this.txtDate.Text = DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd");
            this.txtDate1.Text = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            this.ViewState["qstring"] = UrlWithQueryString;//save because a re-direct is needed because gridview is not
            //refreshing after delete all rates...sucks.

            //load rate plans which are configured for LCR
            using (PartnerEntities conpartner = new PartnerEntities())
            {
                List<rateplan> lstLCRable = conpartner.rateplans.Where(c => c.field3==1).ToList();//field3=lcr ref flag
                DropDownList ddListFrmRatePlan = (DropDownList) this.frmInitLCR.FindControl("ddlistFrmRatePlan");

                this.DropDownListRatePlanLcr.Items.Clear();
                ddListFrmRatePlan.Items.Clear();
                foreach (rateplan rp in lstLCRable)
                {
                    this.DropDownListRatePlanLcr.Items.Add(new ListItem(rp.RatePlanName, rp.id.ToString()));
                    ddListFrmRatePlan.Items.Add(new ListItem(rp.RatePlanName, rp.id.ToString()));
                }

                this.DropDownListRatePlanLcr.DataBind();
                ddListFrmRatePlan.DataBind();
            }

            this.DropDownListViewMode.SelectedIndex = 2;


        }//if !postback
    }





    private class PrefixVsMaxId
    {
        public string Prefix { get; set; }
        public long MaxId { get; set; }
    }
    private string GetFullLCRCurrentIDs(int idRatePlan, string PrefixFilter)
    {
        string Sql = " select prefix as Prefix,max(id) as MaxId from lcr where idrateplan=" + idRatePlan +
                    "  and prefix like '" + (PrefixFilter == "" ? "%" : (PrefixFilter.EndsWith("*") == true ? (PrefixFilter.Replace("*", "") + "%") : PrefixFilter)) + "'" +
                    "  group by prefix; ";
        List<PrefixVsMaxId> lstPMax = null;
        using (PartnerEntities Context = new PartnerEntities())
        {
            lstPMax = Context.Database.SqlQuery<PrefixVsMaxId>(Sql).ToList();
        }
        if (lstPMax.Count > 0)
        {
            return string.Join(",", lstPMax.Select(c => c.MaxId.ToString()).ToList());
        }
        return "";
    }
    private List<List<string>> SegmentedGroupSql(int idRatePlan, int limit, string Prefix)//retuns segmented Sql in batch size of 100, to avoid memory running out
    {
        using (PartnerEntities context = new PartnerEntities())
        {

            List<string> lstPrefix = null;
            if (Prefix == "")
            {
                lstPrefix = context.rates.Where(c => c.idrateplan == idRatePlan).Select(c => c.Prefix).ToList();
            }
            else
            {
                if (Prefix.EndsWith("*") == false)
                {
                    lstPrefix = context.rates.Where(c => c.idrateplan == idRatePlan && c.Prefix == Prefix).Select(c => c.Prefix).ToList();
                }
                else//contains wild card * at the end
                {
                    string PrefixWithoutAsterisk = Prefix.Replace("*", "");
                    lstPrefix = lstPrefix.Where(c => c.StartsWith(PrefixWithoutAsterisk)).ToList();
                }
            }

            List<string> lstGroupByPrefix = new List<string>();
            foreach (string Pr in lstPrefix)
            {
                lstGroupByPrefix.Add(" (select * from lcr where idrateplan=" + idRatePlan + " and prefix=" + Pr.EncloseWith("'")
                    + " order by id desc limit 0," + limit + ") ");
            }
            return SegmentList(lstGroupByPrefix, 100);//segment size=100

        }
    }

    private List<List<string>> SegmentList(List<string> lstGroupBy, int Segment)
    {
        List<List<string>> lstLstString = new List<List<string>>();
        List<string> CurrentList = new List<string>();
        for (int i = 0; i < lstGroupBy.Count; i++)
        {
            CurrentList.Add(lstGroupBy[i]);
            if ((i + 1) % Segment == 0)
            {
                lstLstString.Add(CurrentList);
                CurrentList = new List<string>();
            }
        }
        if (CurrentList.Count > 0)
        {
            lstLstString.Add(CurrentList);
        }
        return lstLstString;
    }

    protected void DropDownListPartner_SelectedIndexChanged(Object sender, EventArgs e)
    {

    }

    protected void ddlservice_SelectedIndexChanged(Object sender, EventArgs e)
    {

    }

    protected void ddlViewMode_SelectedIndexChanged(Object sender, EventArgs e)
    {

    }


    protected void LinkButtonDelete_Click(object sender, EventArgs e)
    {

    }//submit click

    protected void Button1_Click(object sender, EventArgs e)
    {

    }

    public static void ExportToSpreadsheet(DataTable table, string name, List<string> ColNameList, List<int> ColumnSortlist)
    {

    }



    protected void CheckBoxShowByPartner_CheckedChanged(object sender, EventArgs e)
    {

    }


    public void dateInitialize()
    {
        //else
        //{
        //    txtDate.Text = FirstDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
        //    txtDate1.Text = LastDayOfMonthFromDateTime(System.DateTime.Now).ToString("dd/MM/yyyy");
        //}
    }


    protected void GridViewLCR_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        try
        {
            this.GridViewLCR.PageIndex = e.NewPageIndex;
            List<LcrDisplayClass> lstLcrDisplay = GetLcr();
            MyGridViewDataBound(lstLcrDisplay);
        }
        catch (Exception e1)
        {
            this.lblStatus.Text = e1.Message + "<br/>" + e1.InnerException;
            this.lblStatus.ForeColor = Color.Red;
        }
    }
    protected class CostWiseLCREntry
    {
        public double Cost { get; set; }
        public List<RoutePartnerPair> lstRouteCarrierPair { get; set; }
        
        public CostWiseLCREntry(double cost, List<RoutePartnerPair> lstRCP)
        {
            this.Cost = cost;
            this.lstRouteCarrierPair = lstRCP;
        }
    }


    protected void ListViewLcr_ItemDataBound(Object sender, ListViewItemEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            GridView gvRouteCost = (GridView)e.Item.FindControl("GridViewRouteCost");
            List<RoutePartnerPair> lstRouteCarrierPair = (List<RoutePartnerPair>)DataBinder.Eval(e.Item.DataItem, "lstRouteCarrierPair");
            gvRouteCost.DataSource = lstRouteCarrierPair;
            gvRouteCost.DataBind();
        }
    }

    protected void GridViewLCR_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            ListView lvLcr = (ListView)e.Row.FindControl("ListViewLcr");
            Dictionary<string, List<RoutePartnerPair>> dicCostEntities = (Dictionary<string, List<RoutePartnerPair>>)(DataBinder.Eval(e.Row.DataItem, "dicCostEntities"));
            List<CostWiseLCREntry> lstCost = GetCostWiseEntry(dicCostEntities);
            lvLcr.DataSource = lstCost;
            lvLcr.DataBind();
        }
    }
    protected List<CostWiseLCREntry> GetCostWiseEntry(Dictionary<string, List<RoutePartnerPair>> dicCostEntities)
    {
        List<CostWiseLCREntry> lstCost = new List<CostWiseLCREntry>();
        foreach (KeyValuePair<string, List<RoutePartnerPair>> kv in dicCostEntities)
        {
            string Cost = kv.Key;
            double Tempdbl = 0;
            double.TryParse(Cost, out Tempdbl);
            lstCost.Add(new CostWiseLCREntry(Tempdbl, kv.Value));
        }
        return lstCost.OrderBy(c => c.Cost).ToList();
    }
    protected void ButtonExport_Click(object sender, EventArgs e)
    {

        ExportLCR();

    }

    protected void ButtonFind_Click(object sender, EventArgs e)
    {
        List<LcrDisplayClass> lstLcrDisplay = GetLcr();
        MyGridViewDataBound(lstLcrDisplay);

    }

    void MyGridViewDataBound(List<LcrDisplayClass> lstLcrDisplay)
    {
        try
        {

            if (lstLcrDisplay.Count == 0)
            {
                this.lblStatus.Text = "No Records !";
                this.ButtonExport.Visible = false;
            }
            else
            {
                this.GridViewLCR.DataSource = lstLcrDisplay;
                this.GridViewLCR.DataBind();
                this.lblStatus.Text = lstLcrDisplay.Count + " Records Found.";
                this.ButtonExport.Visible = true;
            }
        }
        catch (Exception e1)
        {
            this.lblStatus.Text = e1.Message + "<br/>" + e1.InnerException;
            this.lblStatus.ForeColor = Color.Red;
        }
    }
    
    private class PrefixVsLowestRate
    {
        public string prefix { get; set; }
        public double LowestRate { get; set; }
    }
    public List<LcrDisplayClass> GetLcr()
    {
        List<LcrDisplayClass> lstLCRDisplay = new List<LcrDisplayClass>();
        if (this.DropDownListRatePlanLcr.Items.Count == 0)
        {
            this.lblStatus.Text = "No Rateplan Assigned for LCR!";
            return lstLCRDisplay;
        }
        List<lcr> lstLCR = new List<lcr>();
        int idRatePlan = Convert.ToInt32(this.DropDownListRatePlanLcr.SelectedValue);
        int ViewMode = Convert.ToInt32(this.DropDownListViewMode.SelectedValue);
        string Prefix = this.TextBoxPrefix.Text.Trim();
        string Sql = " select * from lcr where idrateplan=" + idRatePlan;
        string strIds = "";
        switch (ViewMode)
        {
            case 1://inc by date
                string startdate = this.txtDate.Text;
                string enddate = this.txtDate1.Text;

                DateTime dstartdate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
                DateTime denddate = Convert.ToDateTime("1800-01-01", CultureInfo.InvariantCulture);
                DateTime comparedate = dstartdate;

                if (startdate.Length == 10)
                {
                    DateTime.TryParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate);
                }
                else if (startdate.Length > 10)
                {
                    DateTime.TryParseExact(startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dstartdate);
                }

                if (enddate.Length == 10)
                {
                    DateTime.TryParseExact(enddate + " 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out denddate);
                }
                else if (enddate.Length > 10)
                {
                    DateTime.TryParseExact(enddate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out denddate);
                }

                if (dstartdate > comparedate && denddate > comparedate)
                {
                    startdate = dstartdate.ToString("yyyy-MM-dd HH:mm:ss");
                    enddate = denddate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    throw new Exception("Invalid Date!");
                }

                Sql += " and startdate>=" + startdate.EncloseWith("'") + " and startdate < " + enddate.EncloseWith("'");
                Sql += " and prefix like '" + (Prefix == "" ? "%" : (Prefix.EndsWith("*") == true ? (Prefix.Replace("*", "") + "%") : Prefix)) + "' ";

                break;
            case 2://inc by id
                int StartId = 0;
                int EndId = 0;
                if (Int32.TryParse(this.TextBoxStartId.Text, out StartId))
                {
                    throw new Exception("Start Id not in correct format");
                }
                if (Int32.TryParse(this.TextBoxEndId.Text, out EndId))
                {
                    throw new Exception("End Id not in correct format");
                }
                Sql += " and id>= " + StartId;
                Sql += " and id<= " + EndId;
                Sql += " and prefix like '" + (Prefix == "" ? "%" : (Prefix.EndsWith("*") == true ? (Prefix.Replace("*", "") + "%") : Prefix)) + "' ";

                break;
            case 3://full(current)
                strIds = GetFullLCRCurrentIDs(idRatePlan, Prefix);
                Sql += " and id in( " + strIds + ")";
                break;
            case 4://full+history (last 3)
                List<List<string>> lstLstSql = SegmentedGroupSql(idRatePlan, 1, Prefix);//latest instance only
                using (PartnerEntities ConPartner = new PartnerEntities())
                {
                    foreach (List<string> lstSQl in lstLstSql)
                    {
                        Sql = string.Join(" union all ", lstSQl);
                        lstLCR.AddRange(ConPartner.lcrs.SqlQuery(Sql).ToList());
                    }
                }
                break;
        }//switch


        //get any ne of this telcobright partner, required by rate handling objects
        string ConStrPartner = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;
        string DBNameAppConf = "";
        foreach (string Param in ConStrPartner.Split(';'))
        {
            if (Param.ToLower().Contains("database"))
            {
                DBNameAppConf = Param.Split('=')[1].Trim('"');
                break;
            }
        }
        ne ThisNE = null;
        //telcobrightconfig tbc = null;
        using (PartnerEntities ConTelco = new PartnerEntities())
        {
            ThisNE = ConTelco.telcobrightpartners.Where(c => c.databasename == DBNameAppConf).First().nes.First();
            //tbc = ConTelco.telcobrightconfigs.ToList().First();
        }
        Dictionary<string, route> dicRoutes = null;
        Dictionary<string, xyzprefix> dicPrefixTemp = null;//for now use xyzprefix, change later to include product table
        //Dictionary<string, string> dicPrefixDesc = new Dictionary<string, string>();
        Dictionary<string, string> dicPrefixDesc = new Dictionary<string, string>();
        //load rates under reference rate plan to show min customer rate
        List<PrefixVsLowestRate> prefVsLowestRate = new List<PrefixVsLowestRate>();
        int selectedIdRatePlan = Convert.ToInt32(this.DropDownListRatePlanLcr.SelectedValue);
        
        using (PartnerEntities ConPartner = new PartnerEntities())
        {
            //dicPrefixDesc = ConPartner.rates.Where(c => c.idrateplan == selectedIdRatePlan).ToDictionary(c => c.Prefix, c => c.description);
            dicPrefixDesc = ConPartner.rates.Where(c => c.idrateplan == selectedIdRatePlan)
                .GroupBy(g=>new { g.Prefix,g.description})
                .Select(group=>new {Prefix=group.Key.Prefix,description=group.Key.description })
                .ToDictionary(c => c.Prefix, c => c.description);

            dicRoutes = ConPartner.routes.Include("partner").ToDictionary(c => c.idroute.ToString());
            dicPrefixTemp = ConPartner.xyzprefixes.ToDictionary(c => c.Prefix);
            //foreach (KeyValuePair<string, xyzprefix> kv in dicPrefixTemp)
            //{
            //    dicPrefixDesc.Add(kv.Key, kv.Value.Description);
            //}
            if (strIds != "")
            {
                lstLCR = ConPartner.Database.SqlQuery<lcr>(Sql).ToList();//load LCR
            }
            string rateColumnName = "otheramount1";//take from page config in global config later
            prefVsLowestRate = ConPartner.Database.SqlQuery<PrefixVsLowestRate>(" select prefix,min("+rateColumnName+") as LowestRate from rate " +
                                                            " where idrateplan=" + idRatePlan +
                                                            " group by prefix ").ToList();
            Dictionary<string, double> dicLowestRate = new Dictionary<string, double>();
            foreach(PrefixVsLowestRate pfxRate in prefVsLowestRate)
            {
                dicLowestRate.Add(pfxRate.prefix, pfxRate.LowestRate);
            }
            foreach (lcr l in lstLCR)
            {
                LcrDisplayClass newLcrEntry = l.GetDisplayClass(dicRoutes, dicPrefixDesc);
                //min Customer rate part
                double tempLowestRate = 0;
                if (dicLowestRate.TryGetValue(l.Prefix, out tempLowestRate) == true)
                {
                    newLcrEntry.LowestRate = tempLowestRate;

                }
                lstLCRDisplay.Add(newLcrEntry);
            }
            

        }
        return lstLCRDisplay;
    }
    protected class LcrRowForExport
    {
        public string Prefix { get; set; }
        public string Description { get; set; }
        public double LowestSellingRate { get; set; }
        public int Rank { get; set; }
        public double Cost { get; set; }
        public string Supplier { get; set; }
        public string Route { get; set; }
        public string SupplierPrefix { get; set; }
        
    }
    
    protected class supplierAndPrefix
    {
        public string supplier { get; set; }
        public string supplierPrefix { get; set; }
    }
    void ExportLCR()
    {
        try
        {
            List<LcrDisplayClass> lstLcrDisplay = GetLcr();
            if (lstLcrDisplay.Count == 0)
            {
                this.lblStatus.Text = "No Records !";
            }
            else
            {
                //MyGridViewDataBound(lstLcrDisplay);
                List<LcrRowForExport> lstLcrExcel = new List<LcrRowForExport>();
                
                Dictionary<string, string> dicIdRouteVsPartner = new Dictionary<string, string>();
                using (PartnerEntities context = new PartnerEntities())
                {
                    //dicIdRouteVsPartner = context.routes.Include("partners").ToDictionary(c => c.idroute.ToString(), c => c.partner.PartnerName);
                }

                foreach (LcrDisplayClass lcr in lstLcrDisplay)//for each prefix
                {
                    Dictionary<string, List<RoutePartnerPair>> dicCostEntities = lcr.DicCostEntities;
                    List<string> lcrColumns = new List<string>();
                    if (dicCostEntities.Count > 0)
                    {
                        List<CostWiseLCREntry> lstCostWiseEntry = GetCostWiseEntry(dicCostEntities);
                        int rank = 0;
                        foreach (CostWiseLCREntry cEntry in lstCostWiseEntry)
                        {
                            ++rank;
                            List<RoutePartnerPair> lstRouteCarrierPair = cEntry.lstRouteCarrierPair;
                            //export with route
                            foreach (RoutePartnerPair rcPair in lstRouteCarrierPair)
                            {
                                LcrRowForExport lcrRowExport = new LcrRowForExport();
                                lcrRowExport.Prefix = lcr.Prefix;
                                lcrRowExport.Description = lcr.PrefixDescription;
                                lcrRowExport.LowestSellingRate = lcr.LowestRate;
                                lcrRowExport.Cost = cEntry.Cost;
                                lcrRowExport.Route = rcPair.SwitchId + "-" + rcPair.RouteName;
                                lcrRowExport.Supplier = rcPair.PartnerName;
                                lcrRowExport.SupplierPrefix = rcPair.SupplierPrefix;
                                lcrRowExport.Rank = rank;
                                lstLcrExcel.Add(lcrRowExport);
                            }
                        }
                    }
                }
                ExcelPackage pck = CreateExcelFileAspNet.CreateExcelDocument(lstLcrExcel);
                ExcelWorksheet shSupplier = pck.Workbook.Worksheets.Add("Supplier");
                pck.Workbook.Worksheets[1].Name = "Route";
                //set bold for column headers
                for (int i = 1; i <= 8; i++)
                {
                    pck.Workbook.Worksheets[1].Cells[1, i].Style.Font.Bold = true;
                }
                //Rank Sheet Part

                int rankRow = 2;
                int maxRank = 0;//column count for heading
                foreach (LcrDisplayClass lcr in lstLcrDisplay)//for each prefix
                {
                    int rankCol = 1;
                    Dictionary<string, List<RoutePartnerPair>> dicCostEntities = lcr.DicCostEntities;
                    List<string> lcrColumns = new List<string>();
                    shSupplier.Cells[rankRow, rankCol++].Value = lcr.Prefix;
                    shSupplier.Cells[rankRow, rankCol++].Value = lcr.PrefixDescription;
                    shSupplier.Cells[rankRow, rankCol++].Value = lcr.LowestRate;
                    shSupplier.Cells[rankRow, rankCol++].Value = lcr.StartDate;
                    shSupplier.Cells[rankRow, (rankCol - 1)].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    int rankCount = 0;
                    if (dicCostEntities.Count > 0)
                    {
                        List<CostWiseLCREntry> lstCostWiseEntry = GetCostWiseEntry(dicCostEntities);
                        foreach (CostWiseLCREntry cEntry in lstCostWiseEntry)
                        {
                            ++rankCount;
                            maxRank = rankCount > maxRank ? rankCount : maxRank;
                            List<RoutePartnerPair> lstRouteCarrierPair = cEntry.lstRouteCarrierPair;
                            List<supplierAndPrefix> lstSuppx = lstRouteCarrierPair.GroupBy(g => new { g.PartnerName, g.SupplierPrefix })
                                .Select(group => new supplierAndPrefix { supplier = group.Key.PartnerName, supplierPrefix = group.Key.SupplierPrefix }).ToList();

                            shSupplier.Cells[rankRow, rankCol++].Value = cEntry.Cost;
                            shSupplier.Cells[rankRow, rankCol++].Value =
                                string.Join(",",
                                    lstSuppx.Select(c => c.supplier + " (" + c.supplierPrefix + ")"));
                        }
                        
                    }
                    rankRow++;
                }
                //write column header in rank sheet
                int rankColIndex = 5;
                int startRankColIndex = rankColIndex;
                ////set wrap for supplier column
                //for (int i = 1; i <= maxRank; i = i + 2)
                //{
                //    shRank.Column(startRankColIndex + 2 + i).Style.WrapText = true;
                //}
                //set bold for column headers
                for (int i = 1; i <= maxRank * 2 + 5; i++)
                {
                    shSupplier.Cells[1, i].Style.Font.Bold = true;
                }
                for (int i = 1; i <= maxRank; i++)
                {
                    shSupplier.Cells[1, rankColIndex++].Value = "Rank" + i.ToString() + "_Cost";
                    shSupplier.Cells[1, rankColIndex++].Value = "Rank" + i.ToString() + "_Carrier";
                }
                
                shSupplier.Cells[1, 1].Value = "Prefix";
                shSupplier.Cells[1, 2].Value = "Destination";
                shSupplier.Cells[1, 3].Value = "Selling Rate";
                shSupplier.Cells[1, 4].Value = "Effective From";

                
                //autofit supplier columns
                for (int i = 1; i <= maxRank; i = i + 2)
                {
                    shSupplier.Column(startRankColIndex + 1 + i).AutoFit();
                }

                CreateExcelFileAspNet.WriteExcelSheetBrowser(pck,"LCR_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+".xlsx", this.Response);
                this.lblStatus.Text = lstLcrDisplay.Count + " Records Exported.";
                }
        }
        catch (Exception e1)
        {
            this.lblStatus.Text = e1.Message + "<br/>" + e1.InnerException;
            this.lblStatus.ForeColor = Color.Red;
        }
    }
}
