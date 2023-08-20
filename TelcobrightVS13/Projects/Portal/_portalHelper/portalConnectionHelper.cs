using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using TelcobrightInfra;
using TelcobrightMediation;

namespace PortalApp._portalHelper
{
    public class PortalConnectionHelper
    {
        public static PartnerEntities GetPartnerEntitiesDynamic(DatabaseSetting databaseSetting)
        {
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