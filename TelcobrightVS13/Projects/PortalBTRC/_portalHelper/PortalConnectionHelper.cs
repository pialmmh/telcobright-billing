using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.IO;
using System.Web;
using Spring.Expressions;
using Spring.Core.TypeResolution;
using MediationModel;
using TelcobrightMediation.Config;
using LibraryExtensions.ConfigHelper;
using TelcobrightInfra;
/// <summary>
/// Summary description for CommonCode
/// </summary>

namespace PortalApp
{
    public class PortalConnectionHelper
    {
        public static PartnerEntities GetPartnerEntitiesDynamic(DatabaseSetting databaseSetting) {
            TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
            return new PartnerEntities(DbUtil.GetEntityConnectionString(databaseSetting));
        }
        public static PartnerEntities GetPartnerEntitiesDefault()
        {
            TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
            var databaseSetting = telcobrightConfig.DatabaseSetting;
            return new PartnerEntities(DbUtil.GetEntityConnectionString(databaseSetting));
        }
        public static PartnerEntities GetPartnerEntitiesByLoginId(string loggedInIcxName)
        {
            TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
            var databaseSetting = telcobrightConfig.DatabaseSetting;

            databaseSetting.DatabaseName = loggedInIcxName;
            return new PartnerEntities(DbUtil.GetEntityConnectionString(databaseSetting));
        }

        public static string GetReadOnlyConnectionString(DatabaseSetting databaseSetting)
        {
            var constr = DbUtil.getReadOnlyConStrWithDatabase(databaseSetting);
            return constr;
        }

    }
}
