using TelcobrightMediation;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Collections.Generic;
using MediationModel;
using PortalApp;
using TelcobrightInfra;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.UI.WebControls;

public partial class IPJobTableStatusOfICX : Page
{
    private static int currentPageForJobGrid = 0;
    private static int currentIndexForJobGrid = 20;
    private int offset = currentPageForJobGrid * currentIndexForJobGrid;
    private string icxConnstr;
    TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();


    string targetIcxName = "btrc_cas";
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

        BindGridViewForIPJobTableStatus();

        if (!IsPostBack)//initial
        {
        }

    }

    //humayun

    
    private void BindGridViewForIPJobTableStatus()
    {
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
            thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbName).ToList().First();

        }
        //this.lblCustomerDisplayName.Text = thisPartner.CustomerName;
        // Calculate the offset based on the current page index and page size
        //int pageIndex = GridViewCompleted.PageIndex;
        //int pageSize = GridViewCompleted.PageSize;

        string connectionString = DbUtil.getDbConStrWithDatabase(databaseSetting);

        List<GridViewIPJobTableStatus> gridViewIPJobTableStatus = new List<GridViewIPJobTableStatus>();

        // Modify your SQL query to include the OFFSET and FETCH NEXT clauses
        string sqlCommand = $@"SELECT z.IcxName, (SELECT 'REVE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'agni_cas') AS IcxName, COUNT(*) AS ErrorCall FROM agni_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'agni_cas') AS IcxName, COUNT(*) AS TotalJob FROM agni_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'agni_cas') AS IcxName, COUNT(*) AS CompletedJob FROM agni_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'GENBAND') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'agni_cas') AS IcxName, COUNT(*) AS ErrorCall FROM agni_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'agni_cas') AS IcxName, COUNT(*) AS TotalJob FROM agni_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'agni_cas') AS IcxName, COUNT(*) AS CompletedJob FROM agni_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'banglaicx_cas') AS IcxName, COUNT(*) AS ErrorCall FROM banglaicx_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'banglaicx_cas') AS IcxName, COUNT(*) AS TotalJob FROM banglaicx_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'banglaicx_cas') AS IcxName, COUNT(*) AS CompletedJob FROM banglaicx_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'GENBAND') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'banglaicx_cas') AS IcxName, COUNT(*) AS ErrorCall FROM banglaicx_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'banglaicx_cas') AS IcxName, COUNT(*) AS TotalJob FROM banglaicx_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'banglaicx_cas') AS IcxName, COUNT(*) AS CompletedJob FROM banglaicx_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'DIALOGIC') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'banglaTelecom_cas') AS IcxName, COUNT(*) AS ErrorCall FROM banglaTelecom_cas.cdrerror WHERE SwitchId =10) AS x LEFT JOIN (SELECT (SELECT 'banglaTelecom_cas') AS IcxName, COUNT(*) AS TotalJob FROM banglaTelecom_cas.job WHERE idjobdefinition = 1 AND idne= 10) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'banglaTelecom_cas') AS IcxName, COUNT(*) AS CompletedJob FROM banglaTelecom_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 10) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'banglaTelecom_cas') AS IcxName, COUNT(*) AS ErrorCall FROM banglaTelecom_cas.cdrerror WHERE SwitchId =9) AS x LEFT JOIN (SELECT (SELECT 'banglaTelecom_cas') AS IcxName, COUNT(*) AS TotalJob FROM banglaTelecom_cas.job WHERE idjobdefinition = 1 AND idne= 9) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'banglaTelecom_cas') AS IcxName, COUNT(*) AS CompletedJob FROM banglaTelecom_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 9) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'bantel_cas') AS IcxName, COUNT(*) AS ErrorCall FROM bantel_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'bantel_cas') AS IcxName, COUNT(*) AS TotalJob FROM bantel_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'bantel_cas') AS IcxName, COUNT(*) AS CompletedJob FROM bantel_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'bantel_cas') AS IcxName, COUNT(*) AS ErrorCall FROM bantel_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'bantel_cas') AS IcxName, COUNT(*) AS TotalJob FROM bantel_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'bantel_cas') AS IcxName, COUNT(*) AS CompletedJob FROM bantel_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT '') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS ErrorCall FROM btcl_cas.cdrerror WHERE SwitchId =3) AS x LEFT JOIN (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS TotalJob FROM btcl_cas.job WHERE idjobdefinition = 1 AND idne= 3) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS CompletedJob FROM btcl_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 3) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS ErrorCall FROM btcl_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS TotalJob FROM btcl_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS CompletedJob FROM btcl_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS ErrorCall FROM btcl_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS TotalJob FROM btcl_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'btcl_cas') AS IcxName, COUNT(*) AS CompletedJob FROM btcl_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS ErrorCall FROM crossworld_cas.cdrerror WHERE SwitchId =3) AS x LEFT JOIN (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS TotalJob FROM crossworld_cas.job WHERE idjobdefinition = 1 AND idne= 3) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS CompletedJob FROM crossworld_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 3) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'GENBAND') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS ErrorCall FROM crossworld_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS TotalJob FROM crossworld_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS CompletedJob FROM crossworld_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'TELCOBRIDGE_CTG') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS ErrorCall FROM crossworld_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS TotalJob FROM crossworld_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS CompletedJob FROM crossworld_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'TELCOBRIDGE_KHL') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS ErrorCall FROM crossworld_cas.cdrerror WHERE SwitchId =4) AS x LEFT JOIN (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS TotalJob FROM crossworld_cas.job WHERE idjobdefinition = 1 AND idne= 4) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'crossworld_cas') AS IcxName, COUNT(*) AS CompletedJob FROM crossworld_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 4) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS ErrorCall FROM gaziNetworks_cas.cdrerror WHERE SwitchId =3) AS x LEFT JOIN (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS TotalJob FROM gaziNetworks_cas.job WHERE idjobdefinition = 1 AND idne= 3) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS CompletedJob FROM gaziNetworks_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 3) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'GENBAND') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS ErrorCall FROM gaziNetworks_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS TotalJob FROM gaziNetworks_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS CompletedJob FROM gaziNetworks_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'TELCOBRIDGE_KHULNA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS ErrorCall FROM gaziNetworks_cas.cdrerror WHERE SwitchId =4) AS x LEFT JOIN (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS TotalJob FROM gaziNetworks_cas.job WHERE idjobdefinition = 1 AND idne= 4) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS CompletedJob FROM gaziNetworks_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 4) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'TELCOBRIDGE_BOGRA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS ErrorCall FROM gaziNetworks_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS TotalJob FROM gaziNetworks_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'gaziNetworks_cas') AS IcxName, COUNT(*) AS CompletedJob FROM gaziNetworks_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'imamNetwork_cas') AS IcxName, COUNT(*) AS ErrorCall FROM imamNetwork_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'imamNetwork_cas') AS IcxName, COUNT(*) AS TotalJob FROM imamNetwork_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'imamNetwork_cas') AS IcxName, COUNT(*) AS CompletedJob FROM imamNetwork_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'ZTE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'imamNetwork_cas') AS IcxName, COUNT(*) AS ErrorCall FROM imamNetwork_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'imamNetwork_cas') AS IcxName, COUNT(*) AS TotalJob FROM imamNetwork_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'imamNetwork_cas') AS IcxName, COUNT(*) AS CompletedJob FROM imamNetwork_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS ErrorCall FROM jibonDhara_cas.cdrerror WHERE SwitchId =18) AS x LEFT JOIN (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS TotalJob FROM jibonDhara_cas.job WHERE idjobdefinition = 1 AND idne= 18) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS CompletedJob FROM jibonDhara_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 18) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'ZTE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS ErrorCall FROM jibonDhara_cas.cdrerror WHERE SwitchId =7) AS x LEFT JOIN (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS TotalJob FROM jibonDhara_cas.job WHERE idjobdefinition = 1 AND idne= 7) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS CompletedJob FROM jibonDhara_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 7) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'mnh_cas') AS IcxName, COUNT(*) AS ErrorCall FROM mnh_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'mnh_cas') AS IcxName, COUNT(*) AS TotalJob FROM mnh_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'mnh_cas') AS IcxName, COUNT(*) AS CompletedJob FROM mnh_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'mnh_cas') AS IcxName, COUNT(*) AS ErrorCall FROM mnh_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'mnh_cas') AS IcxName, COUNT(*) AS TotalJob FROM mnh_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'mnh_cas') AS IcxName, COUNT(*) AS CompletedJob FROM mnh_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'DIALOGIC') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'mothertelecom_cas') AS IcxName, COUNT(*) AS ErrorCall FROM mothertelecom_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'mothertelecom_cas') AS IcxName, COUNT(*) AS TotalJob FROM mothertelecom_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'mothertelecom_cas') AS IcxName, COUNT(*) AS CompletedJob FROM mothertelecom_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'NOKIA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'mothertelecom_cas') AS IcxName, COUNT(*) AS ErrorCall FROM mothertelecom_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'mothertelecom_cas') AS IcxName, COUNT(*) AS TotalJob FROM mothertelecom_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'mothertelecom_cas') AS IcxName, COUNT(*) AS CompletedJob FROM mothertelecom_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'REVE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'newGenerationTelecom_cas') AS IcxName, COUNT(*) AS ErrorCall FROM newGenerationTelecom_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'newGenerationTelecom_cas') AS IcxName, COUNT(*) AS TotalJob FROM newGenerationTelecom_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'newGenerationTelecom_cas') AS IcxName, COUNT(*) AS CompletedJob FROM newGenerationTelecom_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'NOKIA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'newGenerationTelecom_cas') AS IcxName, COUNT(*) AS ErrorCall FROM newGenerationTelecom_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'newGenerationTelecom_cas') AS IcxName, COUNT(*) AS TotalJob FROM newGenerationTelecom_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'newGenerationTelecom_cas') AS IcxName, COUNT(*) AS CompletedJob FROM newGenerationTelecom_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'paradise_cas') AS IcxName, COUNT(*) AS ErrorCall FROM paradise_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'paradise_cas') AS IcxName, COUNT(*) AS TotalJob FROM paradise_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'paradise_cas') AS IcxName, COUNT(*) AS CompletedJob FROM paradise_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'paradise_cas') AS IcxName, COUNT(*) AS ErrorCall FROM paradise_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'paradise_cas') AS IcxName, COUNT(*) AS TotalJob FROM paradise_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'paradise_cas') AS IcxName, COUNT(*) AS CompletedJob FROM paradise_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'purple_cas') AS IcxName, COUNT(*) AS ErrorCall FROM purple_cas.cdrerror WHERE SwitchId =3) AS x LEFT JOIN (SELECT (SELECT 'purple_cas') AS IcxName, COUNT(*) AS TotalJob FROM purple_cas.job WHERE idjobdefinition = 1 AND idne= 3) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'purple_cas') AS IcxName, COUNT(*) AS CompletedJob FROM purple_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 3) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'purple_cas') AS IcxName, COUNT(*) AS ErrorCall FROM purple_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'purple_cas') AS IcxName, COUNT(*) AS TotalJob FROM purple_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'purple_cas') AS IcxName, COUNT(*) AS CompletedJob FROM purple_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'GNEW') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'ringTech_cas') AS IcxName, COUNT(*) AS ErrorCall FROM ringTech_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'ringTech_cas') AS IcxName, COUNT(*) AS TotalJob FROM ringTech_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'ringTech_cas') AS IcxName, COUNT(*) AS CompletedJob FROM ringTech_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'ZTE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'ringTech_cas') AS IcxName, COUNT(*) AS ErrorCall FROM ringTech_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'ringTech_cas') AS IcxName, COUNT(*) AS TotalJob FROM ringTech_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'ringTech_cas') AS IcxName, COUNT(*) AS CompletedJob FROM ringTech_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'REVE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'sheba_cas') AS IcxName, COUNT(*) AS ErrorCall FROM sheba_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'sheba_cas') AS IcxName, COUNT(*) AS TotalJob FROM sheba_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'sheba_cas') AS IcxName, COUNT(*) AS CompletedJob FROM sheba_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'sheba_cas') AS IcxName, COUNT(*) AS ErrorCall FROM sheba_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'sheba_cas') AS IcxName, COUNT(*) AS TotalJob FROM sheba_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'sheba_cas') AS IcxName, COUNT(*) AS CompletedJob FROM sheba_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'softex_cas') AS IcxName, COUNT(*) AS ErrorCall FROM softex_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'softex_cas') AS IcxName, COUNT(*) AS TotalJob FROM softex_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'softex_cas') AS IcxName, COUNT(*) AS CompletedJob FROM softex_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'softex_cas') AS IcxName, COUNT(*) AS ErrorCall FROM softex_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'softex_cas') AS IcxName, COUNT(*) AS TotalJob FROM softex_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'softex_cas') AS IcxName, COUNT(*) AS CompletedJob FROM softex_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'srTelecom_cas') AS IcxName, COUNT(*) AS ErrorCall FROM srTelecom_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'srTelecom_cas') AS IcxName, COUNT(*) AS TotalJob FROM srTelecom_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'srTelecom_cas') AS IcxName, COUNT(*) AS CompletedJob FROM srTelecom_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'srTelecom_cas') AS IcxName, COUNT(*) AS ErrorCall FROM srTelecom_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'srTelecom_cas') AS IcxName, COUNT(*) AS TotalJob FROM srTelecom_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'srTelecom_cas') AS IcxName, COUNT(*) AS CompletedJob FROM srTelecom_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'DIALOGIC') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'summit_cas') AS IcxName, COUNT(*) AS ErrorCall FROM summit_cas.cdrerror WHERE SwitchId =10) AS x LEFT JOIN (SELECT (SELECT 'summit_cas') AS IcxName, COUNT(*) AS TotalJob FROM summit_cas.job WHERE idjobdefinition = 1 AND idne= 10) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'summit_cas') AS IcxName, COUNT(*) AS CompletedJob FROM summit_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 10) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'ZTE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'summit_cas') AS IcxName, COUNT(*) AS ErrorCall FROM summit_cas.cdrerror WHERE SwitchId =9) AS x LEFT JOIN (SELECT (SELECT 'summit_cas') AS IcxName, COUNT(*) AS TotalJob FROM summit_cas.job WHERE idjobdefinition = 1 AND idne= 9) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'summit_cas') AS IcxName, COUNT(*) AS CompletedJob FROM summit_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 9) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS ErrorCall FROM teleExchange_cas.cdrerror WHERE SwitchId =3) AS x LEFT JOIN (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS TotalJob FROM teleExchange_cas.job WHERE idjobdefinition = 1 AND idne= 3) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS CompletedJob FROM teleExchange_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 3) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS ErrorCall FROM teleExchange_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS TotalJob FROM teleExchange_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS CompletedJob FROM teleExchange_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'TELCOBRIDGE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS ErrorCall FROM teleExchange_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS TotalJob FROM teleExchange_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'teleExchange_cas') AS IcxName, COUNT(*) AS CompletedJob FROM teleExchange_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'WTL') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'telePlusNewyork_cas') AS IcxName, COUNT(*) AS ErrorCall FROM telePlusNewyork_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'telePlusNewyork_cas') AS IcxName, COUNT(*) AS TotalJob FROM telePlusNewyork_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'telePlusNewyork_cas') AS IcxName, COUNT(*) AS CompletedJob FROM telePlusNewyork_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'telePlusNewyork_cas') AS IcxName, COUNT(*) AS ErrorCall FROM telePlusNewyork_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'telePlusNewyork_cas') AS IcxName, COUNT(*) AS TotalJob FROM telePlusNewyork_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'telePlusNewyork_cas') AS IcxName, COUNT(*) AS CompletedJob FROM telePlusNewyork_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'REVE') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'voiceTel_cas') AS IcxName, COUNT(*) AS ErrorCall FROM voiceTel_cas.cdrerror WHERE SwitchId =2) AS x LEFT JOIN (SELECT (SELECT 'voiceTel_cas') AS IcxName, COUNT(*) AS TotalJob FROM voiceTel_cas.job WHERE idjobdefinition = 1 AND idne= 2) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'voiceTel_cas') AS IcxName, COUNT(*) AS CompletedJob FROM voiceTel_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 2) AS xy ON z.IcxName = xy.IcxName union all
                            SELECT z.IcxName, (SELECT 'HUWAEI') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'voiceTel_cas') AS IcxName, COUNT(*) AS ErrorCall FROM voiceTel_cas.cdrerror WHERE SwitchId =1) AS x LEFT JOIN (SELECT (SELECT 'voiceTel_cas') AS IcxName, COUNT(*) AS TotalJob FROM voiceTel_cas.job WHERE idjobdefinition = 1 AND idne= 1) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'voiceTel_cas') AS IcxName, COUNT(*) AS CompletedJob FROM voiceTel_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 1) AS xy ON z.IcxName = xy.IcxName ;";

        //sqlCommand = $@"SELECT z.IcxName, (SELECT 'CATALEYA') AS SwitchName, z.TotalJob, xy.CompletedJob, z.ErrorCall FROM (SELECT x.IcxName, y.TotalJob, x.ErrorCall FROM (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS ErrorCall FROM jibonDhara_cas.cdrerror WHERE SwitchId =18) AS x LEFT JOIN (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS TotalJob FROM jibonDhara_cas.job WHERE idjobdefinition = 1 AND idne= 18) AS y ON x.IcxName = y.IcxName) AS z LEFT JOIN (SELECT (SELECT 'jibonDhara_cas') AS IcxName, COUNT(*) AS CompletedJob FROM jibonDhara_cas.job WHERE idjobdefinition = 1 AND status = 1 AND idne = 18) AS xy ON z.IcxName = xy.IcxName;";
        using (MySqlConnection connection = new MySqlConnection())
        {
            connection.ConnectionString = connectionString;
            connection.Open();
            DataSet dataSet = gridViewCompletedData(connection, sqlCommand);

            bool hasData = dataSet.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);

            if (hasData == true)
            {
                gridViewIPJobTableStatus = ConvertDataSetToListForIPJobTableStatus(dataSet);
                GridViewIPJobTableStatus1.DataSource = gridViewIPJobTableStatus;
                GridViewIPJobTableStatus1.DataBind();
            }
        }
    }

    protected void GridViewCompleted_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        // Set the new page index
    }
    //protected void btnSearch_Click(object sender, EventArgs e)
    //{
    //    // Get the search term from the input field
    //    string searchTerm = GridViewTgs;

    //    // Implement your search logic here
    //    // You can search the database, filter a list, or perform any other search operation

    //    // Update your UI to display search results
    //    // For example, you can bind search results to a GridView or display them in a list
    //}

    protected void PreviousButton_Click(object sender, EventArgs e)
    {
        // Go to the previous page
        currentPageForJobGrid--;
        //currentIndexForJobGrid--;
        //if (GridViewCompleted.PageIndex > 0)
        //{
        //    GridViewCompleted.PageIndex--;
        //    BindGridView();
        //}
    }

    protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Find the DropDownList control in the current row
            DropDownList ddl = (DropDownList)e.Row.FindControl("DropDownListPartner");

            // Check if the DropDownList control is found
            if (ddl != null)
            {
                using (PartnerEntities context = PortalConnectionHelper.GetPartnerEntitiesDynamic(telcobrightConfig.DatabaseSetting))
                {
                    List<partner> iosList = context.partners.ToList();

                    // Clear the DropDownList and add the "Select" option
                    ddl.Items.Clear();
                    ddl.Items.Add(new ListItem("Select", "Select"));

                    foreach (partner p in iosList.OrderBy(x => x.PartnerName))
                    {
                        ddl.Items.Add(new ListItem(p.PartnerName, p.idPartner.ToString()));
                    }
                }
            }
        }
    }

    protected void NextButton_Click(object sender, EventArgs e)
    {
        // Go to the next page
           currentPageForJobGrid++;
        //currentIndexForJobGrid++;
        //if (GridViewCompleted.PageIndex < GridViewCompleted.PageCount - 1)
        //{
        //    GridViewCompleted.PageIndex++;
        //    BindGridView();
        //}
    }

    protected void Timer2_Tick(object sender, EventArgs e)
    {

    }
    
    protected class IntlIn
    {
        public string PartnerName { get; set; }
        public double Minutes { get; set; }
    }
    protected class GridViewCompletedJob
    {
        public int id { get; set; }
        public string jobName { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime completionTime { get; set; }


    }
    protected class GridViewIPJobTableStatus
    {
        public int Id { get; set; }
        public string IcxName { get; set; }
        public string switchName { get; set; }
        public int TotalJob { get; set; }
        public int CompletedJob { get; set; }
        public int ErrorCall { get; set; }
    }

    DataSet gridViewCompletedData(MySqlConnection connection, string sql)
    {

        MySqlCommand cmd = new MySqlCommand(sql, connection);
        cmd.Connection = connection;
        MySqlDataAdapter domDataAdapter = new MySqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        domDataAdapter.Fill(ds);
        return ds;
    }
    List<GridViewIPJobTableStatus> ConvertDataSetToListForIPJobTableStatus(DataSet ds)
    {
        bool hasRecords = ds.Tables.Cast<DataTable>()
            .Any(table => table.Rows.Count != 0);
        List<GridViewIPJobTableStatus> records = new List<GridViewIPJobTableStatus>();
        if (hasRecords == true)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    GridViewIPJobTableStatus record = new GridViewIPJobTableStatus();
                    record.IcxName = row["IcxName"].ToString();
                    record.switchName = row["switchName"].ToString();
                    record.TotalJob = Convert.ToInt32(row["TotalJob"]);
                    record.CompletedJob = Convert.ToInt32(row["CompletedJob"]);
                    record.ErrorCall = Convert.ToInt32(row["ErrorCall"]);
                    records.Add(record);
                }
            }
        }
        return records;
    }
}