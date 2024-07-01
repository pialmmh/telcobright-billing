using TelcobrightMediation;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using reports;
using System.Collections.Generic;
using MediationModel;
using PortalApp._portalHelper;

public partial class DashboardAspx : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
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
        telcobrightpartner thisPartner = null;
        string binpath = System.Web.HttpRuntime.BinDirectory;
        TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
        var databaseSetting = telcobrightConfig.DatabaseSetting;
        string userName = Page.User.Identity.Name;
        string dbName;
        if (telcobrightConfig.DeploymentProfile.UserVsDbName.ContainsKey(userName))
        {
            dbName = telcobrightConfig.DeploymentProfile.UserVsDbName[userName];
        }
        else
        {
            dbName = telcobrightConfig.DatabaseSetting.DatabaseName;
        }
        databaseSetting.DatabaseName = dbName;
        using (PartnerEntities conTelco = PortalConnectionHelper.GetPartnerEntitiesDynamic(databaseSetting))
        {
            List<telcobrightpartner> telcoTelcobrightpartners = conTelco.telcobrightpartners.ToList();
            thisPartner = telcoTelcobrightpartners.Where(c => c.databasename == dbName).ToList().First();
        }
        this.lblCustomerDisplayName.Text = thisPartner.CustomerName;

        //dashboard items
        UpdateErrorCalls();
        this.GridViewCompleted.DataBind();
        UpdateInternationalIncoming();
        this.Timer1.Enabled = true;
        this.Timer2.Enabled = true;
        this.Timer3.Enabled = true;
    }
    private void UpdateErrorCalls()
    {
        List<DashBoard.ErrorCalls> ec = new List<DashBoard.ErrorCalls>();
        using (PartnerEntities conPartner = new PartnerEntities())
        {
            ec = conPartner.Database.SqlQuery<DashBoard.ErrorCalls>(@"select ErrorCode as ErrorReason, count(*) as NumberOfCalls
                                                from cdrerror group by ErrorCode").ToList();
            this.HyperLinkError.Text = ec.Select(c => c.NumberOfCalls).Sum() + " Calls in Error";
            this.GridViewError.DataSource = ec;
            this.GridViewError.DataBind();

        }
    }
    protected void Timer1_Tick(object sender, EventArgs e)
    {
        UpdateErrorCalls();
    }
    protected void Timer2_Tick(object sender, EventArgs e)
    {
        this.GridViewCompleted.DataBind();
    }
    protected void Timer3_Tick(object sender, EventArgs e)
    {
        UpdateInternationalIncoming();
    }
    protected class IntlIn
    {
        public string PartnerName { get; set; }
        public double Minutes { get; set; }
    }
    private void UpdateInternationalIncoming()
    {
        return;
        List<IntlIn> intlIn = new List<IntlIn>();
        using (PartnerEntities context = new PartnerEntities())
        {
            intlIn = context.Database.SqlQuery<IntlIn>(
                " select p.PartnerName,round(Minutes,2) as Minutes from " +
                " (select customerid, sum(roundedduration) / 60 Minutes " +
                " from cdrsummary " +
                " where starttime > '" + DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                " and starttime < '" + (DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                " group by customerid " +
                " ) x " +
                " left join partner p " +
                " on x.customerid = p.idpartner " +
                " order by Minutes desc ").ToList();
        }
        this.HyperLinkIntlIn.Text = intlIn.Sum(c => c.Minutes).ToString() + " Minutes Today";
        this.GridViewIntlin.DataSource = intlIn;
        this.GridViewIntlin.DataBind();
    }
}