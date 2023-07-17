using TelcobrightMediation;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.UI;
using MediationModel;
using PortalApp;
public partial class DefaultAspx : Page
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
        
        if (!string.IsNullOrEmpty(telcobrightConfig.PortalSettings.HomePageUrl))
        {
            this.Response.Redirect(telcobrightConfig.PortalSettings.HomePageUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }
        using (PartnerEntities conTelco = new PartnerEntities())
        {
            thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbNameAppConf).ToList().First();
        }
        this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
    }
}